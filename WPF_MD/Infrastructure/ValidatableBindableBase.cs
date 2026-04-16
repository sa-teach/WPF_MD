using System.Collections;

namespace WPF_MD.Infrastructure;

public abstract class ValidatableBindableBase : BindableBase, System.ComponentModel.INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Count > 0;

    public event EventHandler<System.ComponentModel.DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return _errors.SelectMany(pair => pair.Value);
        }

        return _errors.TryGetValue(propertyName, out List<string>? errors)
            ? errors
            : Enumerable.Empty<string>();
    }

    protected void SetErrors(string propertyName, IEnumerable<string> errors)
    {
        List<string> errorList = errors
            .Where(error => !string.IsNullOrWhiteSpace(error))
            .Distinct()
            .ToList();

        if (errorList.Count == 0)
        {
            if (_errors.Remove(propertyName))
            {
                ErrorsChanged?.Invoke(this, new System.ComponentModel.DataErrorsChangedEventArgs(propertyName));
                OnPropertyChanged(nameof(HasErrors));
            }

            return;
        }

        _errors[propertyName] = errorList;
        ErrorsChanged?.Invoke(this, new System.ComponentModel.DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }
}
