using System;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Diagnostics;

struct Message
{
    public double valueA { get; set; }
    public double valueB { get; set; }
    public double result { get; set; }
}

class PipeClient
{
    public static Mutex mutex = new Mutex();

    private static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Havent arguments");
            return;
        }

        string pipeName = args[0];

        using var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

        try
        {
            await pipeClient.ConnectAsync();

            Console.WriteLine("Client connected to the server.");

            byte[] bytes = new byte[Marshal.SizeOf<Message>()];
            await pipeClient.ReadAsync(bytes, 0, bytes.Length);

            var receivedMessage = MemoryMarshal.Read<Message>(bytes);

            Console.WriteLine($"Received values from server: {receivedMessage.valueA}, {receivedMessage.valueB}");

            double result = TrapezoidIntegral(x => 2 * Math.Sin(x), receivedMessage.valueA, receivedMessage.valueB, 1000);
            receivedMessage.result = result;

            Console.WriteLine($"Sending result back to the server: {result}");

            MemoryMarshal.Write(bytes, ref receivedMessage);
            await pipeClient.WriteAsync(bytes, 0, bytes.Length);

        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
        finally
        {
            pipeClient.Close();
        }
    }

    private static double TrapezoidIntegral(Func<double, double> function, double a, double b, int numIntervals)
    {
        double h = (b - a) / numIntervals;
        double result = 0.5 * (function(a) + function(b));

        for (int i = 1; i < numIntervals; i++)
        {
            double x = a + i * h;
            result += function(x);
        }

        result *= h;
        return result;
    }

}
