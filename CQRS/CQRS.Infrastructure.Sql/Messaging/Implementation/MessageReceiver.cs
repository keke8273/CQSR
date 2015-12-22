using System;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Sql.Messaging.Implementation
{
    public class MessageReceiver : IMessageReceiver, IDisposable
    {
        private readonly IDbConnectionFactory connectionFactory;
        private readonly string name;
        private readonly string readQuery;
        private readonly string deleteQuery;
        private readonly TimeSpan pollingDelay;
        private readonly object lockObject = new object();
        private CancellationTokenSource cancellationSource;

        public MessageReceiver(IDbConnectionFactory connectionFactory, string name, string tableName)
            : this(connectionFactory, name, tableName, TimeSpan.FromMilliseconds(100))
        {
        }

        public MessageReceiver(IDbConnectionFactory connectionFactory, string name, string tableName, TimeSpan pollingDelay)
        {
            this.connectionFactory = connectionFactory;
            this.name = name;
            this.pollingDelay = pollingDelay;

            readQuery =
                string.Format(CultureInfo.InvariantCulture,
                @"SELECT TOP (1)
                {0}.[Id] AS [Id],
                {0}.[Body] AS [Body],
                {0}.[DeliveryDate] AS [DeliveryDate],
                {0}.[CorrelationId] AS [CorrelationId]
                FROM {0} WITH (UPDLOCK, READPAST)
                WHERE ({0}.[DeliveryDate] IS NULL) OR ({0}.[DeliveryDate] <= @CurrentDate)
                ORDER BY {0}.[Id] ASC", tableName
                );

            deleteQuery =
                string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE Id = @Id", tableName);
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = (sender, args) => { };

        public void Start()
        {
            lock (lockObject)
            {
                if(cancellationSource == null)
                {
                    cancellationSource = new CancellationTokenSource();
                    Task.Factory.StartNew(
                        () => ReceiveMessages(cancellationSource.Token),
                        cancellationSource.Token,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Current);
                }
            }
        }

        public void Stop()
        {
            lock (lockObject)
            {
                using (cancellationSource)
                {
                    if(cancellationSource != null)
                    {
                        cancellationSource.Cancel();
                        cancellationSource = null;
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Stop();
        }

        ~MessageReceiver()
        {
            Dispose(false);
        }

        private void ReceiveMessages(CancellationToken cancellationToken)
        {
            while (!cancellationSource.IsCancellationRequested)
            {
                if(!ReceiveMessage())
                {
                    Thread.Sleep(pollingDelay);
                }
            }
        }

        protected bool ReceiveMessage()
        {
            using (var connection = connectionFactory.CreateConnection(name))
            {
                var currentDate = GetCurrentDate();

                connection.Open();

                using (var transacation = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        long messageId = -1;
                        Message message;
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transacation;
                            command.CommandType = CommandType.Text;
                            command.CommandText = readQuery;
                            ((SqlCommand) command).Parameters.Add("@CurrentDate", SqlDbType.DateTime).Value =
                                currentDate;

                            using (var reader = command.ExecuteReader())
                            {
                                if (!reader.Read())
                                    return false; //no new message is received
                                var body = (string) reader["Body"];
                                var deliveryDateValue = reader["DeliveryDate"];
                                var deliveryDate = deliveryDateValue == DBNull.Value
                                    ? (DateTime?) null
                                    : new DateTime?((DateTime) deliveryDateValue);
                                var correlationIdValue = reader["CorrelationId"];
                                var correlationId =
                                    (string) (correlationIdValue == DBNull.Value ? null : correlationIdValue);
                                message = new Message(body, deliveryDate, correlationId);
                                messageId = (long) reader["Id"];
                            }
                        }

                        MessageReceived(this, new MessageReceivedEventArgs(message));

                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transacation;
                            command.CommandType = CommandType.Text;
                            command.CommandText = deleteQuery;
                            ((SqlCommand) command).Parameters.Add("@Id", SqlDbType.BigInt).Value = messageId;

                            command.ExecuteNonQuery();
                        }
                        transacation.Commit();
                    }
                    catch (Exception)
                    {
                        transacation.Rollback();
                        throw;
                    }
                }
            }

            return true;
        }

        protected virtual DateTime GetCurrentDate()
        {
            return DateTime.UtcNow;
        }
    }
}
