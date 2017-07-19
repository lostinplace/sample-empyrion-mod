using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace DeathMessagesModule.Config
{
    public class Message
    {
        [YamlMember(Alias = "DeathType")]
        public Int32 MessageType { get; set; }
        [YamlMember(Alias = "Message", SerializeAs = typeof(String))]
        public String MessageTemplate { get; set; }

        public Message() { }
        public Message(Int32 messageType, String messageTemplate)
        {
            MessageType = messageType;
            MessageTemplate = messageTemplate;
        }
    }
}