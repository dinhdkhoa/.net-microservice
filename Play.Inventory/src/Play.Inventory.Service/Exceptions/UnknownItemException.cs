using System;
using System.Runtime.Serialization;

namespace Play.Inventory.Service.Exceptions
{
    public class UnknownItemException : Exception
    {
        public UnknownItemException(Guid CatalogItemId) : base($"Unknown Item {CatalogItemId}")
        {
            this.CatalogItemId = CatalogItemId;
        }

        public Guid CatalogItemId { get;}
    }
}