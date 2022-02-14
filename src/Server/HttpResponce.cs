using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HttpPack.Server;

// http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html
public enum HttpStatusCode
{
    [Description("Switching Protocols")] SWITCHING_PROTOCOLS = 101,

    [Description("OK")] OK = 200,

    FOUND = 302,

    [Description("Not Modified")] NOT_MODIFIED = 304,

    NOTFOUND = 404,

    SERVERERROR = 500
}

public class HttpResponse
{
    private readonly HttpContent content;
    private readonly HttpRequest request;
    public byte[] bytes;

    public HttpResponse(HttpRequest request)
    {
        this.request = request;

        try
        {
            if (this.request.Path == @"")
            {
                if (HttpController.Primary == null)
                {
                    throw new ApplicationException("Primary Controller Not Defined");
                }

                if (HttpController.Primary.PrimaryAction == null)
                {
                    throw new ApplicationException("Primary Action Not Defined");
                }

                content = HttpController.RequestPrimary(this.request);
            }
            else if (this.request.IsWebSocket && HttpWebSocket.WebSocketControllerExists(this.request))
            {
                var type = HttpWebSocket.GetWebSocketController(this.request);
                this.request.WebSocket = (HttpWebSocket) Activator.CreateInstance(type, this.request);
            }
            else if (HttpController.ActionExists(this.request))
            {
                content = HttpController.RequestAction(this.request);
            }
            else
            {
                content = HttpContent.Read(this.request.Server, this.request.Path);
            }

            if (content is HttpExceptionContent)
            {
                bytes = ConstructResponse(HttpStatusCode.SERVERERROR);
            }
            else if (this.request.IsWebSocket)
            {
                bytes = ConstructResponse(HttpStatusCode.SWITCHING_PROTOCOLS);
            }
            else if (content.ETag == this.request.ETag && !content.ParsingRequired)
            {
                bytes = ConstructResponse(HttpStatusCode.NOT_MODIFIED);
            }
            else
            {
                bytes = ConstructResponse(HttpStatusCode.OK);
            }
        }
        catch (FileNotFoundException ex)
        {
            content = new HttpExceptionContent(ex);
            bytes = ConstructResponse(HttpStatusCode.SERVERERROR);
        }
        catch (Exception ex)
        {
            content = new HttpExceptionContent(ex);
            bytes = ConstructResponse(HttpStatusCode.SERVERERROR);
        }
    }

    public byte[] Bytes => bytes;

    private byte[] ConstructResponse(HttpStatusCode status)
    {
        // Construct response header

        const string GMT = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";
        const string eol = "\r\n";
        var response = "";
        var contentBytes = new byte[] { };

        // status line
        var responsePhrase = Enum.GetName(typeof(HttpStatusCode), status);
        response += " " + (int) status + " " + responsePhrase + eol;

        if (!request.IsWebSocket)
        {
            response += @"Server: " + @"http-pack" + eol;
            response += @"Date: " + DateTime.UtcNow.ToString(GMT) + eol;
            response += @"ETag: " + content.ETag + eol;
            response += @"Set-Cookie: sarSession=" + request.Session.ID + @"; Path=/; expires=" +
                        request.Session.ExpiryDate.ToString(GMT) + ";" + eol;
            response += @"Last-Modified: " + content.LastModified.ToString(GMT) + eol;

            // content details
            if (status != HttpStatusCode.NOT_MODIFIED)
            {
                contentBytes = content.Render(request.Server.Cache);
                response += @"Content-Type: " + content.ContentType + eol;
                response += @"Content-Length: " + contentBytes.Length + eol;
                //response += @"Expires: " + DateTime.UtcNow.AddDays(1).ToString(GMT) + eol;
            }

            /*
            response += @"Access-Control-Allow-Origin: *" + eol;
            response += @"Access-Control-Allow-Methods: POST, GET" + eol;
            response += @"Access-Control-Max-Age: 1728000" + eol;
            response += @"Access-Control-Allow-Credentials: true" + eol;
             */

            // keep-alive
            response += "Keep-Alive: timeout=" + HttpConnection.MAX_TIME + eol;
            response += "Connection: keep-alive";
            response += eol + eol;
        }
        else
        {
            response += @"Connection: Upgrade" + eol;
            response += @"Upgrade: websocket" + eol;

            var hash = SHA1.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(request.WebSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
            response += @"Sec-WebSocket-Accept: " + Convert.ToBase64String(hash) + eol;

            if (!string.IsNullOrEmpty(request.WebSocketProtocol))
            {
                response += @"Sec-WebSocket-Protocol: " + request.WebSocketProtocol + eol;
            }

            response += eol;
        }

        return contentBytes.Length > 0
            ? CombineByteArrays(Encoding.ASCII.GetBytes(response), contentBytes)
            : Encoding.ASCII.GetBytes(response);
    }

    private static byte[] CombineByteArrays(params byte[][] arrays)
    {
        var sum = 0;
        var offset = 0;

        foreach (var array in arrays)
        {
            sum += array.Length;
        }

        var result = new byte[sum];

        foreach (var array in arrays)
        {
            Buffer.BlockCopy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }

        return result;
    }
}