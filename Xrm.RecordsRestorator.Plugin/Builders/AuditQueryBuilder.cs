using Microsoft.Xrm.Sdk.Query;
using System;

namespace Xrm.RecordsRestorator.Plugin.Builders
{
    internal class AuditQueryBuilder : QueryBuilder
    {

        public AuditQueryBuilder(): base("audit")
        {

        }

        public AuditQueryBuilder ByUser(Guid userId)
        {
            AddCondition("userid", ConditionOperator.Equal, userId);

            return this;
        }

        public AuditQueryBuilder ByOperation(int operation)
        {
            AddCondition("operation", ConditionOperator.Equal, operation);

            return this;
        }

        public AuditQueryBuilder ByEntity(string entityName)
        {
            AddCondition("objecttypecode", ConditionOperator.Equal, entityName);

            return this;
        }

        public AuditQueryBuilder ByObjectId(Guid objectId)
        {
            AddCondition("objectid", ConditionOperator.Equal, objectId);

            return this;
        }
    }
}
