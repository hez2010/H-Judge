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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Caching.Memory;
using smartbox.SeaweedFs.Client.Core.Http;
using smartbox.SeaweedFs.Client.Core.Infrastructure;
using smartbox.SeaweedFs.Client.Exception;
using smartbox.SeaweedFs.Client.Utils;

namespace smartbox.SeaweedFs.Client.Core
{
    public class Connection : IDisposable
    {
        private static readonly object LockObj = new object();
        internal static readonly object LockObject4LookupVolumeCache = new object();
        //private const string LookupVolumeCacheAlias = "LookupVolumeCache";

        private CancellationTokenSource _cancellationTokenSource;
        private Task _pollClusterStatusTask;
        
        private SystemClusterStatus _systemClusterStatus;
        private SystemTopologyStatus _systemTopologyStatus;
        private volatile bool _connectionClose;
        private string _leaderUrl;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="leaderUrl"></param>
        /// <param name="connectionTimeout">in milliseconds</param>
        /// <param name="statusExpiry">in milliseconds</param>
        /// <param name="maxConnection"></param>
        /// <param name="isLookupVolumeCacheEnabled"></param>
        /// <param name="lookupVolumeCacheExpiry"></param>
        public Connection(string leaderUrl, int connectionTimeout, int statusExpiry,
               int maxConnection, bool isLookupVolumeCacheEnabled,
               int lookupVolumeCacheExpiry)
        {
            LeaderUrl = ConnectionUtil.ConvertUrlWithScheme(leaderUrl);
            ConnectionTimeout = connectionTimeout;
            StatusExpiry = statusExpiry;
            IsLookupVolumeCacheEnabled = isLookupVolumeCacheEnabled;
            LookupVolumeCacheExpiry = lookupVolumeCacheExpiry;
            MaxConnections = maxConnection;
            ConnectionClose = false;
        }

        public string LeaderUrl
        {
            get { return _leaderUrl; }
            private set
            {
                lock (LockObj)
                {
                    _leaderUrl = value;
                }
            }
        }
        
        /// <summary>
        /// value in milliseconds
        /// </summary>
        private int StatusExpiry { get; }

        /// <summary>
        /// value in milliseconds
        /// </summary>
        private int ConnectionTimeout { get; }

        private bool ConnectionClose
        {
            get => _connectionClose;
            set => _connectionClose = value;
        }

        internal bool IsLookupVolumeCacheEnabled { get; }

        private int LookupVolumeCacheExpiry { get; }
        
        private int MaxConnections { get; }

        /// <summary>
        /// Core server cluster status.
        /// </summary>
        public SystemClusterStatus SystemClusterStatus {
            get
            {
                return _systemClusterStatus;
            }
            private set
            {
                lock (LockObj)
                {
                    _systemClusterStatus = value;
                }
            }
        }

        /// <summary>
        /// Cluster topology status.
        /// </summary>
        public SystemTopologyStatus SystemTopologyStatus {
            get
            {
                return _systemTopologyStatus;
            }
            private set
            {
                lock (LockObj)
                {
                    _systemTopologyStatus = value;
                }
            }
        }
        public MemoryCache VolumeLookupCache { get; private set; }

        private HttpClient HttpClient { get; set; }

        /// <summary>
        /// Connection close flag.
        /// </summary>
        public bool IsConnectionClose => ConnectionClose;

        public async Task Start()
        {
            if (IsLookupVolumeCacheEnabled)
                VolumeLookupCache = new MemoryCache(new MemoryCacheOptions());
            
            var clientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxConnectionsPerServer = MaxConnections
            };
            HttpClient = new HttpClient(clientHandler)
            {
                Timeout = TimeSpan.FromMilliseconds(ConnectionTimeout)
            };

            await FetchSystemStatus(LeaderUrl);
            
            _cancellationTokenSource = new CancellationTokenSource();
            _pollClusterStatusTask = Repeat.Interval(TimeSpan.FromMilliseconds(StatusExpiry), PollClusterStatus,
                _cancellationTokenSource.Token);
            System.Diagnostics.Debug.WriteLine("seaweedfs master server connection is startup");
        }

        public async Task Stop()
        {
            HttpClient.CancelPendingRequests();
            _cancellationTokenSource.Cancel(false);
            await _pollClusterStatusTask;
            System.Diagnostics.Debug.WriteLine("seaweedfs master server connection is shutdown");
        }

        /// <summary>
        /// Check volume server status.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<VolumeStatus> GetVolumeStatus(string url)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(url + RequestPathStrategy.CheckVolumeStatus)
            );
            var jsonResponse = await FetchJsonResultByRequest(request);
            var volumeStatus = JsonConvert.DeserializeObject<VolumeStatus>(jsonResponse.Json, Settings.JsonSerializerSettings);
            volumeStatus.Url = url;
            return volumeStatus;
        }

        /// <summary>
        /// Fetch http API json result.
        /// </summary>
        /// <returns></returns>
        internal async Task<JsonResponse> FetchJsonResultByRequest(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            JsonResponse jsonResponse;
            try
            {
                response = await HttpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                jsonResponse = new JsonResponse(content, response.StatusCode);
            }
            finally 
            {
                request.Dispose();
                response?.Dispose();
            }

            if (jsonResponse.Json.Contains("\"error\":\""))
            {
                var obj = JsonConvert.DeserializeObject<JObject>(jsonResponse.Json);
                var jToken = obj.GetValue("error");
                if (jToken != null)
                    throw new SeaweedFsException(jToken.Value<string>());
            }
            return jsonResponse;
        }

        /// <summary>
        /// Fetch http API status code.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal async Task<HttpStatusCode> FetchStatusCodeByRequest(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            HttpStatusCode statusCode;
            try
            {
                response = await HttpClient.SendAsync(request);
                statusCode = response.StatusCode;
            }
            finally
            {
                request.Dispose();
                response?.Dispose();
            }
            return statusCode;
        }

        /// <summary>
        /// Fetch http API input stream cache.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<StreamResponse> FetchStreamByRequest(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            StreamResponse result;
            try
            {
                response = await HttpClient.SendAsync(request);
                result = new StreamResponse(response.Content.ReadAsStreamAsync().Result, response.StatusCode);
            }
            finally 
            {
                request.Dispose();
                response?.Dispose();
            }
            return result;
        }

        /// <summary>
        /// Fetch http API hearers with status code.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal async Task<HeaderResponse> FetchHeaderByRequest(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            HeaderResponse headerResponse;
            try
            {
                response = await HttpClient.SendAsync(request);
                headerResponse = new HeaderResponse(response.Content.Headers, response.StatusCode);
            }
            finally
            {
                request.Dispose();
                response?.Dispose();
            }
            return headerResponse;
        }

        /// <summary>
        /// Force garbage collection.
        /// </summary>
        public void ForceGarbageCollection(float garbageThreshold)
        {
            var masterWrapper = new MasterWrapper(this);
            masterWrapper.ForceGarbageCollection(new ForceGarbageCollectionParams(garbageThreshold));
        }

        /// <summary>
        /// Force garbage collection.
        /// </summary>
        public void ForceGarbageCollection()
        {
            var masterWrapper = new MasterWrapper(this);
            masterWrapper.ForceGarbageCollection(new ForceGarbageCollectionParams());
        }

        /// <summary>
        /// Pre-allocate volumes.
        /// </summary>
        /// <param name="sameRackCount"></param>
        /// <param name="diffRackCount"></param>
        /// <param name="diffDataCenterCount"></param>
        /// <param name="count"></param>
        /// <param name="dataCenter"></param>
        /// <param name="ttl"></param>
        public async Task PreAllocateVolumes(int sameRackCount, int diffRackCount, int diffDataCenterCount, int count,
            string dataCenter, string ttl)
        {
            var masterWrapper = new MasterWrapper(this);
            await masterWrapper.PreAllocateVolumes(new PreAllocateVolumesParams(
                diffDataCenterCount.ToString() + diffRackCount + sameRackCount,
                count,
                dataCenter,
                ttl
            ));
        }

        /// <summary>
        /// Fetch core server by seaweedfs Http API.
        /// </summary>
        /// <param name="masterUrl"></param>
        /// <returns></returns>
        private async Task<SystemClusterStatus> FetchSystemClusterStatus(string masterUrl)
        {
            MasterStatus leader;
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(masterUrl + RequestPathStrategy.CheckClusterStatus)
            );
            var jsonResponse = await FetchJsonResultByRequest(request);
            var obj = JsonConvert.DeserializeObject<JObject>(jsonResponse.Json);

            var jToken = obj.GetValue("Leader");
            if (jToken != null)
                leader = new MasterStatus(ConnectionUtil.ConvertUrlWithScheme(jToken.Value<string>()));
            else
                throw new SeaweedFsException("not found seaweedfs core leader");

            var peers = new List<MasterStatus>();

            jToken = obj.GetValue("Peers");
            if (jToken != null)
            {
                var rawPeerList = jToken.Value<string[]>();
                peers.AddRange(rawPeerList.Select(url => new MasterStatus(url)));
            }

            jToken = obj.GetValue("IsLeader");
            if (jToken == null || jToken.Type != JTokenType.Boolean)
            {
                peers.Add(new MasterStatus(masterUrl));
                peers.Remove(leader);
                leader.IsActive = await ConnectionUtil.CheckUriAlive(HttpClient, leader.Url);
                if (!leader.IsActive)
                    throw new SeaweedFsException("seaweedfs core leader is failover");
            }
            else
            {
                leader.IsActive = true;
            }

            var i = 0;
            var tasks = new Task<bool>[peers.Count];
            foreach (var item in peers)
            {
                tasks[i++] = ConnectionUtil.CheckUriAlive(HttpClient, item.Url);
            }
            await Task.WhenAll(tasks);

            for (i = 0; i < tasks.Length; i++)
            {
                var result = await tasks[i];
                peers[i].IsActive = result;
            }
            
            return new SystemClusterStatus(leader, peers);
        }

        /// <summary>
        /// Find leader core server from peer core server info.
        /// </summary>
        /// <param name="peers"></param>
        /// <returns></returns>
        private async Task<string> FindLeaderUriByPeers(IReadOnlyCollection<MasterStatus> peers)
        {
            if (peers == null || peers.Count == 0)
                return null;

            foreach (var item in peers)
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    new Uri(item.Url + RequestPathStrategy.CheckClusterStatus)
                );

                JObject obj;
                try
                {
                    var jsonResponse = await FetchJsonResultByRequest(request);
                    obj = JsonConvert.DeserializeObject<JObject>(jsonResponse.Json);
                }
                catch (System.Exception)
                {
                    continue;
                }

                var jToken = obj.GetValue("Leader");
                if (jToken != null)
                {
                    var result = jToken.Value<string>();
                    if (await ConnectionUtil.CheckUriAlive(HttpClient, result))
                        return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Fetch topology by seaweedfs Http Api.
        /// </summary>
        /// <param name="masterUrl"></param>
        /// <returns></returns>
        private async Task<SystemTopologyStatus> FetchSystemTopologyStatus(string masterUrl)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(masterUrl + RequestPathStrategy.CheckTopologyStatus)
            );
            var jsonResponse = await FetchJsonResultByRequest(request);
            var system = JsonConvert.DeserializeObject<SystemTopology>(jsonResponse.Json, Settings.JsonSerializerSettings);
            var systemTopologyStatus = new SystemTopologyStatus
            (
                system.Topology.Max,
                system.Topology.Free,
                system.Topology.DataCenters,
                system.Topology.Layouts
            )
            {
                Version = system.Version
            };
            return systemTopologyStatus;
        }

        internal MemoryCacheEntryOptions CreateCacheItemPolicy()
        {
            var cacheItemPolicy = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddMinutes(LookupVolumeCacheExpiry))
            };
            //cacheItemPolicy.ChangeMonitors.Add(new SignaledChangeMonitor(LookupVolumeCacheAlias));
            return cacheItemPolicy;
        }

        #region IDisposable

        private bool _isDisposed;

        private void Dispose(bool dispose)
        {
            if (_isDisposed) return;
            _isDisposed = dispose;

            if (IsLookupVolumeCacheEnabled)
                VolumeLookupCache.Dispose();
            HttpClient?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Poll cluster status task

        private void PollClusterStatus()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                // sleep some time
                _cancellationTokenSource.Token.WaitHandle.WaitOne(StatusExpiry);
                // poll cluster status
                var statusRequestTask = Task.Factory.StartNew(UpdateSystemStatus,
                    _cancellationTokenSource.Token);
                // wait until task has been completed
                statusRequestTask.Wait(_cancellationTokenSource.Token);
            }
        }
        
        private async Task UpdateSystemStatus()
        {
            try
            {
                await FetchSystemStatus(LeaderUrl);
                ConnectionClose = false;
            }
            catch (InvalidOperationException)
            {
                ConnectionClose = true;
                //log.error("unable connect to the target seaweedfs core [" + leaderUrl + "]");
                System.Diagnostics.Debug.WriteLine("unable connect to the target seaweedfs core [" + LeaderUrl + "]");
            }

            if (ConnectionClose)
            {
                try
                {
                    //log.info("lookup seaweedfs core leader by peers");
                    System.Diagnostics.Debug.WriteLine("lookup seaweedfs core leader by peers");
                    if (SystemClusterStatus == null || SystemClusterStatus.Peers.Count == 0)
                    {
                        //log.error("cloud not found the seaweedfs core peers");
                        System.Diagnostics.Debug.WriteLine("cloud not found the seaweedfs core peers");
                    }
                    else
                    {
                        var url = await FindLeaderUriByPeers(SystemClusterStatus.Peers);
                        if (!string.IsNullOrEmpty(url))
                        {
                            //log.error("seaweedfs core cluster is failover");
                            System.Diagnostics.Debug.WriteLine("seaweedfs core cluster is failover");
                            await FetchSystemStatus(url);
                            ConnectionClose = false;
                        }
                        else
                        {
                            //log.error("seaweedfs core cluster is down");
                            System.Diagnostics.Debug.WriteLine("seaweedfs core cluster is down");
                            SystemClusterStatus.Leader.IsActive = false;
                            ConnectionClose = true;
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    //e.printStackTrace();
                    //log.error("unable connect to the seaweedfs core leader");
                    System.Diagnostics.Debug.WriteLine("unable connect to the seaweedfs core leader");
                }
            }
        }

        private Task FetchSystemStatus(string url)
        {
            var task = FetchSystemClusterStatus(url).ContinueWith(state =>
            {
                SystemClusterStatus = state.Result;
                if (!LeaderUrl.Equals(SystemClusterStatus.Leader.Url))
                {
                    LeaderUrl = ConnectionUtil.ConvertUrlWithScheme(SystemClusterStatus.Leader.Url);
                    //log.info("seaweedfs core leader is change to [" + leaderUrl + "]");
                }
                return LeaderUrl;
            }).ContinueWith(state => FetchSystemTopologyStatus(state.Result),
                TaskContinuationOptions.OnlyOnRanToCompletion).Unwrap().ContinueWith(state =>
            {
                SystemTopologyStatus = state.Result;
            });
            return task;
        }
        
        #endregion        
    }
}
