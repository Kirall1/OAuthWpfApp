using System.Windows;
using System.Windows.Input;
using WpfApp.Core;
using WpfApp.Models;

namespace WpfApp.ViewModels;

public class RegisterViewModel : ObservableObject
{
    private readonly ApiManager _apiManager;
    public string Username { get; set; }
    public string Password { get; set; }
    public ICommand RegisterCommand { get; }
    
    public ICommand ReturnToMainCommand { get; }

    public RegisterViewModel(ApiManager apiManager, ICommand returnToMainCommand)
    {
        _apiManager = apiManager;
        RegisterCommand = new RelayCommand(async o => await Register());
        ReturnToMainCommand = returnToMainCommand;
    }

    private async Task Register()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            MessageBox.Show("Fields cannot be empty");
            return;
        }
        if (Password.Length < 6)
        {
            MessageBox.Show("Password must be at least 6 characters long");
            return;
        }

        try
        {
            var response = await _apiManager.RegisterUser(new User { Username = Username, Password = Password });
            MessageBox.Show(response);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }
}
