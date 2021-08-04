using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using MS.API.Models.Common;

namespace MS.API.Extensions
{
    public static class HttpResponseExtensions
    {
        public static void AddLocationHeader(this HttpResponse httpResponse, string url)
        {
            httpResponse.Headers.TryAdd("Location", url);
        }

        public static void SetStatusCode(this HttpResponse httpResponse, HttpStatusCode statusCode)
        {
            httpResponse.StatusCode = (int) statusCode;
        }

        public static ResponseInfo<T> GetCreatedResource<T>(this HttpResponse httpResponse, T model, string absoluteUrl)
        {
            httpResponse.AddLocationHeader(absoluteUrl);
            httpResponse.SetStatusCode(HttpStatusCode.Created);
            return model;
        }
    }
}