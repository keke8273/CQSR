using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Messaging
{
    using System;

    interface ICommand
    {
        /// <summary>
        /// Gets the command identifier.
        /// </summary>
        public Guid Id { get; }
    }
}
