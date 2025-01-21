using System.IO.Ports;

namespace arK;

public class ArKInterface(SerialPort port) : IAsyncDisposable
{
    private static readonly Random random = new();
    public SerialPort Port { private set; get; } = port;

    public async Task SendKey(char key, int? delay = null, bool special = default){
        delay ??= random.Next(40,60);

        Port.WriteLine($"KN{key}D");
        await Task.Delay((int)delay);
        Port.WriteLine($"KN{key}U");
    }

    public async Task SendSpecialKey(SpecialKey specialKey, int? delay = null){
        delay ??= random.Next(40,60);

        var translatedKey = specialKey switch
        {
            SpecialKey.TAB => 'T',
            _ => throw new Exception($"Invalid SpecialKey {specialKey}")
        };

        Port.WriteLine($"KS{translatedKey}D");
        await Task.Delay((int)delay);
        Port.WriteLine($"KS{translatedKey}U");
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
