using System.IO.Ports;

namespace arK;

public static class SerialWorker
{
    /// <summary>
    /// Clears serial input/output buffer
    /// </summary>
    /// <param name="port">SerialPort whose buffer to clear.</param>
    public static Task ClearSerialBuffer(SerialPort port){
        port.DiscardInBuffer();
        port.DiscardOutBuffer();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Forcefully closes serial port
    /// </summary>
    /// <param name="port">SerialPort to close.</param>
    public static async Task ClosePort(SerialPort? port){
        if(port is null || !port.IsOpen) return;

        _ = Console.Out.WriteLineAsync($"Closing {port.PortName}");

        await ClearSerialBuffer(port);
        port.Close();
    }
}
