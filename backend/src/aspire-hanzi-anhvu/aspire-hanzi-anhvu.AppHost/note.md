dotnet ef migrations add InitialIdentity --project src/module/auth/Auth.Infrastructure --startup-project src/hanzi-anhvu-hsk --context AuthIdentityDbContext

dotnet ef database update \
  --project src/module/auth/Auth.Infrastructure \
  --startup-project src/hanzi-anhvu-hsk



  // chạy để appli migration

dotnet ef migrations add InitialOutbox `
  --context OutboxMessageDbContext `
  -p .\src\module\auth\Auth.Infrastructure\ `
  --startup-project .\src\hanzi-anhvu-hsk\ `
  -o Outbox\Migrations