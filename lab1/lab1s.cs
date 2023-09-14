using System;
using System.IO.Pipes;
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
        using (var pipeServer = new NamedPipeServerStream("Pipe_lab1"))
        {
            Console.WriteLine("Server is waiting for a connection...");
            pipeServer.WaitForConnection();

            Message receivedMessage = ReadMessage(pipeServer);
            Console.WriteLine("Server received data: Result = {0}, Data = {1}", receivedMessage.Result, receivedMessage.Data);

            receivedMessage.Result = true;
            WriteMessage(pipeServer, receivedMessage);
        }

        Console.WriteLine("Server's work is done");
        Console.ReadLine();
    }

    static Message ReadMessage(NamedPipeServerStream pipeStream)
    {
        byte[] buffer = new byte[Marshal.SizeOf<Message>()];
        pipeStream.Read(buffer, 0, buffer.Length);
        return ByteArrayToMessage(buffer);
    }

    static void WriteMessage(NamedPipeServerStream pipeStream, Message message)
    {
        byte[] buffer = MessageToByteArray(message);
        pipeStream.Write(buffer, 0, buffer.Length);
        pipeStream.WaitForPipeDrain();
    }

    static Message ByteArrayToMessage(byte[] buffer)
    {
        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        Message message = (Message)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Message));
        handle.Free();
        return message;
    }

    static byte[] MessageToByteArray(Message message)
    {
        byte[] buffer = new byte[Marshal.SizeOf(message)];
        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        Marshal.StructureToPtr(message, handle.AddrOfPinnedObject(), false);
        handle.Free();
        return buffer;
    }
}
