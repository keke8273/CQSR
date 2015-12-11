using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRS.Infrastructure.Messaging
{
    public abstract class Envelope
    {
        public static Envelope<T> Create<T>(T body)
        {
            return new Envelope<T>(body);
        }
    }

    /// <summary>
    /// Provides the envelop for an object that will be sent to a bus.
    /// </summary>
    public class Envelope<T> : Envelope
    {
        public Envelope(T body)
        {
            this.Body = body;
        }

        public T Body { get; set; }

        /// <summary>
        /// Gets or sets the delay for sending, enqueing or processing the body.
        /// </summary>
        public TimeSpan Delay { get; set; }

        public TimeSpan TimeToLive { get; set; }

        //todo::why is this a string, shouldnt it be GUID?
        public string CorrelationId { get; set; }

        //todo::why is this a string, shouldnt it be GUID?
        public string MessageId { get; set; }

        public static implicit operator Envelope<T>(T body)
        {
            return Envelope.Create(body);
        }
    }
}
