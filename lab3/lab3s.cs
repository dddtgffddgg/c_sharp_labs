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
using System.Reflection.Emit;

internal struct Message
{
    public double valueA { get; set; }
    public double valueB { get; set; }
    public double result { get; set; }
}

internal static class PipeServer
{
    private static readonly Mutex mutex = new Mutex();

    private static readonly PriorityQueue<Message, int> queue = new PriorityQueue<Message, int>();
    private static void WriteToFile(Message message, string filePath)
    {
        using var streamWriter = File.AppendText(filePath);
        streamWriter.WriteLine("value A = {0}, value B = {1} and result {2}", message.valueA, message.valueB, message.result);
    }

    private static Task WriteStructAsync(CancellationTokenSource cancellationTokensourse)
    {
        CancellationToken cancellationToken = cancellationTokensourse.Token;

        return Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Message message = new Message();

                Console.WriteLine("Enter the first value");

                if (double.TryParse(Console.ReadLine(), out double valA))
                {
                    message.valueA = valA;
                }
                else
                {
                    Console.WriteLine("Invalid format");
                }

                Console.WriteLine("Enter the 2nd value");

                if (double.TryParse(Console.ReadLine(), out double valB))
                {
                    message.valueB = valB;

                    Console.WriteLine("Enter the priority of value");
                }
                else
                {
                    Console.WriteLine("Invalid format");
                }

                if (cancellationTokensourse.IsCancellationRequested)
                    break;

                mutex.WaitOne();

                if (int.TryParse(Console.ReadLine(), out int priority))
                {
                    queue.Enqueue(message, priority);
                }
                else
                {
                    queue.Enqueue(message, 0);
                }
                mutex.ReleaseMutex();
            }
        });
    }
    private static Task ReadAndWritePipeAsync(CancellationTokenSource cancellationTokensourse)
    {
        CancellationToken cancellationToken = cancellationTokensourse.Token;

        return Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (queue.Count > 0)
                {
                    mutex.WaitOne();

                    Message message = queue.Dequeue();

                    mutex.ReleaseMutex();

                    _ = testAsync(message, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

                }

            }
        });
    }
    private static async Task testAsync(Message message, CancellationToken cancellationToken)
    {
        Process? clientProcess = default;
        Guid id = Guid.NewGuid();

        NamedPipeServerStream PipeServer = new(id.ToString());
        if (!PipeServer.IsConnected)
        {
            clientProcess = MyClientProcess(id.ToString());
            PipeServer.WaitForConnection();
        }

        Console.WriteLine("Connect");
        Console.WriteLine(message.valueA);
        byte[] bytes = new byte[Unsafe.SizeOf<Message>()];
        MemoryMarshal.Write(bytes, in message);

        try
        {
            await PipeServer.WriteAsync(bytes, cancellationToken);
            await PipeServer.ReadAsync(bytes, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Did not work out(");
        }

        clientProcess?.WaitForExit();
        message = MemoryMarshal.Read<Message>(bytes);
        WriteToFile(message, "C:\\Users\\diana\\OneDrive\\Рабочий стол\\2class\\all_labs\\EVM\\C_Sharp3\\LABA3.txt");

        PipeServer.Dispose();

    }
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Enter your data (CTRC+C to interrupt)");

        CancellationTokenSource cancellationTokensourse = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cancellationTokensourse.Cancel();
        };


        Task t1 = WriteStructAsync(cancellationTokensourse);

        //StartClientProcess(serverPipeName, server_id);

        Task t2 = ReadAndWritePipeAsync(cancellationTokensourse);

        await Task.WhenAll(t1, t2);

    }

    private static Process? MyClientProcess(string name)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = "C:\\Users\\diana\\OneDrive\\Рабочий стол\\2class\\all_labs\\EVM\\C_Sharp3\\LAB3Serv\\LAB3Cli\\bin\\Debug\\net8.0\\LAB3Cli.exe",
            Arguments = $"{name}"
        };

        return Process.Start(startInfo);
    }

}
