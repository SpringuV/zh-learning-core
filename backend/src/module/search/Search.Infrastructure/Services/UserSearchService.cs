using Elastic.Clients.Elasticsearch;
using Search.Application.Interfaces;
using Search.Domain.Entities;
using System.Linq.Expressions;

namespace Search.Infrastructure.Services;

public class UserSearchService : IElasticSearchBase<UserSearchDocument>
{
    private readonly ElasticsearchClient _client;

    public UserSearchService(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task IndexAsync(UserSearchDocument document, CancellationToken cancellationToken = default)
    {
        var response = await _client.IndexAsync(document, i => i
            .Index("hanzi_users")
            .Id(document.Id), cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new Exception($"Failed to index user: {response.DebugInformation}");
        }
    }

    public async Task<UserSearchDocument?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync<UserSearchDocument>(id, g => g.Index("hanzi_users"), cancellationToken);
        return response.Source;
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync(id, d => d.Index("hanzi_users"), cancellationToken);
    }

    public async Task<SearchResult<UserSearchDocument>> SearchAsync(
        string? email = null,
        string? username = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        return null;
    }

    // Example usage:
    // var result = await _userSearchService.SearchAsync(u => u.IsActive && u.Email.Contains("gmail"));
    // foreach (var user in result.Documents) { Console.WriteLine(user.Username); }
    // 
    // Example usage with IQueryable:
    // var activeUsers = _userSearchService.Query()
    //     .Where(u => u.IsActive && u.Email.Contains("gmail"))
    //     .OrderBy(u => u.CreatedAt)
    //     .Take(10);
    // var result = await activeUsers.ToListAsync(cancellationToken);
}