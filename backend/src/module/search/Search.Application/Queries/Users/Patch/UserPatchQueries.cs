using HanziAnhVu.Shared.Application;
using MediatR;
using Search.Contracts.DTOs;
using Search.Contracts.Interfaces;

namespace Search.Application.Queries.Users.Patch;

public record UserPatchQueries(
    string Id,
    string? Email = null,
    string? Username = null,
    string? PhoneNumber = null,
    bool? IsActive = null,
    DateTime? LastLogin = null,
    int? CurrentLevel = null,
    DateTime? LastActivityAt = null,
    string? AvatarUrl = null,
    DateTime? UpdatedAt = null,
    CancellationToken Cancel = default
) : IRequest<UserSearchPatchDocumentResponse>;
