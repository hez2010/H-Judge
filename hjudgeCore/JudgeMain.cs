using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace hjudgeCore
{
    public class JudgeMain
    {
        private string _workingdir;

        [DllImport("./hjudgeExec.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#1", CharSet = CharSet.Ansi)]
        static extern bool Execute(string prarm, [MarshalAs(UnmanagedType.LPStr)]StringBuilder ret);

        public JudgeMain(string environments = null)
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

        public async Task<JudgeResult> JudgeAsync(BuildOption buildOption, JudgeOption judgeOption)
        {
            _workingdir = Path.Combine(Path.GetTempPath(), "hjudgeTest", judgeOption.GuidStr);

            Directory.CreateDirectory(_workingdir);
            var result = new JudgeResult
            {
                JudgePoints = new List<JudgePoint>()
            };

            try
            {
                if (judgeOption.AnswerPoint != null)
                {
                    return await AnswerJudgeAsync(buildOption, judgeOption);
                }

                var fileName = Path.Combine(_workingdir, $"{buildOption.SubmitFileName}{buildOption.ExtensionName}");
                File.WriteAllText(fileName, buildOption.Source, Encoding.UTF8);

                if (buildOption.StaticCheckOption != null)
                {
                    var logs = await StaticCheck(buildOption.StaticCheckOption);
                    result.StaticCheckLog = logs;
                }

                if (buildOption.CompilerOption != null)
                {
                    var (isSucceeded, logs) = await Compile(buildOption.CompilerOption, judgeOption.ExtraFiles);
                    result.CompileLog = logs;
                    if (!isSucceeded)
                    {
                        result.JudgePoints = Enumerable.Repeat(
                            new JudgePoint
                            {
                                ResultType = ResultCode.Compile_Error
                            }, judgeOption.DataPoints.Count)
                        .ToList();
                        return result;
                    }
                }

                for (var i = 0; i < judgeOption.DataPoints.Count; i++)
                {
                    var point = new JudgePoint();
                    if (!File.Exists(judgeOption.RunOption.Exec))
                    {
                        point.ResultType = ResultCode.Compile_Error;
                        point.ExtraInfo = "Cannot find compiled executable";
                        result.JudgePoints.Add(point);
                        continue;
                    }
                    try
                    {
                        try
                        {
                            File.Copy(judgeOption.DataPoints[i].StdInFile.Replace("${index}", (i + 1).ToString()).Replace("${index0}", i.ToString()), Path.Combine(_workingdir, judgeOption.InputFileName), true);
                        }
                        catch
                        {
                            throw new InvalidOperationException("Unable to find standard input file");
                        }
                        var param = new
                        {
                            judgeOption.RunOption.Exec,
                            judgeOption.RunOption.Args,
                            WorkingDir = _workingdir,
                            InputFile = Path.Combine(_workingdir, judgeOption.InputFileName),
                            OutputFile = Path.Combine(_workingdir, judgeOption.OutputFileName),
                            judgeOption.DataPoints[i].TimeLimit,
                            judgeOption.DataPoints[i].MemoryLimit,
                            IsStdIO = judgeOption.UseStdIO
                        };
                        var ret = new StringBuilder(256);
                        if (Execute(JsonConvert.SerializeObject(param), ret))
                        {
                            point = JsonConvert.DeserializeObject<JudgePoint>(ret.ToString()?.Trim() ?? "{}");
                        }
                        else
                        {
                            throw new Exception("Unable to execute target program");
                        }
                        try
                        {
                            File.Copy(judgeOption.DataPoints[i].StdOutFile.Replace("${index}", (i + 1).ToString()).Replace("${index0}", i.ToString()), Path.Combine(_workingdir, $"answer_{judgeOption.GuidStr}.txt"), true);
                        }
                        catch
                        {
                            throw new InvalidOperationException("Unable to find standard output file");
                        }
                        var (resultType, percentage, extraInfo) = point.ResultType == ResultCode.Accepted ?
                            await CompareAsync(fileName, Path.Combine(_workingdir, judgeOption.InputFileName), Path.Combine(_workingdir, $"answer_{judgeOption.GuidStr}.txt"), Path.Combine(_workingdir, judgeOption.OutputFileName), judgeOption)
                            : (point.ResultType, 0, point.ExtraInfo);
                        point.ExtraInfo = extraInfo;
                        if (point.ResultType == ResultCode.Runtime_Error && point.ExitCode != 0)
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
                        }
                        point.ResultType = resultType;
                        point.Score = percentage * judgeOption.DataPoints[i].Score;
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

        private async Task<JudgeResult> AnswerJudgeAsync(BuildOption buildOption, JudgeOption judgeOption)
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

        private async Task<(ResultCode Result, float Percentage, string ExtraInfo)> CompareAsync(string sourceFile, string stdInputFile, string stdOutputFile, string outputFile, JudgeOption judgeOption, bool isAnswerJudge = false)
        {
            if (!File.Exists(outputFile))
            {
                return (ResultCode.Output_File_Error, 0, string.Empty);
            }

            if (judgeOption.SpecialJudgeOption != null)
            {
                var argsBuilder = new StringBuilder();
                if (judgeOption.SpecialJudgeOption.UseSourceFile)
                {
                    argsBuilder.Append($" \"{sourceFile}\"");
                }
                if (judgeOption.SpecialJudgeOption.UseOutputFile)
                {
                    argsBuilder.Append($" \"{outputFile}\"");
                }
                if (judgeOption.SpecialJudgeOption.UseStdInputFile)
                {
                    argsBuilder.Append($" \"{stdInputFile}\"");
                }
                if (judgeOption.SpecialJudgeOption.UseStdOutputFile)
                {
                    argsBuilder.Append($" \"{stdOutputFile}\"");
                }

                using (var judge = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = judgeOption.SpecialJudgeOption.Exec,
                        Arguments = argsBuilder.ToString(),
                        ErrorDialog = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        StandardOutputEncoding = Encoding.UTF8
                    }
                })
                {

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
                        return (ResultCode.Special_Judge_Error, 0, null);
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
                        return (ResultCode.Special_Judge_Error, 0, null);
                    }
                }
            }

            StreamReader std = null, act = null;
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
                string actline = null, stdline = null;
                if (!std.EndOfStream)
                {
                    stdline = std.ReadLine();
                }

                if (!act.EndOfStream)
                {
                    actline = act.ReadLine();
                }

                line++;
                if (judgeOption.ComparingOption?.IgnoreLineTailWhiteSpaces ?? true)
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

                if (judgeOption.ComparingOption?.IgnoreTextTailLineFeeds ?? true)
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

        private async Task<string> StaticCheck(StaticCheckOption checker)
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
                        output.AppendLine(await sta.StandardOutput.ReadToEndAsync());
                    }

                    if (checker.ReadStdError)
                    {
                        output.AppendLine(await sta.StandardError.ReadToEndAsync());
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

                    return sta.ExitCode == 0 ? log : null;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        private async Task<(bool IsSucceeded, string Logs)> Compile(CompilerOption compiler, List<string> extra)
        {
            extra?.ForEach(i => File.Copy(i, Path.Combine(_workingdir, Path.GetFileName(i)), true));
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
                        output.AppendLine(await comp.StandardOutput.ReadToEndAsync());
                    }

                    if (compiler.ReadStdError)
                    {
                        output.AppendLine(await comp.StandardError.ReadToEndAsync());
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

        public static string MatchProblem(string input, ProblemMatcher matcher)
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
