# PRN232 LMS - Lab 2 (Advanced REST API & Security)

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

On startup the API applies EF Core migrations and seeds the database once. Seeded rows: **5 semesters, 10 subjects, 20 courses, 50 students, and 500 enrollments**, plus two demo accounts:

| Username | Password | Role |
|---|---|---|
| `admin` | `123456` | Admin |
| `student` | `123456` | Student |

Every `/api/v1` and `/api/v2` endpoint requires a token. In Swagger: run `POST /api/auth/login`, copy `data.accessToken`, click **Authorize** and paste it.

The JWT signing key comes from the `Jwt__Secret` environment variable (at least 32 bytes). `docker-compose.yml` sets it from `JWT_SECRET`, with a local-only default; set your own before deploying anywhere else:

```text
JWT_SECRET=<a long random string> docker compose up --build -d
```

The API refuses to start when the secret is missing or shorter than 32 bytes. The API container runs as the image's unprivileged `app` user, not root.

> Upgrading from a database created by an older build (before migrations were introduced): run `docker compose down` once before `up`, so the database container is recreated.

Resources (v1): `/api/v1/semesters`, `/api/v1/subjects`, `/api/v1/courses`, `/api/v1/courses/{courseId}/students`, `/api/v1/students`, `/api/v1/enrollments`. Collections support `search`, `sort`, `page`, `size`, `fields`, and `expand`. The Lab 1 URLs without a version (`/api/students`, ...) still work and are served by v1.

[DEMO.md](DEMO.md) walks through every Lab 2 requirement step by step.

Swagger has one document per version: `/swagger/v1/swagger.json` and `/swagger/v2/swagger.json` (pick the version in the top-right selector).

## Architecture

| Project | Responsibility | Models |
|---|---|---|
| `PRN232.LMS.API` | Controllers, request validation, response shaping (`fields`), error handling middleware | Request models, Response models |
| `PRN232.LMS.Services` | Business rules: validating query options, normalizing input, uniqueness and reference checks, delete conflicts, token issuing | Business models |
| `PRN232.LMS.Repositories` | All data access: EF Core `DbContext`, one repository per entity (filter, sort, page, include), migrations, seeding | Entities |

Data flows `Request -> Business model -> Entity` on the way in and `Entity -> Business model -> Response` on the way out, so entities never reach the client.

Services talk to `ISemesterRepository`, `ISubjectRepository`, `ICourseRepository`, `IStudentRepository`, `IEnrollmentRepository` and `IUserRepository`; queries are described with records such as `StudentQuery` and `PageRequest` instead of `IQueryable`. EF Core is a private dependency of the Repositories project (`PrivateAssets="all"`), so the Services project cannot compile code that uses EF Core directly.

### Authentication and authorization

| Endpoint | Access | Purpose |
|---|---|---|
| `POST /api/auth/login` | anonymous, rate-limited | `{ username, password }` -> `{ accessToken, refreshToken, expiresIn }` |
| `POST /api/auth/refresh-token` | anonymous | `{ refreshToken }` -> a new token pair; the old refresh token stops working |
| `POST /api/auth/logout` | anonymous | `{ refreshToken }` -> revokes it |
| `GET /api/auth/me` | any user | the user in the access token |
| `GET`, `POST`, `PUT` on all resources | any user (`[Authorize]`) | |
| `DELETE` on all resources | Admin only (`[Authorize(Roles = "Admin")]`) | |

- Access tokens: JWT signed with HS256, 60 minutes, claims `sub`, `unique_name`, `role`, `jti`. The API validates signature, issuer, audience and lifetime (30 s clock skew).
- Refresh tokens: 64 random bytes, valid 7 days, single use. The `RefreshTokens` table stores only their SHA-256 hash. Presenting an already used refresh token revokes every active refresh token of that user (theft detection). A token is consumed with a single conditional `UPDATE ... WHERE RevokedAt IS NULL`, so two concurrent refreshes with the same token cannot both succeed.
- Login is rate-limited per client IP: 10 attempts per minute by default (`RateLimiting:LoginPermitLimit`, `RateLimiting:LoginWindowSeconds`); further attempts get `429 Too many login attempts.` with a `Retry-After` header.
- Passwords: BCrypt (work factor 11) in `Users.PasswordHash`; never stored in plain text. Login answers the same `Invalid username or password.` for an unknown user and a wrong password.
- `401` (missing, invalid or expired token: `Access token has expired.`) and `403` (wrong role) use the standard envelope and carry `WWW-Authenticate: Bearer` on 401.
- Tokens are issued by `AuthService` (Services) through `ITokenService`, implemented by `JwtTokenService` (API), which also owns the JWT settings in `JwtOptions`.

### Routing and API versioning

- Attribute routing on every controller, with route constraints (`{id:int}`, `{courseId:int}`); a URL that fails a constraint returns `404` with the standard envelope.
- Named routes (`GetStudentById`, `GetCourseById`, ...) build the `Location` header of `201 Created` responses via `CreatedAtRoute`.
- Nested resource: `GET /api/v1/courses/{courseId}/students` lists the students enrolled in a course; `?status=Active` filters by enrollment status. Unknown course: `404`.
- URL segment versioning with `Asp.Versioning` (`/api/v{version}/...`); every response carries `api-supported-versions`. Controllers live in `Controllers/V1` and `Controllers/V2`.
- **v2 (students only)**: `/api/v2/students` takes the same requests as v1 but returns `dateOfBirth` as a date (`"2001-02-02"` instead of `"2001-02-02T00:00:00"`, a breaking change) and `enrollmentCount` instead of an `enrollments` list. Other resources exist only in v1, so e.g. `/api/v2/courses` is `404`.
- Unversioned `/api/...` URLs are rewritten to `/api/v1/...` before routing (`UseRewriter`), so Lab 1 clients keep working.

### Model binding and validation

| Binding | Example |
|---|---|
| Route | `GET /api/students/{id:int}` - `[FromRoute] int id` |
| Query | `GET /api/students?search=&sort=&page=` - `[FromQuery] ListQueryRequest` |
| Body | `POST /api/students` - `[FromBody] StudentRequest` |
| Header | `POST/PUT/DELETE /api/students` - `[FromHeader(Name = "X-Request-Id")]`, written to the audit log line |

Input is validated before it reaches a service; failures return `400` with `"message": "Validation failed."` and field errors in `errors`.

- Data annotations on request models: `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[Phone]` (`StudentRequest`), `[RegularExpression]` (`SubjectRequest.SubjectCode`, `EnrollmentRequest.Status`).
- Custom rule `[FptuStudentCode]`: campus letter (H, S, D, C, Q) + program letter (E, S, A) + 5-6 digits, e.g. `SE19886`, `CE18793`.
- FluentValidation: `SemesterRequestValidator` (name required, `EndDate` after `StartDate`), run by `FluentValidationFilter`.
- Business rules that need the database (unique student code, email and subject code, existing references, date of birth in the past) stay in the services.

Students have a unique `studentCode` and an optional `phone`. Seeded students are `SE190001`...`SE190050`; the `AddStudentCodeAndPhone` migration gives existing rows a code with the same formula.

### Content negotiation

Every endpoint, including error responses, answers in the format named by the `Accept` header:

| Accept | Result |
|---|---|
| none, `*/*`, `application/json` | JSON (default) |
| `application/xml`, `text/xml` | XML |
| anything else, e.g. `text/csv` | `406 Not Acceptable` (an error such as `401` or `404` is still reported, as JSON) |

XML is produced by `ApiXmlOutputFormatter`, which converts the JSON representation, so both formats carry exactly the same data (including `fields` and `expand` results). The root element is `<response>`, arrays become repeated `<item>` elements and null values are marked `xsi:nil="true"`.

### Middleware

Registered in this order in `Program.cs`:

1. `RequestLoggingMiddleware` logs method, path, status code and execution time of every request, e.g. `GET /api/students/1 responded 200 in 3.1 ms [demo-123]`. The id comes from the client's `X-Request-Id` header (1-64 characters of `A-Z a-z 0-9 . _ : -`) or is generated, and is returned in the `X-Request-Id` response header.
2. `ExceptionHandlingMiddleware` maps service exceptions to the `{ success, message, data, errors }` envelope: `UnauthorizedException` 401, `NotFoundException` 404, `BusinessRuleException` 400, `InvalidQueryException` 400, `ConflictException` 409 (e.g. deleting a semester that still has courses). Services check uniqueness and references before writing; if a concurrent request slips in between, `LmsDbContext.SaveChangesAsync` turns the SQL Server unique-index or foreign-key error into `DataConflictException`, also returned as 409 instead of a 500. Any other exception is logged with its request id and returned as a generic `500 Internal server error`, never with exception details.

Adding a migration after changing an entity:

```text
dotnet ef migrations add <Name> --project PRN232.LMS.Repositories --startup-project PRN232.LMS.API
```

## Known limitations

- Access tokens cannot be revoked before they expire (60 minutes); logout revokes only the refresh token.
- Expired and revoked refresh tokens stay in the `RefreshTokens` table; there is no cleanup job.
- The login limit is per IP. Through Docker's port mapping every local client appears with the same gateway address, so locally the limit is shared; behind a reverse proxy, configure forwarded headers so the real client IP is used.
