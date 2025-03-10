using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Play.Inventory.Service.Dtos;

namespace Play.Inventory.Service.Clients
{
    public class CatalogClient
    {
        private readonly HttpClient http;
        public CatalogClient(HttpClient _http)
        {
            http = _http;
        }

        public async Task<IReadOnlyList<CatalogItemDto>> GetListCatalogAsync(){
            var catalogItems = await http.GetFromJsonAsync<IReadOnlyList<CatalogItemDto>>("/items");
            return catalogItems;
        }
    }
}