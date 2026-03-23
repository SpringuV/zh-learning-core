
namespace HanziAnhVu.Shared.Domain;

public abstract class BaseAggregateRoot: IAggregateRoot
{
    //-  Mỗi khi có sự kiện domain nào đó xảy ra, chúng ta sẽ thêm nó vào list này.
    // - Sau đó, khi unit of work hoàn thành, chúng ta sẽ publish tất cả các sự kiện này lên event bus
    // để các handler khác có thể lắng nghe và xử lý.
    // để private vì chỉ có thể thêm sự kiện thông qua phương thức AddDomainEvent,
    // tránh việc thay đổi trực tiếp list từ bên ngoài

    // -- Dùng cho domain event nội bộ trong cùng bounded context.
    // -- •	event xảy ra trong domain
    // •	thường được giữ trong aggregate
    // •	sau đó UnitOfWork hoặc handler nội bộ sẽ xử lý

    private readonly List<IDomainEvent> _domainEvents = new();

    // Cho phép đọc các sự kiện domain đã xảy ra, nhưng không cho phép thay đổi trực tiếp từ bên ngoài
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    // Gọi sau khi publish — clear list đi
    public IReadOnlyList<IDomainEvent> PopDomainEvents()
    {
        var events = _domainEvents.ToList();
        _domainEvents.Clear();
        return events;
    }
}
