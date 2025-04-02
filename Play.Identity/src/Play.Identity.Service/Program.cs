using AspNetCore.Identity.Mongo;
using Play.Common.Settings;
using Play.Identity.Service.Entities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using Microsoft.AspNetCore.Identity;
using Play.Identity.Service.Settings;

var builder = WebApplication.CreateBuilder(args);

BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
var identityServerSettings = builder.Configuration.GetSection(nameof(IdentityServerSettings)).Get<IdentityServerSettings>();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(mongo =>
    {
        mongo.ConnectionString =  $"{mongoDbSettings.ConnectionString}/{serviceSettings.ServiceName}";
    }).AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddIdentityServer(options => {
    options.Events.RaiseSuccessEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseErrorEvents = true;
})
.AddAspNetIdentity<ApplicationUser>() //Connect IdentityServer To AspNetIdentity
.AddInMemoryApiScopes(identityServerSettings.ApiScopes)
.AddInMemoryApiResources(identityServerSettings.ApiResources)
.AddInMemoryClients(identityServerSettings.Clients)
.AddInMemoryIdentityResources(identityServerSettings.IdentityResources); // enable OpenId

builder.Services.AddScoped<SignInManager<ApplicationUser>>();

builder.Services.AddControllers();
builder.Services.AddRazorPages();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();


// "IdentityServerSettings": {
//     "Clients": [
//       {
//         "ClientId": "frontend",
//         "AllowedGrantTypes": [
//           "authorization_code"
//         ],
//         "RequireClientSecret": false,
//         "RedirectUris": [
//           "http://localhost:3000/authentication/login-callback"
//         ],
//         "AllowedScopes": [
//           "openid",
//           "profile",
//           "catalog.fullaccess",
//           "inventory.fullaccess",
//           "trading.fullaccess",
//           "IdentityServerApi",
//           "roles"
//         ],
//         "AlwaysIncludeUserClaimsInIdToken" : true,
//         "PostLogoutRedirectUris":[
//           "http://localhost:3000/authentication/logout-callback"
//         ]
//       },      
//       {
//         "ClientId": "postman",
//         "AllowedGrantTypes": [
//           "authorization_code"
//         ],
//         "RequireClientSecret": false,
//         "RedirectUris": [
//           "urn:ietf:wg:oauth:2.0:oob"
//         ],
//         "AllowedScopes": [
//           "openid",
//           "profile",
//           "catalog.fullaccess",
//           "catalog.readaccess",
//           "catalog.writeaccess",
//           "inventory.fullaccess",
//           "trading.fullaccess",
//           "IdentityServerApi",
//           "roles"
//         ],
//         "AlwaysIncludeUserClaimsInIdToken" : true
//       }
//     ]
//   }