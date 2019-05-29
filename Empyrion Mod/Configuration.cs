using System;
using System.IO;
using YamlDotNet.Serialization;

public class Configuration
{
    public string GameServerIp { get; set; }
    public int GameServerApiPort { get; set; }

    public Configuration()
    {
        GameServerIp = "127.0.0.1";
        GameServerApiPort = 12345;
    }

    public static Configuration GetConfiguration(String filePath)
    {
        using (var input = File.OpenText(filePath))
        {
            return (new Deserializer()).Deserialize<Configuration>(input);
        }
    }
}
