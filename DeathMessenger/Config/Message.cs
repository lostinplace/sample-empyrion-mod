using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeathMessagesModule.Config
{
    public class Message
    {
        public Byte MessageType { get; set; }
        public String MessageTemplate { get; set; }

        public Message(Byte messageType, String messageTemplate)
        {
            MessageType = messageType;
            MessageTemplate = messageTemplate;
        }
    }
}
