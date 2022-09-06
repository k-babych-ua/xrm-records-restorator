using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;

namespace Xrm.RecordsRestorator.Plugin.Repositories
{
    internal class BaseRepository
    {
        protected IOrganizationService _service;

        public BaseRepository(IOrganizationService service)
        {
            _service = service;
        }

        public IEnumerable<Entity> RetrieveMultiple(QueryExpression query)
        {
            return _service.RetrieveMultiple(query).Entities;
        }
    }
}
