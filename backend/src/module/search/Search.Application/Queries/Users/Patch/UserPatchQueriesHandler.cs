using Elastic.Clients.Elasticsearch;
using MediatR;
using Microsoft.Extensions.Logging;
using Search.Contracts.DTOs;
using Search.Domain.Entities;

namespace Search.Application.Queries.Users.Patch;

public class UserPatchQueriesHandler(ElasticsearchClient client, ILogger<UserPatchQueriesHandler> logger) : IRequestHandler<UserPatchQueries, UserSearchPatchDocumentResponse>
{
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<UserPatchQueriesHandler> _logger = logger;

    public async Task<UserSearchPatchDocumentResponse> Handle(UserPatchQueries request, CancellationToken cancellationToken)
    {
        var partialDocument = new Dictionary<string, object>();
        if (request.Email is not null) partialDocument["email"] = request.Email;
        if (request.Username is not null) partialDocument["username"] = request.Username;
        if (request.PhoneNumber is not null) partialDocument["phoneNumber"] = request.PhoneNumber;
        if (request.IsActive.HasValue) partialDocument["isActive"] = request.IsActive.Value;
        if (request.LastLogin.HasValue) partialDocument["lastLogin"] = request.LastLogin.Value;
        if (request.CurrentLevel.HasValue) partialDocument["currentLevel"] = request.CurrentLevel.Value;
        if (request.LastActivityAt.HasValue) partialDocument["lastActivityAt"] = request.LastActivityAt.Value;
        if (request.AvatarUrl is not null) partialDocument["avatarUrl"] = request.AvatarUrl;
        if (request.UpdatedAt.HasValue) partialDocument["updatedAt"] = request.UpdatedAt.Value;

        if (partialDocument.Count == 0)
        {
            throw new ArgumentException("Cần ít nhất một trường để cập nhật", nameof(request));
        }

        var response = await _client.UpdateAsync<UserSearch, Dictionary<string, object>>(ConstantIndexElastic.UserIndex, request.Id, u => u
            .Doc(partialDocument)
            .RetryOnConflict(3), cancellationToken); // dữ liệu trả về là document sau khi update, nếu muốn lấy document 
                                                    // trước khi update thì dùng .Source(false)
        _logger.LogInformation("Patch user {UserId} with fields: {Fields}", request.Id, string.Join(", ", partialDocument.Keys));
        if (!response.IsValidResponse)
        {
            throw new Exception($"Failed to patch user {request.Id}: {response.DebugInformation}");
        }

        return new UserSearchPatchDocumentResponse(
            Id: request.Id,
            Email: request.Email,
            Username: request.Username,
            PhoneNumber: request.PhoneNumber,
            IsActive: request.IsActive,
            LastLogin: request.LastLogin,
            CurrentLevel: request.CurrentLevel,
            LastActivityAt: request.LastActivityAt,
            AvatarUrl: request.AvatarUrl,
            UpdatedAt: request.UpdatedAt
        );
    }
}