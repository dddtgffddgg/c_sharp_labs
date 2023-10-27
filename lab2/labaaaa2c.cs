using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

struct Message
{
    public bool Result { get; set; }
    public int Data { get; set; }
    public int Priority { get; set; }
}

class PipeClient
{
    static void Main(string[] args)
    {
        using (var pipeClient = new NamedPipeClientStream(".", "C_Sharp2", PipeDirection.InOut))
        {
            Console.WriteLine("Client is connecting...");
            pipeClient.Connect();

            Message sentMessage = new Message { Data = 10, Result = false, Priority = 1 };
            WriteMessage(pipeClient, sentMessage);

            // Здесь вы можете отправлять больше сообщений клиентом, если нужно

            Console.WriteLine("Client's work is done");
            Console.ReadLine();
        }
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
