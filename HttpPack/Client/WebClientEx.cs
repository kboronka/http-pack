using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace HttpPack.Client;

public class WebClientEx : WebClient
{
    public const int DefaultHttpTimeout = 100000;

    public WebClientEx(CookieContainer container, int timeout = DefaultHttpTimeout)
    {
        CookieContainer = container;
        Timeout = timeout;
        SetupCertificateSecurity();
    }

    public WebClientEx(int timeout = DefaultHttpTimeout)
    {
        Timeout = timeout;
        SetupCertificateSecurity();
    }

    public CookieContainer CookieContainer { get; set; } = new();

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