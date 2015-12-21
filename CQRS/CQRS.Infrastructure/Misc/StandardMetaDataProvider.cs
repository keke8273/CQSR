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
        public virtual IDictionary<string, string> GetMetaData(object payload)
        {
            var metadata = new Dictionary<string, string>();

            var type = payload.GetType();

            metadata[StandardMetaData.AssemblyName] = Path.GetFileNameWithoutExtension(type.Assembly.ManifestModule.FullyQualifiedName);
            metadata[StandardMetaData.FullName] = type.FullName;
            metadata[StandardMetaData.Namespace] = type.Namespace;
            metadata[StandardMetaData.TypeName] = type.Name;

            var e = payload as IEvent;

            if(e != null)
            {
                metadata[StandardMetaData.SourceId] = e.SourceId.ToString();
                metadata[StandardMetaData.Kind] = StandardMetaData.EventKind;
            }

            var c = payload as ICommand;

            if (c != null)
            {
                metadata[StandardMetaData.Kind] = StandardMetaData.CommandKind;
            }

            return metadata;
        }
    }
}
