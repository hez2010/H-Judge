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

using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace smartbox.SeaweedFs.Client.Core.Http
{
    public class StreamResponse
    {
        public StreamResponse(Stream inputStream, HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            if (inputStream == null)
                return;

            OutputStream = new MemoryStream();
            inputStream.CopyTo(OutputStream);
            OutputStream.Flush();
            if (OutputStream.CanSeek)
                OutputStream.Seek(0, SeekOrigin.Begin);
        }

        public async Task<Stream> GetInputStream()
        {
            if (OutputStream == null)
                return null;
            var stream = new MemoryStream();
            await OutputStream.CopyToAsync(stream);
            stream.Flush();
            return stream;
        }

        public Stream OutputStream { get; }

        public HttpStatusCode StatusCode { get; }

        public long GetLength()
        {
            if (OutputStream == null)
                return 0;
            return OutputStream.Length;
        }

        public override string ToString()
        {
            return "StreamResponse{" +
                   "byteArrayOutputStream=" + OutputStream +
                   ", statusCode=" + StatusCode +
                   ", length=" + GetLength() +
                   '}';
        }
    }
}
