﻿using System;
using System.Data.Entity;

namespace CQRS.Infrastructure.Sql.EventSourcing
{
    public class EventStoreDbContext : DbContext
    {
        public const string SchemaName = "Event";

        public EventStoreDbContext(string nameOrConnectionString) :
            base(nameOrConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Event>().HasKey( x=> new {x.AggregateId, x.AggregateType, x.Version}).ToTable("Events", SchemaName);
        }
    }

    /// <summary>
    /// The entity class that represents the events that source an aggregate
    /// </summary>
    public class Event
    {
        public Guid AggregateId { get; set; }
        public string AggregateType { get; set; }
        public int Version { get; set; }
        public string Payload { get; set; }
        public string CorrelationId { get; set; }
    }
}
