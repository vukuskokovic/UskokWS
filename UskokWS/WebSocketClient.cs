using System.Net.WebSockets;

namespace UskokWS;

public abstract class WebSocketClient
{
    public Guid ConnectionId { get; internal init; }
    public WebSocket Socket { get; internal init; } = null!;
    public CancellationToken ClientCancellationToken { get; internal init; }
    public IWebSocketController Controller { get; internal init; } = null!;

    public async Task Send(ArraySegment<byte> buffer, WebSocketMessageType type)
    {
        try
        {
            await Socket.SendAsync(buffer, type, true, ClientCancellationToken);
        }
        catch (Exception ex)
        {
            if(ex is WebSocketException { WebSocketErrorCode: WebSocketError.InvalidState })
            {
                try
                {
                    await Socket.CloseAsync(WebSocketCloseStatus.Empty, null, ClientCancellationToken);
                }
                catch
                {
                    //
                }
            }
            await Controller.OnError(ex);
        }
    }
}