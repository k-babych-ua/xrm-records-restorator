﻿using Microsoft.Xrm.Sdk.Query;
using System;

namespace Xrm.RecordsRestorator.Plugin.Builders
{
    internal class QueryBuilder
    {
        private const string _dateTimeFormat = "yyyy-MM-dd hh:mm";

        protected QueryExpression Query { get; private set; }

        public QueryBuilder(string entityName)
        {
            Query = new QueryExpression(entityName)
            {
                ColumnSet = new ColumnSet(false),
                PageInfo = new PagingInfo()
                {
                    
                },
                Criteria =
                {
                    Conditions =
                    {

                    }
                }
            };
        }

        public QueryBuilder SetNoLock(bool noLock = true)
        {
            Query.NoLock = noLock;

            return this;
        }

        public QueryBuilder SetPageSize(int pageSize = 500)
        {
            Query.PageInfo.Count = pageSize;

            return this;
        }

        public QueryBuilder WithColumns(params string[] columns)
        {
            Query.ColumnSet = new ColumnSet(columns);

            return this;
        }

        public QueryBuilder AddCondition(ConditionExpression condition)
        {
            Query.Criteria.Conditions.Add(condition);

            return this;
        }

        public QueryBuilder AddCondition(string attributeName, ConditionOperator conditionOperator)
        {
            AddCondition(new ConditionExpression(attributeName, conditionOperator));

            return this;
        }

        public QueryBuilder AddCondition(string attributeName, ConditionOperator conditionOperator, object value)
        {
            AddCondition(new ConditionExpression(attributeName, conditionOperator, value));

            return this;
        }

        public QueryBuilder OrderBy(string attributeName, OrderType orderType)
        {
            Query.AddOrder(attributeName, orderType);

            return this;
        }

        public QueryExpression GetQuery()
        {
            return Query;
        }

        public QueryBuilder ByCreatedOnGreaterEqual(DateTime value)
        {
            AddCondition("createdon", ConditionOperator.GreaterEqual, value.ToString(_dateTimeFormat));

            return this;
        }

        public QueryBuilder ByCreatedOnLessEqual(DateTime value)
        {
            AddCondition("createdon", ConditionOperator.LessEqual, value.ToString(_dateTimeFormat));

            return this;
        }
    }
}
