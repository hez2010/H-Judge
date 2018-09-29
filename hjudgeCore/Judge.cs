using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace hjudgeCore
{
    public class Judge
    {
        private readonly string _guid;
        private readonly string _rootdir;
        private readonly string _workingdir;

        [DllImport("./hjudgeExec.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi,
            EntryPoint = "#1")]
        static extern IntPtr Execute(string prarm);

        public Judge(string environments = null)
        {
            _guid = Guid.NewGuid().ToString().Replace("-", "_");
            _workingdir = $"{Path.GetTempPath()}/hjudgeTest/{_guid}".Replace("\\", "/").Replace("//", "/");
            if (_workingdir.EndsWith("/"))
            {
                _workingdir = _workingdir.Substring(0, _workingdir.Length - 1);
            }

            _rootdir = $"{Environment.CurrentDirectory.Replace("\\", "/")}".Replace("//", "/");
            if (_rootdir.EndsWith("/"))
            {
                _rootdir = _rootdir.Substring(0, _rootdir.Length - 1);
            }

            if (!string.IsNullOrEmpty(environments))
            {
                Environment.SetEnvironmentVariable("PATH",
                    $"{environments}{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ";" : ":")}{Environment.GetEnvironmentVariable("PATH")}");
            }
        }

        public async Task<JudgeResult> JudgeAsync(BuildOption buildOption, JudgeOption judgeOption)
        {
            Directory.CreateDirectory(_workingdir);
            if (judgeOption.AnswerPoint != null)
            {
                return await AnswerJudgeAsync(buildOption, judgeOption);
            }
            var result = new JudgeResult
            {
                JudgePoints = new List<JudgePoint>()
            };

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
                            Result = ResultCode.Compile_Error
                        }, judgeOption.DataPoints.Count)
                    .ToList();
                    return result;
                }
            }

            for (var i = 0; i < judgeOption.DataPoints.Count; i++)
            {
                var point = new JudgePoint();
                try
                {
                    File.Copy(judgeOption.DataPoints[i].StdInFile, _workingdir + "/" + judgeOption.InputFileName);
                    File.Copy(judgeOption.DataPoints[i].StdOutFile, $"{_workingdir}/answer_{_guid}.txt");
                    var param = new
                    {
                        judgeOption.RunOption.Exec,
                        judgeOption.RunOption.Args,
                        WorkingDir = _workingdir,
                        InputFile = _workingdir + "/" + judgeOption.InputFileName,
                        OutputFile = _workingdir + "/" + judgeOption.OutputFileName,
                        judgeOption.DataPoints[i].TimeLimit,
                        judgeOption.DataPoints[i].MemoryLimit,
                        IsStdIO = judgeOption.UseStdIO
                    };
                    var resultPtr = Execute(JsonConvert.SerializeObject(param));
                    point = JsonConvert.DeserializeObject<JudgePoint>(Marshal.PtrToStringAnsi(resultPtr));
                    Marshal.FreeHGlobal(resultPtr);
                }
                catch (Exception ex)
                {
                    point.ExtraInfo = ex.Message;
                    point.Result = ResultCode.Unknown_Error;
                }

                var (resultCode, percentage, extraInfo) = await CompareAsync(_workingdir + "/" + judgeOption.InputFileName, $"{_workingdir}/answer_{_guid}.txt", _workingdir + "/" + judgeOption.OutputFileName, judgeOption);
                point.Result = resultCode;
                point.Score = percentage * judgeOption.DataPoints[i].Score;
                point.ExtraInfo = extraInfo;
                result.JudgePoints.Add(point);
            }

            Directory.Delete(_workingdir, true);
            return result;
        }

        public async Task<JudgeResult> AnswerJudgeAsync(BuildOption buildOption, JudgeOption judgeOption)
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
                result.JudgePoints[0].Result = ResultCode.Problem_Config_Error;
            }

            try
            {
                File.Copy(judgeOption.AnswerPoint.AnswerFile, $"{_workingdir}/answer_{_guid}.txt");
                File.WriteAllText($"{_workingdir}/output_{_guid}.txt", buildOption.Source, Encoding.UTF8);
                var (resultCode, percentage, extraInfo) = await CompareAsync(null, $"{_workingdir}/answer_{_guid}.txt", $"{_workingdir}/output_{_guid}.txt", judgeOption);
                result.JudgePoints[0].Result = resultCode;
                result.JudgePoints[0].Score = percentage * judgeOption.AnswerPoint.Score;
                result.JudgePoints[0].ExtraInfo = extraInfo;
            }
            catch (Exception ex)
            {
                result.JudgePoints[0].Result = ResultCode.Unknown_Error;
                result.JudgePoints[0].ExtraInfo = ex.Message;
            }

            return result;
        }

        public async Task<(ResultCode Result, float Percentage, string ExtraInfo)> CompareAsync(string stdInputFile, string stdOutputFile, string outputFile, JudgeOption judgeOption)
        {
            if (judgeOption.SpecialJudgeOption != null)
            {
                var argsBuilder = new StringBuilder();
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
                        CreateNoWindow = true,
                        ErrorDialog = false,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        StandardOutputEncoding = Encoding.UTF8
                    }
                })
                {

                    if (!judge.Start())
                    {
                        return (ResultCode.Unknown_Error, 0, null);
                    }

                    var (error, output) = (await judge.StandardError.ReadToEndAsync(), await judge.StandardOutput.ReadToEndAsync());

                    judge.WaitForExit();

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
                    std = new StreamReader(stdOutputFile, Encoding.UTF8);
                }
                catch (IOException)
                {
                    Thread.Sleep(50);
                    std = null;
                    retryTimes++;
                }
            } while (std == null && retryTimes < 200);

            if (std == null)
            {
                return (ResultCode.Unknown_Error, 0, "Can not open standard output file");
            }

            retryTimes = 0;
            do
            {
                try
                {
                    act = new StreamReader(outputFile, Encoding.UTF8);
                }
                catch (IOException)
                {
                    Thread.Sleep(50);
                    act = null;
                    retryTimes++;
                }
            } while (act == null && retryTimes < 200);

            if (act == null)
            {
                std.Dispose();
                return (ResultCode.Unknown_Error, 0, "Can not open output file");
            }

            var line = 0;
            var result = new JudgePoint
            {
                Result = ResultCode.Accepted
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
                    result.ExtraInfo =
                        $"Line {line}, expect: {stdline?.Substring(0, 64 < (stdline?.Length ?? 0) ? 64 : stdline?.Length ?? 0) ?? "<nothing>"}{((stdline?.Length ?? 0) > 64 ? "..." : string.Empty)}, output: {actline?.Substring(0, 64 < (actline?.Length ?? 0) ? 64 : actline?.Length ?? 0) ?? "<nothing>"}{((actline?.Length ?? 0) > 64 ? "..." : string.Empty)}";
                    if ((stdline?.Replace(" ", string.Empty) ?? string.Empty) ==
                        (actline?.Replace(" ", string.Empty) ?? string.Empty))
                    {
                        result.Result = ResultCode.Presentation_Error;
                    }
                    else
                    {
                        result.Result = ResultCode.Wrong_Answer;
                        break;
                    }
                }
            }

            std.Dispose();
            act.Dispose();
            return (result.Result, result.Result == ResultCode.Accepted ? 1 : 0, null);
        }

        private async Task<string> StaticCheck(StaticCheckOption checker)
        {
            using (var sta = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = checker.Args,
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    FileName = checker.Exec,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WorkingDirectory = _workingdir
                }
            })
            {
                try
                {
                    sta.Start();
                    var (stderr, stdout) = (sta.StandardError.ReadToEndAsync(), sta.StandardOutput.ReadToEndAsync());
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

                        return null;
                    }

                    var log = MatchProblem(await stdout + "\n" + await stderr, checker.ProblemMatcher)
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
                catch
                {
                    return null;
                }
            }
        }

        private async Task<(bool IsSucceeded, string Logs)> Compile(CompilerOption compiler, List<string> extra)
        {
            extra?.ForEach(i => File.Copy(i, $"{_workingdir}/{Path.GetFileName(i)}", true));
            using (var comp = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = compiler.Args,
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    FileName = compiler.Exec,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WorkingDirectory = _workingdir
                }
            })
            {
                try
                {
                    comp.Start();
                    var (stderr, stdout) = (comp.StandardError.ReadToEndAsync(), comp.StandardOutput.ReadToEndAsync());
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

                        return (false, null);
                    }

                    var log = MatchProblem(await stdout + "\n" + await stderr, compiler.ProblemMatcher)
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
                catch
                {
                    return (false, null);
                }
            }
        }

        public static string MatchProblem(string input, ProblemMatcher matcher)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(matcher.MatchPatterns))
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
