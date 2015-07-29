using System;

namespace RabbitSpike.Subscriber
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter the label name:");

            var labelName = Console.ReadLine();
            using (var runner = new Runner(labelName, new Handler(labelName)))
            {
                runner.Run();
                Console.ReadKey();
            }
        }
    }

    internal struct Message
    {
        public string LabelName { get; set; }
        public string Value { get; set; }
    }
}
