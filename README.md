# Article Catalog API

RESTful API для управления каталогом статей с автоматической организацией по разделам на основе тегов.

## Описание

Article Catalog API позволяет:
- Создавать, обновлять и удалять статьи с тегами
- Автоматически организовывать статьи в разделы на основе уникальных наборов тегов
- Получать список разделов, отсортированных по количеству статей
- Получать статьи в конкретном разделе, отсортированные по дате

### Основные возможности

- **CRUD операции для статей**: создание, чтение, обновление, удаление
- **Автоматическое создание разделов**: разделы создаются автоматически для каждого уникального набора тегов
- **Регистро-независимые теги**: теги "Tag" и "tag" считаются одинаковыми
- **Сохранение порядка тегов**: порядок тегов, указанный при создании статьи, сохраняется
- **Автоматическая очистка**: пустые разделы удаляются автоматически при удалении последней статьи

## Архитектура

```
┌─────────────────────────────────────┐
│         API Layer                   │  ← Контроллеры, настройка DI
├─────────────────────────────────────┤
│      Application Layer              │  ← Сервисы приложения, DTOs, обработчики событий
├─────────────────────────────────────┤
│        Domain Layer                 │  ← Агрегаты, Value Objects, доменные события, интерфейсы репозиториев
├─────────────────────────────────────┤
│     Infrastructure Layer            │  ← Реализация репозиториев, EF Core, конфигурация БД
└─────────────────────────────────────┘
```

## Структура проекта

```
ArticleCatalog.Api/
├── ArticleCatalog.Api/              # API слой (контроллеры, настройка)
│   ├── Controllers/                 # REST API контроллеры
│   ├── Extensions/                  # Расширения для DI
│   └── Program.cs                   # Точка входа приложения
│
├── ArticleCatalog.Application/      # Application слой
│   ├── DTOs/                        # Data Transfer Objects
│   ├── EventHandlers/               # Обработчики доменных событий
│   ├── Interfaces/                  # Интерфейсы сервисов приложения
│   ├── Services/                    # Сервисы приложения
│   └── Validators/                  # FluentValidation валидаторы
│
├── ArticleCatalog.Domain/           # Domain слой
│   ├── Common/                      # Базовые классы (Entity, AggregateRoot, ValueObject)
│   ├── Entities/                    # Доменные сущности и агрегаты
│   ├── Events/                      # Доменные события
│   ├── Exceptions/                  # Доменные исключения
│   ├── Repositories/                # Интерфейсы репозиториев
│   └── ValueObjects/                # Value Objects
│
├── ArticleCatalog.Infrastructure/   # Infrastructure слой
│   ├── Configurations/              # Конфигурация EF Core
│   ├── Data/                        # DbContext
│   ├── Migrations/                  # Миграции БД
│   └── Repositories/                # Реализации репозиториев
│
└── ArticleCatalog.*.Tests/          # Unit тесты для каждого слоя
```

## Основные компоненты

### Domain Layer (Доменный слой)

#### Агрегаты

**`Article`** (`ArticleCatalog.Domain/Entities/Article.cs`)
- Корень агрегата для статей
- Инкапсулирует бизнес-логику создания, обновления и управления тегами
- Публикует доменные события при изменениях
- Валидирует инварианты (название, количество тегов, дубликаты)
- Автоматически устанавливает даты создания и обновления

**`Section`** (`ArticleCatalog.Domain/Entities/Section.cs`)
- Корень агрегата для разделов
- Создается автоматически для каждого уникального набора тегов
- Название формируется из названий тегов через запятую
- Использует `TagSetKey` для идентификации уникального набора тегов

**`Tag`** (`ArticleCatalog.Domain/Entities/Tag.cs`)
- Обычная Entity (не агрегат), используется как справочник
- Имеет `NormalizedName` для регистро-независимого поиска
- Метод `SetName()` автоматически нормализует имя

#### Value Objects

**`TagSetKey`** (`ArticleCatalog.Domain/ValueObjects/TagSetKey.cs`)
- Инкапсулирует логику создания ключа для набора тегов
- Нормализует и сортирует теги для создания уникального ключа
- Используется для группировки статей в разделы

**`SortDate`** (`ArticleCatalog.Domain/ValueObjects/SortDate.cs`)
- Определяет дату для сортировки статей
- Использует дату обновления, если она есть, иначе дату создания

#### Доменные события

**`ArticleCreatedEvent`** - публикуется при создании новой статьи
**`ArticleTagsChangedEvent`** - публикуется при изменении тегов статьи
**`ArticleDeletedEvent`** - публикуется при удалении статьи

#### Базовые классы

**`AggregateRoot<TId>`** - базовый класс для корней агрегатов с поддержкой доменных событий
**`Entity<TId>`** - базовый класс для всех доменных сущностей
**`ValueObject`** - базовый класс для Value Objects с правильной реализацией равенства

### Application Layer (Слой приложения)

#### Сервисы приложения

**`ArticleService`** (`ArticleCatalog.Application/Services/ArticleService.cs`)
- Управляет жизненным циклом статей
- Координирует работу с репозиториями и доменными событиями
- Преобразует доменные объекты в DTOs

**`SectionService`** (`ArticleCatalog.Application/Services/SectionService.cs`)
- Предоставляет методы для получения разделов и статей в разделах
- Сортирует разделы по количеству статей
- Сортирует статьи по дате обновления/создания

**`TagService`** (`ArticleCatalog.Application/Services/TagService.cs`)
- Управляет тегами (получение или создание)
- Обеспечивает регистро-независимую уникальность тегов

#### Обработчики доменных событий

**`ArticleCreatedEventHandler`** - создает раздел при создании статьи
**`ArticleTagsChangedEventHandler`** - создает раздел при изменении тегов статьи
**`ArticleDeletedEventHandler`** - удаляет пустой раздел при удалении последней статьи

**`DomainEventDispatcher`** - диспетчер для публикации доменных событий

#### DTOs

- **`ArticleDto`** - представление статьи для API
- **`SectionDto`** - представление раздела для API
- **`CreateArticleRequest`** - запрос на создание статьи
- **`UpdateArticleRequest`** - запрос на обновление статьи

#### Валидаторы

- **`CreateArticleRequestValidator`** - валидация запроса на создание статьи
- **`UpdateArticleRequestValidator`** - валидация запроса на обновление статьи

### Infrastructure Layer (Слой инфраструктуры)

#### Репозитории

**`ArticleRepository`** - реализация `IArticleRepository`
- Оптимизированные запросы с использованием `TagSetKey`
- Загрузка связанных данных через Include

**`SectionRepository`** - реализация `ISectionRepository`
- Поиск разделов по `TagSetKey`
- Подсчет статей в разделе через SQL COUNT

**`TagRepository`** - реализация `ITagRepository`
- Поиск тегов по нормализованному имени
- Регистро-независимый поиск

**`UnitOfWork`** - реализация `IUnitOfWork`
- Управление транзакциями
- Сохранение изменений в БД

#### Конфигурация EF Core

- **`ArticleConfiguration`** - конфигурация маппинга для `Article`
- **`SectionConfiguration`** - конфигурация маппинга для `Section`
- **`TagConfiguration`** - конфигурация маппинга для `Tag` с уникальным индексом на `NormalizedName`

### API Layer (Слой API)

#### Контроллеры

**`ArticlesController`** (`ArticleCatalog.Api/Controllers/ArticlesController.cs`)
- `GET /api/articles/{id}` - получение статьи по ID
- `POST /api/articles` - создание новой статьи
- `PUT /api/articles/{id}` - обновление статьи
- `DELETE /api/articles/{id}` - удаление статьи

**`SectionsController`** (`ArticleCatalog.Api/Controllers/SectionsController.cs`)
- `GET /api/sections` - получение списка всех разделов
- `GET /api/sections/{sectionId}/articles` - получение статей в разделе

## Запуск проекта

### Требования

- .NET 8.0 SDK
- PostgreSQL 16+ (или Docker)

### Локальный запуск

1. **Клонируйте репозиторий**
   ```bash
   git clone <repository-url>
   cd ArticleCatalog.Api
   ```

2. **Настройте базу данных**
   
   Обновите строку подключения в `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=article_catalog;Username=postgres;Password=your_password"
     }
   }
   ```

3. **Запустите приложение**
   ```bash
   cd ArticleCatalog.Api
   dotnet run
   ```

   Приложение автоматически применит миграции при старте.

4. **Откройте Swagger UI**
   
   Перейдите по адресу: `https://localhost:5001/swagger` (или `http://localhost:5000/swagger`)

### Запуск с Docker

1. **Запустите все сервисы**
   ```bash
   docker-compose up -d
   ```

2. **Проверьте статус**
   ```bash
   docker-compose ps
   ```

3. **Откройте API**
   
   API будет доступно по адресу: `http://localhost:8080/swagger`

4. **Остановите сервисы**
   ```bash
   docker-compose down
   ```

## API Endpoints

### Статьи

#### Создать статью
```http
POST /api/articles
Content-Type: application/json

{
  "title": "Название статьи",
  "tags": ["tag1", "tag2", "tag3"]
}
```

**Ответ:**
```json
{
  "id": "guid",
  "title": "Название статьи",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null,
  "tags": ["tag1", "tag2", "tag3"]
}
```

#### Получить статью
```http
GET /api/articles/{id}
```

#### Обновить статью
```http
PUT /api/articles/{id}
Content-Type: application/json

{
  "title": "Обновленное название",
  "tags": ["tag1", "tag2"]
}
```

#### Удалить статью
```http
DELETE /api/articles/{id}
```

### Разделы

#### Получить все разделы
```http
GET /api/sections
```

**Ответ:**
```json
[
  {
    "id": "guid",
    "title": "tag1, tag2, tag3",
    "tagCount": 3,
    "articlesCount": 5,
    "tags": ["tag1", "tag2", "tag3"]
  }
]
```

#### Получить статьи в разделе
```http
GET /api/sections/{sectionId}/articles
```

## Тестирование

### Запуск unit-тестов

```bash
# Все тесты
dotnet test

# Конкретный проект
dotnet test ArticleCatalog.Domain.Tests
dotnet test ArticleCatalog.Application.Tests
dotnet test ArticleCatalog.Api.Tests
```

### Структура тестов

- **`ArticleCatalog.Domain.Tests`** - тесты доменной логики
- **`ArticleCatalog.Application.Tests`** - тесты сервисов приложения
- **`ArticleCatalog.Api.Tests`** - тесты контроллеров

## Технологии

- **.NET 8.0** - платформа разработки
- **ASP.NET Core** - веб-фреймворк
- **Entity Framework Core 8.0** - ORM
- **PostgreSQL** - база данных
- **FluentValidation** - валидация запросов
- **Swagger/OpenAPI** - документация API
- **Docker** - контейнеризация
- **xUnit** - фреймворк для тестирования

## База данных

### Основные таблицы

- **`articles`** - статьи
- **`tags`** - теги (с уникальным индексом на `NormalizedName`)
- **`article_tags`** - связь многие-ко-многим между статьями и тегами (с полем `Position`)
- **`sections`** - разделы (с уникальным индексом на `TagSetKey`)
- **`section_tags`** - связь многие-ко-многим между разделами и тегами

### Миграции

Миграции применяются автоматически при старте приложения. Для ручного применения:

```bash
dotnet ef database update --project ArticleCatalog.Infrastructure --startup-project ArticleCatalog.Api
```

## Бизнес-логика

### Создание статьи

1. Валидация названия и тегов
2. Получение или создание тегов (регистро-независимо)
3. Создание статьи через фабричный метод `Article.Create()`
4. Установка тегов через `Article.SetTags()`
5. Сохранение в БД
6. Публикация доменного события `ArticleCreatedEvent`
7. Обработчик события создает раздел, если его еще нет

### Обновление статьи

1. Загрузка статьи из БД
2. Обновление названия через `Article.UpdateTitle()`
3. Обновление тегов через `Article.SetTags()`
4. Сохранение изменений
5. Публикация доменного события `ArticleTagsChangedEvent`
6. Обработчик события создает новый раздел, если набор тегов изменился

### Удаление статьи

1. Загрузка статьи из БД
2. Публикация доменного события `ArticleDeletedEvent`
3. Удаление статьи из БД
4. Обработчик события проверяет, остались ли статьи в разделе
5. Если раздел пуст, он удаляется автоматически

### Автоматическое создание разделов

Разделы создаются автоматически при создании или изменении статей:
- Для каждого уникального набора тегов создается один раздел
- Название раздела формируется из названий тегов через запятую
- Разделы сортируются по количеству статей (по убыванию)
- Пустые разделы удаляются автоматически

## Валидация

### Валидация на уровне API

- FluentValidation валидаторы для DTOs
- Автоматическая валидация через `AddFluentValidationAutoValidation()`

### Валидация на уровне Domain

- Валидация в фабричных методах агрегатов
- Защита инвариантов через приватные сеттеры
- Доменные исключения (`ValidationException`, `NotFoundException`)

### Правила валидации

- **Название статьи**: обязательно, максимум 256 символов
- **Теги**: минимум 1, максимум 256, без дубликатов (регистро-независимо)
- **Название тега**: обязательно, максимум 256 символов, уникально (регистро-независимо)

## Обработка ошибок

- Доменные исключения (`NotFoundException`, `ValidationException`)
- Обработка ошибок в контроллерах с логированием
- Стандартизированные ответы об ошибках

## Лицензия

См. файл [LICENSE.txt](LICENSE.txt)
