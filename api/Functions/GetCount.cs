using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using CounterTestApp001.Api.Models;

namespace CounterTestApp001.Api.Functions;

public class GetCount
{
    private readonly ILogger<GetCount> _logger;
    private readonly Container _container;

    public GetCount(ILogger<GetCount> logger, CosmosClient cosmosClient)
    {
        _logger = logger;
         _container = cosmosClient.GetContainer("CounterDB", "Counters");
    }

    [Function("GetCount")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req) // Only 'get' is needed here as the point of this function is to get the current count.
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        try
        {
            ItemResponse<CounterDocument> response = await _container.ReadItemAsync<CounterDocument>(
                id: "visitor-counter",
                partitionKey: new PartitionKey("visitor-counter")
                );  
            return new OkObjectResult(response.Resource.count);          
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "CosmosDB failed in GetCount with status {StatusCode}", ex.StatusCode);
            return new StatusCodeResult(500);
        }
    }
}