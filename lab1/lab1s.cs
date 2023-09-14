using System;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

struct Message
{
    public bool Result { get; set; }
    public int Data { get; set; }
}

class PipeServer
{
    static void Main(string[] args)
    {
        using (var pipeServer = new NamedPipeServerStream("C_Sharp_Labs"))
        {
            Console.WriteLine("Server is waiting for a connection...");
            pipeServer.WaitForConnection();

            Message receivedMessage = ReadMessage(pipeServer);
            Console.WriteLine("Result = {0}, Data = {1}", receivedMessage.Result, receivedMessage.Data);

            receivedMessage.Result = true;
            WriteMessage(pipeServer, receivedMessage);
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
}
