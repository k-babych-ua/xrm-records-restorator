using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using Xrm.RecordsRestorator.Plugin.Model;

namespace Xrm.RecordsRestorator.Plugin.Repositories
{
    internal class AuditRepository : BaseRepository
    {
        public AuditRepository(IOrganizationService service) : base(service)
        {

        }
    }
}
