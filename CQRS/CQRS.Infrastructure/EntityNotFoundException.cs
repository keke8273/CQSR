using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure
{
    [Serializable]
    public class EntityNotFoundException : Exception
    {
        private readonly Guid entityId;
        private readonly string entityType;

        public EntityNotFoundException()
        {
        }

        public EntityNotFoundException(Guid entityId) : base(entityId.ToString())
        {
            this.entityId = entityId;
        }

        public EntityNotFoundException(Guid entityId, string entityType) :
            base(entityType + ": " + entityId.ToString())
        {
            this.entityId = entityId;
            this.entityType = entityType;
        }

        public EntityNotFoundException(Guid entityId, string entityType, string message, Exception inner)
            : base(message, inner)
        {
            this.entityId = entityId;
            this.entityType = entityType;
        }

        //Deserialization
        protected EntityNotFoundException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            entityId = Guid.Parse(info.GetString("entityId"));
            entityType = info.GetString("entityType");
        }

        public Guid EntityId
        {
            get { return entityId; }
        }

        public string EntityType
        {
            get { return entityType; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("entityId", entityId.ToString());
            info.AddValue("entityType", entityType);
        }
    }
}
