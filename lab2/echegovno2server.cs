using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

struct Message
{
    public int valueA { get; set; }
    public int valueB { get; set; }
    public int Priority { get; set; }
}

class PipeServer
{
    static async Task Main(string[] args)
    {
        using (var pipeServer = new NamedPipeServerStream("C_Sharp2"))
        {
            Console.WriteLine("Server is waiting for a connection...");
            await pipeServer.WaitForConnectionAsync();

            PriorityQueue<Message> messageQueue = new PriorityQueue<Message>((m1, m2) => m1.valueA.CompareTo(m2.valueA));
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            // Start a background task to process messages
            Task processingTask = Task.Run(() => ProcessMessages(pipeServer, messageQueue, cancellationTokenSource.Token));

            Console.WriteLine("Для выхода нажмите Ctrl+C.");

            while (true)
            {
                // Read user input and add messages to the queue with priority
                Console.Write("Введите данные: ");
                int valueB = int.Parse(Console.ReadLine());
                int valueA = int.Parse(Console.ReadLine());

                Message newMessage = new Message { valueA = valueA, valueB = valueB };
                messageQueue.Enqueue(newMessage);
            }
        }
    }

    static async Task ProcessMessages(NamedPipeServerStream pipeStream, PriorityQueue<Message> messageQueue, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            while (messageQueue.Count > 0)
            {
                Message message = messageQueue.Dequeue();
                Console.WriteLine($"Sending valueA = {message.valueA}, valueB = {message.valueB}");
                await WriteMessageAsync(pipeStream, message);
            }
        }
        Console.WriteLine("Server's work is done");
    }

    static async Task WriteMessageAsync(NamedPipeServerStream pipeStream, Message message)
    {
        byte[] buffer = new byte[Unsafe.SizeOf<Message>()];

        MemoryMarshal.Write(buffer, ref message);

        await pipeStream.WriteAsync(buffer, 0, buffer.Length);
        await pipeStream.FlushAsync();
    }
}

class PriorityQueue<T>
{
    private List<T> list;
    private Comparison<T> comparison;

    public PriorityQueue(Comparison<T> comparison)
    {
        this.list = new List<T>();
        this.comparison = comparison;
    }

    public int Count => list.Count;

    public void Enqueue(T item)
    {
        list.Add(item);
        list.Sort(comparison);
    }

    public T Dequeue()
    {
        if (list.Count == 0)
            throw new InvalidOperationException("Queue is empty");
        T item = list[0];
        list.RemoveAt(0);
        return item;
    }
}
