
using System.Globalization;
using System.Text;

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

    private readonly List<IDomainEvent> _domainEvents = [];

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
    
    /// <summary>
    /// Generate URL-friendly slug from text
    /// Example: "HSK 1 - Beginner" -> "hsk-1-beginner"
    /// Reusable across all aggregates (Course, Topic, Exercise, etc.)
    /// </summary>
    protected static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Remove diacritics first so slug remains ASCII-only (e.g., "tiếp" -> "tiep").
        var normalized = text.Normalize(NormalizationForm.FormD);
        var noDiacriticsBuilder = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            noDiacriticsBuilder.Append(c switch
            {
                'đ' => 'd',
                'Đ' => 'D',
                _ => c
            });
        }

        var sanitized = noDiacriticsBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();

        var slugBuilder = new StringBuilder(sanitized.Length);
        var lastWasDash = false;
        foreach (var c in sanitized)
        {
            if (char.IsLetterOrDigit(c))
            {
                slugBuilder.Append(c);
                lastWasDash = false;
                continue;
            }

            if (!lastWasDash)
            {
                slugBuilder.Append('-');
                lastWasDash = true;
            }
        }

        return slugBuilder.ToString().Trim('-');
    }
}
