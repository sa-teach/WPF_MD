using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using WPF_MD.Infrastructure;
using WPF_MD.Models;
using WPF_MD.Services;

namespace WPF_MD.ViewModels;

public sealed class MainWindowViewModel : BindableBase
{
    private const string AllDepartmentsItem = "Все отделы";
    private readonly IEmployeeFileService _employeeFileService;
    private readonly IDialogService _dialogService;
    private readonly string _defaultFilePath;
    private string _searchText = string.Empty;
    private string _selectedDepartment = AllDepartmentsItem;
    private string _currentFilePath;
    private Employee? _selectedEmployee;
    private bool _isBusy;

    public MainWindowViewModel(IEmployeeFileService employeeFileService, IDialogService dialogService)
    {
        _employeeFileService = employeeFileService;
        _dialogService = dialogService;
        _defaultFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "employees.json");
        _currentFilePath = _defaultFilePath;

        Employees = [];
        Departments = [AllDepartmentsItem];
        EmployeesView = CollectionViewSource.GetDefaultView(Employees);
        EmployeesView.Filter = FilterEmployee;

        AddCommand = new RelayCommand(AddEmployee);
        EditCommand = new RelayCommand(EditEmployee, CanEditOrDeleteEmployee);
        DeleteCommand = new RelayCommand(DeleteEmployee, CanEditOrDeleteEmployee);
        OpenCommand = new RelayCommand(EditEmployee, CanEditOrDeleteEmployee);
        LoadCommand = new RelayCommand(async () => await LoadFromSelectedFileAsync(), () => !IsBusy);
        SaveCommand = new RelayCommand(async () => await SaveAsync(), () => !IsBusy);
        SaveAsCommand = new RelayCommand(async () => await SaveAsAsync(), () => !IsBusy);
    }

    public ObservableCollection<Employee> Employees { get; }

    public ObservableCollection<string> Departments { get; }

    public ICollectionView EmployeesView { get; }

    public RelayCommand AddCommand { get; }

    public RelayCommand EditCommand { get; }

    public RelayCommand DeleteCommand { get; }

    public RelayCommand OpenCommand { get; }

    public RelayCommand LoadCommand { get; }

    public RelayCommand SaveCommand { get; }

    public RelayCommand SaveAsCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                EmployeesView.Refresh();
            }
        }
    }

    public string SelectedDepartment
    {
        get => _selectedDepartment;
        set
        {
            if (SetProperty(ref _selectedDepartment, value))
            {
                EmployeesView.Refresh();
            }
        }
    }

    public Employee? SelectedEmployee
    {
        get => _selectedEmployee;
        set
        {
            if (SetProperty(ref _selectedEmployee, value))
            {
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                OpenCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string CurrentFilePath
    {
        get => _currentFilePath;
        private set => SetProperty(ref _currentFilePath, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                LoadCommand.RaiseCanExecuteChanged();
                SaveCommand.RaiseCanExecuteChanged();
                SaveAsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public async Task InitializeAsync()
    {
        await LoadFromFileAsync(_defaultFilePath, showSuccessMessage: false);
    }

    private bool FilterEmployee(object item)
    {
        if (item is not Employee employee)
        {
            return false;
        }

        bool matchesDepartment = SelectedDepartment == AllDepartmentsItem
            || string.Equals(employee.Department, SelectedDepartment, StringComparison.OrdinalIgnoreCase);

        if (!matchesDepartment)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        string search = SearchText.Trim();
        return employee.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)
            || employee.Position.Contains(search, StringComparison.OrdinalIgnoreCase)
            || employee.Department.Contains(search, StringComparison.OrdinalIgnoreCase)
            || employee.Email.Contains(search, StringComparison.OrdinalIgnoreCase)
            || employee.Phone.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private bool CanEditOrDeleteEmployee()
    {
        return SelectedEmployee is not null;
    }

    private void AddEmployee()
    {
        bool? dialogResult = _dialogService.ShowEmployeeEditor(null, out Employee? employee);
        if (dialogResult == true && employee is not null)
        {
            Employees.Add(employee);
            SelectedEmployee = employee;
            RefreshDepartments();
            EmployeesView.Refresh();
        }
    }

    private void EditEmployee()
    {
        if (SelectedEmployee is null)
        {
            return;
        }

        bool? dialogResult = _dialogService.ShowEmployeeEditor(SelectedEmployee, out Employee? updatedEmployee);
        if (dialogResult != true || updatedEmployee is null)
        {
            return;
        }

        int index = Employees.IndexOf(SelectedEmployee);
        if (index < 0)
        {
            return;
        }

        Employees[index] = updatedEmployee;
        SelectedEmployee = updatedEmployee;
        RefreshDepartments();
        EmployeesView.Refresh();
    }

    private void DeleteEmployee()
    {
        if (SelectedEmployee is null)
        {
            return;
        }

        if (!_dialogService.Confirm($"Удалить сотрудника \"{SelectedEmployee.FullName}\"?", "Подтверждение удаления"))
        {
            return;
        }

        Employees.Remove(SelectedEmployee);
        SelectedEmployee = null;
        RefreshDepartments();
        EmployeesView.Refresh();
    }

    private async Task LoadFromSelectedFileAsync()
    {
        string? filePath = _dialogService.PickOpenJsonFile();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        await LoadFromFileAsync(filePath, showSuccessMessage: true);
    }

    private async Task LoadFromFileAsync(string filePath, bool showSuccessMessage)
    {
        try
        {
            IsBusy = true;
            IReadOnlyList<Employee> employees = await _employeeFileService.LoadAsync(filePath);

            Employees.Clear();
            foreach (Employee employee in employees)
            {
                Employees.Add(employee);
            }

            CurrentFilePath = filePath;
            SelectedEmployee = Employees.FirstOrDefault();
            RefreshDepartments();
            EmployeesView.Refresh();

            if (showSuccessMessage)
            {
                _dialogService.ShowInfo("Данные успешно загружены из JSON-файла.", "Загрузка завершена");
            }
        }
        catch (Exception exception)
        {
            _dialogService.ShowError($"Не удалось загрузить файл.\n\n{exception.Message}", "Ошибка загрузки");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        await SaveToFileAsync(CurrentFilePath, showSuccessMessage: true);
    }

    private async Task SaveAsAsync()
    {
        string? filePath = _dialogService.PickSaveJsonFile(CurrentFilePath);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        await SaveToFileAsync(filePath, showSuccessMessage: true);
    }

    private async Task SaveToFileAsync(string filePath, bool showSuccessMessage)
    {
        try
        {
            IsBusy = true;
            await _employeeFileService.SaveAsync(filePath, Employees);
            CurrentFilePath = filePath;

            if (showSuccessMessage)
            {
                _dialogService.ShowInfo("Изменения успешно сохранены в JSON-файл.", "Сохранение завершено");
            }
        }
        catch (Exception exception)
        {
            _dialogService.ShowError($"Не удалось сохранить файл.\n\n{exception.Message}", "Ошибка сохранения");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void RefreshDepartments()
    {
        string previousValue = SelectedDepartment;
        string[] actualDepartments = Employees
            .Select(employee => employee.Department)
            .Where(department => !string.IsNullOrWhiteSpace(department))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(department => department)
            .ToArray();

        Departments.Clear();
        Departments.Add(AllDepartmentsItem);

        foreach (string department in actualDepartments)
        {
            Departments.Add(department);
        }

        SelectedDepartment = Departments.Contains(previousValue)
            ? previousValue
            : AllDepartmentsItem;
    }
}
