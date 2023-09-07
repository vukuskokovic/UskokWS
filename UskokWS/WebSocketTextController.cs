using System.Net.WebSockets;
using System.Text;

namespace UskokWS;

public abstract class WebSocketTextController<T> : WebSocketController<T> where T : WebSocketClient, new()
{
    public override Task OnMessage(MemoryStream stream, WebSocketMessageType type)
    {
        if (type == WebSocketMessageType.Binary) return OnBinaryMessage(stream);

        var text = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
        return OnTextMessage(text);
    }

    public virtual Task OnBinaryMessage(MemoryStream stream)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnTextMessage(string text)
    {
        return Task.CompletedTask;
    }
}