using System;
using System.IO.Pipes;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

struct Message
{
    public int valueA { get; set; }
    public int valueB { get; set; }
    public int Priority { get; set; }
}

class PipeServer
{
    static PriorityQueue<Message, int> messageQueue = new PriorityQueue<Message, int>();

    async static Task Main(string[] args)
    {
        using (var pipeServer = new NamedPipeServerStream("C_Sharp2"))
        {
            Console.WriteLine("Server is waiting for a connection...");
            await pipeServer.WaitForConnectionAsync(); //асинхронное подключение

            Console.WriteLine("Введите данные и приоритет (Ctrl+C для завершения).");

            Console.CancelKeyPress += async (sender, e) =>
            {
                e.Cancel = true;
                pipeServer.Close();

                Console.WriteLine("Сохранение valueB в файл или отображение на экране:");

                var sortedMessage = messageQueue.OrderBy(message => message.Priority).ToList();

                foreach (var message in sortedMessage)
                {
                    Console.WriteLine($"valueA = {message.valueA}, valueB = {message.valueB}, Priority = {message.Priority}");
                }

                await Task.Delay(1000); //для того чтобы данные успели сохраниться
                Environment.Exit(0);
            };

            while (true)
            {
                try
                {
                    int valueA = int.Parse(Console.ReadLine());
                    int valueB = int.Parse(Console.ReadLine());
                    int priority = int.Parse(Console.ReadLine());

                    await AddMessageToQueueAsync(valueA, valueB, priority);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Ошибка при вводе данных: {e.Message}");
                }
            }
        }
    }

    static async Task AddMessageToQueueAsync(int valueA, int valueB, int priority)
    {
        Message receivedMessage = new Message { valueA = valueA, valueB = valueB, Priority = priority };
        messageQueue.Enqueue()(receivedMessage);
    }
}
