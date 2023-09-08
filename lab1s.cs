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
        Message message = new Message();
        message.data = 10;
        message.result = false;
        using (var newServer = new NamedPipeServerStream("Pipe_lab1"))
        {
                        
            Console.WriteLine("Server is working...");
            
            //struct to byte
            byte[] bytes= new byte[1024];
            var sendingMessage = getBytes(message); 
            Console.WriteLine("{0}, {1}", message.data, message.result);  
            newServer.WaitForConnection();  
            newServer.Write(sendingMessage);
            newServer.Read(bytes);
            var newMessage = fromBytes(bytes);
            Console.WriteLine("{0}", newMessage.result);

            
        }
        Console.WriteLine("Server's work is done");
        Console.ReadLine();
    }
}
