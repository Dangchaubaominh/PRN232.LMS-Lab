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

The API creates the schema and seeds automatically. Seeded rows: **5 semesters, 10 subjects, 20 courses, 50 students, and 500 enrollments**.

Resources: `/api/semesters`, `/api/subjects`, `/api/courses`, `/api/students`, `/api/enrollments`. Collections support `search`, `sort`, `page`, `size`, `fields`, and `expand`.

## Known limitations

- Authentication and authorization are outside the Lab 1 scope.
