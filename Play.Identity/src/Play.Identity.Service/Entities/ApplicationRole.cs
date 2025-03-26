using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace Play.Identity.Service.Entities
{
    [CollectionName("Roles")]

    public class ApplicationRole : MongoIdentityRole<Guid>
    {
        
    }
}