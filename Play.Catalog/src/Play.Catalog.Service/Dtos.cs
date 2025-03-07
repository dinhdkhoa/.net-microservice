using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Catalog.Service.Dtos
{
    public record ItemDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public decimal Price { get; init; }
        public DateTimeOffset CreatedDate { get; init; }

        // Custom constructor to set default value for CreatedDate
        public ItemDto(string name, string description, decimal price)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            Price = price;
            CreatedDate = DateTimeOffset.UtcNow;
        }
    }
    public record CreateItemDto([Required]string Name, string Description, [Range(0,1000)]decimal Price);
    public record UpdateItemDto([Required]string Name, string Description, [Range(0,1000)]decimal Price);
}