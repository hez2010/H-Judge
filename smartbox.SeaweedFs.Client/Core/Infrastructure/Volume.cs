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
using Newtonsoft.Json;
using smartbox.SeaweedFs.Client.Utils;

namespace smartbox.SeaweedFs.Client.Core.Infrastructure
{
    public class Volume
    {
        [JsonProperty("Id")]
        public long Id { get; private set; }
        [JsonProperty("Size")]
        public long Size { get; private set; }
        [JsonProperty("ReplicaPlacement")]
        public ReplicaPlacement ReplicaPlacement { get; private set; }
        [JsonProperty("Ttl")]
        [JsonConverter(typeof(ArrayJsonConverter))]
        public List<string> TTL { get; private set; }
        [JsonProperty("Collection")]
        public string Collection { get; private set; }
        [JsonProperty("Version")]
        public long Version { get; private set; }
        [JsonProperty("FileCount")]
        public long FileCount { get; private set; }
        [JsonProperty("DeleteCount")]
        public long DeleteCount { get; private set; }
        [JsonProperty("DeletedByteCount")]
        public long DeletedByteCount { get; private set; }
        [JsonProperty("ReadOnly")]
        public bool ReadOnly { get; private set; }

        public override string ToString()
        {
            return "Volume{" +
                   "id=" + Id +
                   ", size=" + Size +
                   ", replicaPlacement=" + ReplicaPlacement +
                   ", ttl=" + TTL +
                   ", collection='" + Collection + "\'" +
                   ", version=" + Version +
                   ", fileCount=" + FileCount +
                   ", deleteCount=" + DeleteCount +
                   ", deletedByteCount=" + DeletedByteCount +
                   ", readOnly=" + ReadOnly +
                   '}';
        }
    }
}
