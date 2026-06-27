# Технічна документація: Cars API

> **Версія рішення:** .NET 10 · **Дата документу:** 2025
> _Credentials замінено на `[EMAIL]`, `[PASSWORD]`, `[CONNECTION_STRING]`_

---

## 1. ОГЛЯД ПРОЄКТУ

### 1.1 Архітектура проєкту

Проєкт реалізований за патерном **Layered Architecture (N-tier / тришаровий)**, де чітко виділено три рівні відповідальності:

| Шар | Проєкт | Відповідальність |
|---|---|---|
| Presentation | `Cars.API` | HTTP-контролери, middleware, фонові задачі, DI-конфігурація |
| Business Logic | `Cars.BLL` | Бізнес-правила, сервіси, DTO, маппери |
| Data Access | `Cars.DAL` | Entity Framework Core, entities, репозиторії, міграції, сидер |

Такий вибір є стандартним для REST API на ASP.NET Core: кожен шар залежить лише від нижнього, API-контролери не знають про БД напряму, а зміни в схемі даних не торкаються контролерів.

### 1.2 Що вирішує проєкт

**Cars API** — це серверна частина застосунку для управління каталогом автомобілів з прив'язкою до виробників. Цільова аудиторія: адміністратори (повний CRUD) та анонімні / зареєстровані користувачі (перегляд).

### 1.3 Tech Stack та обґрунтування

| Технологія | Версія | Обґрунтування |
|---|---|---|
| **.NET / ASP.NET Core** | 10.0 | LTS-платформа, висока продуктивність, нативна DI/middleware |
| **EF Core + Npgsql** | 10.0.5 / 10.0.1 | Code-first ORM з підтримкою PostgreSQL, LINQ-запити, міграції |
| **ASP.NET Core Identity** | 10.0.5 | Готова система управління користувачами, ролями, хешування паролів |
| **JWT Bearer** | 10.0.5 | Stateless-автентифікація, стандарт RFC 7519 для REST API |
| **AutoMapper** | 16.1.1 | Декларативне відображення між Entity↔DTO без boilerplate-коду |
| **Quartz.NET** | 3.18.0 | Enterprise-рівень планувальник задач з CRON-виразами, вбудований в ASP.NET host |
| **Serilog** | 4.3.1 / 10.0.0 | Структурований логгер зі sink-ами (File тощо), конфігурується з `appsettings.json` |
| **Swashbuckle** | 10.1.7 | Автогенерація Swagger UI з Bearer-підтримкою для тестування API |
| **PostgreSQL** | — | Надійна open-source RDBMS, підтримка `numeric(p,s)` для грошових полів |

### 1.4 Архітектурна ASCII-схема

```
┌──────────────────────────────────────────────────────────────┐
│                         Cars.API                             │
│  ┌──────────────┐  ┌────────────────┐  ┌──────────────────┐  │
│  │ Controllers  │  │  Middlewares   │  │   Quartz Jobs    │  │
│  │ Auth         │  │ ExceptionMidd  │  │ RefreshTokens    │  │
│  │ Cars         │  └────────────────┘  │ CleanupJob       │  │
│  │ Manufactures │                      └──────────────────┘  │
│  └──────┬───────┘                                            │
│         │ DI (Scoped)                                        │
└─────────┼──────────────────────────────────────────────────-─┘
		  │
┌─────────▼──────────────────────────────────────────────────-─┐
│                         Cars.BLL                             │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────────┐ │
│  │  AuthService │  │  CarService  │  │  ManufactureService │ │
│  │  JwtService  │  │  ImageService│  └─────────────────────┘ │
│  └──────────────┘  └──────────────┘                          │
│  ┌──────────────────────────┐  ┌──────────────────────────┐  │
│  │   CarMapperProfile       │  │  ManufactureMapperProfile│  │
│  └──────────────────────────┘  └──────────────────────────┘  │
│  DTOs: Auth/* · Car/* · Manufacture/* · Common/*             │
└─────────┬────────────────────────────────────────────────────┘
		  │
┌─────────▼──────────────────────────────────────────────────-─┐
│                         Cars.DAL                             │
│  ┌──────────────────────────────────────────────────────┐    │
│  │  AppDbContext (IdentityDbContext<AppUser, AppRole>)  │    │
│  └──────────────────────────────────────────────────────┘    │
│  Entities: CarEntity · ManufactureEntity                     │
│            RefreshTokenEntity · AppUserEntity · AppRoleEntity│
│  Repositories: RefreshTokenRepository                        │
│  Seed: Seeder (roles+users) · DataSeeder (cars+manufactures) │
│  Migrations: 2 файли                                         │
│                                                              │
│                    ▼  Npgsql  ▼                              │
│            [ PostgreSQL Database ]                           │
└──────────────────────────────────────────────────────────────┘
```

---

## 2. КАРТА ПРОЄКТУ ПО ФАЙЛАХ

### 2.1 Cars.API — Presentation Layer

#### `Program.cs`

**Призначення:** точка входу, конфігурація всього middleware-pipeline та DI-контейнера.

**Ключові секції:**
- Serilog bootstrap — `Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(...).CreateLogger()`
- `AddDbContext<AppDbContext>` з Npgsql
- `AddIdentity<AppUserEntity, AppRoleEntity>` з пом'якшеними правилами паролю
- `AddJwtAuthentication(builder.Configuration)` — extension-метод
- `AddJobs(...)` — extension-метод для Quartz
- CORS policy `allowAll` (дозволяє `http://localhost:5173`)
- `ApiBehaviorOptions.InvalidModelStateResponseFactory` — формат відповіді при валідаційних помилках
- Swagger з Bearer security definition
- Реєстрація сервісів як `Scoped`
- `AddAutoMapper` з `CarMapperProfile.Assembly`
- `UseStaticFiles` для `/images/cars` → `Storage/Cars/`
- `Seeder.SeedAsync(app.Services)` в кінці

**Взаємодіє з:** всіма шарами, `DependencyInjectionExtensions`, `Seeder`

---

#### `Infrastructure/DependencyInjectionExtensions.cs`

**Призначення:** два extension-методи для `IServiceCollection`, що виносять складні конфігурації з `Program.cs`.

| Метод | Опис |
|---|---|
| `AddJwtAuthentication(IConfiguration)` | Реєструє JWT Bearer з повною валідацією (Issuer, Audience, LifeTime, ClockSkew=0) |
| `AddJobs(params (Type, string)[])` | Динамічно реєструє будь-яку кількість Quartz-задач з CRON-розкладом |

**Взаємодіє з:** `JwtSettings` (через `IConfiguration`), `Quartz`

---

#### `Controllers/AuthController.cs`

**Маршрут:** `api/auth` · **Авторизація:** відкрита (anonymous)

| Endpoint | Метод | Дія |
|---|---|---|
| `POST /register` | `RegisterAsync` | Реєстрація нового користувача |
| `POST /login` | `LoginAsync` | Вхід, повертає `AccessToken` + `RefreshToken` |
| `POST /refresh` | `RefreshAsync` | Обмін refresh-токена на новий JWT |
| `GET /confirm-email` | `ConfirmEmailAsync` | Підтвердження email за `userId` + `token` |

**Взаємодіє з:** `AuthService`, `JwtService`

---

#### `Controllers/CarsController.cs`

**Маршрут:** `api/cars` · **Авторизація:** GET — anonymous, POST/PUT/DELETE — `[Authorize(Roles = "admin")]`

| Endpoint | Метод | Дія |
|---|---|---|
| `GET /` | `GetAsync` | Пагінований список авто з фільтрацією |
| `GET /{id}` | `GetByIdAsync` | Авто за ID |
| `GET /by-price` | `GetByPriceAsync` | Авто в ціновому діапазоні |
| `POST /` | `CreateAsync` | Створення авто (`[FromForm]` з файлом) |
| `PUT /{id}` | `UpdateAsync` | Оновлення авто |
| `DELETE /{id}` | `DeleteAsync` | Видалення авто |

**Ключова деталь:** шлях до фізичного сховища (`_carsPath`) обчислюється в конструкторі через `IWebHostEnvironment.ContentRootPath` + `StaticFilesSettings`.

---

#### `Controllers/ManufacturesController.cs`

**Маршрут:** `api/manufactures` · **Авторизація:** аналогічно `CarsController`

| Endpoint | Дія |
|---|---|
| `GET /` | Пагінований список виробників |
| `GET /{id}` | Виробник за ID |
| `POST /` | Створення (тільки `admin`) |
| `PUT /{id}` | Оновлення (тільки `admin`) |
| `DELETE /{id}` | Видалення (тільки `admin`, захист від видалення якщо є авто) |

---

#### `Middlewares/ExceptionMiddleware.cs`

**Призначення:** глобальна обробка виключень, що перетворює їх на HTTP-відповіді з `ErrorResponseDto`.

| Виключення | HTTP статус | Лог-рівень |
|---|---|---|
| `ArgumentException` | 400 Bad Request | Warning |
| `InvalidOperationException` | 400 Bad Request | Warning |
| `Exception` (загальний) | 500 Internal Server Error | Error |

---

#### `Jobs/RefreshTokensCleanupJob.cs`

**Призначення:** Quartz-задача, яка виконується **щонеділі о 00:00** (CRON: `0 0 0 ? * SUN`).
Викликає `RefreshTokenRepository.DeleteExpiredOlderThanDaysAsync(7)` — видаляє протерміновані токени, старші за 7 днів.

---

#### `Settings/StaticFilesSettings.cs`

**Призначення:** статичні константи для шляхів файлового сховища.

```
StorageDir = "Storage"
CarsDir    = "Cars"
CarsUrl    = "/images/cars"
→ фізичний шлях: {ContentRoot}/Storage/Cars/
→ URL-префікс:   /images/cars/
```

---

#### `Models/ErrorResponseDto.cs`

Уніфікований формат помилок: `{ message: string, errors?: { [key]: string[] } }`.

---

### 2.2 Cars.BLL — Business Logic Layer

#### `Services/AuthService.cs`

**Призначення:** бізнес-логіка реєстрації, входу та підтвердження email.

| Метод | Що робить |
|---|---|
| `RegisterAsync(RegisterDto)` | Перевіряє унікальність email/username, створює user через `UserManager`, призначає роль `user`, генерує token підтвердження |
| `LoginAsync(LoginDto)` | Знаходить user за email, перевіряє пароль, делегує генерацію токенів `JwtService` |
| `ConfirmEmailAsync(userId, token)` | URL-decode token + `UserManager.ConfirmEmailAsync` |

**Взаємодіє з:** `UserManager<AppUserEntity>`, `RoleManager<AppRoleEntity>`, `JwtService`

---

#### `Services/JwtService.cs`

**Призначення:** генерація та оновлення JWT + Refresh Token.

| Метод | Що робить |
|---|---|
| `GenerateTokensAsync(user)` | Генерує access token (HMAC-SHA256) + refresh token (64 байти, `RandomNumberGenerator`), зберігає refresh в БД |
| `RefreshAsync(refreshToken)` | Валідує старий refresh (не протерміновий, не використаний), позначає `IsUsed=true`, генерує нову пару |
| `GenerateAccessTokenAsync` (private) | Формує Claims (Id, Name, Email, Roles, firstName, lastName, image), підписує HS256 |
| `GenerateRefreshToken` (private static) | 64 байти через `RandomNumberGenerator.Create()` → Base64 |

**Час дії:** AccessToken — `JwtSettings.ExpHours` (1 год), RefreshToken — 7 днів.

---

#### `Services/CarService.cs`

**Призначення:** CRUD для автомобілів з пагінацією, фільтрацією та управлінням зображеннями.

| Метод | Опис |
|---|---|
| `GetAllAsync(GetCarsQueryDto)` | Пагінація + `ApplyFilter` (name/manufacture/year/color/volume) |
| `GetByPriceAsync(GetCarsByPriceQueryDto)` | Фільтр за ціновим діапазоном, auto-swap якщо min > max |
| `GetByIdAsync(id)` | `Include(Manufacture)`, `AsNoTracking` |
| `CreateAsync(dto, path)` | Маппінг DTO→Entity, дефолтне зображення по виробнику, збереження файлу через `ImageService` |
| `UpdateAsync(id, dto, path)` | Маппінг в існуючу entity, розумна логіка заміни зображення |
| `DeleteAsync(id, path)` | Видалення файлу (захист дефолтних) + видалення з БД |
| `GetDefaultImageByManufactureIdAsync` (private) | Switch expression: toyota→Toyota-logo.jpg, bmw→BMW-logo.png, audi→audi-logo.png |
| `NormalizePaging` (private static) | page<1→1; pageSize<1→100; pageSize>500→500 |
| `ApplyFilter` (private static) | IQueryable switch по назві поля |
| `IsDefaultImage` (private static) | Перевірка чи URL є одним із захищених дефолтних |

---

#### `Services/ManufactureService.cs`

**Призначення:** CRUD для виробників з захистом від дублікатів та каскадного видалення.

| Метод | Захисна логіка |
|---|---|
| `CreateAsync` | Перевіряє унікальність імені (case-insensitive) |
| `UpdateAsync` | Перевіряє, що нова назва не зайнята іншим записом |
| `DeleteAsync` | Забороняє видалення якщо є пов'язані авто |

---

#### `Services/ImageService.cs`

**Призначення:** збереження та видалення фізичних файлів зображень.

| Метод | Логіка |
|---|---|
| `SaveAsync(IFormFile, path, requestPath)` | `Guid.NewGuid() + extension` → `{path}/{guid}.ext`, повертає URL |
| `DeleteIfExists(imageUrl, path, protectedUrls[])` | Захищає дефолтні зображення від видалення; видаляє лише фізичний файл |

---

#### `Settings/JwtSettings.cs`

POCO-клас для `IOptions<JwtSettings>`: `Issuer`, `Audience`, `SecretKey`, `ExpHours`.

---

#### `MapperProfiles/CarMapperProfile.cs`

Маппінги:
- `ManufactureEntity → CarManufactureDto`
- `CarEntity → CarItemDto` (з `Manufacture`)
- `CreateCarDto → CarEntity` (`Image` ігнорується, `Description` = `Description ?? Desciption`)
- `UpdateCarDto → CarEntity` (аналогічно)

#### `MapperProfiles/ManufactureMapperProfile.cs`

Маппінги: `ManufactureEntity↔ManufactureItemDto`, `CreateManufactureDto→ManufactureEntity`, `UpdateManufactureDto→ManufactureEntity`

---

#### DTOs

| Клас | Призначення |
|---|---|
| `ApiResponseDto<T>` | Обгортка `{ data: T }` для всіх успішних відповідей |
| `PagedDataDto<T>` | `{ items, totalCount, page, pageSize }` |
| `ErrorResponseDto` | `{ message, errors? }` |
| `CarItemDto` | Читання авто (включає `CarManufactureDto`) |
| `CreateCarDto` | Створення авто; `[FromForm]`, `IFormFile? Image` |
| `UpdateCarDto` | Оновлення авто (аналог) |
| `GetCarsQueryDto` | Параметри пагінації + фільтр (property/value) |
| `GetCarsByPriceQueryDto` | Пагінація + minValue/maxValue |
| `RegisterDto` | Email + UserName + Password + ім'я |
| `LoginDto` | Email + Password |
| `JwtDto` | AccessToken + RefreshToken + ExpiresAtUtc |
| `AuthResultDto` | `JwtDto` + `AuthUserDto` |
| `RegisterResultDto` | Message + UserId + ConfirmationToken |
| `RefreshTokenRequestDto` | `{ refreshToken: string }` |

---

### 2.3 Cars.DAL — Data Access Layer

#### `AppDbContext.cs`

Успадковує `IdentityDbContext<AppUserEntity, AppRoleEntity, string>`.

**DbSet-и:** `Manufactures`, `Cars`, `RefreshTokens`

**Fluent API конфігурації:**
- `ManufactureEntity`: unique index на `Name`, `MaxLength(100)`
- `CarEntity`: `numeric(10,2)` для Volume, `numeric(12,2)` для Price, FK з `OnDelete(Restrict)`
- `RefreshTokenEntity`: unique index на `Token`, CASCADE delete при видаленні User

---

#### `Entities/`

| Entity | Поля |
|---|---|
| `ManufactureEntity` | `Id`, `Name`, `Cars` (1-to-many) |
| `CarEntity` | `Id`, `Name`, `ManufactureId` (FK), `Year`, `Volume`, `Price`, `Color`, `Description?`, `Image?` |
| `RefreshTokenEntity` | `Id`, `Token`, `Expires`, `IsUsed`, `IsExpired` (computed), `UserId` (FK) |
| `AppUserEntity` | Успадковує `IdentityUser`, додає `FirstName?`, `LastName?`, `Image?`, `RefreshTokens` |
| `AppRoleEntity` | Успадковує `IdentityRole` (без додаткових полів) |

---

#### `Repositories/RefreshTokenRepository.cs`

**Призначення:** інкапсулює EF-операції для `RefreshTokenEntity`.

| Метод | Опис |
|---|---|
| `CreateAsync` | Додає та зберігає |
| `UpdateAsync` | Оновлює (позначає `IsUsed`) |
| `GetByTokenAsync(string)` | Пошук по значенню токена |
| `DeleteExpiredOlderThanDaysAsync(int days)` | Видаляє де `Expires < (UtcNow - days)`, повертає кількість |
| `RefreshTokens` (property) | `IQueryable` для зовнішнього доступу |

---

#### `Seed/Seeder.cs` + `Seed/DataSeeder.cs`

**`Seeder.SeedAsync(IServiceProvider)`** — оркестратор:
1. `context.Database.MigrateAsync()` — автоматичне застосування міграцій
2. `SeedRolesAsync` — створює ролі `admin` та `user`
3. `SeedUsersAsync` — створює `admin@[EMAIL]` (роль admin) та `user@[EMAIL]` (роль user)
4. `DataSeeder.SeedAsync(context)` — наповнює авто і виробники

**`DataSeeder.SeedAsync(AppDbContext)`** — ідемпотентний (перевіряє наявність):
- 3 виробники: Toyota, BMW, Audi
- 10 Toyota + 10 BMW + 10 Audi = **30 автомобілів**
- Кожне авто має відповідний дефолтний URL зображення

---

#### `Migrations/`

| Файл | Опис |
|---|---|
| `20260417125232_InitIdentityAuth` | Базові таблиці Identity + Manufactures + Cars |
| `20260417140323_FinalAuthFilesQuartz` | RefreshTokens + Image для AppUser + налаштування стовпців |

---

## 3. КЛЮЧОВІ ПОТОКИ

### Потік 1: Авторизація (Login → отримання JWT)

```
1. Клієнт:  POST /api/auth/login  { email, password }
			│
2. [AuthController.LoginAsync]
			│ → await _authService.LoginAsync(dto)
			│
3. [AuthService.LoginAsync]
			│ → _userManager.FindByEmailAsync(dto.Email)
			│   (якщо null → throw InvalidOperationException)
			│ → _userManager.CheckPasswordAsync(user, dto.Password)
			│   (якщо false → throw InvalidOperationException)
			│ → await _jwtService.GenerateTokensAsync(user)
			│ → await _userManager.GetRolesAsync(user)
			│
4. [JwtService.GenerateTokensAsync]
			│ → GenerateAccessTokenAsync(user)   ← HMAC-SHA256, exp=1h
			│ → GenerateRefreshToken()           ← 64 bytes RNG → Base64
			│ → refreshTokenRepository.CreateAsync(token)
			│
5. Відповідь: 200 OK
   { data: { tokens: { accessToken, refreshToken, expiresAtUtc }, user: { id, userName, email, roles } } }
```

---

### Потік 2: Refresh Token → нова пара токенів

```
1. Клієнт:  POST /api/auth/refresh  { refreshToken: "..." }
			│
2. [AuthController.RefreshAsync]
			│ → _jwtService.RefreshAsync(dto.RefreshToken)
			│
3. [JwtService.RefreshAsync]
			│ → _refreshTokenRepository.GetByTokenAsync(token)
			│   (null || IsExpired || IsUsed) → throw InvalidOperationException
			│ → _userManager.FindByIdAsync(oldToken.UserId)
			│ → oldToken.IsUsed = true
			│ → _refreshTokenRepository.UpdateAsync(oldToken)  ← токен більше не можна повторно використати
			│ → GenerateTokensAsync(user)                      ← нова пара
			│
4. Відповідь: 200 OK  { data: { accessToken, refreshToken, expiresAtUtc } }
```

---

### Потік 3: Створення автомобіля (admin-only)

```
1. Клієнт:  POST /api/cars
			Authorization: Bearer {adminAccessToken}
			Content-Type: multipart/form-data  { name, manufactureId, year, volume, price, color, image? }
			│
2. [JWT Middleware] → валідує token, встановлює ClaimsPrincipal
   [CarsController.CreateAsync] → [Authorize(Roles="admin")] ✓
			│ → _carService.CreateAsync(dto, _carsPath)
			│
3. [CarService.CreateAsync]
			│ → context.Manufactures.AnyAsync(x => x.Id == dto.ManufactureId)
			│   (false → throw ArgumentException)
			│ → _mapper.Map<CarEntity>(dto)    ← AutoMapper
			│ → GetDefaultImageByManufactureIdAsync(manufactureId)
			│   ("/images/cars/Toyota-logo.jpg" для Toyota тощо)
			│ → (якщо dto.Image != null) _imageService.SaveAsync(file, path, url)
			│     └─ Guid + extension → фізичний файл; повертає "/images/cars/{guid}.ext"
			│ → context.Cars.Add(entity)
			│ → context.SaveChangesAsync()
			│ → GetByIdAsync(entity.Id)  ← завантажує з Manufacture для відповіді
			│
4. Відповідь: 201 Created
   Location: /api/cars/{id}
   { data: { id, name, manufactureId, manufacture: {...}, year, volume, price, color, image } }
```

---

### Потік 4: Пагінований список авто з фільтром

```
1. Клієнт:  GET /api/cars?page=2&page_size=10&property=manufacture&value=BMW
			│
2. [CarsController.GetAsync]
			│ → _carService.GetAllAsync(new GetCarsQueryDto { Page=2, PageSize=10, Property="manufacture", Value="BMW" })
			│
3. [CarService.GetAllAsync]
			│ → NormalizePaging(page, pageSize)       ← захист від некоректних значень
			│ → query = context.Cars.AsNoTracking().Include(Manufacture).AsQueryable()
			│ → ApplyFilter(query, "manufacture", "BMW")
			│     └─ query.Where(x => x.Manufacture.Name.ToLower().Contains("bmw"))
			│ → totalCount = await query.CountAsync()
			│ → entities = await query.OrderBy(x => x.Id).Skip(10).Take(10).ToListAsync()
			│ → _mapper.Map<List<CarItemDto>>(entities)
			│
4. Відповідь: 200 OK
   { data: { items: [...], totalCount: 10, page: 2, pageSize: 10 } }
```

---

### Потік 5: Quartz — автоматичне очищення refresh токенів

```
[Quartz Scheduler]  CRON: "0 0 0 ? * SUN"  (щонеділі о 00:00 UTC)
			│
[RefreshTokensCleanupJob.Execute(IJobExecutionContext)]
			│ → _refreshTokenRepository.DeleteExpiredOlderThanDaysAsync(7)
			│     └─ threshold = DateTime.UtcNow.AddDays(-7)
			│     └─ WHERE Expires < threshold → RemoveRange → SaveChangesAsync
			│ → _logger.LogInformation("...Видалено refresh token: {Count}", deleted)
```

---

## 4. ЧОМУ ЦЕ ЗРОБЛЕНО САМЕ ТАК

---

### 4.1 Refresh Token як окрема Entity в БД

**а) ПРОБЛЕМА:** JWT access token не можна інвалідувати до закінчення його терміну дії. Якщо токен вкрадено — зловмисник має доступ до кінця терміну.

**б) РІШЕННЯ:** `RefreshTokenEntity` зберігається в PostgreSQL (`AppDbContext.RefreshTokens`), кожен токен має прапорець `IsUsed` і обчислене поле `IsExpired`. В `JwtService.RefreshAsync` виконується перевірка обох.

**в) ЧОМУ ПРАВИЛЬНО:** Це стандартна практика — короткоживучий access token (1 год) + довгоживучий refresh token (7 днів) у БД дає можливість явно інвалідувати сесії.

**г) АЛЬТЕРНАТИВА:** Stateful-сесії на сервері з Redis або cookie-based авторизація.

**д) ПОРІВНЯННЯ:** Поточне рішення — stateless для access token, але stateful для refresh. Redis дав би кращу масштабованість, але потребує додаткової інфраструктури.

---

### 4.2 ExceptionMiddleware — централізована обробка помилок

**а) ПРОБЛЕМА:** При відсутності централізованої обробки кожен контролер мав би окремі `try/catch` блоки, що призводить до дублювання коду.

**б) РІШЕННЯ:** `ExceptionMiddleware.InvokeAsync` перехоплює `ArgumentException` (400), `InvalidOperationException` (400) та загальний `Exception` (500), записує в лог і повертає `ErrorResponseDto`.

**в) ЧОМУ ПРАВИЛЬНО:** Middleware pipeline в ASP.NET Core — стандартне місце для cross-cutting concerns. Сервіси не залежать від HTTP-абстракцій.

**г) АЛЬТЕРНАТИВА:** `ProblemDetails` middleware або `UseExceptionHandler` із захистом stack trace.

**д) ПОРІВНЯННЯ:** Поточне рішення простіше у розумінні для навчальних цілей. В продакшн `ProblemDetails` + RFC 7807 було б стандартнішим.

---

### 4.3 AutoMapper для DTO↔Entity

**а) ПРОБЛЕМА:** Ручне копіювання полів між Entity та DTO — монотонний boilerplate-код, схильний до помилок (забуте поле, опечатка).

**б) РІШЕННЯ:** `CarMapperProfile` та `ManufactureMapperProfile` оголошують маппінги декларативно. Важливий `opt.Ignore()` для `Image` — щоб AutoMapper не затирав ручну логіку збереження файлу.

**в) ЧОМУ ПРАВИЛЬНО:** AutoMapper — найпоширеніша бібліотека маппінгу в .NET екосистемі. Профілі тестуються окремо через `AssertConfigurationIsValid()`.

**г) АЛЬТЕРНАТИВА:** Mapster або ручні extension-методи `ToDto() / ToEntity()`.

**д) ПОРІВНЯННЯ:** AutoMapper має більший community та документацію. Mapster — швидший за benchmark. Ручні методи — максимальна прозорість без рефлексії.

---

### 4.4 Scoped сервіси vs Static сервіси

**а) ПРОБЛЕМА:** Неправильний lifetime сервісу може призвести до "captive dependency" (Singleton тримає Scoped → leak або race condition) або зайвого створення об'єктів.

**б) РІШЕННЯ:**
- `ManufactureService`, `CarService`, `AuthService`, `JwtService`, `ImageService`, `RefreshTokenRepository` — зареєстровані як **`Scoped`** (один екземпляр на HTTP-запит).
- `DataSeeder`, `StaticFilesSettings` — **`static`** класи (не потребують DI, не мають стану).

**в) ЧОМУ ПРАВИЛЬНО:** `AppDbContext` (EF Core) за визначенням є Scoped, тому всі сервіси що його використовують мусять бути Scoped або Transient. Singleton + DbContext → виключення в runtime.

**г) АЛЬТЕРНАТИВА:** Transient для сервісів без стану (але витратніше через часте виділення пам'яті).

**д) ПОРІВНЯННЯ:** Scoped є найбезпечнішим вибором для веб-сервісів з DbContext.

---

### 4.5 DependencyInjectionExtensions — extension-методи

**а) ПРОБЛЕМА:** `Program.cs` розростається при додаванні нових сервісів і втрачає читабельність.

**б) РІШЕННЯ:** `AddJwtAuthentication` та `AddJobs` виведено в `DependencyInjectionExtensions.cs`. `AddJobs` — параметричний, приймає `params (Type, string)[]`, що дозволяє реєструвати довільну кількість задач без зміни методу.

**в) ЧОМУ ПРАВИЛЬНО:** Feature-based extension methods — стандартна практика в ASP.NET Core (сам фреймворк так організований).

**г) АЛЬТЕРНАТИВА:** Installer pattern (`IInstaller` + reflection) для ще більших проєктів.

**д) ПОРІВНЯННЯ:** Extension-методи простіші та прозоріші. Installer pattern доречний при десятках модулів.

---

### 4.6 Захист дефолтних зображень від видалення

**а) ПРОБЛЕМА:** При видаленні авто або заміні зображення не можна видаляти дефолтні файли (`Toyota-logo.jpg` тощо), які є спільними для багатьох записів.

**б) РІШЕННЯ:** `ImageService.DeleteIfExists(imageUrl, path, params string[] protectedUrls)` — перевіряє `protectedUrls.Any(x => x == imageUrl)` перед `File.Delete`. В `CarService` при виклику явно передаються всі 3 захищені URL.

**в) ЧОМУ ПРАВИЛЬНО:** Захист через whitelist (не blacklist) є надійнішим підходом.

**г) АЛЬТЕРНАТИВА:** Зберігати прапорець `IsDefault` у БД.

**д) ПОРІВНЯННЯ:** Поточний підхід простіший — не потребує додаткового поля в схемі.

---

### 4.7 AsNoTracking для read-only запитів

**а) ПРОБЛЕМА:** EF Core за замовчуванням відстежує всі завантажені entity (Change Tracker), що споживає пам'ять і час при великих вибірках.

**б) РІШЕННЯ:** Всі `GET`-запити в `CarService` та `ManufactureService` використовують `.AsNoTracking()`.

**в) ЧОМУ ПРАВИЛЬНО:** Стандартна оптимізація для read-only scenarios — зменшує споживання пам'яті та прискорює запити.

**г) АЛЬТЕРНАТИВА:** `AsNoTrackingWithIdentityResolution` для складних графів об'єктів.

**д) ПОРІВНЯННЯ:** `AsNoTracking` — найшвидший варіант для плоских вибірок; `AsNoTrackingWithIdentityResolution` потрібен лише при складних joins з дублікатами.

---

## 5. НАВЧАЛЬНІ КОМПРОМІСИ І ЩО Я БИ ЗРОБИВ В ПРОДАКШН

1. **Credentials в `appsettings.json`.**
   Я знаю, що зберігати рядок підключення, JWT SecretKey і паролі сидера в `appsettings.json` є спрощенням. В продакшн-проєкті я б використав **Azure Key Vault** або **User Secrets** (`dotnet user-secrets`) для dev і **environment variables** / Secret Manager для prod, тому що credentials не повинні потрапляти в систему контролю версій.

2. **Сервіси без інтерфейсів (`ICarService`, `IAuthService`).**
   Я знаю, що реєстрація конкретних класів без інтерфейсу є спрощенням. В продакшн-проєкті я б визначив інтерфейси (`ICarService`, `IAuthService` тощо), тому що це дозволяє unit-тестування через мок-об'єкти (Moq/NSubstitute) та полегшує заміну реалізацій.

3. **Відсутність unit-тестів та integration-тестів.**
   Я знаю, що відсутність тестів є критичним недоліком. В продакшн-проєкті я б написав unit-тести для сервісів (xUnit + Moq) та integration-тести для контролерів (WebApplicationFactory + testcontainers-dotnet для PostgreSQL), тому що це єдиний спосіб впевнено вносити зміни.

4. **Email підтвердження без реального відправлення.**
   Я знаю, що метод `ConfirmEmailAsync` повертає `ConfirmationToken` у тілі відповіді, а не відправляє листа — це спрощення. В продакшн-проєкті я б використав **MailKit** або **SendGrid** для відправлення email з посиланням, тому що відкривати токени через API небезпечно.

5. **`Directory.CreateDirectory` в конструкторі контролера.**
   Я знаю, що виконання I/O операцій у конструкторі контролера є поганою практикою. В продакшн-проєкті я б виносив таку ініціалізацію у `IHostedService.StartAsync` або `Program.cs`, тому що конструктор має бути швидким і без side effects.

6. **Поле `Desciption` (орфографічна помилка) в `CreateCarDto`/`UpdateCarDto`.**
   Я знаю, що збереження опечатки в API-контракті для сумісності з фронтом є технічним боргом. В продакшн-проєкті я б зробив версіонування API та виправив назву поля у новій версії.

7. **`CORS AllowAll` для localhost:5173.**
   Я знаю, що дозволяти CORS лише для локального dev-хосту є правильним для dev, але в продакшн я б додав конфігурований список origins через `IConfiguration`, а не hardcode у `Program.cs`.

8. **Відсутність rate limiting.**
   В продакшн-проєкті я б додав ASP.NET Core Rate Limiting (`AddRateLimiter`) на endpoints авторизації (`/login`, `/refresh`) для захисту від brute force атак.

9. **Логування Serilog тільки у File sink.**
   Я знаю, що в продакшн варто додати Serilog sink до **Seq** або **Elasticsearch** для централізованого збору та пошуку логів.

---

## 6. ГЛОСАРІЙ ТЕРМІНІВ

| Термін | Пояснення |
|---|---|
| **JWT (JSON Web Token)** | Стандарт RFC 7519 для передачі claims між сторонами у вигляді підписаного JSON-об'єкту |
| **Access Token** | Короткоживучий JWT (1 год), надсилається в `Authorization: Bearer` заголовку |
| **Refresh Token** | Довгоживучий рандомний токен (7 днів) для отримання нової пари токенів без повторного логіну |
| **Claims** | Пари ключ-значення всередині JWT: Id, Name, Email, Role тощо |
| **HMAC-SHA256** | Алгоритм підпису JWT — `H`ash-based `M`essage `A`uthentication `C`ode |
| **IdentityDbContext** | EF Core контекст що включає таблиці ASP.NET Identity (Users, Roles, Claims тощо) |
| **Scoped (DI lifetime)** | Один екземпляр сервісу на HTTP-запит |
| **Singleton (DI lifetime)** | Один екземпляр на весь час роботи застосунку |
| **Transient (DI lifetime)** | Новий екземпляр при кожному resolve |
| **AsNoTracking** | EF Core: не додавати entity до Change Tracker — оптимізація для read-only |
| **AutoMapper Profile** | Клас що успадковує `Profile` і оголошує маппінги між типами |
| **Quartz.NET** | .NET планувальник задач з підтримкою CRON-виразів |
| **CRON-вираз** | Рядковий формат розкладу (наприклад `0 0 0 ? * SUN` = щонеділі о 00:00) |
| **Middleware** | Компонент pipeline ASP.NET Core, що обробляє запит і/або відповідь |
| **DTO (Data Transfer Object)** | Об'єкт для передачі даних між шарами або через мережу, без бізнес-логіки |
| **Fluent API** | Спосіб конфігурації EF Core через ланцюгові виклики в `OnModelCreating` |
| **Seeder** | Клас що наповнює БД початковими даними при старті застосунку |
| **Extension Method** | Статичний метод що розширює існуючий тип без спадкування (`this IServiceCollection`) |
| **IFormFile** | ASP.NET Core абстракція для завантаженого файлу в multipart/form-data |
| **RandomNumberGenerator** | .NET клас для криптографічно безпечної генерації випадкових байтів |
| **ClockSkew** | Допустима різниця годинників між сервером і клієнтом при валідації JWT; `TimeSpan.Zero` = без допуску |
| **Npgsql** | .NET data provider для PostgreSQL |
| **`numeric(p,s)`** | PostgreSQL тип з фіксованою точністю: `p` — всього цифр, `s` — після коми |
| **DeleteBehavior.Restrict** | EF Core: заборонити видалення запису якщо є залежні записи |
| **DeleteBehavior.Cascade** | EF Core: автоматично видаляти залежні записи |
| **IOptions\<T\>** | ASP.NET Core механізм прив'язки конфігурації до POCO-класу |

---

## 7. ЙМОВІРНІ ПИТАННЯ НА ІНТЕРВ'Ю

---

**Q1: Поясни архітектуру проєкту. Чому ти поділив на 3 проєкти?**

Я реалізував тришарову архітектуру: `Cars.DAL` (доступ до даних), `Cars.BLL` (бізнес-логіка), `Cars.API` (presentation). Я вибрав такий підхід, тому що він чітко розмежовує відповідальності — контролери не мають прямого доступу до `AppDbContext`, а сервіси не залежать від HTTP-контексту. Це дозволяє змінювати БД або фреймворк незалежно від бізнес-логіки. Доказ: `CarService` залежить від `AppDbContext` безпосередньо в `Cars.BLL`, а контролер в `Cars.API` лише від `CarService`.

---

**Q2: Як працює JWT-авторизація в проєкті? Що відбувається після логіну?**

Я реалізував стандартну схему access + refresh token. Після успішного логіну в `AuthService.LoginAsync` викликається `JwtService.GenerateTokensAsync`: генерується HMAC-SHA256 JWT (термін 1 година) з claims (Id, Email, Roles тощо) та 64-байтний криптографічно безпечний refresh token через `RandomNumberGenerator`, який зберігається в таблиці `RefreshTokens` у БД. При наступних запитах клієнт надсилає access token у заголовку `Authorization: Bearer`. Коли access token спливає — клієнт викликає `POST /api/auth/refresh`, старий refresh token позначається `IsUsed=true` і генерується нова пара. Це файл `Cars.BLL/Services/JwtService.cs`.

---

**Q3: Навіщо ти зберігаєш refresh token в БД і що таке `IsUsed`?**

Я зберігаю refresh token в PostgreSQL у таблиці `RefreshTokenEntity`, тому що stateless рішення (лише JWT) не дає змоги інвалідувати токен до його закінчення. Поле `IsUsed` реалізує rotation — кожен refresh token можна використати лише один раз. Якщо зловмисник вкраде refresh token і спробує використати його після того, як законний користувач вже його використав — отримає помилку. Я усвідомлюю, що це не повністю вирішує проблему (якщо зловмисник першим використав токен), але це значно краще ніж взагалі без перевірки. Реалізація: `JwtService.RefreshAsync`, `RefreshTokenRepository`.

---

**Q4: Чому Quartz.NET, а не `BackgroundService` або `IHostedService`?**

Я вибрав Quartz.NET, тому що він підтримує CRON-вирази, що є стандартним способом задавати розклад (наприклад `0 0 0 ? * SUN` = щонеділі о 00:00). `BackgroundService` потребував би ручного підрахунку часу через `Task.Delay`. Quartz також інтегрується з ASP.NET DI через `IJob` — кожне виконання задачі отримує власний scope, тому `RefreshTokensCleanupJob` коректно отримує `RefreshTokenRepository` як Scoped-сервіс. Конфігурація в `DependencyInjectionExtensions.AddJobs`, задача в `Jobs/RefreshTokensCleanupJob.cs`.

---

**Q5: Як ти захистив endpoints від несанкціонованого доступу?**

Я використав `[Authorize(Roles = "admin")]` на POST, PUT, DELETE endpoints в `CarsController` та `ManufacturesController`. Читання (GET) відкрите для анонімних. Ролі `admin` та `user` сидяться при старті через `Seeder.cs`. Роль вбудована в claims JWT при генерації (`ClaimTypes.Role`). Middleware-pipeline: `UseAuthentication()` → `UseAuthorization()` → `UseMiddleware<ExceptionMiddleware>()` в `Program.cs`. Я усвідомлюю, що `[Authorize(Roles = "admin")]` — базовий механізм; в складнішому проєкті я б додав policy-based authorization.

---

**Q6: Поясни логіку роботи із зображеннями авто.**

Я реалізував два рівні: дефолтні зображення та завантажені. `CarService` визначає дефолтне зображення по назві виробника через switch expression в `GetDefaultImageByManufactureIdAsync`. `ImageService.SaveAsync` зберігає файл фізично під назвою `{Guid}.ext` в `Storage/Cars/` і повертає URL-шлях `/images/cars/{guid}.ext`. При видаленні або заміні викликається `ImageService.DeleteIfExists`, який перевіряє whitelist захищених URL (Toyota-logo.jpg, BMW-logo.png, audi-logo.png) — ці файли ніколи не видаляються. Static files middleware в `Program.cs` мапить фізичну папку `Storage/Cars/` на URL-префікс `/images/cars`. Файл: `Cars.BLL/Services/ImageService.cs`, `Cars.BLL/Services/CarService.cs`.

---

**Q7: Що таке `ApiResponseDto<T>` і навіщо вона?**

Я обгорнув усі успішні відповіді у `{ "data": ... }`. Це дозволяє клієнту мати однорідну структуру відповіді — завжди можна зробити `response.data` незалежно від типу. Помилки я відповідно повертаю через `ErrorResponseDto` з `{ "message": "...", "errors": {...} }`. Це розділяє success-path та error-path структурно. Я усвідомлюю, що `ApiResponseDto` без додаткових метаданих (status, code) — досить мінімалістична реалізація. В продакшн я б розглянув RFC 7807 (`ProblemDetails`).

---

**Q8: Як ти реалізував пагінацію і фільтрацію?**

Я реалізував cursor-less offset pagination: `Skip((page-1)*pageSize).Take(pageSize)`. `NormalizePaging` захищає від некоректних значень (page < 1 → 1, pageSize > 500 → 500). Фільтрація через `ApplyFilter` — switch expression по назві поля, що будує різні предикати `IQueryable`. Все це відбувається на рівні SQL завдяки `IQueryable` — я не завантажую всі дані в пам'ять перед фільтрацією. Кількість для `TotalCount` підраховується до `Skip/Take`. Реалізація: `CarService.GetAllAsync`, `CarService.ApplyFilter`.

---

**Q9: Навіщо `Seeder.SeedAsync` викликає `MigrateAsync` при старті?**

Я додав `context.Database.MigrateAsync()` на початку `Seeder.SeedAsync`, щоб додаток автоматично застосовував нові міграції при запуску, не потребуючи ручного виконання `dotnet ef database update`. Це зручно для навчального проєкту та CI/CD pipelines. Я усвідомлюю, що в продакшн автоматичне застосування міграцій при старті ризиковане (якщо міграція падає — падає весь pod). В продакшн я б виносив міграції в окремий init-container або deploy-step.

---

**Q10: Розкажи про lifecycle сервісів у проєкті. В чому різниця між Scoped і Singleton?**

Я зареєстрував всі основні сервіси (`CarService`, `AuthService`, `JwtService`, `ManufactureService`, `ImageService`, `RefreshTokenRepository`) як **Scoped** — один екземпляр на HTTP-запит. Це обов'язково, бо вони залежать від `AppDbContext`, який сам є Scoped у EF Core. Якби я зареєстрував сервіс як **Singleton** і він тримав би посилання на Scoped `AppDbContext` — це "captive dependency" проблема: DbContext жив би за межами одного запиту, що призводить до race conditions і помилок відстеження стану. `DataSeeder` та `StaticFilesSettings` — static класи без стану, тому вони взагалі не потребують DI. Конфігурація в `Program.cs`, рядки 118–126.

---

_Документ згенеровано автоматично на основі аналізу вихідного коду рішення. Усі credentials замінено на placeholder-и._
