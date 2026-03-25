using Auth.Contracts;

namespace Auth.Infrastructure.Services
{
    public class TokenClaimService : ITokenClaimService
    {
        private readonly AuthIdentityDbContext _dbContext;

        public TokenClaimService(AuthIdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ValidateUser đã chứa sẵn Id, UserName và Roles từ IdentityService
        // → không cần FindByName hay GetRoles thêm lần nào nữa
        public async Task<TokenResult> GetTokenAsync(ValidateUser user, CancellationToken cancellationToken)
        {
            // nếu user đã bị xóa hoặc vô hiệu hóa sau khi validate → không issue token
            // cancel token ngay để tránh issue token cho user không hợp lệ
            cancellationToken.ThrowIfCancellationRequested();

            var accessToken = GenerateAccessTokenAsync(user.Id, user.Username, user.Roles);
            var refreshToken = await CreateRefreshTokenAsync(user.Id, cancellationToken);
            return new TokenResult(accessToken, refreshToken);
        }

        public async Task<TokenResult?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
        {
            // validate token
            var tokenStored = await _dbContext.RefreshTokens
                .Include(t => t.AuthUser)
                    .ThenInclude(u => u.UserRoles)
                .SingleOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);
            if (tokenStored is null || tokenStored.IsRevoked || tokenStored.ExpiresAt < DateTime.UtcNow) 
            {
                return null;
            }
            tokenStored.IsRevoked = true; // revoke token cũ

            // tạo token mới
            var roleIds = tokenStored.AuthUser.UserRoles.Select(ur => ur.RoleId).ToList();
            var roleNames = await _dbContext.Roles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Name!)
                .ToListAsync(cancellationToken);

            var newAccessToken = GenerateAccessTokenAsync(tokenStored.UserId, tokenStored.AuthUser.UserName!, roleNames.AsReadOnly());
            var newRefreshToken = await CreateRefreshTokenAsync(tokenStored.UserId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new TokenResult(newAccessToken, newRefreshToken);
        }

        public Task RevokeAsync(string refreshToken, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private string GenerateAccessTokenAsync(string userId, string userName, IEnumerable<string> roles)
        {
            // Sử dụng user.Id, user.UserName, user.Roles để tạo access token
            // Ví dụ: sử dụng JWT để tạo token với các claim tương ứng
            var key = Encoding.ASCII.GetBytes(Constants.JWT_SECRET_KEY);

            // "sub" (subject) = chuẩn JWT, lưu userId → dùng để identify user trong DB
            // "name"          = userName, khớp với NameClaimType = "name" trong TokenValidationParameters
            //                   → User.Identity.Name sẽ trả về đúng userName
            // "role"          = khớp với RoleClaimType = "role" → [Authorize(Roles=...)] hoạt động
            var claims = new List<Claim>
            {
                new("sub", userId),
                new("name", userName)
            };
            foreach (var role in roles)
                claims.Add(new Claim("role", role));

            var now = DateTime.UtcNow;
            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = Constants.JWT_ISSUER,
                Subject = new ClaimsIdentity(claims),
                Expires = now.AddHours(AuthTokenConstants.AccessTokenExpireHours),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(descriptor));
        }

        private async Task<string> CreateRefreshTokenAsync(string userId, CancellationToken cancellationToken)
        {
            // Token ngẫu nhiên 64 bytes — Base64URL (không có +, /, = → an toàn trong cookie/URL)
            var token = Guid.CreateVersion7().ToString("N"); // 32 chars hex, đủ ngẫu nhiên cho refresh token

            _dbContext.RefreshTokens.Add(new RefreshToken
            {
                Token = token,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(AuthTokenConstants.RefreshTokenExpireDays),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            return token;
        }
    }
}
