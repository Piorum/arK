namespace arK;

public class Program
{
    public static async Task Main(){
        var arKPort = await ArduinoHook.RetrievePort("arK") ?? throw new("Failed To Retrieve Port");

        _ = Console.Out.WriteLineAsync("Found Port!");

        await using (ArKInterface arK = new(arKPort))
        {
            await Task.Delay(1000);
            await arK.SendKey('Q',50);
            await arK.SendSpecialKey(SpecialKey.TAB,50);
        };

        await Task.Delay(1000);
    }
}