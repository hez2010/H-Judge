using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace hjudge.Core
{
    public class JudgeMain
    {
        private string workingDir = string.Empty;
        private string dataCacheDir = string.Empty;
        private readonly ILogger? logger;
        private const int LineLimits = 256;
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = false,
            IgnoreNullValues = true
        };

        [DllImport("./hjudge.Exec.Windows.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "execute", CharSet = CharSet.Ansi)]
        static extern bool ExecuteWindows(string prarm, [MarshalAs(UnmanagedType.LPStr)]StringBuilder ret);

        [DllImport("./hjudge.Exec.Linux.so", CallingConvention = CallingConvention.Cdecl, EntryPoint = "execute", CharSet = CharSet.Ansi)]
        static extern bool ExecuteLinux(string prarm, [MarshalAs(UnmanagedType.LPStr)]StringBuilder ret);

        static (bool Succeeded, JudgePoint? Result) Execute(string param)
        {
            var result = false;
            var retBuffer = new StringBuilder(256);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) result = ExecuteWindows(param, retBuffer);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) result = ExecuteLinux(param, retBuffer);

            JudgePoint? ret = null;
            try
            {
                var retStr = retBuffer.ToString().Trim();
                ret = JsonSerializer.Deserialize<JudgePoint>(retStr, options);
            }
            catch { /* ignored */ }

            return (result, ret);
        }

        public JudgeMain(string environments = "", ILogger? logger = null)
        {
            if (!string.IsNullOrEmpty(environments))
            {
                var current = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                var newValue = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? environments : environments.Replace(';', ':');
                if (current.IndexOf(newValue, StringComparison.Ordinal) < 0)
                {
                    Environment.SetEnvironmentVariable("PATH",
                        $"{newValue}{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ";" : ":")}{Environment.GetEnvironmentVariable("PATH")}");
                }
            }
            this.logger = logger;
        }

        public static string EscapeFileName(string? fileName) => fileName
                ?.Replace(":", "_1_")
                .Replace("\\", "_2_")
                .Replace("/", "_2_")
                .Replace("?", "_3_")
                .Replace("*", "_4_")
                .Replace("\"", "")
                .Replace("<", "_5_")
                .Replace(">", "_6_")
                .Replace("|", "_7_") ?? string.Empty;

        private string? GetTargetFilePath(string? originalFilePath)
        {
            if (originalFilePath?.StartsWith("R:") ?? false) return Path.Combine(dataCacheDir, EscapeFileName(originalFilePath![2..]));
            return originalFilePath;
        }

        private string? GetMiddleDirectoryPath(string? originalFilePath)
        {
            if (originalFilePath != null && originalFilePath.StartsWith("R:"))
            {
                var fileName = originalFilePath[2..];
                var fragment = fileName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                // path: Data/{Problem Id}/.../file
                if (fragment.Length >= 2 && fragment[0] == "Data")
                {
                    var sb = new StringBuilder();
                    for (var i = 2; i < fragment.Length - 1; i++)
                    {
                        sb.Append(fragment[i]);
                        sb.Append('/');
                    }
                    var result = sb.ToString();
                    // remove the last '/'
                    return result.Length == 0 ? "" : sb.ToString()[..^1];
                }
            }
            return "";
        }

        public async Task<JudgeResult> JudgeAsync(BuildOptions buildOptions, JudgeOptions judgeOptions, string workingDir, string dataCacheDir)
        {
            this.workingDir = workingDir;
            this.dataCacheDir = dataCacheDir;

            Directory.CreateDirectory(this.workingDir);
            var result = new JudgeResult
            {
                JudgePoints = new List<JudgePoint>()
            };

            try
            {
                if (judgeOptions.AnswerPoint != null)
                {
                    return await AnswerJudgeAsync(buildOptions, judgeOptions);
                }

                foreach (var i in buildOptions.SourceFiles)
                {
                    var path = Path.Combine(workingDir, i.FileName);
                    var dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllText(path, i.Content, Encoding.UTF8);
                }

                if (buildOptions.StaticCheckOption != null)
                {
                    var logs = await StaticCheck(buildOptions.StaticCheckOption);
                    result.StaticCheckLog = logs;
                }

                if (buildOptions.CompilerOption != null)
                {
                    var (isSucceeded, logs) = await Compile(buildOptions.CompilerOption, judgeOptions.ExtraFiles);
                    result.CompileLog = logs;
                    if (!isSucceeded)
                    {
                        result.JudgePoints = Enumerable.Repeat(
                            new JudgePoint
                            {
                                ResultType = ResultCode.Compile_Error
                            }, judgeOptions.DataPoints.Count)
                        .ToList();
                        return result;
                    }
                }

                for (var i = 0; i < judgeOptions.DataPoints.Count; i++)
                {
                    this.logger?.LogInformation("Run standard judge point --> {0}", $"Point: {i + 1}");
                    var point = new JudgePoint();
                    if (!File.Exists(judgeOptions.RunOptions.Exec))
                    {
                        point.ResultType = ResultCode.Compile_Error;
                        point.ExtraInfo = "Cannot find compiled executable file.";
                        result.JudgePoints.Add(point);
                        continue;
                    }
                    try
                    {
                        try
                        {
                            File.Copy(GetTargetFilePath(judgeOptions.DataPoints[i].StdInFile), Path.Combine(this.workingDir, judgeOptions.InputFileName), true);
                        }
                        catch
                        {
                            throw new InvalidOperationException("Unable to find standard input file.");
                        }
                        var strErrFile = Path.Combine(this.workingDir, $"stderr_{judgeOptions.GuidStr}.dat");
                        var inputFile = Path.Combine(this.workingDir, judgeOptions.InputFileName);
                        var outputFile = Path.Combine(this.workingDir, judgeOptions.OutputFileName);
                        var param = new ExecOptions
                        {
                            Exec = judgeOptions.RunOptions.Exec,
                            Args = judgeOptions.RunOptions.Args,
                            WorkingDir = this.workingDir,
                            StdErrRedirectFile = strErrFile,
                            InputFile = inputFile,
                            OutputFile = outputFile,
                            TimeLimit = judgeOptions.DataPoints[i].TimeLimit,
                            MemoryLimit = judgeOptions.DataPoints[i].MemoryLimit,
                            IsStdIO = judgeOptions.UseStdIO,
                            ActiveProcessLimit = judgeOptions.ActiveProcessLimit
                        };
                        var (succeeded, ret) = Execute(JsonSerializer.Serialize(param, options));
                        if (succeeded && ret != null)
                        {
                            point = ret;
                        }
                        else
                        {
                            throw new Exception("Unable to execute target program or fetch result.");
                        }
                        try
                        {
                            var path = judgeOptions.DataPoints[i].StdOutFile.Replace("${index}", (i + 1).ToString()).Replace("${index0}", i.ToString());
                            File.Copy(GetTargetFilePath(path), Path.Combine(this.workingDir, $"answer_{judgeOptions.GuidStr}.dat"), true);
                        }
                        catch
                        {
                            throw new InvalidOperationException("Unable to find standard output file.");
                        }

                        if (point.ResultType == ResultCode.Runtime_Error)
                        {
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                                Enum.IsDefined(typeof(WindowsExceptionCode), (uint)point.ExitCode))
                            {
                                point.ExtraInfo = Enum.GetName(typeof(WindowsExceptionCode), (uint)point.ExitCode) ?? string.Empty;
                            }
                            else if (Enum.IsDefined(typeof(LinuxExceptionCode), point.ExitCode - 128))
                            {
                                point.ExtraInfo = Enum.GetName(typeof(LinuxExceptionCode), point.ExitCode - 128) ?? string.Empty;
                            }
                            else
                            {
                                point.ExtraInfo = "UNKNOWN_EXCEPTION";
                            }
                            point.Score = 0;
                        }
                        else
                        {
                            if (judgeOptions.StandardErrorBehavior != StdErrBehavior.Ignore)
                            {
                                try
                                {
                                    var stderr = File.ReadAllText(strErrFile).Trim()
                                                     .Replace(this.workingDir, "...")
                                                     .Replace(this.workingDir.Replace("/", "\\"), "...");

                                    if (!string.IsNullOrWhiteSpace(stderr.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty).Trim()))
                                    {
                                        switch (judgeOptions.StandardErrorBehavior)
                                        {
                                            case StdErrBehavior.TreatAsCompileError:
                                                point.ResultType = ResultCode.Compile_Error;
                                                point.ExtraInfo = stderr;
                                                break;
                                            case StdErrBehavior.TreatAsRuntimeError:
                                                point.ResultType = ResultCode.Runtime_Error;
                                                point.ExtraInfo = stderr;
                                                break;
                                        }
                                        point.Score = 0;
                                    }
                                }
                                catch
                                {
                                    //ignored
                                }
                            }

                            if (point.ResultType == ResultCode.Accepted)
                            {
                                var (resultType, percentage, extraInfo) = point.ResultType == ResultCode.Accepted ?
                                   await CompareAsync(workingDir, inputFile, Path.Combine(this.workingDir, $"answer_{judgeOptions.GuidStr}.dat"), outputFile, judgeOptions)
                                    : (point.ResultType, 0, point.ExtraInfo);
                                point.ExtraInfo = extraInfo;
                                point.ResultType = resultType;
                                point.Score = percentage * judgeOptions.DataPoints[i].Score;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        point.ExtraInfo = ex.Message;
                        if (ex is InvalidOperationException)
                        {
                            point.ResultType = ResultCode.Problem_Config_Error;
                        }
                        else
                        {
                            point.ResultType = ResultCode.Unknown_Error;
                        }
                    }

                    result.JudgePoints.Add(point);
                }
            }
            finally
            {
                try
                {
                    Directory.Delete(this.workingDir, true);
                }
                catch { /* ignored */ }
            }
            return result;
        }

        private async Task<JudgeResult> AnswerJudgeAsync(BuildOptions buildOptions, JudgeOptions judgeOptions)
        {
            this.logger?.LogInformation("Run answer judge point");
            var result = new JudgeResult
            {
                JudgePoints = new List<JudgePoint>
                {
                    new JudgePoint()
                }
            };

            if (judgeOptions.AnswerPoint == null || !File.Exists(judgeOptions.AnswerPoint.AnswerFile))
            {
                result.JudgePoints[0].ResultType = ResultCode.Problem_Config_Error;
            }

            try
            {
                File.Copy(GetTargetFilePath(judgeOptions.AnswerPoint?.AnswerFile), Path.Combine(workingDir, $"answer_{judgeOptions.GuidStr}.txt"), true);

                foreach (var i in buildOptions.SourceFiles)
                {
                    var path = Path.Combine(workingDir, i.FileName);
                    var dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllText(path, i.Content, Encoding.UTF8);
                }

                var (resultType, percentage, extraInfo) = await CompareAsync(
                    workingDir,
                    string.Empty,
                    Path.Combine(workingDir, $"answer_{judgeOptions.GuidStr}.txt"),
                    buildOptions.SourceFiles.Count == 0 ? string.Empty : Path.Combine(workingDir, buildOptions.SourceFiles[0].FileName), judgeOptions, true);
                result.JudgePoints[0].ResultType = resultType;
                result.JudgePoints[0].Score = percentage * (judgeOptions.AnswerPoint?.Score ?? 0);
                result.JudgePoints[0].ExtraInfo = extraInfo;
            }
            catch (Exception ex)
            {
                result.JudgePoints[0].ResultType = ResultCode.Unknown_Error;
                result.JudgePoints[0].ExtraInfo = ex.Message;
            }

            return result;
        }

        private async Task<(ResultCode Result, float Percentage, string ExtraInfo)> CompareAsync(string workingDir, string stdInputFile, string stdOutputFile, string outputFile, JudgeOptions judgeOption, bool isAnswerJudge = false)
        {
            if (!File.Exists(outputFile))
            {
                return (ResultCode.Output_File_Error, 0, string.Empty);
            }

            if (judgeOption.SpecialJudgeOptions != null)
            {
                var argsBuilder = new StringBuilder();
                if (judgeOption.SpecialJudgeOptions.UseWorkingDir)
                {
                    argsBuilder.Append($" \"{workingDir}\"");
                }
                if (judgeOption.SpecialJudgeOptions.UseOutputFile)
                {
                    argsBuilder.Append($" \"{outputFile}\"");
                }
                if (judgeOption.SpecialJudgeOptions.UseStdInputFile)
                {
                    argsBuilder.Append($" \"{stdInputFile}\"");
                }
                if (judgeOption.SpecialJudgeOptions.UseStdOutputFile)
                {
                    argsBuilder.Append($" \"{stdOutputFile}\"");
                }

                if (!File.Exists(judgeOption.SpecialJudgeOptions.Exec))
                {
                    try
                    {
                        var realExec = judgeOption.SpecialJudgeOptions.Exec.StartsWith("R:") ?
                            Path.Combine(workingDir, Path.GetFileName(judgeOption.SpecialJudgeOptions.Exec)) : judgeOption.SpecialJudgeOptions.Exec;
                        File.Copy(GetTargetFilePath(judgeOption.SpecialJudgeOptions.Exec), realExec);
                        judgeOption.SpecialJudgeOptions.Exec = realExec;
                    }
                    catch
                    {
                        return (ResultCode.Special_Judge_Error, 0, "Cannot find special judge");
                    }
                }

                this.logger?.LogInformation("Run SPJ --> {0}", $"Command: {judgeOption.SpecialJudgeOptions.Exec} {argsBuilder.ToString()}");
                using var judge = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = judgeOption.SpecialJudgeOptions.Exec,
                        Arguments = argsBuilder.ToString(),
                        ErrorDialog = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        StandardOutputEncoding = Encoding.UTF8
                    }
                };
                try
                {
                    judge.Start();
                }
                catch (Exception ex)
                {
                    return (ResultCode.Special_Judge_Error, 0, ex.Message);
                }

                if (judge.WaitForExit(60 * 1000))
                {
                    try
                    {
                        judge.Kill();
                    }
                    catch
                    {
                        /* ignored */
                    }
                }

                var (error, output) = (await judge.StandardError.ReadToEndAsync(), await judge.StandardOutput.ReadToEndAsync());

                if (judge.ExitCode != 0)
                {
                    return (ResultCode.Special_Judge_Error, 0, $"Special judge exited with code {judge.ExitCode}");
                }

                try
                {
                    var percentage = Convert.ToSingle(output.Trim());
                    return (
                        Math.Abs(percentage - 1f) < 0.001 ?
                            ResultCode.Accepted : ResultCode.Wrong_Answer,
                        percentage,
                        error);
                }
                catch
                {
                    return (ResultCode.Special_Judge_Error, 0, "Bad output format from special judge");
                }
            }

            StreamReader? std = null, act = null;
            var retryTimes = 0;
            do
            {
                try
                {
                    std = new StreamReader(new FileStream(stdOutputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    std?.Dispose();
                    std = null;
                    retryTimes++;
                    if (retryTimes > 100)
                    {
                        return (ResultCode.Unknown_Error, 0, ex.Message);
                    }
                    await Task.Delay(100);
                }
            } while (std == null);

            retryTimes = 0;
            do
            {
                try
                {
                    act = new StreamReader(new FileStream(outputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8, true);
                }
                catch (Exception ex)
                {
                    act?.Dispose();
                    act = null;
                    retryTimes++;
                    if (retryTimes > 100)
                    {
                        std.Dispose();
                        return (ResultCode.Unknown_Error, 0, ex.Message);
                    }
                    await Task.Delay(100);
                }
            } while (act == null);

            var line = 0;
            var result = new JudgePoint
            {
                ResultType = ResultCode.Accepted
            };

            while (!act.EndOfStream || !std.EndOfStream)
            {
                string? actline = null, stdline = null;
                if (!std.EndOfStream)
                {
                    stdline = std.ReadLine();
                }

                if (!act.EndOfStream)
                {
                    actline = act.ReadLine();
                }

                line++;
                if (judgeOption.ComparingOptions.IgnoreLineTailWhiteSpaces)
                {
                    if (!string.IsNullOrEmpty(stdline))
                    {
                        stdline = stdline.TrimEnd();
                    }

                    if (!string.IsNullOrEmpty(actline))
                    {
                        actline = actline.TrimEnd();
                    }
                }

                if (judgeOption.ComparingOptions.IgnoreTextTailLineFeeds)
                {
                    if (stdline == null)
                    {
                        stdline = string.Empty;
                    }

                    if (actline == null)
                    {
                        actline = string.Empty;
                    }
                }

                if (stdline != actline)
                {
                    if (!isAnswerJudge)
                    {
                        result.ExtraInfo =
                        $"Line {line}: \nexpect: {stdline?.Substring(0, LineLimits < stdline.Length ? LineLimits : stdline.Length) ?? "<nothing>"}{((stdline?.Length ?? 0) > LineLimits ? "..." : string.Empty)} \noutput: {actline?.Substring(0, LineLimits < actline.Length ? LineLimits : actline.Length) ?? "<nothing>"}{((actline?.Length ?? 0) > LineLimits ? "..." : string.Empty)}";
                    }

                    if ((stdline?.Replace(" ", string.Empty) ?? string.Empty) ==
                        (actline?.Replace(" ", string.Empty) ?? string.Empty))
                    {
                        result.ResultType = ResultCode.Presentation_Error;
                    }
                    else
                    {
                        result.ResultType = ResultCode.Wrong_Answer;
                        break;
                    }
                }
            }

            std.Dispose();
            act.Dispose();
            return (result.ResultType, result.ResultType == ResultCode.Accepted ? 1 : 0, result.ExtraInfo);
        }

        private async Task<string> StaticCheck(StaticCheckOptions checker)
        {
            this.logger?.LogInformation("Static check --> {0}", $"Command: {checker.Exec} {checker.Args}");
            using var sta = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = checker.Args,
                    ErrorDialog = false,
                    FileName = checker.Exec,
                    RedirectStandardError = checker.ReadStdError,
                    RedirectStandardOutput = checker.ReadStdOutput,
                    UseShellExecute = false,
                    WorkingDirectory = workingDir
                }
            };
            try
            {
                sta.Start();

                StringBuilder output = new StringBuilder();

                if (!sta.WaitForExit(30 * 1000))
                {
                    try
                    {
                        sta.Kill();
                    }
                    catch
                    {
                        /* ignored */
                    }
                }

                if (checker.ReadStdOutput)
                {
                    var temp = (await sta.StandardOutput.ReadToEndAsync()).Trim();
                    if (!string.IsNullOrWhiteSpace(temp))
                    {
                        output.AppendLine(temp);
                    }
                }

                if (checker.ReadStdError)
                {
                    var temp = (await sta.StandardError.ReadToEndAsync()).Trim();
                    if (!string.IsNullOrWhiteSpace(temp))
                    {
                        output.AppendLine(temp);
                    }
                }

                var log = MatchProblem(output.ToString(), checker.ProblemMatcher)
                    .Replace(workingDir, "...")
                    .Replace(workingDir.Replace("/", "\\"), "...");

                try
                {
                    sta.Kill();
                }
                catch
                {
                    /* ignored */
                }

                return sta.ExitCode == 0 ? log : string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task<(bool IsSucceeded, string Logs)> Compile(CompilerOptions compiler, List<string> extra)
        {
            try
            {
                extra.ForEach(i =>
                {
                    var filePath = Path.Combine(workingDir, GetMiddleDirectoryPath(i), Path.GetFileName(i) ?? string.Empty);
                    var directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                    File.Copy(GetTargetFilePath(i), filePath, true);
                });
            }
            catch
            {
                return (false, "Cannot copy one of extra files");
            }

            this.logger?.LogInformation("Compile --> {0}", $"Command: {compiler.Exec} {compiler.Args}");
            using var comp = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = compiler.Args,
                    ErrorDialog = false,
                    FileName = compiler.Exec,
                    RedirectStandardError = compiler.ReadStdError,
                    RedirectStandardOutput = compiler.ReadStdOutput,
                    UseShellExecute = false,
                    WorkingDirectory = workingDir
                }
            };
            try
            {
                comp.Start();

                StringBuilder output = new StringBuilder();
                if (!comp.WaitForExit(30 * 1000))
                {
                    try
                    {
                        comp.Kill();
                    }
                    catch
                    {
                        /* ignored */
                    }
                }

                if (compiler.ReadStdOutput)
                {
                    var temp = (await comp.StandardOutput.ReadToEndAsync()).Trim();
                    if (!string.IsNullOrWhiteSpace(temp))
                    {
                        output.AppendLine(temp);
                    }
                }

                if (compiler.ReadStdError)
                {
                    var temp = (await comp.StandardError.ReadToEndAsync()).Trim();
                    if (!string.IsNullOrWhiteSpace(temp))
                    {
                        output.AppendLine(temp);
                    }
                }

                var log = MatchProblem(output.ToString(), compiler.ProblemMatcher)
                    .Replace(workingDir, "...")
                    .Replace(workingDir.Replace("/", "\\"), "...");
                try
                {
                    comp.Kill();
                }
                catch
                {
                    /* ignored */
                }

                return comp.ExitCode != 0 || !File.Exists(compiler.OutputFile) ? (false, log) : (true, log);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static string MatchProblem(string input, ProblemMatcher? matcher)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(matcher?.MatchPatterns))
            {
                return input;
            }

            try
            {
                var result = new StringBuilder();
                var re = new Regex(matcher.MatchPatterns, RegexOptions.Multiline);
                var ret = re.Matches(input);
                if (ret.Count == 0)
                {
                    return string.Empty;
                }

                foreach (Match? item in ret)
                {
                    if (item == null) continue;
                    GroupCollection matches = item.Groups;
                    var temp = matcher.DisplayFormat;
                    for (var i = 0; i < matches.Count; i++)
                    {
                        temp = temp.Replace($"${i}", matches[i].Value);
                    }

                    result.AppendLine(temp);
                }
                return result.ToString();
            }
            catch
            {
                return input;
            }
        }
    }
}
