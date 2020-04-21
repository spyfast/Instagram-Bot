using System;
namespace InstaBot.Config
{
    public class Log
    {
        public static void Push(string message)
        {
            Console.WriteLine($"[{DateTime.Now}]: {message}");
        }
    }
}
