using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Services;

public interface ISemesterService
{
    Task<PagedResult<SemesterModel>> GetAllAsync(ListQuery query);
    Task<SemesterModel> GetByIdAsync(int id);
    Task<SemesterModel> CreateAsync(SemesterModel semester);
    Task UpdateAsync(int id, SemesterModel semester);
    Task DeleteAsync(int id);
}
