cd backend
dotnet ef migrations add InitialIdentity --project src/module/auth/Auth.Infrastructure --startup-project src/hanzi-anhvu-hsk --context AuthIdentityDbContext

dotnet ef database update \
 --project src/module/auth/Auth.Infrastructure \
 --startup-project src/hanzi-anhvu-hsk

// chạy để appli migration

dotnet ef migrations add InitialOutbox `  --context OutboxMessageDbContext`
-p .\src\module\auth\Auth.Infrastructure\ `  --startup-project .\src\hanzi-anhvu-hsk\`
-o Outbox\Migrations

Build project: dotnet build hanzi-anhvu.slnx (hoặc chỉ dotnet build nếu trong thư mục có file .slnx).
Run project: dotnet watch run --project aspire-hanzi-anhvu.AppHost (vì đây là Aspire AppHost, cần chỉ định project cụ thể).
Publish: dotnet publish hanzi-anhvu.slnx --configuration Release.

// user secrets, Trên Windows, user-secrets nằm ở:
%APPDATA%\Microsoft\UserSecrets<UserSecretsId>\secrets.json

Với project AppHost của bạn, UserSecretsId hiện là:
dotnet user-secrets set "Elastic:KibanaServiceToken" "<TOKEN_VALUE>" --project .\backend\src\aspire-hanzi-anhvu\aspire-hanzi-anhvu.AppHost\AspireHanziAnhVu.AppHost.csproj

dotnet user-secrets list --project .\src\aspire-hanzi-anhvu\aspire-hanzi-anhvu.AppHost\AspireHanziAnhVu.AppHost.csproj
