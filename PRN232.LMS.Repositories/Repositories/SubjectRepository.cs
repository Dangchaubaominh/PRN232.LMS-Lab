using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Queries;

namespace PRN232.LMS.Repositories.Repositories;

public class SubjectRepository(LmsDbContext context) : Repository<Subject>(context), ISubjectRepository
{
    private static readonly Dictionary<string, Expression<Func<Subject, object>>> SortKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["subjectId"] = x => x.SubjectId,
        ["subjectCode"] = x => x.SubjectCode,
        ["subjectName"] = x => x.SubjectName,
        ["credit"] = x => x.Credit
    };

    public IReadOnlyCollection<string> SortFields => SortKeys.Keys;

    public Task<PagedEntities<Subject>> GetPageAsync(SubjectQuery query, PageRequest page) =>
        Query
            .Where(x => query.Search == null || x.SubjectName.Contains(query.Search) || x.SubjectCode.Contains(query.Search))
            .ApplySort(page.Sort, SortKeys, x => x.SubjectId)
            .ToPageAsync(page);

    public Task<Subject?> GetWithCoursesAsync(int id) =>
        Query.Include(x => x.Courses).SingleOrDefaultAsync(x => x.SubjectId == id);

    public Task<bool> HasCoursesAsync(int subjectId) =>
        Context.Courses.AnyAsync(x => x.SubjectId == subjectId);

    public Task<bool> CodeExistsAsync(string subjectCode, int? excludeSubjectId) =>
        Query.AnyAsync(x => x.SubjectCode == subjectCode && (excludeSubjectId == null || x.SubjectId != excludeSubjectId));
}
