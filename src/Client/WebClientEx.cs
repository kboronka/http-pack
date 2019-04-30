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

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace HttpPack.Net
{
	public class WebClientEx : WebClient
	{
		private CookieContainer container = new CookieContainer();

		public const int DefaultHttpTimeout = 100000;

		public CookieContainer CookieContainer
		{
			get { return container; }
			set { container = value; }
		}

		public int Timeout { get; set; }
		
		public HttpStatusCode StatusCode { get; set; }

		public WebClientEx(CookieContainer container, int timeout = DefaultHttpTimeout)
		{
			this.container = container;
			this.Timeout = timeout;
			SetupCertificateSecurity();
		}

		public WebClientEx(int timeout = DefaultHttpTimeout)
		{
			this.Timeout = timeout;
			SetupCertificateSecurity();
		}
		
		private void SetupCertificateSecurity()
		{
			ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
			ServicePointManager.SecurityProtocol = 
				SecurityProtocolType.Tls | // TLS 1.0
			(SecurityProtocolType)3072 |	// TLS 1.2
			(SecurityProtocolType)768 | // TLS 1.1
			SecurityProtocolType.Ssl3;
			ServicePointManager.CertificatePolicy = new WebClientPolicy();
		}
		
		private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
		{
			// If the certificate is a valid, signed certificate, return true.
			if (error == System.Net.Security.SslPolicyErrors.None)
			{
				return true;
			}
			
			return false;
		}
		
		public WebRequest GetWebRequestEx(Uri address)
		{
			return this.GetWebRequest(address);
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest r = base.GetWebRequest(address);
			var request = r as HttpWebRequest;
			request.Timeout = this.Timeout;
			if (request != null)
			{
				request.CookieContainer = container;
			}
			
			return r;
		}

		protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
		{
			request.Timeout = this.Timeout;
			WebResponse response = base.GetWebResponse(request, result);
			var httpResponse = (HttpWebResponse)response;
			StatusCode = httpResponse.StatusCode;
			ReadCookies(response);
			return response;
		}

		protected override WebResponse GetWebResponse(WebRequest request)
		{
			request.Timeout = this.Timeout;
			WebResponse response = base.GetWebResponse(request);
			var httpResponse = (HttpWebResponse)response;
			StatusCode = httpResponse.StatusCode;
			ReadCookies(response);
			return response;
		}

		private void ReadCookies(WebResponse r)
		{
			var response = r as HttpWebResponse;
			if (response != null)
			{
				CookieCollection cookies = response.Cookies;
				container.Add(cookies);
			}
		}
		
		public string Post(string path, string content)
		{
			var response = UploadString(path, content);
			return response;
		}
		
		public string Get(string path)
		{
			var response = DownloadString(path);
			return response;
		}
		
		public string Put(string path, string content)
		{
			var response = UploadString(path, "PUT", content);
			return response;
		}
		
		public string Delete(string path)
		{
			var response = UploadString(path, "DELETE", "");
			return response;
		}
	}
	
	public class WebClientPolicy : ICertificatePolicy
	{
		public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate cert, WebRequest request, int certificateProblem)
		{
			return true;
		}
	}
}
