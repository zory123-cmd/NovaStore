# NovaStore — современный маркетплейс

**NovaStore** — полноценный интернет-магазин (аналог Ozon / Wildberries), построенный на **.NET 9** и **React 18** с использованием **Clean Architecture**. Включает каталог товаров с иерархией категорий, корзину, оформление заказов с промокодами, систему отзывов и рейтингов, управление профилем и адресами, а также админ-панель.

---

## Архитектура

Проект следует принципам **Clean Architecture** с чётким разделением на четыре слоя:

```
┌──────────────────────────────────────┐
│         Frontend (React + TS)        │
│         nova-store-ui/               │
├──────────────────────────────────────┤
│    WebApi (ASP.NET Core)             │
│    NovaStore.WebApi — Controllers,   │
│    Middleware, Swagger, HealthChecks │
├──────────────────────────────────────┤
│    Application (Services, DTOs)      │
│    NovaStore.Application — бизнес-   │
│    логика, валидация (FluentValidation)│
├──────────────────────────────────────┤
│    Infrastructure (Data, EF Core)    │
│    NovaStore.Infrastructure — DbCtx, │
│    миграции, Options (Jwt, Promo)    │
├──────────────────────────────────────┤
│    Domain (Models, Enums)            │
│    NovaStore.Domain — чистые         │
│    сущности, без EF Core             │
└──────────────────────────────────────┘
```

- **Domain** — чистые модели (`Product`, `Category`, `Order`, `CartItem`, `Review`, `User`, `Address`, `OrderItem`) и перечисления (`OrderStatus`, `PaymentStatus`, `ShippingStatus`). Не зависит от EF Core и внешних фреймворков.
- **Application** — сервисы бизнес-логики, DTO, интерфейсы, валидаторы FluentValidation, единый `PasswordValidator`.
- **Infrastructure** — `DbContext` с миграциями EF Core, Options-классы (`JwtSettings`, `PromoCodesSettings`), реализация репозиториев и инфраструктурных сервисов.
- **WebApi** — контроллеры, JWT-аутентификация, middleware обработки ошибок, Swagger, health checks, rate limiting, CSP с nonce-токенами.
- **Frontend** — React 19 + TypeScript + Vite + Tailwind CSS.

---

## Стек технологий

### Backend
| Компонент | Технология |
|-----------|-----------|
| Язык | C# 13 / .NET 9 |
| Фреймворк | ASP.NET Core Web API |
| ORM | Entity Framework Core 9 |
| База данных | PostgreSQL 16 |
| Кэш | Redis 7 |
| Аутентификация | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Валидация | FluentValidation 11 |
| Логирование | Serilog (Console + File) |
| Health Checks | `AspNetCore.HealthChecks.NpgSql` + `AspNetCore.HealthChecks.Redis` |
| Тестирование | xUnit + Moq + `Microsoft.AspNetCore.Mvc.Testing` |
| Хеширование паролей | BCrypt.Net-Next |

### Frontend
| Компонент | Технология |
|-----------|-----------|
| Язык | TypeScript 5.8 |
| Фреймворк | React 19 |
| Сборщик | Vite 6 |
| Стили | Tailwind CSS 3 |
| HTTP-клиент | Axios |
| Маршрутизация | React Router DOM 7 |
| Уведомления | react-hot-toast |

---

## Функциональность

### Товары и категории
- Иерархическая структура категорий (родитель → подкатегории)
- Поиск товаров по названию, категории, цене, бренду
- Фильтрация, сортировка, пагинация
- Изображения, бренды, рейтинг, остатки на складе

### Корзина
- Добавление / удаление товаров
- Обновление количества (с проверкой остатков)
- Автоматический расчёт суммы
- Очистка корзины

### Заказы
- Оформление заказа из корзины
- Выбор адреса доставки и способа оплаты
- **Промокоды**: `WELCOME10` (скидка 10%), `SAVE50` (скидка 500 руб.)
- **Автомат состояний заказа** — валидация переходов между статусами:
  - `Created → Confirmed`, `Created → Cancelled`
  - `Confirmed → Shipped`, `Confirmed → Cancelled`
  - `Shipped → Delivered`
  - `Delivered`, `Cancelled` — конечные состояния (переход запрещён)
- Статусы оплаты: `Pending → Paid → Failed → Refunded`
- Статусы доставки: `Pending → Shipped → Delivered`
- Отмена заказа через `POST /api/orders/{id}/cancel` (до отгрузки)
- Автоматическое списание товара со склада

### Отзывы
- Рейтинг от 1 до 5
- Защита от дубликатов (один отзыв на товар от пользователя)
- Возможность редактирования и удаления своего отзыва
- Атомарное сохранение рейтинга и отзыва (транзакционная целостность)

### Пользователи
- Регистрация и JWT-аутентификация
- Профиль (ФИО, телефон, аватар)
- Смена пароля
- Адресная книга (множественные адреса с флагом `IsDefault`)

### Администрирование
- Роль `Admin` (управление товарами, категориями, заказами)
- CRUD для товаров и категорий
- Обновление статусов заказов

---

## Структура проекта

```
C:\opencode\NovaStore\
├── .env.example                # Шаблон переменных окружения
├── docker-compose.yml          # PostgreSQL + Redis + API
├── Dockerfile                  # Multi-stage .NET 9 build
├── NovaStore.sln               # Решение .NET
│
├── src/
│   ├── NovaStore.Domain/       # Чистые модели и перечисления
│   │   ├── Models/
│   │   │   ├── Product.cs
│   │   │   ├── Category.cs
│   │   │   ├── Order.cs
│   │   │   ├── OrderItem.cs
│   │   │   ├── CartItem.cs
│   │   │   ├── Review.cs
│   │   │   ├── User.cs
│   │   │   ├── Address.cs
│   │   │   └── Enums/
│   │   │       ├── OrderStatus.cs
│   │   │       ├── PaymentStatus.cs
│   │   │       └── ShippingStatus.cs
│   │   └── NovaStore.Domain.csproj
│   │
│   ├── NovaStore.Application/  # Бизнес-логика, DTO, валидация
│   │   ├── Common/
│   │   │   └── PasswordValidator.cs
│   │   ├── DTOs/
│   │   │   └── MarketplaceDto.cs
│   │   ├── Interfaces/
│   │   │   ├── IAuthService.cs
│   │   │   ├── IProductService.cs
│   │   │   ├── ICategoryService.cs
│   │   │   ├── ICartService.cs
│   │   │   ├── IOrderService.cs
│   │   │   ├── IReviewService.cs
│   │   │   └── IUserService.cs
│   │   ├── Services/
│   │   │   ├── AuthService.cs
│   │   │   ├── ProductService.cs
│   │   │   ├── CategoryService.cs
│   │   │   ├── CartService.cs
│   │   │   ├── OrderService.cs
│   │   │   ├── ReviewService.cs
│   │   │   └── UserService.cs
│   │   └── NovaStore.Application.csproj
│   │
│   ├── NovaStore.Infrastructure/  # EF Core, миграции, конфигурация
│   │   ├── Data/
│   │   │   ├── NovaStoreDbContext.cs
│   │   │   └── Migrations/
│   │   │       ├── 20260522141521_InitialCreate.cs
│   │   │       ├── 20260522141521_InitialCreate.Designer.cs
│   │   │       └── NovaStoreDbContextModelSnapshot.cs
│   │   ├── Options/
│   │   │   ├── JwtSettings.cs
│   │   │   └── PromoCodesSettings.cs
│   │   └── NovaStore.Infrastructure.csproj
│   │
│   └── NovaStore.WebApi/       # ASP.NET Core Web API
│       ├── Controllers/
│       │   ├── BaseController.cs        # Абстрактный базовый контроллер
│       │   ├── AuthController.cs
│       │   ├── ProductsController.cs
│       │   ├── CategoriesController.cs
│       │   ├── CartController.cs
│       │   ├── OrdersController.cs
│       │   ├── ReviewsController.cs
│       │   └── UsersController.cs
│       ├── Middleware/
│       │   └── ErrorHandlingMiddleware.cs
│       ├── Program.cs
│       ├── appsettings.json
│       └── appsettings.Development.json
│
├── nova-store-ui/              # React + TypeScript Frontend
│   ├── src/
│   │   ├── pages/
│   │   ├── components/
│   │   ├── services/
│   │   ├── contexts/
│   │   ├── types/
│   │   └── App.tsx
│   ├── index.html
│   ├── package.json
│   ├── vite.config.ts
│   ├── tailwind.config.js
│   └── tsconfig.json
│
└── tests/
    └── NovaStore.WebApi.Tests/ # xUnit + Moq тесты (всего 50)
        ├── Controllers/
        │   ├── AuthControllerTests.cs
        │   ├── CartControllerTests.cs
        │   ├── CategoriesControllerTests.cs
        │   ├── OrdersControllerTests.cs
        │   ├── ProductsControllerTests.cs
        │   ├── ReviewsControllerTests.cs
        │   └── UsersControllerTests.cs
        ├── Services/
        │   └── OrderServiceTests.cs
        └── NovaStore.WebApi.Tests.csproj
```

---

## API Endpoints

### Аутентификация (`/api/auth`)
| Метод | Путь | Аутентификация | Описание |
|-------|------|---------------|----------|
| POST | `/api/auth/register` | — | Регистрация нового пользователя |
| POST | `/api/auth/login` | — | Вход, получение JWT-токена |

### Товары (`/api/products`)
| Метод | Путь | Аутентификация | Описание |
|-------|------|---------------|----------|
| GET | `/api/products` | — | Поиск товаров с фильтрацией и пагинацией |
| GET | `/api/products/{id}` | — | Получение товара по ID |
| GET | `/api/products/category/{id}` | — | Товары по категории (с пагинацией) |
| POST | `/api/products` | Admin | Создание товара |
| PUT | `/api/products/{id}` | Admin | Обновление товара |
| DELETE | `/api/products/{id}` | Admin | Удаление товара |

### Категории (`/api/categories`)
| Метод | Путь | Аутентификация | Описание |
|-------|------|---------------|----------|
| GET | `/api/categories` | — | Все категории (иерархически) |
| GET | `/api/categories/root` | — | Корневые категории |
| GET | `/api/categories/{id}` | — | Категория по ID |
| GET | `/api/categories/{id}/subcategories` | — | Подкатегории |
| POST | `/api/categories` | Admin | Создание категории |
| PUT | `/api/categories/{id}` | Admin | Обновление категории |
| DELETE | `/api/categories/{id}` | Admin | Удаление категории |

### Корзина (`/api/cart`)
| Метод | Путь | Аутентификация | Описание |
|-------|------|---------------|----------|
| GET | `/api/cart` | JWT | Корзина текущего пользователя |
| POST | `/api/cart` | JWT | Добавление товара в корзину |
| PUT | `/api/cart/{id}` | JWT | Обновление количества |
| DELETE | `/api/cart/{id}` | JWT | Удаление товара из корзины |
| DELETE | `/api/cart` | JWT | Очистка корзины |

### Заказы (`/api/orders`)
| Метод | Путь | Аутентификация | Описание |
|-------|------|---------------|----------|
| GET | `/api/orders` | Admin | Все заказы |
| GET | `/api/orders/my` | JWT | Заказы текущего пользователя |
| GET | `/api/orders/{id}` | JWT | Заказ по ID |
| POST | `/api/orders` | JWT | Оформление заказа из корзины |
| PUT | `/api/orders/{id}/status` | Admin | Обновление статуса заказа |
| POST | `/api/orders/{id}/cancel` | JWT | Отмена заказа (до отгрузки) |
| DELETE | `/api/orders/{id}` | Admin | Удаление заказа |

### Отзывы (`/api/reviews`)
| Метод | Путь | Аутентификация | Описание |
|-------|------|---------------|----------|
| GET | `/api/reviews/product/{productId}` | — | Отзывы на товар (с пагинацией) |
| POST | `/api/reviews` | JWT | Создание отзыва (1 раз на товар) |
| PUT | `/api/reviews/{id}` | JWT | Редактирование своего отзыва |
| DELETE | `/api/reviews/{id}` | JWT | Удаление своего отзыва |

### Пользователи (`/api/users`)
| Метод | Путь | Аутентификация | Описание |
|-------|------|---------------|----------|
| GET | `/api/users/profile` | JWT | Профиль пользователя |
| PUT | `/api/users/profile` | JWT | Обновление профиля |
| POST | `/api/users/change-password` | JWT | Смена пароля |
| GET | `/api/users/addresses` | JWT | Список адресов |
| POST | `/api/users/addresses` | JWT | Добавление адреса |
| PUT | `/api/users/addresses/{id}` | JWT | Обновление адреса |
| DELETE | `/api/users/addresses/{id}` | JWT | Удаление адреса |

### Health
| Метод | Путь | Аутентификация | Описание |
|-------|------|---------------|----------|
| GET | `/health` | — | Health check (PostgreSQL + Redis) |

---

## Инструкция по запуску

### Через Docker (рекомендуется)

```bash
# Клонировать репозиторий
git clone <repo-url>
cd NovaStore

# Запустить все сервисы (API, PostgreSQL, Redis)
docker compose up -d

# API будет доступен на http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
# PostgreSQL: localhost:5432
# Redis: localhost:6379

# Остановить
docker compose down

# Просмотр логов
docker compose logs -f novastore-api
```

### Локальный запуск (без Docker)

**Требования:**
- .NET 9 SDK
- PostgreSQL 16 (работающий на `localhost:5432`)
- Redis 7 (работающий на `localhost:6379`)

```bash
# 1. Настроить базу данных
#    Убедитесь, что PostgreSQL запущен, база `novastore` создана,
#    или измените строку подключения в appsettings.json

# 2. Запустить API (применит миграции автоматически)
cd src/NovaStore.WebApi
dotnet run

# 3. Запустить фронтенд (в отдельном терминале)
cd nova-store-ui
npm install
npm run dev

# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
# Frontend: http://localhost:5173
```

### Настройка базы данных

При первом запуске в режиме `Development` миграции применяются автоматически. Если требуется создать миграцию вручную:

```bash
cd src/NovaStore.WebApi
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## Переменные окружения

| Переменная | Описание | Значение по умолчанию |
|-----------|----------|----------------------|
| `JWT_KEY` | Секретный ключ для подписи JWT (мин. 32 символа) | **Обязателен.** Задаётся только через переменную окружения или `.env`-файл (не читается из `appsettings.json`) |
| `ConnectionStrings__DefaultConnection` | Строка подключения к PostgreSQL | `Host=localhost;Port=5432;Database=novastore;Username=postgres;Password=postgres` |
| `ConnectionStrings__Redis` | Адрес Redis | `localhost:6379` |
| `ASPNETCORE_ENVIRONMENT` | Среда выполнения | `Development` / `Production` |

> **Важно:** `JWT_KEY` строго обязателен при любом окружении. В отличие от остальных настроек, он читается **только** из переменной окружения или `.env`-файла — fallback на `appsettings.json` отсутствует. При запуске через Docker обязательно задайте его в `.env`-файле. Ключ должен быть длиной не менее 32 символов.

JWT настройки в `appsettings.json`:

```json
{
  "Jwt": {
    "Issuer": "NovaStore",
    "Audience": "NovaStoreClient",
    "ExpireMinutes": 1440
  }
}
```

### Быстрый старт: настройка JWT_KEY

Скопируйте и заполните `.env.example`:

```bash
cp .env.example .env
# Отредактируйте .env, заменив JWT_KEY на свой секретный ключ
```

---

## Rate Limiting

| Политика | Лимит | Применение |
|----------|-------|-----------|
| `Global` | 100 запросов / мин | Все endpoints, кроме аутентификации |
| `Auth` | 5 запросов / мин | `/api/auth/register`, `/api/auth/login` |

---

## Промокоды

Промокоды настраиваются через конфигурацию (`appsettings.json` или переменные окружения) в секции `PromoCodes`:

```json
{
  "PromoCodes": {
    "WELCOME10": {
      "Type": "PERCENTAGE",
      "Value": "10"
    },
    "SAVE50": {
      "Type": "FIXED",
      "Value": "500"
    }
  }
}
```

Поддерживаются два типа скидок:

| Тип | Формат Value | Пример |
|-----|-------------|--------|
| `PERCENTAGE` | Процент скидки (0–100) | `"10"` — скидка 10% |
| `FIXED` | Фиксированная сумма в рублях | `"500"` — скидка 500 руб. |

Код промокода регистронезависим — при вводе автоматически приводится к верхнему регистру. Если секция `PromoCodes` отсутствует, промокоды недоступны.

---

## Тестирование

```bash
# Запуск всех тестов
dotnet test

# Запуск с детальным выводом
dotnet test --logger "console;verbosity=detailed"
```

Тесты написаны с использованием **xUnit** и **Moq**. **50 тестов, все проходят.** Покрытие включает:

### Контроллеры (7 файлов)
- `AuthControllerTests` — регистрация, логин, валидация, конфликты, неверные credentials
- `CartControllerTests` — работа корзины (добавление, обновление, удаление, очистка, проверка остатков)
- `CategoriesControllerTests` — CRUD категорий, иерархия, кэширование
- `OrdersControllerTests` — создание заказа, отмена, получение, валидация статусов
- `ProductsControllerTests` — CRUD товаров, фильтрация, пагинация
- `ReviewsControllerTests` — создание, редактирование, удаление отзывов, транзакционность
- `UsersControllerTests` — профиль, адреса, смена пароля

### Сервисы (1 файл)
- `OrderServiceTests` — автомат состояний заказа (валидация переходов статусов), отмена, бизнес-логика

---

## Swagger / OpenAPI

При запуске в режиме `Development` Swagger UI доступен по адресу:
```
http://localhost:5000/swagger
```

---

## Безопасность

| Мера | Описание |
|------|----------|
| **JWT-аутентификация** | Bearer-токены с настраиваемым временем жизни (по умолчанию 1440 мин.) |
| **JWT_KEY из среды** | Секретный ключ читается строго из переменной окружения — fallback на `appsettings.json` отсутствует |
| **CSP с nonce** | Content-Security-Policy использует nonce-токен вместо `'unsafe-inline'` для скриптов и стилей |
| **Единый PasswordValidator** | Централизованная валидация пароля в `Application/Common/PasswordValidator.cs` |
| **BCrypt** | Хеширование паролей через BCrypt.Net-Next |
| **Rate Limiting** | 100 запросов/мин глобально, 5 запросов/мин на аутентификацию |
| **Error Handling Middleware** | Централизованная обработка ошибок без раскрытия стека в production |

---

## Кэширование

Категории кэшируются в **Redis** на **10 минут** для ускорения загрузки каталога. Инвалидация кэша происходит при создании, обновлении или удалении категории.
