using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRS.Infrastructure.Serialization;

namespace CQRS.Infrastructure.Sql.Messaging.Handling
{
    public abstract class MessageProcessor : IProcessor, IDisposable
    {
        private readonly IMessageReceiver receiver;
        private readonly ITextSerializer serializer;
        private readonly object lockObject = new object();
        private bool disposed;
        private bool started = false;

        protected MessageProcessor(IMessageReceiver receiver, ITextSerializer serializer)
        {
            this.receiver = receiver;
            this.serializer = serializer;
        }

        public virtual void Start()
        {
            ThrowIfDisposed();
            lock (lockObject)
            {
                if (!started)
                {
                    receiver.MessageReceived += OnMessageReceived;
                    receiver.Start();
                    started = true;
                }
            }
        }

        public virtual void Stop()
        {
            lock (lockObject)
            {
                if (started)
                {
                    receiver.Stop();
                    receiver.MessageReceived -= OnMessageReceived;
                    started = false;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void ProcessMessage(object payload, string correlationId);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Stop();
                    disposed = true;

                    using (receiver as IDisposable)
                    {
                        //Dispose receiver if it is disposable
                    }
                }
            }
        }

        ~MessageProcessor()
        {
            Dispose(false);
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            Trace.WriteLine(new string('-', 100));

            try
            {
                var body = Deserialize(args.Message.Body);
                TracePayload(body);
                Trace.WriteLine("");

                ProcessMessage(body, args.Message.CorrelationId);
                Trace.WriteLine(new string('-', 100));
            }
            catch (Exception e)
            {
                Trace.TraceError("An exception happened while processing message through handler/s:\r\n{0}", e);
                Trace.TraceWarning("Error will be ignored and message receiving will continue.");
            }
        }

        protected object Deserialize(string serializedPayload)
        {
            using (var reader = new StringReader(serializedPayload))
            {
                return serializer.Deserialize(reader);
            }
        }

        protected string Serialize(object payload)
        {
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, payload);
                return writer.ToString();
            }
        }

        private void ThrowIfDisposed()
        {
            if(disposed)
                throw new ObjectDisposedException("MessageProcessor");
        }

        [Conditional("TRACE")]
        private void TracePayload(object payload)
        {
            Trace.WriteLine(Serialize(payload));
        }
        
    }
}
