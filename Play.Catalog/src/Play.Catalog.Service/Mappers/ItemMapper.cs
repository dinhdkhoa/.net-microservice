using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service.Mappers
{
    public static class ItemMapper
    {
        public static ItemDto ToItemDto(this Item item)
        {
            return new ItemDto(item.Name, item.Description, item.Price);
        }
        public static Item ToItemFromDto(this ItemDto item)
        {
            return new Item{
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                CreatedDate = item.CreatedDate
            };
        }
    }
}