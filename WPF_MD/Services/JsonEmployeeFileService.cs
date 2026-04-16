using System.IO;
using System.Text.Json;
using WPF_MD.Models;

namespace WPF_MD.Services;

public sealed class JsonEmployeeFileService : IEmployeeFileService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public async Task<IReadOnlyList<Employee>> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Файл данных не найден.", filePath);
        }

        await using FileStream stream = File.OpenRead(filePath);
        List<Employee>? employees = await JsonSerializer.DeserializeAsync<List<Employee>>(stream, SerializerOptions);

        if (employees is null)
        {
            throw new InvalidDataException("Не удалось прочитать данные сотрудников из JSON-файла.");
        }

        return employees;
    }

    public async Task SaveAsync(string filePath, IEnumerable<Employee> employees)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using FileStream stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, employees, SerializerOptions);
    }
}
