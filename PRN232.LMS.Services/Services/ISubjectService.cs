using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Services;

public interface ISubjectService
{
    Task<PagedResult<SubjectModel>> GetAllAsync(ListQuery query);
    Task<SubjectModel> GetByIdAsync(int id);
    Task<SubjectModel> CreateAsync(SubjectModel subject);
    Task UpdateAsync(int id, SubjectModel subject);
    Task DeleteAsync(int id);
}
