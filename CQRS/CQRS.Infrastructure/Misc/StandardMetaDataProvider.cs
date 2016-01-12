using CQRS.Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Misc
{
    public class StandardMetaDataProvider: IMetadataProvider
    {
        public virtual IDictionary<string, string> GetMetadata(object payload)
        {
            var metadata = new Dictionary<string, string>();

            var type = payload.GetType();

            metadata[StandardMetadata.AssemblyName] = Path.GetFileNameWithoutExtension(type.Assembly.ManifestModule.FullyQualifiedName);
            metadata[StandardMetadata.FullName] = type.FullName;
            metadata[StandardMetadata.Namespace] = type.Namespace;
            metadata[StandardMetadata.TypeName] = type.Name;

            var e = payload as IEvent;

            if(e != null)
            {
                metadata[StandardMetadata.SourceId] = e.SourceId.ToString();
                metadata[StandardMetadata.Kind] = StandardMetadata.EventKind;
            }

            var c = payload as ICommand;

            if (c != null)
            {
                metadata[StandardMetadata.Kind] = StandardMetadata.CommandKind;
            }

            return metadata;
        }
    }
}
