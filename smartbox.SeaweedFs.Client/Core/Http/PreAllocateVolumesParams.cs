/*
 * Original work Copyright (c) 2016 Lokra Studio
 * Url: https://github.com/lokra/seaweedfs-client
 * Modified work Copyright 2017 smartbox software solutions
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace smartbox.SeaweedFs.Client.Core.Http
{
    public class PreAllocateVolumesParams
    {
        public PreAllocateVolumesParams(string dataCenter)
        {
            DataCenter = dataCenter;
        }

        public PreAllocateVolumesParams(int count)
        {
            Count = count;
        }

        public PreAllocateVolumesParams(string replication, int count, string dataCenter, string ttl)
        {
            Replication = replication;
            Count = count;
            DataCenter = dataCenter;
            TTL = ttl;
        }

        public string Replication { get; set; }
        public int Count { get; set; }
        public string DataCenter { get; set; }
        public string TTL { get; set; }
        public string Collection { get; set; }

        public string ToUrlParams()
        {
            string result = "?";
            if (!string.IsNullOrEmpty(Replication))
                result = result + "replication=" + Replication + "&";
            if (!string.IsNullOrEmpty(DataCenter))
                result = result + "dataCenter=" + DataCenter + "&";
            if (Count > 0)
                result = result + "count=" + Count + "&";
            if (!string.IsNullOrEmpty(Collection))
                result = result + "collection=" + Collection + "&";
            if (!string.IsNullOrEmpty(TTL))
                result = result + "ttl=" + TTL;
            return result.TrimEnd('&');
        }

        public override string ToString()
        {
            return "PreAllocateVolumesParams{" +
                   "replication='" + Replication + "\'" +
                   ", count=" + Count +
                   ", dataCenter='" + DataCenter + "\'" +
                   ", ttl='" + TTL + "\'" +
                   ", collection='" + Collection + "\'" +
                   '}';
        }
    }
}
