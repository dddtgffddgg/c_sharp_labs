using System;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

struct Message
{
    public bool Result { get; set; }
    public int Data { get; set; }
}

class PipeClient
{
    static void Main(string[] args)
    {
        using (var pipeClient = new NamedPipeClientStream(".", "C_Sharp_Labs", PipeDirection.InOut))
        {
            Console.WriteLine("Client is connecting...");
            pipeClient.Connect();

            Message sentMessage = new Message { Data = 10, Result = false };
            WriteMessage(pipeClient, sentMessage);

            Message receivedMessage = ReadMessage(pipeClient);
            Console.WriteLine("Result = {0}, Data = {1}", receivedMessage.Result, receivedMessage.Data);
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
}
