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

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using smartbox.SeaweedFs.Client.Core.Http;
using smartbox.SeaweedFs.Client.Exception;
using smartbox.SeaweedFs.Client.Utils;

namespace smartbox.SeaweedFs.Client.Core
{
    /// <summary>
    /// Master server operation wrapper
    /// </summary>
    public class MasterWrapper
    {
        private readonly Connection _connection;

        public MasterWrapper(Connection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Assign a file key.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public async Task<AssignFileKeyResult> AssignFileKey(AssignFileKeyParams values)
        {
            CheckConnection();
            var url = _connection.LeaderUrl + RequestPathStrategy.AssignFileKey + values.ToUrlParams();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(url)
            );
            var jsonResponse = await _connection.FetchJsonResultByRequest(request);
            var obj = JsonConvert.DeserializeObject<AssignFileKeyResult>(jsonResponse.Json, Settings.JsonSerializerSettings);
            return obj;
        }

        /// <summary>
        /// Force garbage collection.
        /// </summary>
        /// <param name="values"></param>
        public Task<JsonResponse> ForceGarbageCollection(ForceGarbageCollectionParams values)
        {
            CheckConnection();
            var url = _connection.LeaderUrl + RequestPathStrategy.ForceGarbageCollection + values.ToUrlParams();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(url)
            );
            return _connection.FetchJsonResultByRequest(request);
        }

        /// <summary>
        /// Pre-Allocate volumes.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public async Task<PreAllocateVolumesResult> PreAllocateVolumes(PreAllocateVolumesParams values)
        {
            CheckConnection();
            var url = _connection.LeaderUrl + RequestPathStrategy.PreAllocateVolumes + values.ToUrlParams();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(url)
            );
            var jsonResponse = await _connection.FetchJsonResultByRequest(request);
            var obj = JsonConvert.DeserializeObject<PreAllocateVolumesResult>(jsonResponse.Json, Settings.JsonSerializerSettings);
            return obj;
        }

        /// <summary>
        /// Lookup volume.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<LookupVolumeResult> LookupVolume(LookupVolumeParams value)
        {
            CheckConnection();
            if (!_connection.IsLookupVolumeCacheEnabled)
                return await FetchLookupVolumeResult(value);

            var result = _connection.VolumeLookupCache.GetCachedData(
                value.VolumeId.ToString(),
                Connection.LockObject4LookupVolumeCache,
                _connection.CreateCacheItemPolicy(), 
                async () =>
                {
                    var volume = await FetchLookupVolumeResult(value);
                    return volume;
                });
            return result;
        }

        /// <summary>
        /// Fetch lookup volume result.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private async Task<LookupVolumeResult> FetchLookupVolumeResult(LookupVolumeParams value)
        {
            CheckConnection();
            var url = _connection.LeaderUrl + RequestPathStrategy.LookupVolume + value.ToUrlParams();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(url)
            );
            var jsonResponse = await _connection.FetchJsonResultByRequest(request);
            var obj = JsonConvert.DeserializeObject<LookupVolumeResult>(jsonResponse.Json, Settings.JsonSerializerSettings);
            return obj;
        }

        /// <summary>
        /// Check connection is alive.
        /// </summary>
        private void CheckConnection()
        {
            if (_connection.IsConnectionClose)
                throw new SeaweedFsException(Messages.ConnectionClosedError);
        }
    }
}
