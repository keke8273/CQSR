using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Sql.MessageLog
{
    public class MessageLogDbContext : DbContext
    {
        public const string SchemaName = "MessageLog";

        public MessageLogDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MessageLogEntity>().ToTable("Messages", SchemaName);
        }
    }
}
