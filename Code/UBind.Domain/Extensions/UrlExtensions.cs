// <copyright file="UrlExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Flurl;
    using Flurl.Http;

    /// <summary>
    /// Extension methods for Flurl.Url.
    /// </summary>
    /// <remarks>
    /// Based on https://stackoverflow.com/questions/32829763/how-to-add-content-header-to-flurl
    /// .</remarks>
    public static class UrlExtensions
    {
        /// <summary>
        /// Extension method for uploading a file.
        /// </summary>
        /// <param name="request">The instance of IFlurlRequest to use.</param>
        /// <param name="fileContent">A byte array containing the file content.</param>
        /// <returns>A Task whose result is the received HttpResponseMessage.</returns>
        public static async Task<HttpResponseMessage> PutFileAsync(this IFlurlRequest request, byte[] fileContent)
        {
            var content = new ByteArrayContent(fileContent);
            content.Headers.Add("Content-Type", "application/octet-stream");
            content.Headers.Add("Content-Length", fileContent.Length.ToString());
            var response = await request.SendAsync(HttpMethod.Put, content: content);
            return response.ResponseMessage;
        }

        /// <summary>
        /// Extension method for uploading a file.
        /// </summary>
        /// <param name="url">The url to upload the file to.</param>
        /// <param name="fileContent">A byte array containing the file content.</param>
        /// <returns>A Task whose result is the received HttpResponseMessage.</returns>
        public static Task<HttpResponseMessage> PutFileAsync(this Url url, byte[] fileContent)
        {
            return new FlurlRequest(url).PutFileAsync(fileContent);
        }

        /// <summary>
        /// Extension method for uploading a file.
        /// </summary>
        /// <param name="url">The url to upload the file to.</param>
        /// <param name="filepath">The path of the file in the local file system.</param>
        /// <returns>A Task whose result is the received HttpResponseMessage.</returns>
        public static Task<HttpResponseMessage> PutFileAsync(this Url url, string filepath)
        {
            return new FlurlRequest(url).PutFileAsync(filepath);
        }

        /// <summary>
        /// Extension method for uploading a file.
        /// </summary>
        /// <param name="request">The instance of IFlurlRequest to use.</param>
        /// <param name="filepath">The path of the file in the local file system.</param>
        /// <returns>A Task whose result is the received HttpResponseMessage.</returns>
        public static Task<HttpResponseMessage> PutFileAsync(this IFlurlRequest request, string filepath)
        {
            var fileContent = File.ReadAllBytes(filepath);
            return request.PutFileAsync(fileContent);
        }

        public static Url AppendPathWithQuery(this Url url, string path)
        {
            var newUrl = new Url(path);
            foreach (var queryParam in newUrl.QueryParams)
            {
                url = url.SetQueryParam(queryParam.Name, queryParam.Value);
            }

            return url.AppendPathSegment(newUrl.Path);
        }
    }
}
