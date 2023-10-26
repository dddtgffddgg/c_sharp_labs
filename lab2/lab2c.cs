using System;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

struct Message
{
    public bool Result { get; set; }
    public int Data { get; set; }
}

class PipeClient
{
    static void Main(string[] args)
    {
        using (var pipeClient = new NamedPipeClientStream(".", "C_Sharp2", PipeDirection.InOut))
        {
            Console.WriteLine("Client is connecting...");
            pipeClient.Connect();

            Console.WriteLine("Введите данные и приоритет (Ctrl+C для завершения).");

            while (true)
            {
                try
                {
                    int data = int.Parse(Console.ReadLine());
                    bool result = bool.Parse(Console.ReadLine());
                    Message sentMessage = new Message { Data = data, Result = result };
                    WriteMessage(pipeClient, sentMessage);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Ошибка при вводе данных: {e.Message}");
                }
            }
        }

        Console.WriteLine("Client's work is done");
    }

    static void WriteMessage(NamedPipeClientStream pipeStream, Message message)
    {
        byte[] buffer = new byte[Unsafe.SizeOf<Message>()];

        MemoryMarshal.Write(buffer, ref message);

        pipeStream.Write(buffer, 0, buffer.Length);
        pipeStream.Flush();
    }
}
