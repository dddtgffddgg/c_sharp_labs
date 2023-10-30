using System;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

struct Message
{
    public int valueA { get; set; }
    public int valueB { get; set; }
    public int Priority { get; set; }
}

class PipeClient
{
    static void Main(string[] args)
    {
        using (var pipeClient = new NamedPipeClientStream(".", "MyNamedPipe", PipeDirection.InOut))
        {
            Console.WriteLine("Client is connecting...");
            pipeClient.Connect();

            while (true) 
            {
                Message sentMessage = new Message();

                Console.WriteLine("Enter the first value");
                if (int.TryParse(Console.ReadLine(), out int valA))
                {
                    sentMessage.valueA = valA;
                }
                else
                {
                    Console.WriteLine("Invalid format");
                    continue;
                }

                Console.WriteLine("Enter the 2nd value");
                if (int.TryParse(Console.ReadLine(), out int valB))
                {
                    sentMessage.valueB = valB;
                }
                else
                {
                    Console.WriteLine("Invalid format");
                    continue;
                }

                Console.WriteLine("Enter the priority of value");
                if (int.TryParse(Console.ReadLine(), out int priority))
                {
                    sentMessage.Priority = priority;
                }
                else
                {
                    Console.WriteLine("Invalid format");
                    continue;
                }

                WriteMessage(pipeClient, sentMessage);

                Message receivedMessage = ReadMessage(pipeClient);
                Console.WriteLine("Value A = {0}, Value B = {1}, Priority = {2}", receivedMessage.valueA, receivedMessage.valueB, receivedMessage.Priority);
            }
        }

        Console.WriteLine("Client's work is done");
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
