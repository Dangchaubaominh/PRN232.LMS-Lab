using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Queries;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Exceptions;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Queries;

namespace PRN232.LMS.Services.Services;

public class SubjectService(ISubjectRepository subjects) : ISubjectService
{
    private const string NotFoundMessage = "Subject not found.";

    public async Task<PagedResult<SubjectModel>> GetAllAsync(ListQuery query)
    {
        var page = QueryOptions.ToPageRequest(query, subjects.SortFields, expandable: []);

        var result = await subjects.GetPageAsync(new SubjectQuery(query.Search), page);
        return result.ToPagedResult(page, x => x.ToModel());
    }

    public async Task<SubjectModel> GetByIdAsync(int id)
    {
        var subject = await subjects.GetWithCoursesAsync(id) ?? throw new NotFoundException(NotFoundMessage);

        return subject.ToModel() with { Courses = subject.Courses.Select(c => c.ToModel()).ToList() };
    }

    public async Task<SubjectModel> CreateAsync(SubjectModel subject)
    {
        var code = NormalizeCode(subject.SubjectCode);
        await EnsureCodeAvailableAsync(code, excludeSubjectId: null);

        var entity = new Subject
        {
            SubjectCode = code,
            SubjectName = subject.SubjectName.Trim(),
            Credit = subject.Credit
        };
        await subjects.AddAsync(entity);
        await subjects.SaveChangesAsync();

        return entity.ToModel();
    }

    public async Task UpdateAsync(int id, SubjectModel subject)
    {
        var entity = await subjects.FindAsync(id) ?? throw new NotFoundException(NotFoundMessage);
        var code = NormalizeCode(subject.SubjectCode);
        await EnsureCodeAvailableAsync(code, excludeSubjectId: id);

        entity.SubjectCode = code;
        entity.SubjectName = subject.SubjectName.Trim();
        entity.Credit = subject.Credit;
        await subjects.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await subjects.FindAsync(id) ?? throw new NotFoundException(NotFoundMessage);
        if (await subjects.HasCoursesAsync(id))
        {
            throw new ConflictException("Subject cannot be deleted while it still has courses.");
        }

        subjects.Remove(entity);
        await subjects.SaveChangesAsync();
    }

    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    private async Task EnsureCodeAvailableAsync(string code, int? excludeSubjectId)
    {
        if (await subjects.CodeExistsAsync(code, excludeSubjectId))
        {
            throw new BusinessRuleException("SubjectCode already exists.");
        }
    }
}
