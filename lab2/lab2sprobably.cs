using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

struct Message
{
    public int valueA { get; set; }
    public int valueB { get; set; }
    public int Priority { get; set; }
}
//добавить в очередь и после этого добавить две асинхронные задачи 
class PipeServer
{
    static List<Message> messageBuffer = new List<Message>();

    static void Main(string[] args)
    {
        using (var pipeServer = new NamedPipeServerStream("C_Sharp2"))
        {
            Console.WriteLine("Server is waiting for a connection...");
            pipeServer.WaitForConnection();

            Console.WriteLine("Введите данные и приоритет (Ctrl+C для завершения).");

            
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; 
                pipeServer.Close(); 

                Console.WriteLine("Saving valueB to file or displaying on screen:");

                messageBuffer = messageBuffer.OrderByDescending(message => message.Priority).ToList();

                foreach (var message in messageBuffer)
                {
                    Console.WriteLine($"valueA = {message.valueA}, valueB = {message.valueB}, Priority = {message.Priority}");
                }

                Environment.Exit(0);
            };

            while (true)
            {
                try
                {
                    int valueB = int.Parse(Console.ReadLine());
                    int valueA = int.Parse(Console.ReadLine());

                    int priority = int.Parse(Console.ReadLine());

                    Message receivedMessage = new Message { valueB = valueB, valueA = valueA, Priority = priority };
                    messageBuffer.Add(receivedMessage);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Ошибка при вводе данных: {e.Message}");
                }
            }
        }
    }
}
