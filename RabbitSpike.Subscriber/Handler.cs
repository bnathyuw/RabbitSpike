using System;

namespace RabbitSpike.Subscriber
{
    internal class Handler : IHandler
    {
        private readonly string _labelName;

        public Handler(string labelName)
        {
            _labelName = labelName;
        }

        public void Handle(Message message)
        {
            if (message.LabelName == _labelName)
                Console.WriteLine(message.Value);
        }
    }
}