# PRN232 LMS - Lab 1

Student: **SE193293 - Dang Chau Bao Minh**  
Class: **SE1932**


Database: **SQL Server 2022**

## Run

From this solution root, run exactly:

```text
docker compose up --build -d
```

Health: <http://localhost:8080/health>  
Swagger: <http://localhost:8080/swagger>

On startup the API applies EF Core migrations and seeds the database once. Seeded rows: **5 semesters, 10 subjects, 20 courses, 50 students, and 500 enrollments**.

> Upgrading from a database created by an older build (before migrations were introduced): run `docker compose down` once before `up`, so the database container is recreated.

Resources: `/api/semesters`, `/api/subjects`, `/api/courses`, `/api/students`, `/api/enrollments`. Collections support `search`, `sort`, `page`, `size`, `fields`, and `expand`.

## Architecture

| Project | Responsibility | Models |
|---|---|---|
| `PRN232.LMS.API` | Controllers, request validation, response shaping (`fields`), error handling middleware | Request models, Response models |
| `PRN232.LMS.Services` | Business rules, query options (search, sort, paging, expand) | Business models |
| `PRN232.LMS.Repositories` | EF Core `DbContext`, migrations, seeding, data access | Entities |

Data flows `Request -> Business model -> Entity` on the way in and `Entity -> Business model -> Response` on the way out, so entities never reach the client.

### Middleware

Registered in this order in `Program.cs`:

1. `RequestLoggingMiddleware` logs method, path, status code and execution time of every request, e.g. `GET /api/students/1 responded 200 in 3.1 ms [demo-123]`. The id comes from the client's `X-Request-Id` header (1-64 characters of `A-Z a-z 0-9 . _ : -`) or is generated, and is returned in the `X-Request-Id` response header.
2. `ExceptionHandlingMiddleware` maps service exceptions to the `{ success, message, data, errors }` envelope: `NotFoundException` 404, `BusinessRuleException` 400, `InvalidQueryException` 400, `ConflictException` 409 (e.g. deleting a semester that still has courses). Any other exception is logged with its request id and returned as a generic `500 Internal server error`, never with exception details.

Adding a migration after changing an entity:

```text
dotnet ef migrations add <Name> --project PRN232.LMS.Repositories --startup-project PRN232.LMS.API
```

## Known limitations

- Authentication and authorization are outside the Lab 1 scope.
