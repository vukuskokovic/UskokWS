using System.Net.WebSockets;

namespace UskokWS;

public abstract class WebSocketClient
{
    public Guid ConnectionId { get; internal init; }
    public WebSocket Socket { get; internal init; } = null!;
    public CancellationToken ClientCancellationToken { get; internal init; }
    public IWebSocketController Controller { get; internal init; } = null!;

    public Task Send(ArraySegment<byte> buffer, WebSocketMessageType type)
    {
        try
        {
            return Socket.SendAsync(buffer, type, true, ClientCancellationToken);
        }
        catch (Exception ex)
        {
            return Controller.OnError(ex);
        }
    }
}