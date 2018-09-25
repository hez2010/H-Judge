using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hjudgeCore
{
    public class JudgeResult
    {
        public List<JudgePoint> JudgePoints { get; set; }
        public float FullScore => JudgePoints?.Sum(i => i.Score) ?? 0;
        public string CompileLog { get; set; }
        public string StaticCheckLog { get; set; }
        public int Type { get; set; }

        public ResultCode Result
        {
            get
            {
                if (JudgePoints == null)
                    return ResultCode.Judging;
                if (JudgePoints.Count == 0 || JudgePoints.All(i => i.Result == ResultCode.Accepted))
                    return ResultCode.Accepted;
                var mostPresentTimes =
                    JudgePoints.Select(i => i.Result).Distinct().Max(i =>
                        JudgePoints.Count(j => j.Result == i && j.Result != ResultCode.Accepted));
                var mostPresent =
                    JudgePoints.Select(i => i.Result).Distinct().FirstOrDefault(
                        i => JudgePoints.Count(j => j.Result == i && j.Result != ResultCode.Accepted) ==
                             mostPresentTimes
                    );
                return mostPresent;
            }
        }
    }
}
