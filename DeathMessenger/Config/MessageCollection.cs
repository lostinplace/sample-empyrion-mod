using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeathMessagesModule.Config
{
    public class MessageCollection : List<Message>
    {
        Dictionary<Int32, Int32> lastMessageCounter;

        public MessageCollection() : base()
        {
            lastMessageCounter = new Dictionary<Int32, Int32>();
        }

        public String GetNextMessage(Int32 messageType)
        {
            // If there is no message available, use the default
            if (this.Where(m => m.MessageType == messageType).Count() == 0)
                messageType = 0;

            // Create a counter if we don't have one.
            if (lastMessageCounter.Where(l => l.Key == messageType).Count() == 0)
            {
                lastMessageCounter.Add(messageType, -1);
            }

            // Get and calculate the next message.
            var lastCount = lastMessageCounter.FirstOrDefault(l => l.Key == messageType).Value;
            lastCount++;

            if (this.Where(m => m.MessageType == messageType).Count() <= lastCount)
            {
                lastCount = 0;
            }

            lastMessageCounter[messageType] = lastCount;

            // Return the message
            return this.Where(m => m.MessageType == messageType).Skip(lastCount).FirstOrDefault().MessageTemplate;
        }
    }
}