using Microsoft.Xrm.Sdk;
using System;

namespace Xrm.RecordsRestorator.Plugin.Wrappers
{
    internal class EntityReferenceWrapper
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string LogicalName { get; set; }

        public EntityReferenceWrapper(EntityReference entityReference)
        {
            if (entityReference == null)
            {
                throw new ArgumentNullException("entityReference");
            }

            Id = entityReference.Id;
            Name = entityReference.Name;
            LogicalName = entityReference.LogicalName;
        }

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? Id.ToString() : Name;
    }
}
