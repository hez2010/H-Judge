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
using System.Threading.Tasks;
using smartbox.SeaweedFs.Client.Core;
using smartbox.SeaweedFs.Client.Core.Infrastructure;
using smartbox.SeaweedFs.Client.Exception;
using smartbox.SeaweedFs.Client.Utils;

namespace smartbox.SeaweedFs.Client
{
    /// <summary>
    /// Seaweed file system connection source.
    /// </summary>
    public class MasterConnection : IMasterConnection, IDisposable
    {
        private volatile bool _running;
        private Connection _connection;

        public MasterConnection(string host, int port = 9333)
        {
            Host = host;
            Port = port;
            ConnectionTimeout = 10 * 1000;
            StatusExpiry = 30 * 1000;
            MaxConnection = 100;
            EnableLookupVolumeCache = false;
            LookupVolumeCacheExpiry = 10;
        }

        public string Host { get; }

        public int Port { get; }

        /// <summary>
        /// in millisenconds
        /// </summary>
        public int ConnectionTimeout { get; set; }

        /// <summary>
        /// in milliseconds
        /// </summary>
        public int StatusExpiry { get; set; }

        public int MaxConnection { get; set; }

        public bool EnableLookupVolumeCache { get; set; }

        /// <summary>
        /// in minutes
        /// </summary>
        public int LookupVolumeCacheExpiry { get; set; }
        
        public Connection GetConnection()
        {
            return _connection;
        }

        /// <summary>
        /// Start up the connection to the Seaweedfs server
        /// </summary>
        public async Task Start()
        {
            if (_running)
            {
                System.Diagnostics.Debug.WriteLine("connect is already startup");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("start connect to the seaweedfs master server [" +
                                                   ConnectionUtil.ConvertUrlWithScheme(Host + ":" + Port) + "]");
                
                if (_connection == null)
                {
                    _connection = new Connection(
                        ConnectionUtil.ConvertUrlWithScheme(Host + ":" + Port),
                        ConnectionTimeout,
                        StatusExpiry,
                        MaxConnection,
                        EnableLookupVolumeCache,
                        LookupVolumeCacheExpiry);
                }
                await _connection.Start();
                _running = true;
            }
        }

        /// <summary>
        /// Shutdown connect to the any Seaweedfs server
        /// </summary>
        public async Task Stop()
        {
            //log.info("stop connect to the seaweedfs master server");
            if (_connection != null)
            {
                await _connection.Stop();
                _connection.Dispose();
                _connection = null;
            }            
            _running = false;
        }

        ///// <summary>
        ///// Flush cached items associated with "name" change monitors
        ///// </summary>
        ///// <param name="name"></param>
        //public void ClearCache(string name)
        //{
        //    SignaledChangeMonitor.Signal(name);
        //}

        /// <summary>
        /// Force garbage collection.
        /// </summary>
        public void ForceGarbageCollection(float garbageThreshold)
        {
            _connection?.ForceGarbageCollection(garbageThreshold);
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
            await _connection.PreAllocateVolumes(sameRackCount, diffRackCount, diffDataCenterCount, count, dataCenter, ttl);
        }

        /// <summary>
        /// Get master server cluster status.
        /// </summary>
        /// <returns></returns>
        public SystemClusterStatus GetSystemClusterStatus()
        {
            if (_running)
                return _connection.SystemClusterStatus;            
            throw new SeaweedFsException(Messages.FetchClusterStatusError);
        }

        /// <summary>
        /// Get cluster topology status.
        /// </summary>
        /// <returns></returns>
        public SystemTopologyStatus GetSystemTopologyStatus()
        {
            if (_running)
                return _connection.SystemTopologyStatus;
            throw new SeaweedFsException(Messages.FetchClusterStatusError);
        }

        /// <summary>
        /// Check volume server status.
        /// </summary>
        /// <param name="volumeUrl"></param>
        /// <returns></returns>
        public async Task<VolumeStatus> GetVolumeStatus(string volumeUrl)
        {
            if (_running)
                return await _connection.GetVolumeStatus(volumeUrl);
            throw new SeaweedFsException(Messages.FetchClusterStatusError);
        }

        #region IDisposable

        private bool _isDisposed;

        private void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                _isDisposed = true;
                _connection?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
