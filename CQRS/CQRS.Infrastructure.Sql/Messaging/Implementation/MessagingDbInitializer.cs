using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Sql.Messaging.Implementation
{
    public class MessagingDbInitializer
    {
        public static void CreateDatabaseObjects(string connectionString, string schema, bool createDatabase = false)
        {
            if (createDatabase)
            {
                
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = string.Format(CultureInfo.InvariantCulture,
                        @"
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'{0}')
EXECUTE sp_executesql N'CREATE SCHEMA [{0}] AUTHORIZATION [dbo]';
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[Commands]') AND type in (N'U'))
CREATE TABLE [{0}].[Commands](
    [Id] [bigint] IDENTITY(1,1) NOT NULL,
    [Body] [nvarchar](max) NOT NULL,
    [DeliveryDate] [datetime] NULL,
    [CorrelationId] [nvarchar](max) NULL,
 CONSTRAINT [PK_{0}.Commands] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY];
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[Events]') AND type in (N'U'))
CREATE TABLE [{0}].[Events](
    [Id] [bigint] IDENTITY(1,1) NOT NULL,
    [Body] [nvarchar](max) NOT NULL,
    [DeliveryDate] [datetime] NULL,
    [CorrelationId] [nvarchar](max) NULL,
 CONSTRAINT [PK_{0}.Events] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY];
", schema);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
