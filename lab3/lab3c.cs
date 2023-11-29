using System;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text;


/*struct Message
{
    public int valueA { get; set; }
    public int valueB { get; set; }
}*/

class PipeClient
{
    public static Mutex mutex = new Mutex();

    public struct Home
    {
        public Home(double n, double s)
        {
            valueA = n;
            valueB = s;
        }
        public double valueA { get; set; }
        public double valueB { get; set; }
    }

    private static async Task Main()
    {
        string filePath = "C:\\Users\\diana\\C_sharp_labs\\Lab2.txt";

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

                    if (receivedWork.valueA == 1)
                    {
                        double result = CalculateTrapezoidalIntegral(0, Math.PI, 1000);
                        Console.WriteLine("Calculated integral result: " + result);

                        Home resultMessage = new Home { valueA = 0, valueB = (double)result };
                        messageBuffer = MemoryMarshal.AsBytes(resultMessage);
                        await pipeClient.WriteAsync(messageBuffer);

                    }

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

    private static double CalculateTrapezoidalIntegral(double a, double b, int numIntervals)
    {
        double h = (b - a) / numIntervals;
        double sum = 0.5 * (Math.Sin(a) + Math.Sin(b));

        for (int i = 1; i < numIntervals; i++)
        {
            double x = a + i * h;
            sum += Math.Sin(x);
        }

        return h * sum;
    }

}
