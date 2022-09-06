using Microsoft.Xrm.Sdk;
using System;
using Xrm.RecordsRestorator.Plugin.Wrappers;

namespace Xrm.RecordsRestorator.Plugin.Model
{
    internal class AuditItem
    {
        public Guid Id { get; set; }

        public string Entity { get; set; }

        public EntityReferenceWrapper ObjectId { get; set; }

        public EntityReferenceWrapper UserId { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
