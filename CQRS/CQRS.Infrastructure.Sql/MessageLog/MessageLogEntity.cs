using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Sql.MessageLog
{
    public class MessageLogEntity
    {
        public Guid Id { get; set; }
        public string Kind { get; set; }
        public string SourceId { get; set; }
        public string AssemblyName { get; set; }
        public string Namespace { get; set; }
        public string FullName { get; set; }
        public string TypeName { get; set; }
        public string SourceType { get; set; }
        public string CreationDate { get; set; }
        public string Payload { get; set; }
    }
}
