using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Misc
{
    public static class StandardMetaData
    {
        public const string EventKind = "Event";

        public const string CommandKind = "Command";

        public const string Kind = "Kind";

        public const string SourceId = "SourceId";

        public const string AssemblyName = "AssemblyName";

        public const string Namespace = "Namespace";

        public const string FullName = "FullName";

        public const string TypeName = "TypeName";

        public const string SourceType = "SourceType";
    }
}
