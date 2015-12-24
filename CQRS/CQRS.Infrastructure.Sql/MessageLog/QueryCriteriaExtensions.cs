﻿using CQRS.Infrastructure.MessageLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Sql.MessageLog
{
    internal static class QueryCriteriaExtensions
    {
        public static Expression<Func<MessageLogEntity, bool>> ToExpression(this QueryCriteria criteria)
        {
            Expression<Func<MessageLogEntity, bool>> expression = null;

            foreach (var asm in criteria.AssemblyNames)
            {
                var value = asm;
                if (expression == null)
                    expression = e => e.AssemblyName == value;
                else
                    expression = Expression.Or(e => e.AssemblyName == value);
            }

            Expression<Func<MessageLogEntity, bool>> filter = null;
            foreach (var item in criteria.FullNames)
            {
                var value = item;
                if (filter == null)
                    filter = e => e.FullName == value;
                else
                    filter = filter.Or(e => e.FullName == value);
            }

            if(filter ！= null)
            {
                expression = (expression == null) ? filter : expression.Add(filter);
                filter = null;
            }

            foreach (var item in criteria.Namespaces)
	        {
                var value = item;
                if(filter == null)
                    filter = e => e.Namespace == value;
                else
                    filter = filter.Or(e => e.Namespace == value);
	        }

            if(filter != null)
            {
                expression = (expression == null) ? filter:expression.Add(filter);
                filter = null;
            }

            foreach (var item in criteria.SourceIds)
	{
                var value = item;
                if(filter == null)
                    filter = e => e.SourceId == value;
		        else
                    filter = filter.Or(e => e.SourceId == value);
	}
            
            if(filter != null)
            {
                expression = (expression == null) ? filter:expression.Add(filter);
                filter = null;
            }

            foreach (var item in criteria.SourceTypes)
            {
                var value = item;
                if (filter == null)
                    filter = e => e.SourceType == value;
                else
                    filter = filter.Or(e => e.SourceType == value);
            }

            if (filter != null)
            {
                expression = (expression == null) ? filter : expression.And(filter);
                filter = null;
            }

            foreach (var item in criteria.TypeNames)
            {
                var value = item;
                if (filter == null)
                    filter = e => e.TypeName == value;
                else
                    filter = filter.Or(e => e.TypeName == value);
            }

            if (filter != null)
            {
                expression = (expression == null) ? filter : expression.And(filter);
                filter = null;
            }

            if(criteria.EndDate.HasValue)
            {
                var creationDateFilter = criteria.EndDate.Value.ToString("0");
                filter = e => e.CreationDate.CompareTo(creationDateFilter) < 0;

                expression = (expression == null) ? filter : expression.And(filter);
                filter = null;
            }

            return expression;
        }
    }
}
