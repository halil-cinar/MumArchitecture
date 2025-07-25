# MumArchitecture

MumArchitecture is a base architecture project designed to provide a clean, extensible, and maintainable foundation for enterprise-level applications using ASP.NET Core Razor Pages and .NET 8. The project is structured with clear separation of concerns, leveraging layered architecture and best practices for scalability and testability.

## Project Layers

### 1. **Domain Layer**
- **Purpose:** Contains core business entities, enums, DTOs, validation attributes, and business rules.
- **Key Classes:**
  - `Entity`: Base class for all entities, includes common properties like `Id`, `CreateDate`, `UpdateDate`, and `IsDeleted`.
  - `Dto`: Base class for Data Transfer Objects.
  - `User`, `Identity`, `NotificationContent`: Main entities representing users, authentication, and notification templates.
  - Validation Attributes: `LocalizedRequiredAttribute`, `LocalizedMaxLengthAttribute`, `LocalizedMinLengthAttribute` for localized validation.

### 2. **DataAccess Layer**
- **Purpose:** Handles data persistence using Entity Framework Core.
- **Key Classes/Interfaces:**
  - `IRepository<T>`: Generic repository interface for CRUD operations.
  - `DatabaseContext`: EF Core DbContext implementation.
  - `Filter<T>`, `DBQuery<T>`: Filtering and querying helpers for flexible data access.

### 3. **Business Layer**
- **Purpose:** Contains business logic, service interfaces, and implementations.
- **Key Classes/Interfaces:**
  - `ServiceBase<TEntity>`: Abstract base class for services, provides access to repositories, HTTP context, and user authentication.
  - `IIdentityService`, `IdentityService`: Handles user authentication, password management, and related workflows.
  - `INotificationContentService`: Manages notification templates with support for dynamic variables.
  - `IMailService`: Handles email sending and mailbox management.
  - `SystemResult<T>`, `PaggingResult<T>`: Standardized result wrappers for service responses.

### 4. **WebApp Layer**
- **Purpose:** Presentation layer using Razor Pages, includes TagHelpers and client-side scripts.
- **Key Components:**
  - Custom TagHelpers for grid, modal, input, and filter rendering.
  - JavaScript utilities (e.g., `grid.js`) for dynamic UI interactions.

## Core Capabilities

- **Authentication & Authorization:**  
  Managed via `IdentityService`, supporting password checks, password reset flows, and JWT-based user identification.

- **Notification System:**  
  `INotificationContentService` allows for dynamic notification templates with variable replacement, supporting multiple channels (Email, Telegram).

- **Validation:**  
  Localized validation attributes ensure user-friendly, multi-language error messages.

- **Repository Pattern:**  
  All data access is abstracted via generic repositories, supporting testability and flexibility.

- **Extensibility:**  
  The architecture is designed for easy extension with new entities, services, and UI components.

## Example: Key Classes and Methods

### `ServiceBase<TEntity>`
- Provides common service functionality, including repository access and user context.
- **Constructor:**  
  `ServiceBase(IServiceProvider serviceProvider)`
- **Protected Properties:**  
  - `Repository`: Access to the entity's repository.
  - `AuthUserId`: Current authenticated user's ID.

### `IIdentityService` / `IdentityService`
- **Methods:**
  - `Task<SystemResult<bool>> Save(IdentityDto identity)`
  - `Task<SystemResult<bool>> CheckPassword(IdentityCheckDto identity)`
  - `Task<SystemResult<bool>> ForgatPassword(string email)`
  - `Task<SystemResult<bool>> ForgatPassword(string key, IdentityCheckDto identity)`

### `INotificationContentService`
- **Methods:**
  - `Task<SystemResult<NotificationContentListDto>> Save(NotificationContentDto dto)`
  - `Task<SystemResult<NotificationContentListDto>> Get(int id)`
  - `Task<SystemResult<NotificationContentListDto>> Get(Filter<NotificationContent> filter)`
  - `Task<SystemResult<List<NotificationContentListDto>>> Get(Filter<NotificationContent> filter, Dictionary<string, string> variables)`
  - `Task<SystemResult<NotificationContentListDto>> Delete(int id)`
  - `Task<PaggingResult<NotificationContentListDto>> GetAll(Filter<NotificationContent> filter)`

### `IRepository<T>`
- **Methods:**
  - `Task<T?> Get(Expression<Func<T, bool>> filter)`
  - `Task<List<T>> GetAll(Expression<Func<T, bool>>? filter = null)`
  - `Task<T> Add(T entity)`
  - `Task<T> Update(T entity)`
  - `Task<T?> Delete(Expression<Func<T, bool>> filter)`

## Getting Started

1. **Clone the repository.**
2. **Configure your database connection** in `appsettings.json`.
3. **Run database migrations** using Entity Framework Core tools.
4. **Start the application** and access the Razor Pages UI.

## Extending the Architecture

- Add new entities to the Domain layer.
- Implement new repositories or services in the DataAccess and Business layers.
- Create new Razor Pages with TagHelpers for UI features.

  
## Versions

| Version | Date       | Changes |
|---------|------------|---------|
| 1.5.0   | 2025-07-07 | Introduced `AjaxForm` and `FormInput` Tag Helpers; integrated **FilePond** with preview plugins for video, audio, image, and PDF files. |
| 1.4.0   | 2025-07-06 | Added `MailController` with initial page layouts (in progress); extended `AuthorizationAttribute` to support `*` wildcard access; completed `Upload` method in `ExcelDataService`; automated generation of `localization.js`; integrated **NToastNotify** library for toast notifications; implemented multiple improvements to **mum.js**. |
| 1.3.0   | 2025-07-04 | MumArchitecture features and error handling improved; optional parameters added to `IMenuService` for filtering; error handling enhanced in `LogManager` and `SessionMiddleware`; default Number set in `Filter` and new methods added to `PaggingResult` and `SystemResult`; new classes introduced: `EArea`, `ExcelDataListDto`, and `IExcelDataService` (in progress); `MenuController`, `RoleController`, and `UserController` refined for better data management; Razor views updated for new features and CRUD operations; JavaScript helper functions in **Mum.js** updated; database migrations added for new fields and relationships. |
| 1.2.0   | 2025-06-25 | Added five middleware (**Security**, **RequestLog**, **Session**, **Validation**, **Caching**); introduced **Admin Panel** with basic designs and dynamic, role-based menus; provided admin UI to ease manual localization edits; adopted **MVC Areas**; generated migrations; improved tag helpers. |
| 1.1.0   | 2025-06-21 | **EnableCachingAttribute** and caching middleware added. Endpoints decorated with `EnableCaching` are now cached for a configurable duration, and the middleware operates dynamically on each incoming request. |

---


This project provides a robust starting point for scalable, maintainable, and testable enterprise applications using modern .NET technologies.
