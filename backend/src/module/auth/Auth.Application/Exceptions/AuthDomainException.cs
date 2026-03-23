using HanziAnhVu.Shared.Domain.Exceptions;

namespace Auth.Domain.Exceptions
{
    public class AuthDomainException: DomainException
    {
        public AuthDomainException(string message) : base(message) { }

        // constructor này cho phép truyền vào một message và một inner exception,
        // giúp giữ nguyên thông tin lỗi gốc khi ném lại exception
        public AuthDomainException(string message, Exception innerException) : base(message, innerException) { }
    }
}
