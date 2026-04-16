using System.Text.RegularExpressions;
using WPF_MD.Infrastructure;
using WPF_MD.Models;

namespace WPF_MD.ViewModels;

public sealed class EmployeeDetailsViewModel : ValidatableBindableBase
{
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private readonly RelayCommand _saveCommand;
    private readonly Guid _employeeId;
    private readonly bool _isNewEmployee;
    private string _fullName = string.Empty;
    private string _position = string.Empty;
    private string _department = string.Empty;
    private string _ageText = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;

    public EmployeeDetailsViewModel(Employee? employee)
    {
        _isNewEmployee = employee is null;
        _employeeId = employee?.Id ?? Guid.NewGuid();
        _fullName = employee?.FullName ?? string.Empty;
        _position = employee?.Position ?? string.Empty;
        _department = employee?.Department ?? string.Empty;
        _ageText = employee?.Age.ToString() ?? string.Empty;
        _email = employee?.Email ?? string.Empty;
        _phone = employee?.Phone ?? string.Empty;

        _saveCommand = new RelayCommand(Save, CanSave);
        CancelCommand = new RelayCommand(Cancel);

        ValidateAll();
    }

    public event EventHandler<bool?>? CloseRequested;

    public string WindowTitle => _isNewEmployee ? "Новый сотрудник" : "Карточка сотрудника";

    public string FullName
    {
        get => _fullName;
        set
        {
            if (SetProperty(ref _fullName, value))
            {
                ValidateFullName();
            }
        }
    }

    public string Position
    {
        get => _position;
        set
        {
            if (SetProperty(ref _position, value))
            {
                ValidatePosition();
            }
        }
    }

    public string Department
    {
        get => _department;
        set
        {
            if (SetProperty(ref _department, value))
            {
                ValidateDepartment();
            }
        }
    }

    public string AgeText
    {
        get => _ageText;
        set
        {
            if (SetProperty(ref _ageText, value))
            {
                ValidateAge();
            }
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
            {
                ValidateEmail();
            }
        }
    }

    public string Phone
    {
        get => _phone;
        set
        {
            if (SetProperty(ref _phone, value))
            {
                ValidatePhone();
            }
        }
    }

    public RelayCommand SaveCommand => _saveCommand;

    public RelayCommand CancelCommand { get; }

    public Employee BuildEmployee()
    {
        return new Employee
        {
            Id = _employeeId,
            FullName = FullName.Trim(),
            Position = Position.Trim(),
            Department = Department.Trim(),
            Age = int.Parse(AgeText),
            Email = Email.Trim(),
            Phone = Phone.Trim()
        };
    }

    private bool CanSave()
    {
        return !HasErrors;
    }

    private void Save()
    {
        ValidateAll();
        if (HasErrors)
        {
            return;
        }

        CloseRequested?.Invoke(this, true);
    }

    private void Cancel()
    {
        CloseRequested?.Invoke(this, false);
    }

    private void ValidateAll()
    {
        ValidateFullName();
        ValidatePosition();
        ValidateDepartment();
        ValidateAge();
        ValidateEmail();
        ValidatePhone();
    }

    private void ValidateFullName()
    {
        SetErrors(nameof(FullName), string.IsNullOrWhiteSpace(FullName)
            ? ["Введите ФИО сотрудника."]
            : []);
        _saveCommand.RaiseCanExecuteChanged();
    }

    private void ValidatePosition()
    {
        SetErrors(nameof(Position), string.IsNullOrWhiteSpace(Position)
            ? ["Введите должность."]
            : []);
        _saveCommand.RaiseCanExecuteChanged();
    }

    private void ValidateDepartment()
    {
        SetErrors(nameof(Department), string.IsNullOrWhiteSpace(Department)
            ? ["Введите отдел."]
            : []);
        _saveCommand.RaiseCanExecuteChanged();
    }

    private void ValidateAge()
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(AgeText))
        {
            errors.Add("Введите возраст.");
        }
        else if (!int.TryParse(AgeText, out int age))
        {
            errors.Add("Возраст должен содержать только цифры.");
        }
        else if (age < 18 || age > 100)
        {
            errors.Add("Возраст должен быть в диапазоне от 18 до 100.");
        }

        SetErrors(nameof(AgeText), errors);
        _saveCommand.RaiseCanExecuteChanged();
    }

    private void ValidateEmail()
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(Email))
        {
            errors.Add("Введите email.");
        }
        else if (!EmailPattern.IsMatch(Email.Trim()))
        {
            errors.Add("Введите корректный email.");
        }

        SetErrors(nameof(Email), errors);
        _saveCommand.RaiseCanExecuteChanged();
    }

    private void ValidatePhone()
    {
        SetErrors(nameof(Phone), string.IsNullOrWhiteSpace(Phone)
            ? ["Введите номер телефона."]
            : []);
        _saveCommand.RaiseCanExecuteChanged();
    }
}
