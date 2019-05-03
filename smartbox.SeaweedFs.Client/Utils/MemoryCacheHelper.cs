/*
 * Copyright 2017 smartbox software solutions
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
using Microsoft.Extensions.Caching.Memory;

namespace smartbox.SeaweedFs.Client.Utils
{
    internal static class MemoryCacheHelper
    {
        public static T GetCachedData<T>(this MemoryCache cache, string cacheKey, object cacheLock,
            MemoryCacheEntryOptions cacheItemPolicy, Func<Task<T>> getData) where T : class
        {
            //Returns null if the string does not exist, prevents a race condition where the cache invalidates between the contains check and the retreival.
            var cachedData = cache.Get(cacheKey) as T;

            if (cachedData != null)
            {
                return cachedData;
            }

            lock (cacheLock)
            {
                //Check to see if anyone wrote to the cache while we where waiting our turn to write the new value.
                cachedData = cache.Get(cacheKey) as T;

                if (cachedData != null)
                {
                    return cachedData;
                }

                //The value still did not exist so we now write it in to the cache.
                cachedData = getData().GetAwaiter().GetResult();
                cache.Set(cacheKey, cachedData, cacheItemPolicy);
                return cachedData;
            }
        }
    }

    //internal class SignaledChangeEventArgs : EventArgs
    //{
    //    public string Name { get; }
    //    public SignaledChangeEventArgs(string name = null) { Name = name; }
    //}

    ///// <summary>
    ///// Cache change monitor that allows an app to fire a change notification
    ///// to all associated cache items.
    ///// </summary>
    //internal class SignaledChangeMonitor : ChangeMonitor
    //{
    //    // Shared across all SignaledChangeMonitors in the AppDomain
    //    private static event EventHandler<SignaledChangeEventArgs> Signaled;

    //    private readonly string _name;
    //    private readonly string _uniqueId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);

    //    public override string UniqueId => _uniqueId;

    //    public SignaledChangeMonitor(string name = null)
    //    {
    //        _name = name;
    //        // Register instance with the shared event
    //        Signaled += OnSignalRaised;
    //        InitializationComplete();
    //    }

    //    public static void Signal(string name = null)
    //    {
    //        // Raise shared event to notify all subscribers
    //        Signaled?.Invoke(null, new SignaledChangeEventArgs(name));
    //    }

    //    protected override void Dispose(bool disposing)
    //    {
    //        Signaled -= OnSignalRaised;
    //    }

    //    private void OnSignalRaised(object sender, SignaledChangeEventArgs e)
    //    {
    //        if (string.IsNullOrWhiteSpace(e.Name) || string.Compare(e.Name, _name, StringComparison.OrdinalIgnoreCase) == 0)
    //        {
    //            Debug.WriteLine(_uniqueId + " notifying cache of change.", "SignaledChangeMonitor");
    //            // Cache objects are obligated to remove entry upon change notification.
    //            OnChanged(null);
    //        }
    //    }
    //}
}
