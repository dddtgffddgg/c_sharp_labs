using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
struct Message
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
    public string Buffer;
    public bool result;
    public int data;
}
class PipeClient
{
    static void Main(string[] args)
    {
        Message fromBytes(byte[] arr)
{
    Message str = new Message();
    int size = Marshal.SizeOf(str);
    IntPtr ptr = IntPtr.Zero;
    try
    {
        ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(arr, 0, ptr, size);
        str = (Message)Marshal.PtrToStructure(ptr, str.GetType());
    }
    finally
    {
        Marshal.FreeHGlobal(ptr);
    }
    return str;
}
byte[] getBytes(Message str) {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];
        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return arr;
}
        
        using (var newClient = new NamedPipeClientStream("Pipe_lab1"))
        {
            byte[] bytes= new byte[1024];
            Console.WriteLine("Client is working...");
            newClient.Connect();
            newClient.Read(bytes);
        
            var newMessage = fromBytes(bytes);
            Console.WriteLine("{0}", newMessage.result);
            newMessage.result = true;
            bytes = getBytes(newMessage);
            newClient.Write(bytes);
            Console.WriteLine("{0}", newMessage.result);
            Console.WriteLine("{0}", newMessage.data);

        }
        Console.WriteLine("Client's work is done");
        Console.ReadLine();
    }
}   
