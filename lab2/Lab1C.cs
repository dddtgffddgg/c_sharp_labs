using System;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text;


/*struct Message
{
    public int valueA { get; set; }
    public int valueB { get; set; }
}*/

class PipeClient
{
    public static Mutex mutex = new Mutex();

    public struct Home
    {
        public Home(int n, int s)
        {
            valueA = n;
            valueB = s;
        }
        public int valueA { get; set; } 
        public int valueB { get; set; }
    }
         
    private static async Task Main()
    {
        string filePath = "C:\\Users\\diana\\C_sharp_labs\\Lab2.txt";

        using (var pipeClient = new NamedPipeClientStream(".", "MyNamedPipe", PipeDirection.InOut))
        {
            try
            {
                await pipeClient.ConnectAsync();
                Console.WriteLine("The client is connecting...");
                
                while (true)
                {

                    byte[] messageBuffer = new byte[Unsafe.SizeOf<Home>()];
                    await pipeClient.ReadAsync(messageBuffer);

                    Home receivedWork = MemoryMarshal.Read<Home>(messageBuffer);
                    Console.WriteLine("Received from server: {0} and {1}.", receivedWork.valueA, receivedWork.valueB);

                    Console.WriteLine("End of data transmission...");

                    await pipeClient.WriteAsync(messageBuffer);

                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            /*Console.WriteLine("The client is connecting...");
            await pipeClient.ConnectAsync();

            var dataBuffer = new StringWriter();

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
            };

            while (true)
            {
                try
                {
                    Message receivedMessage = ReadMessage(pipeClient);
                    Console.WriteLine("Received Message: Value A = {0}, Value B = {1}", receivedMessage.valueA, receivedMessage.valueB);

                }
                catch (InvalidDataException ex)
                {
                    Console.WriteLine("Error reading the message: " + ex.Message);
                }
                
            }

            Console.WriteLine("The client's work is completed");
        }
    }

    static Message ReadMessage(NamedPipeClientStream pipeStream)
    {
        while (true) 
        {
            try 
            {
                var message = new Message();//try - catch Exepcion
                var buffer = new byte[Marshal.SizeOf(message)];
                int bytesRead = pipeStream.Read(buffer, 0, buffer.Length);

                if (bytesRead == buffer.Length)
                
                {
                    message.valueA = BitConverter.ToInt32(buffer, 0);
                    message.valueB = BitConverter.ToInt32(buffer, sizeof(int));
                    return message;
                }

                else

                {
                    throw new InvalidOperationException("Failed to read the message.");
                }

            } 
            catch(InvalidDataException ex)
            {
                Console.WriteLine("Error reading the message: " + ex.Message);
            }*/
            
        }
    }
}
