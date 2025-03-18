using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

namespace UskokWS;

public abstract class WebSocketMessageTypeController<T> : WebSocketController<T> where T : WebSocketClient, new()
{
    private Encoding Encoding { get; }
    public WebSocketMessageTypeController(Encoding? encoding = null)
    {
        Encoding = encoding ?? Encoding.UTF8;
    }

    [NonAction]
    public sealed override Task OnMessage(MemoryStream stream, WebSocketMessageType type)
    {
        if (type == WebSocketMessageType.Binary) return OnBinaryMessage(stream);

        var text = Encoding.GetString(stream.GetBuffer(), 0, (int)stream.Length);
        return OnTextMessage(text);
    }
    [NonAction]
    public virtual Task OnBinaryMessage(MemoryStream stream) => Task.CompletedTask;
    [NonAction]
    public virtual Task OnTextMessage(string text) => Task.CompletedTask;
}