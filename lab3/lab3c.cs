using System;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

struct Message
{
    public int valueA { get; set; }
    public int valueB { get; set; }
}

class PipeClient
{
    public static Mutex mutex = new Mutex();

    public struct Home
    {
        public Home(int n, int s)
        {
            valueA = n;
            valueB = s;
        }
        public int valueA { get; set; }
        public int valueB { get; set; }
    }

    private static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Not enough command line arguments");
            return;
        }

        int valueA = int.Parse(args[0]);
        int valueB = int.Parse(args[1]);

        double result = TrapezoidalIntegration(1000, x => 2*Math.Sin(x), valueA, valueB);

        Console.WriteLine($"Result of integral calculation: {result}");

        Console.WriteLine("The client's work is completed");
        //string filePath = "C:\\Users\\diana\\C_sharp_labs\\Lab2.txt";

        using (var pipeClient = new NamedPipeClientStream(".", "MyNamedPipe", PipeDirection.InOut))
        {
            try
            {
                await pipeClient.ConnectAsync();
                Console.WriteLine("The client is connecting...");

                while (true)
                {

                    byte[] messageBuffer = new byte[Unsafe.SizeOf<Home>()];
                    await pipeClient.ReadAsync(messageBuffer);

                    Home receivedWork = MemoryMarshal.Read<Home>(messageBuffer);
                    Console.WriteLine("Received from server: {0} and {1}.", receivedWork.valueA, receivedWork.valueB);

                    Console.WriteLine("End of data transmission...");

                    await pipeClient.WriteAsync(messageBuffer);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

      
    }
    }
    private static double TrapezoidalIntegration(int n, Func<double, double> func, double a, double b)
    {
        double h = (b - a) / n;
        double result = (func(a) + func(b)) / 2.0;

        for (int i = 1; i < n; i++)
        {
            double x = a + i * h;
            result += func(x);
        }

        result *= h;

        return result;
    }
}
