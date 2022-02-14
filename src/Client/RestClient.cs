using System;
using System.IO;
using System.Net;
using System.Text;
using HttpPack.Json;

namespace HttpPack.Client;

public class HttpClient<T>
{
    private const int TimeoutFallbackValue = 1000; // 1 second
    private readonly int timeoutOverride;
    private WebClientEx client;

    public HttpClient(int timeoutOverride)
    {
        this.timeoutOverride = timeoutOverride;
    }

    public HttpClient()
        : this(TimeoutFallbackValue)
    {
    }

    /// <summary>
    ///     RESTful request
    /// </summary>
    /// <remarks>
    ///     It will return a FetchResponse from a REST API call.
    /// </remarks>
    /// <param name="url">URL to fetch</param>
    /// <param name="method">
    ///     <see cref="System.Net.WebRequestMethods.Http" />
    /// </param>
    /// <param name="requestBody">json body</param>
    /// <param name="authorization">added to the http head if authorization is not null or empty</param>
    /// <returns>Returns a FetchResponse, holding a response status code and a string representing JSON.</returns>
    public FetchResponse<T> Post(string url, string requestBody, string contentType, string authorization)
    {
        return Fetch(url, "POST", requestBody, authorization, contentType);
    }

    public FetchResponse<T> Post(string url, JsonKeyValuePairs json, string authorization)
    {
        return Fetch(url, "POST", json.Stringify(), authorization, "application/json");
    }

    public FetchResponse<T> Post(string url, IJsonObject jsonObject, string authorization)
    {
        return Fetch(url, "POST", jsonObject.KeyValuePairs.Stringify(), authorization, "application/json");
    }

    public FetchResponse<T> Get(string url, string authorization)
    {
        return Fetch(url, "GET", "", authorization, "");
    }

    /// <summary>
    ///     RESTful request
    /// </summary>
    /// <remarks>
    ///     It will return a FetchResponse from a REST API call.
    /// </remarks>
    /// <param name="url">URL to fetch</param>
    /// <param name="method">
    ///     <see cref="System.Net.WebRequestMethods.Http" />
    /// </param>
    /// <param name="requestBody">json body</param>
    /// <param name="authorization">added to the http head if authorization is not null or empty</param>
    /// <param name="contentType">request content type</param>
    /// <returns>Returns a FetchResponse, holding a response status code and a string representing JSON.</returns>
    private FetchResponse<T> Fetch(string url, string method, string requestBody, string authorization,
        string contentType)
    {
        var startedOnUtc = DateTime.UtcNow;
        string responseBody = null;
        string responseHeaders = null;
        var responseStatusCode = 0;

        try
        {
            client = new WebClientEx(timeoutOverride);

            if (!string.IsNullOrEmpty(contentType))
            {
                client.Headers["Content-Type"] = contentType;
            }

            if (!string.IsNullOrWhiteSpace(authorization))
            {
                client.Headers["Authorization"] = authorization;
            }

            switch (method.ToUpper())
            {
                case WebRequestMethods.Http.Post:
                    responseBody = client.Post(url, requestBody);
                    break;
                case WebRequestMethods.Http.Put:
                    responseBody = client.Put(url, requestBody);
                    break;
                case WebRequestMethods.Http.Get:
                    responseBody = client.Get(url);
                    break;
                case "DELETE":
                    responseBody = client.Delete(url);
                    break;
            }

            responseStatusCode = (int) client.StatusCode;
        }
        catch (WebException ex)
        {
            if (ex.Response != null)
            {
                using (ex.Response)
                {
                    ProcessResponse(ex.Response, out responseBody, out responseHeaders, out responseStatusCode);
                }
            }

            responseBody = "<!-- " + ex.Message + " -->" + responseBody;
        }

        var response = new FetchResponse<T>(responseStatusCode, responseBody);
        return response;
    }

    private static void ProcessResponse(WebResponse response,
        out string recurlyResponse, out string responseHeaders, out int responseStatusCode)
    {
        recurlyResponse = responseHeaders = string.Empty;
        responseStatusCode = 0;

        using (var responseStream = response.GetResponseStream())
        {
            using (var sr = new StreamReader(responseStream, Encoding.UTF8))
            {
                recurlyResponse = sr.ReadToEnd();
            }
        }

        responseHeaders = response.Headers.ToString();
        var httpWebResponse = response as HttpWebResponse;
        if (httpWebResponse != null)
        {
            responseStatusCode = (int) httpWebResponse.StatusCode;
        }
    }

    public static string GetOAuthToken(string clientID, string clientSecret, string oauthURI)
    {
        var identity = clientID + ":" + clientSecret;
        var uri = oauthURI + "/access_token?grant_type=client_credentials&scope=read+write";
        var client = new WebClientEx();
        var textAsBytes = Encoding.UTF8.GetBytes(identity);
        var identityBase64 = Convert.ToBase64String(textAsBytes);

        client.Headers["Authorization"] = "Basic " + identityBase64;
        client.Headers["Content-Type"] = "application/x-www-form-urlencoded";

        var response = client.Get(uri);
        var jsonResponse = new JsonKeyValuePairs(response);
        return (string) jsonResponse["access_token"];
    }
}