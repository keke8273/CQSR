using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Serialization
{
    public class JsonTextSerializer : ITextSerializer
    {
        private readonly JsonSerializer serializer;

        public JsonTextSerializer()
        {
            serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
                });
        }

        public JsonTextSerializer(JsonSerializer serializer)
        {
            this.serializer = serializer;
        }

        public void Serialize(TextWriter writer, object graph)
        {
            var jsonWriter = new JsonTextWriter(writer);
#if DEBUG
            jsonWriter.Formatting = Formatting.Indented;
#endif
            serializer.Serialize(jsonWriter, graph);

            //Dispose the jsonWriter here will dispose the writer too.  
            writer.Flush();
        }

        public object Deserialize(TextReader reader)
        {
            var jsonReader = new JsonTextReader(reader);

            try
            {
                return serializer.Deserialize(jsonReader);
            }
            catch (JsonSerializationException e)
            {

                throw new SerializationException(e.Message, e);
            }
        }
    }
}
