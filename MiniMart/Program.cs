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
app.MapGet("config", async (IOptions<StripeOptions> options) =>
{
    return Results.Ok(new { publishableKey = options.Value.PublishableKey });
});

//// Map route for Stripe payment intent creation
//app.MapPost("create-payment-intent", async (
//    HttpRequest req, 
//    PaymentIntentService service, 
//    [FromBody] StripeRequestData requestData) =>
//{
//    // 1. Ensure the amount is valid
//    if (requestData?.Amount <= 0)
//    {
//        return Results.BadRequest("Invalid amount.");
//    }

//    // 2. Create the payment intent with the dynamic amount
//    var options = new PaymentIntentCreateOptions
//    {
//        Amount = requestData.Amount, // Use the amount from the request
//        Currency = "cad", // You can also change this dynamically if needed
//        AutomaticPaymentMethods = new()
//        {
//            Enabled = true
//        }
//    };

//    // 3. Create the payment intent via the Stripe service
//    var paymentIntent = await service.CreateAsync(options);

//    // 4. Return the client secret as part of the response
//    return Results.Ok(new { clientSecret = paymentIntent.ClientSecret });
//});


// Map route for Stripe webhook
app.MapPost("webhook", async (HttpRequest req, IOptions<StripeOptions> options, ILogger<Program> logger) =>
{
    var json = await new StreamReader(req.Body).ReadToEndAsync();
    Event stripeEvent;
    try
    {
        stripeEvent = EventUtility.ConstructEvent(
            json,
            req.Headers["Stripe-Signature"],
            options.Value.WebhookSecret,
            throwOnApiVersionMismatch: false // Disable API version mismatch check
        );
        logger.LogInformation($"Webhook notification with type: {stripeEvent.Type} found for {stripeEvent.Id}");
    }
    catch (Exception e)
    {
        logger.LogError(e, $"Something failed => {e.Message}");
        return Results.BadRequest();
    }

    if (stripeEvent.Type == Events.PaymentIntentSucceeded)
    {
        var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
        logger.LogInformation($"💰PaymentIntent ID: {paymentIntent.Id}");
    }

    return Results.Ok();
});


app.Run();

