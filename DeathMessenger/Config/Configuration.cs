using System;
using System.IO;
using YamlDotNet.Serialization;

namespace DeathMessagesModule.Config
{
    public class Configuration
    {
        public Boolean MessageInChat { get; set; }
        public MessageCollection Messages { get; set; }

        public Configuration()
        {
            Messages = new MessageCollection();
        }

        public static Configuration GetConfiguration(String filePath)
        {
            var input = File.OpenText(filePath);

            var deserializer = new Deserializer();

            return deserializer.Deserialize<Configuration>(input);
        }
    }
}