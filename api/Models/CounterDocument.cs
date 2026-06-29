namespace CounterTestApp001.Api.Models;

public class CounterDocument // This class mirrors the structure of the document shape. This is done toto be used as to give properties to the 'response.Resource' object in the IncrementCount function.
{
    public string id {get; set; } = string.Empty;
    public int count {get; set; }
}