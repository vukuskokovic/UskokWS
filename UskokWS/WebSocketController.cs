using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;

namespace UskokWS;

[ApiController]
public abstract class WebSocketController<T> : ControllerBase, IWebSocketController where T : WebSocketClient, new()
{
    public static readonly ConcurrentDictionary<Guid, T> ConnectedSockets = new();
    protected T Client { get; private set; } = null!;

    [NonAction]
    protected async Task HandleConnection(HttpContext context, CancellationToken token)
    {
        var socket = await context.WebSockets.AcceptWebSocketAsync();
        Client = new T
        {
            Socket = socket,
            ClientCancellationToken = token,
            ConnectionId = Guid.NewGuid(),
            Controller = this
        };
        ConnectedSockets[Client.ConnectionId] = Client;
        await OnConnected(token);
        var closeStatus = WebSocketCloseStatus.NormalClosure;
        var stream = new MemoryStream();
        while (true)
        {
            try
            {
                var buffer = new byte[5096];
                var receiveResult = await socket.ReceiveAsync(buffer, token);
                
                if (receiveResult.CloseStatus != null || receiveResult.MessageType == WebSocketMessageType.Close || socket.State != WebSocketState.Open)
                {
                    closeStatus = receiveResult.CloseStatus ?? WebSocketCloseStatus.Empty;
                    break;
                }

                await stream.WriteAsync(buffer.AsMemory(0, receiveResult.Count), token);
                if (receiveResult.EndOfMessage)
                {
                    try
                    {
                        stream.Position = 0;
                        await OnMessage(stream, receiveResult.MessageType);
                    }
                    catch (Exception ex)
                    {
                        await OnError(ex);
                    }
                    finally
                    {
                        await stream.DisposeAsync();
                        stream = new MemoryStream();
                    }
                }
            }
            
            
            catch (Exception ex)
            {
                if (ex is TaskCanceledException)
                {
                    break;
                }

                if (ex is WebSocketException or OperationCanceledException or ObjectDisposedException)
                {
                    closeStatus = WebSocketCloseStatus.Empty;
                    break;
                }

                await OnError(ex);
            }
        }

        try
        {
            await stream.DisposeAsync();
        }
        catch
        {
            //
        }

        ConnectedSockets.TryRemove(Client.ConnectionId, out _);
        await OnDisconnected(closeStatus);
    }

    [NonAction]
    public virtual Task OnConnected(CancellationToken token)
    {
        return Task.CompletedTask;
    }
    [NonAction]
    public virtual Task OnDisconnected(WebSocketCloseStatus status)
    {
        return Task.CompletedTask;
    }
    [NonAction]
    public virtual Task OnMessage(MemoryStream stream, WebSocketMessageType type)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// This method cannot fail!!!
    /// </summary>
    /// <param name="ex">The exception that occured</param>
    [NonAction]
    public virtual Task OnError(Exception ex)
    {
        return Task.CompletedTask;
    }
}

public interface IWebSocketController
{
    public Task OnConnected(CancellationToken token);

    public Task OnDisconnected(WebSocketCloseStatus status);

    public Task OnMessage(MemoryStream stream, WebSocketMessageType type);

    public Task OnError(Exception ex);
}