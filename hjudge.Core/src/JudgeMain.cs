using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace hjudge.Core
{
    public class JudgeMain
    {
        private string _workingdir = string.Empty;

        [DllImport("./hjudge.Exec.Windows.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "excute", CharSet = CharSet.Ansi)]
        static extern bool ExecuteWindows(string prarm, [MarshalAs(UnmanagedType.LPStr)]StringBuilder ret);

        [DllImport("./hjudge.Exec.Linux.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "excute", CharSet = CharSet.Ansi)]
        static extern bool ExecuteLinux(string prarm, [MarshalAs(UnmanagedType.LPStr)]StringBuilder ret);

        static (bool Succeeded, string? Result) Execute(string param)
        {
            var result = false;
            var ret = new StringBuilder(256);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) result = ExecuteWindows(param, ret);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) result = ExecuteLinux(param, ret);

            return (result, ret.ToString()?.Trim() ?? "{}");
        } 

        public JudgeMain(string environments = "")
        {
            if (!string.IsNullOrEmpty(environments))
            {
                var current = Environment.GetEnvironmentVariable("PATH");
                var newValue = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? environments : environments.Replace(';', ':');
                if (current.IndexOf(newValue) < 0)
                {
                    Environment.SetEnvironmentVariable("PATH",
                        $"{newValue}{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ";" : ":")}{Environment.GetEnvironmentVariable("PATH")}");
                }
            }
        }

        public async Task<JudgeResult> JudgeAsync(BuildOptions buildOptions, JudgeOptions judgeOptions)
        {
            _workingdir = Path.Combine(Path.GetTempPath(), "hjudgeTest", judgeOptions.GuidStr);

            Directory.CreateDirectory(_workingdir);
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

                var fileName = Path.Combine(_workingdir, $"{buildOptions.SubmitFileName}{buildOptions.ExtensionName}");
                File.WriteAllText(fileName, buildOptions.Source, Encoding.UTF8);

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
                    var point = new JudgePoint();
                    if (!File.Exists(judgeOptions.RunOptions.Exec))
                    {
                        point.ResultType = ResultCode.Compile_Error;
                        point.ExtraInfo = "Cannot find compiled executable file";
                        result.JudgePoints.Add(point);
                        continue;
                    }
                    try
                    {
                        try
                        {
                            File.Copy(judgeOptions.DataPoints[i].StdInFile, Path.Combine(_workingdir, judgeOptions.InputFileName), true);
                        }
                        catch
                        {
                            throw new InvalidOperationException("Unable to find standard input file");
                        }
                        var strErrFile = Path.Combine(_workingdir, $"stderr_{judgeOptions.GuidStr}.dat");
                        var inputFile = Path.Combine(_workingdir, judgeOptions.InputFileName);
                        var outputFile = Path.Combine(_workingdir, judgeOptions.OutputFileName);
                        var param = new ExecOptions
                        {
                            Exec = judgeOptions.RunOptions.Exec,
                            Args = judgeOptions.RunOptions.Args,
                            WorkingDir = _workingdir,
                            StdErrRedirectFile = strErrFile,
                            InputFile = inputFile,
                            OutputFile = outputFile,
                            TimeLimit = judgeOptions.DataPoints[i].TimeLimit,
                            MemoryLimit = judgeOptions.DataPoints[i].MemoryLimit,
                            IsStdIO = judgeOptions.UseStdIO,
                            ActiveProcessLimit = judgeOptions.ActiveProcessLimit
                        };
                        var (succeeded, ret) = Execute(Encoding.UTF8.GetString(SpanJson.JsonSerializer.Generic.Utf8.Serialize(param)));
                        if (succeeded)
                        {
                            point = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<JudgePoint>(Encoding.UTF8.GetBytes(ret));
                        }
                        else
                        {
                            throw new Exception("Unable to execute target program");
                        }
                        try
                        {
                            File.Copy(judgeOptions.DataPoints[i].StdOutFile, Path.Combine(_workingdir, $"answer_{judgeOptions.GuidStr}.dat"), true);
                        }
                        catch
                        {
                            throw new InvalidOperationException("Unable to find standard output file");
                        }

                        if (point.ResultType == ResultCode.Runtime_Error)
                        {
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                                Enum.IsDefined(typeof(WindowsExceptionCode), (uint)point.ExitCode))
                            {
                                point.ExtraInfo = Enum.GetName(typeof(WindowsExceptionCode), (uint)point.ExitCode);
                            }
                            else if (Enum.IsDefined(typeof(LinuxExceptionCode), point.ExitCode - 128))
                            {
                                point.ExtraInfo = Enum.GetName(typeof(LinuxExceptionCode), point.ExitCode - 128);
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
                                                     .Replace(_workingdir, "...")
                                                     .Replace(_workingdir.Replace("/", "\\"), "...");

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
                                   await CompareAsync(fileName, inputFile, Path.Combine(_workingdir, $"answer_{judgeOptions.GuidStr}.dat"), outputFile, judgeOptions)
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
                    Directory.Delete(_workingdir, true);
                }
                catch { /* ignored */ }
            }
            return result;
        }

        private async Task<JudgeResult> AnswerJudgeAsync(BuildOptions buildOption, JudgeOptions judgeOption)
        {
            var result = new JudgeResult
            {
                JudgePoints = new List<JudgePoint>
                {
                    new JudgePoint()
                }
            };

            if (judgeOption.AnswerPoint == null || !File.Exists(judgeOption.AnswerPoint.AnswerFile ?? string.Empty))
            {
                result.JudgePoints[0].ResultType = ResultCode.Problem_Config_Error;
            }

            try
            {
                File.Copy(judgeOption.AnswerPoint.AnswerFile, Path.Combine(_workingdir, $"answer_{judgeOption.GuidStr}.txt"), true);
                var fileName = Path.Combine(_workingdir, buildOption.SubmitFileName);
                File.WriteAllText(fileName, buildOption.Source, Encoding.UTF8);
                var (resultType, percentage, extraInfo) = await CompareAsync(fileName, string.Empty, Path.Combine(_workingdir, $"answer_{judgeOption.GuidStr}.txt"), Path.Combine(_workingdir, buildOption.SubmitFileName), judgeOption, true);
                result.JudgePoints[0].ResultType = resultType;
                result.JudgePoints[0].Score = percentage * judgeOption.AnswerPoint.Score;
                result.JudgePoints[0].ExtraInfo = extraInfo;
            }
            catch (Exception ex)
            {
                result.JudgePoints[0].ResultType = ResultCode.Unknown_Error;
                result.JudgePoints[0].ExtraInfo = ex.Message;
            }

            return result;
        }

        private async Task<(ResultCode Result, float Percentage, string ExtraInfo)> CompareAsync(string sourceFile, string stdInputFile, string stdOutputFile, string outputFile, JudgeOptions judgeOption, bool isAnswerJudge = false)
        {
            if (!File.Exists(outputFile))
            {
                return (ResultCode.Output_File_Error, 0, string.Empty);
            }

            if (judgeOption.SpecialJudgeOptions != null)
            {
                var argsBuilder = new StringBuilder();
                if (judgeOption.SpecialJudgeOptions.UseSourceFile)
                {
                    argsBuilder.Append($" \"{sourceFile}\"");
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

                using (var judge = new Process
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
                })
                {
                    await Task.Delay(100);
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
                        return (ResultCode.Special_Judge_Error, 0, string.Empty);
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
                        return (ResultCode.Special_Judge_Error, 0, string.Empty);
                    }
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
                        std?.Dispose();
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
                        $"Line {line}: \nexpect: {stdline?.Substring(0, 64 < (stdline?.Length ?? 0) ? 64 : stdline?.Length ?? 0) ?? "<nothing>"}{((stdline?.Length ?? 0) > 64 ? "..." : string.Empty)} \noutput: {actline?.Substring(0, 64 < (actline?.Length ?? 0) ? 64 : actline?.Length ?? 0) ?? "<nothing>"}{((actline?.Length ?? 0) > 64 ? "..." : string.Empty)}";
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
            using (var sta = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = checker.Args,
                    ErrorDialog = false,
                    FileName = checker.Exec,
                    RedirectStandardError = checker.ReadStdError,
                    RedirectStandardOutput = checker.ReadStdOutput,
                    UseShellExecute = false,
                    WorkingDirectory = _workingdir
                }
            })
            {
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
                        .Replace(_workingdir, "...")
                        .Replace(_workingdir.Replace("/", "\\"), "...");

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
        }

        private async Task<(bool IsSucceeded, string Logs)> Compile(CompilerOptions compiler, List<string> extra)
        {
            try
            {
                extra.ForEach(i => File.Copy(i, Path.Combine(_workingdir, Path.GetFileName(i)), true));
            }
            catch
            {
                return (false, "Cannot copy one of extra files");
            }

            using (var comp = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = compiler.Args,
                    ErrorDialog = false,
                    FileName = compiler.Exec,
                    RedirectStandardError = compiler.ReadStdError,
                    RedirectStandardOutput = compiler.ReadStdOutput,
                    UseShellExecute = false,
                    WorkingDirectory = _workingdir
                }
            })
            {
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
                        .Replace(_workingdir, "...")
                        .Replace(_workingdir.Replace("/", "\\"), "...");
                    try
                    {
                        comp.Kill();
                    }
                    catch
                    {
                        /* ignored */
                    }

                    if (comp.ExitCode != 0 || !File.Exists(compiler.OutputFile))
                    {
                        return (false, log);
                    }

                    return (true, log);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
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

                foreach (Match item in ret)
                {
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
