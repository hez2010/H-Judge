using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static hjudgeShared.Judge.JudgeInfo;

namespace hjudgeShared.Utils
{
    public class ConcurrentPriorityQueue<T> : IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
    {
        private readonly ConcurrentQueue<T> low = new ConcurrentQueue<T>();
        private readonly ConcurrentQueue<T> normal = new ConcurrentQueue<T>();
        private readonly ConcurrentQueue<T> high = new ConcurrentQueue<T>();

        public int Count => low.Count + normal.Count + high.Count;

        public void Enqueue(T item, JudgePriority priority = JudgePriority.Normal)
        {
            switch (priority)
            {
                case JudgePriority.Low:
                    low.Enqueue(item);
                    break;
                case JudgePriority.Normal:
                    normal.Enqueue(item);
                    break;
                case JudgePriority.High:
                    high.Enqueue(item);
                    break;
                default:
                    break;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach(var i in high)
            {
                yield return i;
            }
            foreach (var i in normal)
            {
                yield return i;
            }
            foreach (var i in low)
            {
                yield return i;
            }
        }

        public bool TryDequeue(out T result)
        {
            return high.TryDequeue(out result) || normal.TryDequeue(out result) || low.TryDequeue(out result);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var i in high)
            {
                yield return i;
            }
            foreach (var i in normal)
            {
                yield return i;
            }
            foreach (var i in low)
            {
                yield return i;
            }
        }
    }
}