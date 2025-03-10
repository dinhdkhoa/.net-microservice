using System;
using System.ComponentModel.DataAnnotations;

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
        public ItemDto(string name, string description, decimal price , Guid id, DateTimeOffset createdDate)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            CreatedDate = createdDate;
        }

        public ItemDto(string name, string description, decimal price)
        {
            Name = name;
            Id = Guid.NewGuid();
            CreatedDate = DateTimeOffset.UtcNow;
            Description = description;
            Price = price;
        }
    }
    public record CreateItemDto([Required]string Name, string Description, [Range(0,1000)]decimal Price);
    public record UpdateItemDto([Required]string Name, string Description, [Range(0,1000)]decimal Price);
}