using HanziAnhVu.Shared.Domain;
using MediatR;

namespace Auth.Application.DomainEvents;


// sealed record để đảm bảo tính bất biến và hiệu quả, vì domain event thường chỉ chứa dữ liệu và không có logic phức tạp
public sealed record AuthUserCreatedDomainEvent(Guid UserId, string Email, string UserName) : BaseDomainEvent, INotification;
