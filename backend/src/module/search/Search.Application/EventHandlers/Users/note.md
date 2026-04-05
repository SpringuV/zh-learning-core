```
flow:
UserProfileUpdatedIntegrationEvent (từ Auth.EventBus)
↓
UserProfileUpdatedEventHandler (lắng nghe & tạo request)
↓
IUserSearchQueriesServices.PatchAsync() (Service adapter/API layer)
↓
Mediator.Send<UserPatchQueries>()
↓
UserPatchQueriesHandler (business logic)
↓
Elasticsearch
```
