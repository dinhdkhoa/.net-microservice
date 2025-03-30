using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using MongoDB.Bson;

namespace Play.Identity.Service.Entities
{
    public class ApplicationRole : MongoRole<Guid>
    {
        
    }
}