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
    private static async Task Main(string[] args)
    {
        using (var pipeClient = new NamedPipeClientStream(".", "MyNamedPipe", PipeDirection.InOut))
        {
            try
            {
                Console.WriteLine("Client is connecting...");
                pipeClient.Connect();

                Console.CancelKeyPress += (sender, e) => //обработчик для Ctrl+C
                {
                    e.Cancel = true; 

                    pipeClient.Close();
                };

                while (true)
                {
                    Message receivedMessage = ReadMessage(pipeClient);
                    Console.WriteLine("Received Message: Value A = {0}, Value B = {1}, Priority = {2}", receivedMessage.valueA, receivedMessage.valueB, receivedMessage.Priority);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        Console.WriteLine("Client's work is done");
    }
    private static Message ReadMessage(NamedPipeClientStream pipeStream)
    {
        byte[] buffer = new byte[Unsafe.SizeOf<Message>()];
        pipeStream.Read(buffer, 0, buffer.Length);
        return MemoryMarshal.Read<Message>(buffer);
    }

}
