using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System.Collections.Generic;
using System.Linq;
using Xrm.RecordsRestorator.Plugin.Model;

namespace Xrm.RecordsRestorator.Plugin.Repositories
{
    internal class MetadataRepository
    {
        private IOrganizationService _service;

        internal MetadataRepository(IOrganizationService service)
        {
            _service = service;
        }

        public IEnumerable<EntityItem> GetAuditableEntity()
        {
            RetrieveAllEntitiesRequest request = new RetrieveAllEntitiesRequest()
            {
                EntityFilters = Microsoft.Xrm.Sdk.Metadata.EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            var retrieveEntitiesResponse = (RetrieveAllEntitiesResponse)_service.Execute(request);

            return retrieveEntitiesResponse
                .EntityMetadata
                .Where(x => x.IsAuditEnabled.Value)
                .Select(x => new EntityItem()
                {
                    LogicalName = x.LogicalName,
                    Name = x.DisplayName.UserLocalizedLabel.Label,
                    PrimaryKey = x.PrimaryIdAttribute,
                })
                .OrderBy(x => x.Name);
        }
    }
}
