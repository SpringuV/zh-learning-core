using Elastic.Clients.Elasticsearch;

namespace HanziAnhVuHsk.Api.Apis;

public static class SearchApi
{
    public static async Task<IResult> IndexDocument(IndexSearchDocumentRequest request, ElasticsearchClient client, CancellationToken ct)
    {
        var document = new SearchDocument
        {
            Id = request.Id,
            Title = request.Title,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        var response = await client.IndexAsync(document, x => x
            .Index("hanzi_documents")
            .Id(document.Id), ct);

        if (!response.IsValidResponse)
        {
            return Results.Problem(response.DebugInformation, statusCode: StatusCodes.Status500InternalServerError);
        }

        return Results.Ok(new { message = "Indexed", document.Id });
    }

    public static async Task<IResult> SearchDocuments(string q, ElasticsearchClient client, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Results.BadRequest(new { message = "Query 'q' is required." });
        }

        var response = await client.SearchAsync<SearchDocument>(x => x
            .Indices("hanzi_documents")
            .Query(query => query.Bool(b => b
                .Should(
                    s => s.Match(m => m.Field(f => f.Title).Query(q)),
                    s => s.Match(m => m.Field(f => f.Content).Query(q))
                ))), ct);

        if (!response.IsValidResponse)
        {
            return Results.Problem(response.DebugInformation, statusCode: StatusCodes.Status500InternalServerError);
        }

        var results = response.Hits
            .Select(h => new
            {
                id = h.Source?.Id,
                title = h.Source?.Title,
                content = h.Source?.Content,
                score = h.Score
            })
            .ToList();

        return Results.Ok(results);
    }

    public sealed class IndexSearchDocumentRequest
    {
        public string Id { get; set; } = Guid.CreateVersion7().ToString("N");
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public sealed class SearchDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
