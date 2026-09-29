using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Exceptions;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Queries;

namespace PRN232.LMS.Services.Services;

public class SemesterService(ILmsRepository repository) : ISemesterService
{
    private const string NotFoundMessage = "Semester not found.";

    private static readonly Dictionary<string, Expression<Func<Semester, object>>> SortKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["semesterId"] = x => x.SemesterId,
        ["semesterName"] = x => x.SemesterName,
        ["startDate"] = x => x.StartDate,
        ["endDate"] = x => x.EndDate
    };

    private static readonly string[] Expandable = ["courses"];

    public Task<PagedResult<SemesterModel>> GetAllAsync(ListQuery query)
    {
        QueryHelper.EnsureSupported(query, SortKeys, Expandable);
        var includeCourses = QueryHelper.SplitList(query.Expand).Contains("courses");

        var semesters = repository.Semesters
            .Where(x => query.Search == null || x.SemesterName.Contains(query.Search));
        semesters = QueryHelper.ApplySort(semesters, query.Sort, SortKeys, x => x.SemesterId);
        if (includeCourses)
        {
            semesters = semesters.Include(x => x.Courses);
        }

        return QueryHelper.ToPagedResultAsync(semesters, query, x => x.ToModel() with
        {
            Courses = includeCourses ? x.Courses.Select(c => c.ToModel()).ToList() : null
        });
    }

    public async Task<SemesterModel> GetByIdAsync(int id)
    {
        var semester = await repository.Semesters
            .Include(x => x.Courses)
            .SingleOrDefaultAsync(x => x.SemesterId == id)
            ?? throw new NotFoundException(NotFoundMessage);

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
        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        return entity.ToModel();
    }

    public async Task UpdateAsync(int id, SemesterModel semester)
    {
        var entity = await repository.FindAsync<Semester>(id) ?? throw new NotFoundException(NotFoundMessage);
        EnsureValidDateRange(semester);

        entity.SemesterName = semester.SemesterName.Trim();
        entity.StartDate = semester.StartDate;
        entity.EndDate = semester.EndDate;
        await repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await repository.FindAsync<Semester>(id) ?? throw new NotFoundException(NotFoundMessage);

        repository.Remove(entity);
        await repository.SaveChangesAsync();
    }

    private static void EnsureValidDateRange(SemesterModel semester)
    {
        if (semester.EndDate <= semester.StartDate)
        {
            throw new BusinessRuleException("EndDate must be after StartDate.");
        }
    }
}
