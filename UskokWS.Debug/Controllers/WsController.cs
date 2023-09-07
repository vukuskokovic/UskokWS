using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;

namespace UskokWS.Debug.Controllers;

[Route("ws")]
public class WsController : WebSocketController<TestClient>
{
    private ILogger<WsController> Logger { get; }
    public WsController(ILogger<WsController> logger)
    {
        Logger = logger;
    }
    
    [HttpGet]
    public Task Test(CancellationToken token)
    {
        return HandleConnection(HttpContext, token);
    }

    public override Task OnConnected(CancellationToken token)
    {
        Logger.LogInformation("{Id}", Client.ConnectionId);
        return Task.CompletedTask;
    }

    public override async Task OnMessage(MemoryStream stream, WebSocketMessageType type)
    {
        await Task.Delay(3_000, Client.ClientCancellationToken);
        Logger.LogInformation("{DidCancel}", Client.ClientCancellationToken.IsCancellationRequested);
    }

    public override Task OnDisconnected(WebSocketCloseStatus status)
    {
        Logger.LogInformation("{Status}", status);
        return Task.CompletedTask;
    }
}

public class TestClient : WebSocketClient
{
    
}