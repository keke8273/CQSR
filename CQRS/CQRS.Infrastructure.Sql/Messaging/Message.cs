using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRS.Infrastructure.Sql.Messaging
{
    public class Message
    {
        public Message(string body, DateTime? deliveryDate = null, string correlationId = null)
        {
            this.Body = body;
            DeliveryDate = deliveryDate;
            CorrelationId = correlationId;
        }

        /// <summary>
        /// Gets or sets the body, where it is the serialized event or command.
        /// </summary>
        public string Body { get; private set; }

        public string CorrelationId { get; private set; }

        public DateTime? DeliveryDate { get; private set; }


    }
}
