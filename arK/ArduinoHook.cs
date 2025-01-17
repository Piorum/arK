using System.IO.Ports;

namespace arK;

public static class ArduinoHook
{
    public const int baudRate = 9600;
    public const int timeoutLength = 3000;

    /// <summary>
    /// Sends $"{handshakeMessage}?" to all serial ports
    /// waits for a $"{handshakeMessage}!".
    /// </summary>
    /// <param name="handshakeMessage">Base handshake message to send.</param>
    /// <returns>Serial port if handshake received or null.</returns>
    public static async Task<SerialPort?> RetrievePort(string handshakeMessage){

        var portNames = SerialPort.GetPortNames();
        if (portNames.Length is 0){
            _ = Console.Out.WriteLineAsync("Found No Ports.");
            return null;
        }

        List<SerialPort> ports = [];
        foreach(var portName in portNames){
            SerialPort newPort = new()
            {
                PortName = portName,
                BaudRate = baudRate,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = timeoutLength,
                WriteTimeout = timeoutLength,
                DtrEnable = true
            };

            ports.Add(newPort);
        }

        CancellationTokenSource cts = new();
        List<Task<SerialPort?>> portTasks = [];
        foreach(var port in ports){
            var pingTask = Task.Run(async () => await PingPort(port, handshakeMessage, cts.Token));
            portTasks.Add(pingTask);
        }

        var result = await Task.WhenAny(portTasks.ToArray());

        if(result is not null){
            _ = cts.CancelAsync();
            var retrievedPort = await result;

            if(retrievedPort is not null){
                _ = Console.Out.WriteLineAsync($"Found port {retrievedPort.PortName}");

                foreach(var port in ports)
                    if (port.PortName != retrievedPort.PortName) _ = SerialWorker.ClosePort(port);

                return retrievedPort;
            }

            foreach(var port in ports)
                _ = SerialWorker.ClosePort(port);

            _ = Console.Out.WriteLineAsync($"No port returned handshake.");
            return null;
        }

        _ = Console.Out.WriteLineAsync($"Received no results from ArduinoHook.PingPort.");
        return null;
    }

    /// <summary>
    /// Pings SerialPort with handshakeMessage
    /// </summary>
    /// <param name="testPort">SerialPort to open and ping.</param>
    /// <param name="handshakeMessage">Base handshake message to send.</param>
    /// <returns>Serial port if handshake received or null.</returns>
    private static async Task<SerialPort?> PingPort(SerialPort testPort, string handshakeMessage, CancellationToken token = default){
        _ = Console.Out.WriteLineAsync($"Pinging {testPort.PortName}.");

        try{
            testPort.Open();
            
            while(!testPort.IsOpen){
                token.ThrowIfCancellationRequested();
                await Task.Delay(5,cancellationToken:token);
            }
            
            await SerialWorker.ClearSerialBuffer(testPort);

            testPort.WriteLine($"{handshakeMessage}?");

            string response = testPort.ReadLine().Trim();
            _ = Console.Out.WriteLineAsync($"{response}");

            if (response == $"{handshakeMessage}!"){
                _ = Console.Out.WriteLineAsync($"Handshake succeeded at {testPort.PortName}.");
                return testPort;
            } 

            await SerialWorker.ClosePort(testPort);

        } catch (TimeoutException) {
            _ = Console.Out.WriteLineAsync($"Timed out at {testPort.PortName}.");
        } catch (OperationCanceledException) {
            _ = Console.Out.WriteLineAsync($"CancellationToken Received {testPort.PortName}-Task.");
        } catch (ObjectDisposedException) {
            _ = Console.Out.WriteLineAsync($"Port Not Open {testPort.PortName}");
        } catch (Exception e) {
            _ = Console.Out.WriteLineAsync($"{e}");
        }

        await SerialWorker.ClosePort(testPort);
        _ = Console.Out.WriteLineAsync($"{testPort.PortName} did not return handshake.");
        return null;
    }
}
