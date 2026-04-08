namespace Search.Application.Exceptions;

/// <summary>
/// Exception thrown when Elasticsearch operation fails
/// </summary>
public class ElasticsearchException : Exception
{
    public ElasticsearchException(string message) : base(message) { }
    
    public ElasticsearchException(string message, Exception innerException) 
        : base(message, innerException) { }
}
