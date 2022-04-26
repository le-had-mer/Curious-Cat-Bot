using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Curious_Cat
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter your Discord bot token: ");

            var bot = new CatBot(Console.ReadLine());

            bot.RunAsync().GetAwaiter().GetResult();

            Console.WriteLine("Bye World!");
           
            Console.ReadKey();
        }
    }
}
