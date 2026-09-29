using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Services;

public interface ICourseService
{
    Task<PagedResult<CourseModel>> GetAllAsync(ListQuery query);
    Task<CourseModel> GetByIdAsync(int id);
    Task<CourseModel> CreateAsync(CourseModel course);
    Task UpdateAsync(int id, CourseModel course);
    Task DeleteAsync(int id);
}
