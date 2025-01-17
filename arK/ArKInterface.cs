using System.IO.Ports;

namespace arK;

public class ArKInterface(SerialPort port) : IAsyncDisposable
{
    private static readonly Random random = new();
    public SerialPort Port { private set; get; } = port;

    public async Task SendKey(char key, int? delay = null){
        delay ??= random.Next(40,60);

        Port.WriteLine($"D{key}");
        await Task.Delay((int)delay);
        Port.WriteLine($"U{key}");
    }

    public Task Deactivate(){
        Port.WriteLine($"arK/");
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await Deactivate();
        await SerialWorker.ClosePort(Port);

        Port.Dispose();

        GC.SuppressFinalize(this);
    }
}
