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

namespace HttpPack
{
    public class WebClientEx : WebClient
    {
        public const int DefaultHttpTimeout = 100000;

        public WebClientEx(CookieContainer container, int timeout = DefaultHttpTimeout)
        {
            this.CookieContainer = container;
            Timeout = timeout;
            SetupCertificateSecurity();
        }

        public WebClientEx(int timeout = DefaultHttpTimeout)
        {
            Timeout = timeout;
            SetupCertificateSecurity();
        }

        public CookieContainer CookieContainer { get; set; } = new CookieContainer();

        public int Timeout { get; set; }

        public System.Net.HttpStatusCode StatusCode { get; set; }

        private void SetupCertificateSecurity()
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12;

            ServicePointManager.ServerCertificateValidationCallback = OnValidateCertificate;
            ServicePointManager.Expect100Continue = true;
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain,
            SslPolicyErrors error)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (error == SslPolicyErrors.None)
            {
                return true;
            }

            return false;
        }

        public WebRequest GetWebRequestEx(Uri address)
        {
            return GetWebRequest(address);
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var r = base.GetWebRequest(address);
            var request = r as HttpWebRequest;
            request.Timeout = Timeout;
            if (request != null)
            {
                request.CookieContainer = CookieContainer;
            }

            return r;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            request.Timeout = Timeout;
            var response = base.GetWebResponse(request, result);
            var httpResponse = (HttpWebResponse) response;
            StatusCode = httpResponse.StatusCode;
            ReadCookies(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            request.Timeout = Timeout;
            var response = base.GetWebResponse(request);
            var httpResponse = (HttpWebResponse) response;
            StatusCode = httpResponse.StatusCode;
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                var cookies = response.Cookies;
                CookieContainer.Add(cookies);
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

        private static bool OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}