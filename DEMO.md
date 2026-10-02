# Lab 2 demo script

A walk-through of every Lab 2 requirement, in the order of the assignment. Steps marked **Swagger** run in <http://localhost:8080/swagger>; steps marked **.http** use `PRN232.LMS.API/PRN232.LMS.API.http` (Visual Studio 17.12+ or VS Code REST Client; run its login request first). Each step names the code that implements it.

## 0. Start from scratch

```text
docker compose down -v
docker compose up --build -d
```

- <http://localhost:8080/health> -> `{"success":true,...,"data":{"status":"healthy"}}`.
- `docker compose logs api` shows the migrations and one log line per request.

## 1. Architecture (3 layers, 4 model types)

| Layer | Project | Models |
|---|---|---|
| API | `PRN232.LMS.API` (controllers only map and delegate) | `RequestModels/`, `ResponseModels/` |
| Service | `PRN232.LMS.Services` (all business rules) | `BusinessModels/` |
| Repository | `PRN232.LMS.Repositories` (EF Core, no rules) | `Entities/` |

Show `Controllers/V1/StudentsController.cs` (no logic) next to `Services/Services/StudentService.cs` (unique code/email, date of birth in the past). Entities never leave the service layer: `Services/Mappings/EntityMappings.cs` -> `API/Mappings/ResponseMappings.cs`.

## 2. Authentication, authorization, JWT (section 9)

1. **Swagger** `GET /api/v1/students` without logging in -> `401`, `"Authentication required..."`.
2. **Swagger** `POST /api/auth/login` with `{ "username": "admin", "password": "123456" }` -> `accessToken`, `refreshToken`, `expiresIn: 3600`.
3. Click **Authorize**, paste `accessToken` -> padlocks close. `GET /api/auth/me` -> `role: Admin`. `GET /api/v1/students` -> `200`.
4. Wrong password -> `401 Invalid username or password.` (same message for an unknown user).
5. Log in as `student` / `123456`, Authorize with that token: `GET` and `POST` work, `DELETE /api/v1/semesters/1` -> `403 You do not have permission...` (admin-only endpoints: every `DELETE`).
6. `POST /api/auth/refresh-token` with the refresh token -> a new pair. Send the **old** refresh token again -> `401 Invalid refresh token.`, and the new one is revoked as well (reuse detection).
7. `POST /api/auth/logout` with a refresh token -> it can no longer be refreshed.
8. Passwords and refresh tokens are stored hashed:

   ```text
   docker compose exec db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Your_password123 -C -d LMS -Q "SELECT Username, Role, PasswordHash FROM Users; SELECT TOP 3 TokenHash, ExpiresAt, RevokedAt FROM RefreshTokens"
   ```

9. Rate limiting (do this last: logins stay blocked for up to a minute): send a wrong password 11 times in a row -> the 11th answer is `429 Too many login attempts.` with `Retry-After`, even with the right password.

Code: `Controllers/AuthController.cs`, `Services/Services/AuthService.cs`, `API/Security/JwtTokenService.cs`, `API/Extensions/AuthenticationExtensions.cs`, `API/Extensions/RateLimitingExtensions.cs`, `Services/Security/BCryptPasswordHasher.cs`.

## 3. Content negotiation (section 4)

1. **Swagger** any GET, set the response media type to `application/xml` -> XML with the same data (`<response><success>true</success>...`).
2. **.http** "Content negotiation: unsupported format" (`Accept: text/csv`) -> `406`.

Code: `API/Formatters/ApiXmlOutputFormatter.cs`, `ReturnHttpNotAcceptable` in `Program.cs`.

## 4. Model binding (section 5)

| Binding | Show |
|---|---|
| Route | `GET /api/v1/students/{id}` - `[FromRoute] int id` |
| Query | `GET /api/v1/students?search=SE1900&sort=-studentCode&page=1&size=5&fields=studentCode,fullName` - `[FromQuery] ListQueryRequest` |
| Body | `POST /api/v1/students` - `[FromBody] StudentRequest` |
| Header | `POST /api/v1/students` with the `X-Request-Id` field filled (e.g. `demo-1`) - `[FromHeader]`; `docker compose logs api` shows `Student 51 (...) created [demo-1]` and the response header echoes it |

## 5. Validation (section 6)

`POST /api/v1/students` with:

```json
{ "studentCode": "AB12345", "fullName": "A", "email": "not-an-email", "phone": "call me", "dateOfBirth": "2003-01-01" }
```

-> `400 Validation failed.` with errors for `StudentCode` (custom `[FptuStudentCode]`), `FullName` (`[StringLength]`), `Email` (`[EmailAddress]`) and `Phone` (`[Phone]`). A valid code: `SE193293`, `CE18793`.

- `[RegularExpression]`: `POST /api/v1/subjects` with `"subjectCode": "PRN-232"`, or an enrollment with `"status": "Pending"`. `[Range]`: subject `"credit": 11`.
- FluentValidation: `POST /api/v1/semesters` with `endDate` before `startDate` -> `errors.EndDate: ["EndDate must be after StartDate."]` (`API/Validators/SemesterRequestValidator.cs`).

## 6. Routing and versioning (section 7)

1. Route constraint: `GET /api/v1/students/abc` -> `404 Resource not found.`
2. Nested resource: `GET /api/v1/courses/1/students?status=Active`.
3. Named route: create a student -> `Location: /api/v1/students/{id}` built by `CreatedAtRoute("GetStudentById", ...)`.
4. Versioning: switch the Swagger document selector to **V2**; compare `GET /api/v1/students/1` (`"dateOfBirth":"2001-02-02T00:00:00"`, `enrollments`) with `GET /api/v2/students/1` (`"dateOfBirth":"2001-02-02"`, `enrollmentCount`). Responses carry `api-supported-versions: 1.0, 2.0`.
5. The Lab 1 URL `/api/students` still works (rewritten to v1).

## 7. Middleware (section 8)

1. Request logging: `docker compose logs -f api`, then call any endpoint -> `GET /api/v1/students responded 200 in 4.2 ms [<request id>]` (method, path, status, time).
2. Global exception handling: stop the database and call the API:

   ```text
   docker compose stop db
   ```

   Any authorized call -> after the retries, `500` with `{"success":false,"message":"Internal server error","data":null,"errors":null}`; the log has the full exception with the same request id. Then `docker compose start db`.
3. Business errors keep the same envelope: `DELETE /api/v1/semesters/1` as admin -> `409 Semester cannot be deleted while it still has courses.`

Code: `API/Middlewares/RequestLoggingMiddleware.cs`, `API/Middlewares/ExceptionHandlingMiddleware.cs`.

## 8. Docker (section 10)

- `Dockerfile` (multi-stage) and `docker-compose.yml` (API + SQL Server with a health check; the API waits for it).
- JWT secret from the environment: `Jwt__Secret: "${JWT_SECRET:-...}"` in `docker-compose.yml`; the API refuses to start without a 32-byte secret. Show: `docker compose exec api printenv Jwt__Secret`.
- Connection string points at the `db` service: `Server=db,1433`.

## 9. Swagger (section 11)

- **Authorize** button (Bearer JWT), padlocks only on protected operations, `401`/`403` documented, one document per version, XML and JSON response types.
