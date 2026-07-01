using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using CounterTestApp001.Api.Models;

namespace CounterTestApp001.Api.Functions;

public class IncrementCount
{
    // The lines in the below block are the direct injection for outside classes and objects.
    private readonly ILogger<IncrementCount> _logger;
    private readonly Container _container;

    public IncrementCount(ILogger<IncrementCount> logger, CosmosClient cosmosClient)
    {
        _logger = logger;
        _container = cosmosClient.GetContainer("CounterDB", "Counters");
    }

    [Function("IncrementCount")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        
        List<PatchOperation> patchOperations = new List<PatchOperation>() // Use a list for single patch operation because .PatchItemAsync expects an IReadOnly list.
        {
            PatchOperation.Increment("/count", 1)
        };
        // Error handling below. As a note, error handling is proportional to caller agency.
        try
        {
            ItemResponse<CounterDocument> response = await _container.PatchItemAsync<CounterDocument>(
                id: "visitor-counter",
                partitionKey: new PartitionKey("visitor-counter"),
                patchOperations: patchOperations
            );
            return new OkObjectResult(response.Resource.count);
        }   
        catch (CosmosException ex) // A generic error handler can be used for the exception. The initial thought of catching 404 errors is not needed, the user does not specify the unique ID, it is hardcoded.
        {
            _logger.LogError(ex, "CosmosDB operation failed in IncrementCount with status {StatusCode}", ex.StatusCode);
            return new StatusCodeResult(500);
        }
    }
}