using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;

namespace CounterTestApp001.Api.Functions;

public class IncrementCount
{
    // The lines in the below block are the direct injection for outside classes and objects.
    private readonly ILogger<IncrementCount> _logger;
    private readonly Container _container;

    public class CounterDocument // This class mirrors the structure of the document shape. This is done toto be used as to give properties to the 'response.Resource' object in the IncrementCount function.
    {
        public string id {get; set; } = string.Empty;
        public int count {get; set; }
    }

    public IncrementCount(ILogger<IncrementCount> logger, CosmosClient cosmosClient)
    {
        _logger = logger;
        _container = cosmosClient.GetContainer("CounterDb", "Counters");
    }

    [Function("IncrementCount")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        
        List<PatchOperation> patchOperations = new List<PatchOperation>() // Use a list for single patch operation because .PatchItemAsync expects an IReadOnly list.
        {
            PatchOperation.Increment("/count", 1)
        };

           
        ItemResponse<CounterDocument> response = await _container.PatchItemAsync<CounterDocument>(
            id: "visitor-counter",
            partitionKey: new PartitionKey("visitor-counter"),
            patchOperations: patchOperations
        );

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            return new OkObjectResult(response.Resource.count);
        }
        return new OkObjectResult(response.Resource.count); // Placeholder line
    }
}