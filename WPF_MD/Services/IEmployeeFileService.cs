using WPF_MD.Models;

namespace WPF_MD.Services;

public interface IEmployeeFileService
{
    Task<IReadOnlyList<Employee>> LoadAsync(string filePath);

    Task SaveAsync(string filePath, IEnumerable<Employee> employees);
}
