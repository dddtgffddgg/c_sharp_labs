using System;
using System.IO.Pipes;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Добавляем пространство имен для работы с задачами

struct Message
{
    public int valueA { get; set; }
    public int valueB { get; set; }
    public int Priority { get; set; }
}

class PipeServer
{
    static List<Message> messageBuffer = new List<Message>();

    async static Task Main(string[] args) // Метод Main теперь асинхронный
    {
        using (var pipeServer = new NamedPipeServerStream("C_Sharp2"))
        {
            Console.WriteLine("Server is waiting for a connection...");
            await pipeServer.WaitForConnectionAsync(); // Используем асинхронный метод ожидания подключения

            Console.WriteLine("Введите данные и приоритет (Ctrl+C для завершения).");

            Console.CancelKeyPress += async (sender, e) =>
            {
                e.Cancel = true;
                pipeServer.Close();

                Console.WriteLine("Сохранение valueB в файл или отображение на экране:");

                messageBuffer = messageBuffer.OrderByDescending(message => message.Priority).ToList();

                foreach (var message in messageBuffer)
                {
                    Console.WriteLine($"valueA = {message.valueA}, valueB = {message.valueB}, Priority = {message.Priority}");
                }

                await Task.Delay(1000); // Ожидаем 1 секунду перед завершением программы
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
        messageBuffer.Add(receivedMessage);
    }
}
