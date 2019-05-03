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

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace smartbox.SeaweedFs.Client.Core.Http
{
    public class HeaderResponse
    {
        public HeaderResponse(HttpHeaders headers, HttpStatusCode statusCode)
        {
            Headers = new Dictionary<string, IEnumerable<string>>();
            using (var enumerator = headers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    Headers.Add(item.Key, item.Value);
                }
            }
            StatusCode = statusCode;
        }

        private Dictionary<string, IEnumerable<string>> Headers { get; }
        public HttpStatusCode StatusCode { get; }

        public string GetLastHeader(string name)
        {
            IEnumerable<string> result;
            return Headers.TryGetValue(name, out result) ? result.LastOrDefault() : null;
        }

        public string GetFirstHeader(string name)
        {
            IEnumerable<string> result;
            return Headers.TryGetValue(name, out result) ? result.FirstOrDefault() : null;
        }

        public override string ToString()
        {
            return "HeaderResponse{" +
                   "headers=" + Headers +
                   ", statusCode=" + StatusCode +
                   '}';
        }
    }
}
