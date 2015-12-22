using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Sql.Messaging.Implementation
{
    public class MessageReceiver:IMessageReceiver, IDisposable
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

            this.readQuery =
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

            this.deleteQuery =
                string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE Id = @Id", tableName);
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = (sender, args) => { };

        public void Start()
        {
            lock (this.lockObject)
            {
                if(cancellationSource == null)
                {
                    cancellationSource = new CancellationTokenSource();
                    Task.Factory.StartNew(
                        () => this.ReceiveMessages(this.cancellationSource.Token),
                        this.cancellationSource.Token,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Current);
                }
            }
        }

        public void Stop()
        {
            lock (lockObject)
            {
                using (this.cancellationSource)
                {
                    if(this.cancellationSource != null)
                    {
                        this.cancellationSource.Cancel();
                        this.cancellationSource = null;
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
            this.Stop();
        }

        ~MessageReceiver()
        {
            Dispose(false);
        }

        private void ReceiveMessages(CancellationToken cancellationToken)
        {
            while (!cancellationSource.IsCancellationRequested)
            {
                if(!this.ReceiveMessage())
                {
                    Thread.Sleep(this.pollingDelay);
                }
            }
        }

        protected bool ReceiveMessage()
        {
            using (var connection = this.connectionFactory.CreateConnection(this.name))
            {
                var currentDate = GetCurrentDate();

                connection.Open();

                using (var transacation = )
                {
                    
                }
            }
        }
    }
}
