﻿using System;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace FoxIDs.Logic
{
    public class DownloadLogic
    {
        private readonly IHttpClientFactory httpClientFactory;

        public DownloadLogic(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<string> DownloadAsync(string url, string name)
        {
            var httpClient = httpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
            // Handle the response
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var stringResult = await response.Content.ReadAsStringAsync();
                    return stringResult;

                default:
                    throw new Exception($"Download {name} error, status code={response.StatusCode}, from URL '{url}'.");
            }
        }

        public async Task<byte[]> DownloadAsBytesAsync(string url, string name)
        {
            var httpClient = httpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
            // Handle the response
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var byteArrayResult = await response.Content.ReadAsByteArrayAsync();
                    return byteArrayResult;

                default:
                    throw new Exception($"Download {name} error, status code={response.StatusCode}, from URL '{url}'.");
            }
        }
    }
}
