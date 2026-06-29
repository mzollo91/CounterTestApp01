using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args); // Sets up the host builder, isolated-worker style

builder.ConfigureFunctionsWebApplication(); // wires in the ASP.NET Core integration, and also the reason why the function uses HttpRequest/IActionResult instead of HttpRequestData/HttpResponeData (legacy options)

builder.Services // the below lines are for telemetry/logging to Azure Application Insights. Set up with the wizard, but can be left.
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Registering CosmosClient as a singleton here, reading the CosmosDbEndpoint and CosmosDbKey from the configuration.
string endpointUrl = builder.Configuration["CosmosDbEndpoint"]
    ?? throw new InvalidOperationException("CosmosDbEndpoint is not configured.");
string apiKey = builder.Configuration["CosmosDbKey"]
    ?? throw new InvalidOperationException("CosmosDbKey is not configured");

builder.Services.AddSingleton<CosmosClient>(serviceProvider => new CosmosClient(endpointUrl, apiKey)); 
// Using the serviceProvider and lambda setup (instead of just new CosmosClient()) allows construction of the container to occur when it's actually needed, rather than at the time of registration.

builder.Build().Run(); // builds the host and starts it, blocking until shutdown.
