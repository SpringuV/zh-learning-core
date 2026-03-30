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
