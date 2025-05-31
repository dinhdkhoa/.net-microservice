using AspNetCore.Identity.Mongo;
using Play.Common.Settings;
using Play.Identity.Service.Entities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using Microsoft.AspNetCore.Identity;
using Play.Identity.Service.Settings;
using Play.Identity.Service.HostedServices;
using Play.Common.RabbitMQ;
using MassTransit;
using Play.Identity.Service.Exceptions;
using GreenPipes;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using Play.Identity.Service.HealthChecks;
using Play.Common.HealthChecks;
using Play.Common.Configuration;
using Microsoft.AspNetCore.HttpOverrides;
using System.Security.Cryptography.X509Certificates;
using Microsoft.CodeAnalysis.CSharp.Syntax;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAzureKeyVault();

BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

builder.Services
    .Configure<IdentitySettings>(builder.Configuration.GetSection(nameof(IdentitySettings)))
    .AddIdentity<ApplicationUser, ApplicationRole>()
    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(mongo =>
    {
        mongo.ConnectionString =  $"{mongoDbSettings.ConnectionString}/{serviceSettings.ServiceName}";
    }).AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddMessageBroker(builder.Configuration, retryConfig =>
{
    retryConfig.Interval(3, TimeSpan.FromSeconds(5));
    retryConfig.Ignore(typeof(InsufficientFundsException));
    retryConfig.Ignore(typeof(UnknownUserException));
});

AddIdentityServer(builder);

builder.Services.AddLocalApiAuthentication();

builder.Services.AddScoped<SignInManager<ApplicationUser>>();

builder.Services.AddControllers();
builder.Services.AddHostedService<IdentitySeedHostedService>();
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Register", "register");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "login");
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddMongoDbHealthCheck();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    //forward headers from emissary ingress gateway + line 90

    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

var AllowedOriginSettings = builder.Configuration["AllowedOrigin"];
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(builder => {
        builder.WithOrigins(AllowedOriginSettings).AllowAnyHeader().AllowAnyMethod();
    });
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.Use((context, next) =>
{
    //config path for emissary ingress gateway
    var identityServer = builder.Configuration.GetSection(nameof(IdentitySettings)).Get<IdentitySettings>();
    context.Request.PathBase = new PathString(identityServer.PathBase);
    return next();
});

app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();

app.UseCookiePolicy(new CookiePolicyOptions{
    MinimumSameSitePolicy = SameSiteMode.Lax
});

app.MapControllers();
app.MapRazorPages();

app.MapPlayEconomyHealthChecks();

app.Run();

static void AddIdentityServer(WebApplicationBuilder builder)
{
    var identityServerSettings = builder.Configuration.GetSection(nameof(IdentityServerSettings)).Get<IdentityServerSettings>();

    var identityServerBuilder = builder.Services.AddIdentityServer(options =>
    {
        options.Events.RaiseSuccessEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseErrorEvents = true;
        options.KeyManagement.KeyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    })
    .AddAspNetIdentity<ApplicationUser>() //Connect IdentityServer To AspNetIdentity
    .AddInMemoryApiScopes(identityServerSettings.ApiScopes)
    .AddInMemoryApiResources(identityServerSettings.ApiResources)
    .AddInMemoryClients(identityServerSettings.Clients)
    .AddInMemoryIdentityResources(identityServerSettings.IdentityResources); // enable OpenId

    if (builder.Environment.IsProduction())
    {
        // persist secrets keys with k8s secrets
        var identitySettings = builder.Configuration.GetSection(nameof(IdentitySettings)).Get<IdentitySettings>();
        var cert = X509Certificate2.CreateFromPemFile(
            identitySettings.CertificateCrtFilePath,
            identitySettings.CertificateKeyFilePath
        );
        identityServerBuilder.AddSigningCredential(cert);
    }
}