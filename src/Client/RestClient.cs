/* Copyright (C) 2018 Kevin Boronka
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;

namespace HttpPack
{
	public class HttpClient<T>
	{
		private WebClientEx client;
		private const int TimeoutFallbackValue = 1000;	// 1 second
		private readonly int timeoutOverride;
		
		public HttpClient(int timeoutOverride)
		{
			this.timeoutOverride = timeoutOverride;
		}
		
		public HttpClient()
			: this(TimeoutFallbackValue)
		{
			
		}
		
		/// <summary>
		/// RESTful request
		/// </summary>
		/// <remarks>
		/// It will return a FetchResponse from a REST API call.
		/// </remarks>
		/// <param name="url">URL to fetch</param>
		/// <param name="method"><see cref="System.Net.WebRequestMethods.Http"/></param>
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
		/// RESTful request
		/// </summary>
		/// <remarks>
		/// It will return a FetchResponse from a REST API call.
		/// </remarks>
		/// <param name="url">URL to fetch</param>
		/// <param name="method"><see cref="System.Net.WebRequestMethods.Http"/></param>
		/// <param name="requestBody">json body</param>
		/// <param name="authorization">added to the http head if authorization is not null or empty</param>
		/// <param name="contentType">request content type</param>
		/// <returns>Returns a FetchResponse, holding a response status code and a string representing JSON.</returns>
		private FetchResponse<T> Fetch(string url, string method, string requestBody, string authorization, string contentType)
		{
			DateTime startedOnUtc = DateTime.UtcNow;
			string responseBody = null;
			string responseHeaders = null;
			int responseStatusCode = 0;

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
				
				responseStatusCode = (int)client.StatusCode;
			}
			catch (System.Net.WebException ex)
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
		
		private static void ProcessResponse(System.Net.WebResponse response,
			out string recurlyResponse, out string responseHeaders, out int responseStatusCode)
		{
			recurlyResponse = responseHeaders = String.Empty;
			responseStatusCode = 0;

			using (var responseStream = response.GetResponseStream())
			{
				using (var sr = new System.IO.StreamReader(responseStream, Encoding.UTF8))
				{
					recurlyResponse = sr.ReadToEnd();
				}
			}

			responseHeaders = response.Headers.ToString();
			var httpWebResponse = response as System.Net.HttpWebResponse;
			if (httpWebResponse != null)
			{
				responseStatusCode = (int)httpWebResponse.StatusCode;
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
			return (string)jsonResponse["access_token"];
		}
	}
}