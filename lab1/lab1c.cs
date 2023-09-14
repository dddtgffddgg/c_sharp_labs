using System;
using System.IO.Pipes;
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
        using (var pipeClient = new NamedPipeClientStream(".", "Pipe_lab1", PipeDirection.InOut))
        {
            Console.WriteLine("Client is connecting...");
            pipeClient.Connect();

            Message sentMessage = new Message { Data = 10, Result = false };
            WriteMessage(pipeClient, sentMessage);

            Message receivedMessage = ReadMessage(pipeClient);
            Console.WriteLine("Client received data: Result = {0}, Data = {1}", receivedMessage.Result, receivedMessage.Data);
        }

        Console.WriteLine("Client's work is done");
        Console.ReadLine();
    }

    static Message ReadMessage(NamedPipeClientStream pipeStream)
    {
        byte[] buffer = new byte[Marshal.SizeOf<Message>()];
        pipeStream.Read(buffer, 0, buffer.Length);
        return ByteArrayToMessage(buffer);
    }

    static void WriteMessage(NamedPipeClientStream pipeStream, Message message)
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
