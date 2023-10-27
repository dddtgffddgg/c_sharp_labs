using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

struct Message
{
    public bool Result { get; set; }
    public int Data { get; set; }
    public int Priority { get; set; }
}

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

            // Обработка Ctrl+C
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Предотвращаем завершение по Ctrl+C
                pipeServer.Close(); // Закрываем канал перед завершением

                Console.WriteLine("Saving data to file or displaying on screen:");

                // Отсортировать данные по приоритету (по убыванию)
                messageBuffer = messageBuffer.OrderByDescending(message => message.Priority).ToList();

                // Вывести данные на экран
                foreach (var message in messageBuffer)
                {
                    Console.WriteLine($"Result = {message.Result}, Data = {message.Data}, Priority = {message.Priority}");
                }

                // Здесь можно добавить код для записи данных в файл
                Environment.Exit(0);
            };

            while (true)
            {
                try
                {
                    int data = int.Parse(Console.ReadLine());
                    bool result = bool.Parse(Console.ReadLine());

                    int priority = int.Parse(Console.ReadLine());

                    Message receivedMessage = new Message { Data = data, Result = result, Priority = priority };
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
