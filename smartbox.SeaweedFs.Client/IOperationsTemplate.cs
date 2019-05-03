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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using smartbox.SeaweedFs.Client.Core.File;
using smartbox.SeaweedFs.Client.Core.Http;

namespace smartbox.SeaweedFs.Client
{
    public interface IOperationsTemplate
    {
        /// <summary>
        /// Save a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        Task<FileHandleStatus> SaveFileByStream(string fileName, Stream stream);

        /// <summary>
        /// Save files by stream map.
        /// </summary>
        /// <param name="streamMap"></param>
        /// <returns></returns>
        Task<Dictionary<string, FileHandleStatus>> SaveFilesByStreamMap(Dictionary<string, Stream> streamMap);

        /// <summary>
        /// Delete file.
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Task<bool> DeleteFile(string fileId);

        /// <summary>
        /// Delete files.
        /// </summary>
        /// <param name="fileIds"></param>
        /// <returns></returns>
        Task DeleteFiles(IList<string> fileIds);

        /// <summary>
        /// Update file, if file id is not exist, it wouldn't throw any exception.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="fileName"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        Task<FileHandleStatus> UpdateFileByStream(string fileId, string fileName, Stream stream);

        /// <summary>
        /// Get file stream, this is the faster method to get file stream from server.
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Task<StreamResponse> GetFileStream(string fileId);

        /// <summary>
        /// Get file status without file stream.
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Task<FileHandleStatus> GetFileStatus(string fileId);

        Task<bool> CheckFileExists(string fileId);

        /// <summary>
        /// Get file url, could get file directly from seaweedfs volume server.
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Task<string> GetFileUrl(string fileId);
    }
}