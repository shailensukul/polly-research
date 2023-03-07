using System.Reflection.Metadata.Ecma335;
using Client.Service;
using Microsoft.Extensions.Logging.Console;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Logging.AddConsole();

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole();
});

// Registering the client services as shown in the next snippet, makes the DefaultClientFactory create a standard HttpClient for each service.
// The typed client is registered as transient with DI container.
// In the following code, AddHttpClient() registers ProductService as transient services so they can be injected and consumed directly without any need for additional registrations.
builder.Services.AddHttpClient<IProductService, ProductService>(client =>
    {
        client.BaseAddress = new Uri("http://localhost:5017");
    })
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCustomPolicy(loggerFactory.CreateLogger("Custom")))
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));;
// Each time you get an HttpClient object from the IHttpClientFactory, a new instance is returned.
// But each HttpClient uses an HttpMessageHandler that's pooled and reused by the IHttpClientFactory to reduce resource consumption, as long as the HttpMessageHandler's lifetime hasn't expired.
//  The default value is two minutes, but it can be overridden per Typed Client.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        // HTTP 5XX status codes (server errors)
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCustomPolicy(ILogger logger)
{
    return Policy<HttpResponseMessage>.Handle<HttpRequestException>()
        .OrResult(msg =>
        {
            var content = msg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            bool handled = (int)msg.StatusCode == 333 && content.Contains("Weird error");
            if (handled)
            {
                logger.LogWarning("Handling 333 weird error");
            }
            return handled;
        })
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}