﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace CQRS.Infrastructure.Sql.Messaging
{
    public class MessageSender : IMessageSender
    {
        private readonly IDbConnectionFactory connectionFactory;
        private readonly string name;
        private readonly string insertQuery;

        public MessageSender(IDbConnectionFactory connectionFactory, string name, string tableName)
        {
            this.connectionFactory = connectionFactory;
            this.name = name;
            insertQuery = string.Format("INSERT INTO {0} (Body, DeliveryDate, CorrelationId) VALUES (@Body, @DeliveryDate, @CorrelationId)", tableName);
        }

        public void Send(Message message)
        {
            using (var connection = connectionFactory.CreateConnection(name))
            {
                connection.Open();

                InsertMessage(message, connection);
            }
        }

        public void Send(IEnumerable<Message> messages)
        {
            //todo:: maybe switch to Database.BeginTransaction API since we are uisng EF 6.0
            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                using (var connection = connectionFactory.CreateConnection(name))
                {
                    connection.Open();

                    foreach (var message in messages)
                    {
                        InsertMessage(message, connection);
                    }
                }

                scope.Complete();
            }
        }

        private void InsertMessage(Message message, DbConnection connection)
        {
            using (var command = (SqlCommand)connection.CreateCommand())
            {
                command.CommandText = insertQuery;
                command.CommandType = CommandType.Text;

                command.Parameters.Add("@Body", SqlDbType.NVarChar).Value = message.Body;
                command.Parameters.Add("@DeliveryDate", SqlDbType.DateTime).Value = message.DeliveryDate.HasValue ? (object)message.DeliveryDate.Value : DBNull.Value;
                command.Parameters.Add("@CorrelationId", SqlDbType.NVarChar).Value = (object)message.CorrelationId ?? DBNull.Value;

                command.ExecuteNonQuery();
            }
        }
    }
}
