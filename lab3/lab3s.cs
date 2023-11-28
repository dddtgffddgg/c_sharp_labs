using System;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Net;
using System.Text;

internal struct Message
{
    public int valueA { get; set; }
    public int valueB { get; set; }
}

internal static class PipeServer
{
    private static Mutex mutex = new Mutex();

    private static PriorityQueue<Message, int> queue = new PriorityQueue<Message, int>();

    static string filePath = "C:\\Users\\diana\\C_sharp_labs\\Lab2.txt"; //путь к файлу для сохранения данных

    static Task WriteStructAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Message message = new Message();

                Console.WriteLine("Enter the first value");

                if (int.TryParse(Console.ReadLine(), out int valA))
                {
                    message.valueA = valA;
                }
                else
                {
                    Console.WriteLine("Invalid format");
                }

                Console.WriteLine("Enter the 2nd value");

                if (int.TryParse(Console.ReadLine(), out int valB))
                {
                    message.valueB = valB;

                }
                else
                {
                    Console.WriteLine("Invalid format");
                }

                mutex.WaitOne();

                if (int.TryParse(Console.ReadLine(), out int priority))
                {
                    queue.Enqueue(message, 0);

                    StartClientApp();
                }
                else
                {
                    queue.Enqueue(message, 0);
                }

                mutex.ReleaseMutex();

            }
        });
    }

    private static Task ReadAndWritePipeAsync(NamedPipeServerStream pipeServer, CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (queue.Count > 0)
                {
                    mutex.WaitOne();

                    Message message = queue.Dequeue();

                    mutex.ReleaseMutex();


                    if (!pipeServer.IsConnected)
                    {
                        continue;
                    }

                    Console.WriteLine("Connected!");
                    byte[] bytes = new byte[Unsafe.SizeOf<Message>()];

                    MemoryMarshal.Write(bytes, ref message);

                    try
                    {
                        await pipeServer.WriteAsync(bytes, cancellationToken);
                        //await pipeServer.ReadAsync(bytes, cancellationToken);
                        //message = MemoryMarshal.Read<Message>(bytes);
                        //WriteToFile(message, filePath); //сохранить данные в файл
                        StartClientApp(message);

                    }

                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }

            }
        });
    }
    private static void StartClientApp()
    {
        try
        {
            string clientAppPath = "C:\\Users\\diana\\source\\repos\\Lab3C"; // путь к клиентскому приложению

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = clientAppPath,
                UseShellExecute = true,
                CreateNoWindow = false,
                Arguments = $"{message.valueA} {message.valueB}"
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                Console.WriteLine("Client application started.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting client application: {ex.Message}");
        }
    }


    private static async Task Main()
    {
        CancellationTokenSource cts = new CancellationTokenSource();

        var newServer = new NamedPipeServerStream("MyNamedPipe");

        Console.WriteLine("Enter your data (CTRC+C to interrupt)");

        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Ctrl+C pressed.\n Sending data to the client and exiting...");
            cts.Cancel();
            e.Cancel = true;
        };


        Task t1 = WriteStructAsync(cts.Token);

        newServer.WaitForConnection();

        Task t2 = ReadAndWritePipeAsync(newServer, cts.Token);

        StartClientApp(); //запуск второго консольного приложения

        await Task.WhenAll(t1, t2);

        await newServer.DisposeAsync();

        Console.WriteLine("Server's work is done");
    }

    private static void WriteToFile(Message message, string filePath)
    {
        using var streamWriter = File.AppendText(filePath);
        streamWriter.WriteLine("Value A = {0}", message.valueA);
        streamWriter.WriteLine("Value B = {0}", message.valueB);

    }

}
