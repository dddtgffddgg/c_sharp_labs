using System;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

struct Message
{
    public bool Result { get; set; }
    public int Data { get; set; }
}

class PipeClient
{
    static Queue<Message> messageQueue = new Queue<Message>();

    static async Task Main(string[] args)
    {
        using (var pipeClient = new NamedPipeClientStream(".", "C_Sharp_Labs", PipeDirection.InOut))
        {
            Console.WriteLine("Client is connecting...");
            await pipeClient.ConnectAsync();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task sendMessagesTask = Task.Run(() => SendMessages(pipeClient, cancellationTokenSource.Token));

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true; // Prevent the process from terminating immediately
                cancellationTokenSource.Cancel(); // Signal cancellation
            };

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                Message receivedMessage = ReadMessage(pipeClient);
                Console.WriteLine("Received: Result = {0}, Data = {1}", receivedMessage.Result, receivedMessage.Data);
            }
        }

        Console.WriteLine("Client's work is done");
        Console.ReadLine();
    }

    static Message ReadMessage(NamedPipeClientStream pipeStream)
    {
        byte[] buffer = new byte[Unsafe.SizeOf<Message>()];
        pipeStream.Read(buffer, 0, buffer.Length);
        return MemoryMarshal.Read<Message>(buffer);
    }

    static void WriteMessage(NamedPipeClientStream pipeStream, Message message)
    {
        byte[] buffer = new byte[Unsafe.SizeOf<Message>()];

        MemoryMarshal.Write(buffer, ref message);

        pipeStream.Write(buffer, 0, buffer.Length);
        pipeStream.Flush();
    }

    static async Task SendMessages(NamedPipeClientStream pipeStream, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Message message = new Message { Data = 10, Result = false };
            messageQueue.Enqueue(message);

            Console.WriteLine("Sent: Result = {0}, Data = {1}", message.Result, message.Data);

            await Task.Delay(1000);
        }
    }
}
