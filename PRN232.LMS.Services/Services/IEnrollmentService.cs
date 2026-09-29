using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Services;

public interface IEnrollmentService
{
    Task<PagedResult<EnrollmentModel>> GetAllAsync(ListQuery query);
    Task<EnrollmentModel> GetByIdAsync(int id);
    Task<EnrollmentModel> CreateAsync(EnrollmentModel enrollment);
    Task UpdateAsync(int id, EnrollmentModel enrollment);
    Task DeleteAsync(int id);
}
