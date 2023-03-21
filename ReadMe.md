# Polly Retry Demo


## What is Polly
[Polly](https://github.com/App-vNext/Polly) is a .NET resilience and transient-fault-handling library that allows developers to express policies such as Retry, Circuit Breaker, Timeout, Bulkhead Isolation, Rate-limiting and Fallback in a fluent and thread-safe manner.

## Overview

The solution has 2 projects.

One is a Web API service which has 2 endpoints:

- `/Product/Get` - the "good" endpoint
- `/Product/GetWithRandomFailure` - which succeeds once every 3 tries and fails other times with either a "service unavailable" error or a custom 333 = "weird error" error

![image](https://user-images.githubusercontent.com/564911/223391366-7298346a-6a4f-4492-a450-3bc7eddbde69.png)

The other project is an ASP.Net MVC app which tries to call the "bad" endpoint with and without a retry policy.
The policies are defined in Program.cs against a typed HttpClient:


```
builder.Services.AddHttpClient<IProductService, ProductService>(client =>
    {
        client.BaseAddress = new Uri("http://localhost:5017");
    })
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCustomPolicy());
```
...
```
static IAsyncPolicy<HttpResponseMessage> GetCustomPolicy()
{
    return Policy<HttpResponseMessage>.Handle<HttpRequestException>()
        .OrResult(msg =>
        {
            var content = msg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            bool handled = (int)msg.StatusCode == 333 && content.Contains("Weird error");
            if (handled)
            {
                Console.WriteLine("Handling 333 weird error");
            }
            return handled;
        })
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
```

The reslient button leverages the policies above to automatically retry failures so it can provide a consitent and stable experience.

![image](https://user-images.githubusercontent.com/564911/223392764-a481c61b-8e03-41a1-a4cd-4cb67403cde2.png)


# Final Note
This Microsoft article is worth a read when using `HttpClient`(https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)

`HttpClient` is intended to be instantiated once and reused throughout the life of an application. Instantiating an HttpClient class for every request will exhaust the number of sockets available under heavy loads. That issue will result in SocketException errors. Possible approaches to solve that problem are based on the creation of the HttpClient object as singleton or static, as explained in this Microsoft article on HttpClient usage. This can be a good solution for short-lived console apps or similar, that run a few times a day.

Another issue that developers run into is when using a shared instance of HttpClient in long-running processes. In a situation where the HttpClient is instantiated as a singleton or a static object, it fails to handle the DNS changes.

However, the issue isn't really with HttpClient per se, but with the default constructor for HttpClient, because it creates a new concrete instance of HttpMessageHandler, which is the one that has sockets exhaustion and DNS changes issues mentioned above.

To address the issues mentioned above and to make HttpClient instances manageable, .NET Core 2.1 introduced two approaches, one of them being IHttpClientFactory. It's an interface that's used to configure and create HttpClient instances in an app through Dependency Injection (DI). It also provides extensions for Polly-based middleware to take advantage of delegating handlers in HttpClient.

The alternative is to use SocketsHttpHandler with configured PooledConnectionLifetime. This approach is applied to long-lived, static or singleton HttpClient instances. 


Registering the client services as shown in the next snippet, makes the DefaultClientFactory create a standard HttpClient for each service.
The typed client is registered as transient with DI container.
In the following code, AddHttpClient() registers ProductService as transient services so they can be injected and consumed directly without any need for additional registrations.

```
builder.Services.AddHttpClient<IProductService, ProductService>(client =>
    {
        client.BaseAddress = new Uri("http://localhost:5017");
    })
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCustomPolicy())
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));;
 ```

Each time you get an HttpClient object from the IHttpClientFactory, a new instance is returned.
But each HttpClient uses an HttpMessageHandler that's pooled and reused by the IHttpClientFactory to reduce resource consumption, as long as the HttpMessageHandler's lifetime hasn't expired.
The default value is two minutes, but it can be overridden per Typed Client.

A Typed Client is effectively a transient object, that means a new instance is created each time one is needed. It receives a new HttpClient instance each time it's constructed. However, the HttpMessageHandler objects in the pool are the objects that are reused by multiple HttpClient instances.

