using WPF_MD.Models;

namespace WPF_MD.Services;

public interface IDialogService
{
    bool? ShowEmployeeEditor(Employee? employee, out Employee? result);

    bool Confirm(string message, string title);

    void ShowInfo(string message, string title);

    void ShowError(string message, string title);

    string? PickOpenJsonFile();

    string? PickSaveJsonFile(string currentFilePath);
}
