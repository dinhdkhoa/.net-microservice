using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Catalog.Contracts
{
    public class Contracts
    {
        public record CatalogItemCreated(Guid Id, string name, string description);
        public record CatalogItemUpdated(Guid Id, string name, string description);
        public record CatalogItemDeleted(Guid Id);
    }
}