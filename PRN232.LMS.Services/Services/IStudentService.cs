using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Services;

public interface IStudentService
{
    Task<PagedResult<StudentModel>> GetAllAsync(ListQuery query);
    Task<StudentModel> GetByIdAsync(int id);
    Task<StudentModel> CreateAsync(StudentModel student);
    Task UpdateAsync(int id, StudentModel student);
    Task DeleteAsync(int id);
}
