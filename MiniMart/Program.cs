using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MiniMart.Data;
using MiniMart.Repositories;
using MiniMart.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.Options;
using Stripe;
using Microsoft.AspNetCore.Mvc;
using MiniMart.Services;
using dotenv.net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.UI.Services;


DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// configure swagger for bearer token
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });

    // Register custom OperationFilter
    options.OperationFilter<MiniMart.Helpers.SecurityRequirementsOperationFilter>();
});



builder.Services.AddControllers();

builder.Services.AddAuthorization();

// configure MySQL connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString)));

// add the identity api endpoint 
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>() // Add service for identity roles
    .AddEntityFrameworkStores<AppDbContext>();

// Add CORS services to allow cross-origin requests
builder.Services.AddCors(options =>
{
    // Add CORS policy for specific origin
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500") // Allow specific frontend origin
              .AllowAnyHeader()                    // Allow any headers
              .AllowAnyMethod()                    // Allow any methods (GET, POST, etc.)
              .AllowCredentials();                 // Allow credentials (cookies, authentication headers, etc.)
    });

});

builder.Services.AddScoped<IImageRepository, CloudinaryImageRepository>();

// Configure Cookie options
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None; // Allow cross-origin requests
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Transmit cookies only over HTTPS
    options.Cookie.HttpOnly = true; // Prevent JavaScript access to the cookie
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Set cookie expiration time to 60 minutes
    options.SlidingExpiration = true; // Enable sliding expiration
});

// Configure for Stripe 

builder.Services.Configure<StripeOptions>(options =>
{
    options.PublishableKey = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY");
    options.SecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
    options.WebhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");
    options.Domain = Environment.GetEnvironmentVariable("DOMAIN");
});
builder.Services.AddSingleton<IStripeClient>(new StripeClient(builder.Configuration["STRIPE_SECRET_KEY"]));
builder.Services.AddSingleton<PaymentIntentService>();
builder.Services.AddScoped<PaymentService>();

// Register email sender
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Register PaymentService
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddSingleton<PaymentService>();

// Register StripeWebhookService
builder.Services.AddScoped<StripeWebhookService>();

// Register check admin service
builder.Services.AddScoped<CheckIsAdminService>();

builder.Services.AddSingleton<RsaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// add the identity middleware 
app.MapIdentityApi<IdentityUser>();
app.UseHttpsRedirection();

// use CORS middleware
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

// Configure controller routes
app.MapControllers();

// Map route for Stripe publishable key
app.MapGet("config", (PaymentService paymentService) =>
{
    return Results.Ok(new { publishableKey = paymentService.GetPublishableKey() });
});



// Map route for Stripe webhook
app.MapPost("webhook", async (HttpRequest req, StripeWebhookService webhookService) =>
{
    return await webhookService.HandleWebhookAsync(req);
});

// Create scope and seed admin account
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var configuration = app.Configuration;
    await SeedRoles.Initialize(services, configuration);
}

app.Run();

