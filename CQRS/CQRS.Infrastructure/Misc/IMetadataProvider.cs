using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Misc
{
    public interface IMetadataProvider
    {
        /// <summary>
        /// Gets the metadata associated with the payload, which can be used by
        /// processors to filter and selectively subscribe to messages.
        /// </summary>
        IDictionary<string, string> GetMetaData(object payload);
    }
}
