using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Data;
using System.Collections.Generic;

struct Message
{
    public bool Result { get; set; }
    public int Data { get; set; }
}

class PipeServer
{
    static void Main(string[] args)
    {
        using (var pipeServer = new NamedPipeServerStream("C_Sharp2"))
        {
            Console.WriteLine("Server is waiting for a connection...");
            pipeServer.WaitForConnection();

            // создаем очередь для данных с приоритетом
            PriorityQueue<Message, int> dataQueue = new PriorityQueue<Message, int>();

            // поток для обработки данных
            Thread dataProcessingThread = new Thread(() =>
            {
                while (true)
                {
                    if (dataQueue.Count > 0)
                    {
                        var message = dataQueue.Dequeue();
                        Console.WriteLine($"Result = {message.Result}, Data = {message.Data}");
                    }
                }
            });

            dataProcessingThread.Start();

            Console.WriteLine("Введите данные и приоритет (Ctrl+C для завершения).");

            // Обработка Ctrl+C
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Предотвращаем завершение по Ctrl+C
                dataProcessingThread.Join(); // Дожидаемся завершения потока обработки данных
                pipeServer.Close(); // Закрываем канал перед завершением
                Console.WriteLine("Server's work is done");
                Environment.Exit(0);
            };

            while (true)
            {
                try
                {
                    int data = int.Parse(Console.ReadLine());
                    bool result = bool.Parse(Console.ReadLine());
                    Message receivedMessage = new Message { Data = data, Result = result };

                    int priority = 1;

                    dataQueue.Enqueue(receivedMessage, priority);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Ошибка при вводе данных: {e.Message}");
                }
            }
        }
    }
}
