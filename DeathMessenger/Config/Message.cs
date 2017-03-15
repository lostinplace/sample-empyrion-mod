using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeathMessagesModule.Config
{
    public class Message
    {
        public Int32 MessageType { get; set; }
        public String MessageTemplate { get; set; }

        public Message(Int32 messageType, String messageTemplate)
        {
            MessageType = messageType;
            MessageTemplate = messageTemplate;
        }
    }
}
