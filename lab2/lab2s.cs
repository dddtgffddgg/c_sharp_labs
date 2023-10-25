using System;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

struct Message
{
    public bool Result { get; set; }
    public int Data { get; set; }
}

class PipeServer
{
    static Queue<Message> messageQueue = new Queue<Message>();
    static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    static List<Message> receivedMessages = new List<Message>();

    static async Task Main(string[] args)
    {
        using (var pipeServer = new NamedPipeServerStream("C_Sharp_Labs"))
        {
            Console.WriteLine("Server is waiting for a connection...");
            pipeServer.WaitForConnection();

            Task processMessagesTask = Task.Run(() => ProcessMessages(pipeServer));

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true; // Prevent the process from terminating immediately
                cancellationTokenSource.Cancel(); // Signal cancellation
            };

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                Message newMessage = ReadMessage(pipeServer);
                Console.WriteLine("Received: Result = {0}, Data = {1}", newMessage.Result, newMessage.Data);
                receivedMessages.Add(newMessage);
            }

            // Save received messages to a file or display them
            // Example: SaveMessagesToFile(receivedMessages);

            await processMessagesTask; // Wait for the ProcessMessages task to complete
        }

        Console.WriteLine("Server's work is done");
        Console.ReadLine();
    }

    static Message ReadMessage(NamedPipeServerStream pipeStream)
    {
        byte[] buffer = new byte[Unsafe.SizeOf<Message>()];
        pipeStream.Read(buffer, 0, buffer.Length);
        return MemoryMarshal.Read<Message>(buffer);
    }

    static void WriteMessage(NamedPipeServerStream pipeStream, Message message)
    {
        byte[] buffer = new byte[Unsafe.SizeOf<Message>()];

        MemoryMarshal.Write(buffer, ref message);

        pipeStream.Write(buffer, 0, buffer.Length);
        pipeStream.Flush();
    }

    static async Task ProcessMessages(NamedPipeServerStream pipeServer)
    {
        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            if (messageQueue.Count > 0)
            {
                Message message = messageQueue.Dequeue();
                WriteMessage(pipeServer, message); 
            }

            await Task.Delay(100); 
        }
    }
}
