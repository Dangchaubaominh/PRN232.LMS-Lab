using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Queries;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Exceptions;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Queries;

namespace PRN232.LMS.Services.Services;

public class SemesterService(ISemesterRepository semesters) : ISemesterService
{
    private const string NotFoundMessage = "Semester not found.";

    private static readonly string[] Expandable = ["courses"];

    public async Task<PagedResult<SemesterModel>> GetAllAsync(ListQuery query)
    {
        var page = QueryOptions.ToPageRequest(query, semesters.SortFields, Expandable);
        var includeCourses = QueryOptions.SplitList(query.Expand).Contains("courses");

        var result = await semesters.GetPageAsync(new SemesterQuery(query.Search, includeCourses), page);
        return result.ToPagedResult(page, x => x.ToModel() with
        {
            Courses = includeCourses ? x.Courses.Select(c => c.ToModel()).ToList() : null
        });
    }

    public async Task<SemesterModel> GetByIdAsync(int id)
    {
        var semester = await semesters.GetWithCoursesAsync(id) ?? throw new NotFoundException(NotFoundMessage);

        return semester.ToModel() with { Courses = semester.Courses.Select(c => c.ToModel()).ToList() };
    }

    public async Task<SemesterModel> CreateAsync(SemesterModel semester)
    {
        EnsureValidDateRange(semester);

        var entity = new Semester
        {
            SemesterName = semester.SemesterName.Trim(),
            StartDate = semester.StartDate,
            EndDate = semester.EndDate
        };
        await semesters.AddAsync(entity);
        await semesters.SaveChangesAsync();

        return entity.ToModel();
    }

    public async Task UpdateAsync(int id, SemesterModel semester)
    {
        var entity = await semesters.FindAsync(id) ?? throw new NotFoundException(NotFoundMessage);
        EnsureValidDateRange(semester);

        entity.SemesterName = semester.SemesterName.Trim();
        entity.StartDate = semester.StartDate;
        entity.EndDate = semester.EndDate;
        await semesters.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await semesters.FindAsync(id) ?? throw new NotFoundException(NotFoundMessage);
        if (await semesters.HasCoursesAsync(id))
        {
            throw new ConflictException("Semester cannot be deleted while it still has courses.");
        }

        semesters.Remove(entity);
        await semesters.SaveChangesAsync();
    }

    private static void EnsureValidDateRange(SemesterModel semester)
    {
        if (semester.EndDate <= semester.StartDate)
        {
            throw new BusinessRuleException("EndDate must be after StartDate.");
        }
    }
}
