using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet
{
    public static class YAMLSerializationHelper
    {
        public static T DeSerialize<T>(string Document)
        {
            var input = new StringReader(Document);

            var deserializer = new DeserializerBuilder()
              .Build();

            T result = deserializer.Deserialize<T>(input);

            input.Close();

            return result;
        }


        public static string Serialize(System.Object graph) 
        {
            var serializer = new SerializerBuilder().Build();

            var yaml = serializer.Serialize(graph);
            
            return yaml;
        }
    }
}
