using System.IO;

namespace CQRS.Infrastructure.Serialization
{
    public interface ITextSerializer
    {
        void Serialize(TextWriter writer, object graph);

        object Deserialize(TextReader reader);
    }
}
