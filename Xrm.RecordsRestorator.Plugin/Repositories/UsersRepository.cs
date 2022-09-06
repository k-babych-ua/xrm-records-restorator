using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.Linq;
using Xrm.RecordsRestorator.Plugin.Model;

namespace Xrm.RecordsRestorator.Plugin.Repositories
{
    internal class UsersRepository
    {
        private IOrganizationService _service;

        public UsersRepository(IOrganizationService service)
        {
            _service = service;
        }

        public IEnumerable<User> GetEnabledUsers()
        {
            QueryExpression query = new QueryExpression("systemuser")
            {
                NoLock = true,
                ColumnSet = new ColumnSet("fullname", "systemuserid"),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("isdisabled", ConditionOperator.Equal, false)
                    }
                },
                Orders =
                {
                    new OrderExpression("fullname", OrderType.Ascending)
                }
            };

            return _service
                .RetrieveMultiple(query)
                .Entities
                .Select(x => new User()
                {
                    DisplayName = x.GetAttributeValue<string>("fullname"),
                    Id = x.Id
                });
        }
    }
}
