using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Exceptions;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Queries;

namespace PRN232.LMS.Services.Services;

public class SubjectService(ILmsRepository repository) : ISubjectService
{
    private const string NotFoundMessage = "Subject not found.";

    private static readonly Dictionary<string, Expression<Func<Subject, object>>> SortKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["subjectId"] = x => x.SubjectId,
        ["subjectCode"] = x => x.SubjectCode,
        ["subjectName"] = x => x.SubjectName,
        ["credit"] = x => x.Credit
    };

    public Task<PagedResult<SubjectModel>> GetAllAsync(ListQuery query)
    {
        QueryHelper.EnsureSupported(query, SortKeys, expandable: []);

        var subjects = repository.Subjects
            .Where(x => query.Search == null || x.SubjectName.Contains(query.Search) || x.SubjectCode.Contains(query.Search));
        subjects = QueryHelper.ApplySort(subjects, query.Sort, SortKeys, x => x.SubjectId);

        return QueryHelper.ToPagedResultAsync(subjects, query, x => x.ToModel());
    }

    public async Task<SubjectModel> GetByIdAsync(int id)
    {
        var subject = await repository.Subjects
            .Include(x => x.Courses)
            .SingleOrDefaultAsync(x => x.SubjectId == id)
            ?? throw new NotFoundException(NotFoundMessage);

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
        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        return entity.ToModel();
    }

    public async Task UpdateAsync(int id, SubjectModel subject)
    {
        var entity = await repository.FindAsync<Subject>(id) ?? throw new NotFoundException(NotFoundMessage);
        var code = NormalizeCode(subject.SubjectCode);
        await EnsureCodeAvailableAsync(code, excludeSubjectId: id);

        entity.SubjectCode = code;
        entity.SubjectName = subject.SubjectName.Trim();
        entity.Credit = subject.Credit;
        await repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await repository.FindAsync<Subject>(id) ?? throw new NotFoundException(NotFoundMessage);

        repository.Remove(entity);
        await repository.SaveChangesAsync();
    }

    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    private async Task EnsureCodeAvailableAsync(string code, int? excludeSubjectId)
    {
        var taken = await repository.Subjects
            .AnyAsync(x => x.SubjectCode == code && (excludeSubjectId == null || x.SubjectId != excludeSubjectId));
        if (taken)
        {
            throw new BusinessRuleException("SubjectCode already exists.");
        }
    }
}
