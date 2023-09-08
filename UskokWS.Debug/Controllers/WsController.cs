using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;

namespace UskokWS.Debug.Controllers;

[Route("ws")]
public class WsController : WebSocketMessageTypeController<TestClient>
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

    public override Task OnBinaryMessage(MemoryStream stream)
    {
        Logger.LogInformation("Binary {Length}", stream.Length);
        return Task.CompletedTask;
    }

    public override Task OnTextMessage(string text)
    {
        Logger.LogInformation("Text '{Text}'", text);
        return Task.CompletedTask;
    }

    public override Task OnDisconnected(WebSocketCloseStatus status)
    {
        Logger.LogInformation("{Status}", status);
        return Task.CompletedTask;
    }

    public override Task OnError(Exception ex)
    {
        Logger.LogError("{Ex}", ex.ToString());
        return Task.CompletedTask;
    }
}

public class TestClient : WebSocketClient
{
    
}