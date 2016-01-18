using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Sql.Processes
{
    public class UndispatchedMessages
    {
        public UndispatchedMessages(Guid id)
        {
            this.Id = id;
        }

        public Guid Id { get; private set; }

        public string Commands { get; set; }
    }
}
