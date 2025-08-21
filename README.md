Bu repo, iki mikroservisten oluşan basit bir CMS kurulumudur: **UserService** ve **ContentService**.
Amaç; bağımsız servisler, PostgreSQL, Docker, Swagger ve birim testlerle birlikte temiz bir başlangıç mimarisi sunmaktır.

- **Mimari:** Yazma akışı Domain Service üzerinden, okuma akışı hafif **CQRS Query** ile; Controller’lar ince tutuldu.
- **Dayanıklılık:** HttpClientFactory + Polly (timeout, retry, basit bulkhead).
- **Hata modeli:** Global Exception Middleware, ProblemDetails ile 422/409/503/500 dönüşleri standartlaştırır.
- **Test:** xUnit + Moq + FluentAssertions. Postman koleksiyonu ile uçtan uca senaryolar.
- **Çalıştırma:** İstersen Docker Compose, istersen manuel Docker komutları. Migration komutları README’de yer alıyor.

Hızlı test için Postman koleksiyonunu içeri aktarın ve README’deki sıralamayı izleyin.

/ContentService.Api
/ContentService.Application
/ContentService.Domain
/ContentService.Infrastructure
/UserService
/UserService.Domain
/UserService.Application
/UserService.Infrastructure
/tests/ContentService.Tests
/tests/UserService.Tests
/postman/Microservice.Arc.Basic.postman_collection.json
/README.md
/docker-compose.yml (opsiyonel)


## Hızlı Başlangıç

### 0) Önkoşullar
- Docker Desktop
- .NET SDK (8 veya 9) + EF Core Tools (`dotnet tool install --global dotnet-ef`)
- İsteğe bağlı: Postman (koleksiyon paylaşılacak)

### 1) Her şeyi Docker ile ayağa kaldır
```bash
docker compose up -d --build

dotnet ef database update \
  -p ./UserService.Infrastructure/UserService.Infrastructure.csproj \
  -s ./UserService/UserService.Api.csproj \
  --connection "Host=localhost;Port=5433;Database=users;Username=app;Password=app"

  dotnet ef database update \
  -p ./ContentService.Infrastructure/ContentService.Infrastructure.csproj \
  -s ./ContentService.Api/ContentService.Api.csproj \
  --connection "Host=localhost;Port=5434;Database=contents;Username=app;Password=app"

  dotnet build .\Microservice.Arc.Basic.sln

# ContentService yeniden paketle
docker rm -f content-api 2>$null
docker build --no-cache -t content-api -f .\ContentService.Api\Dockerfile .

docker run --rm -p 5080:8080 --name content-api `
  -e ASPNETCORE_URLS="http://+:8080" `
  -e ASPNETCORE_ENVIRONMENT="Production" `
  -e ConnectionStrings__WriteDb="Host=host.docker.internal;Port=5434;Database=contents;Username=app;Password=app" `
  -e ConnectionStrings__ReadDb="Host=host.docker.internal;Port=5434;Database=contents;Username=app;Password=app" `
  -e UserApi__BaseAddress="http://host.docker.internal:5081" `
  -e Http__TimeoutSeconds=3 -e Http__RetryCount=2 -e Http__BulkheadMaxParallel=50 `
  content-api

# UserService'i Docker'da çalıştır (DB hostta 5433)
docker rm -f user-api 2>$null
docker build --no-cache -t user-api -f .\UserService\Dockerfile .

docker run --rm -p 5081:8080 --name user-api `
  -e ASPNETCORE_URLS="http://+:8080" `
  -e ASPNETCORE_ENVIRONMENT="Production" `
  -e ConnectionStrings__WriteDb="Host=host.docker.internal;Port=5433;Database=users;Username=app;Password=app" `
  -e ConnectionStrings__ReadDb="Host=host.docker.internal;Port=5433;Database=users;Username=app;Password=app" `
  user-api

  “User’ı sildiğinizde Content oluşturma/güncelleme testleri, Author kontrolü nedeniyle 422/409 verebilir. Koleksiyonu çalıştırırken Delete User adımını en sona bırakın ya da tekrar bir kullanıcı oluşturun.”

  **Postman:** [Microservice.Arc.Basic.postman_collection.json](./postman/Microservice.Arc.Basic.postman_collection.json)
