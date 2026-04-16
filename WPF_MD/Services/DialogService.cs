using System.IO;
using System.Windows;
using Microsoft.Win32;
using WPF_MD.Models;
using WPF_MD.ViewModels;
using WPF_MD.Views;

namespace WPF_MD.Services;

public sealed class DialogService : IDialogService
{
    public bool? ShowEmployeeEditor(Employee? employee, out Employee? result)
    {
        EmployeeDetailsViewModel viewModel = new(employee);
        EmployeeDetailsWindow window = new()
        {
            Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(window => window.IsActive),
            DataContext = viewModel
        };

        viewModel.CloseRequested += (_, dialogResult) =>
        {
            window.DialogResult = dialogResult;
            window.Close();
        };

        bool? dialogResult = window.ShowDialog();
        result = dialogResult == true ? viewModel.BuildEmployee() : null;
        return dialogResult;
    }

    public bool Confirm(string message, string title)
    {
        MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public void ShowInfo(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public string? PickOpenJsonFile()
    {
        OpenFileDialog dialog = new()
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? PickSaveJsonFile(string currentFilePath)
    {
        SaveFileDialog dialog = new()
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            FileName = Path.GetFileName(currentFilePath),
            InitialDirectory = Path.GetDirectoryName(currentFilePath)
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
