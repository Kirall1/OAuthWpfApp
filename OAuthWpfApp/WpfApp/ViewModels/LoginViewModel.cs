using System.Windows;
using System.Windows.Input;
using WpfApp.Core;
using WpfApp.Models;

namespace WpfApp.ViewModels;

public class LoginViewModel : ObservableObject
{
    private readonly ApiManager _apiManager;
    public string Username { get; set; }
    public string Password { get; set; }
    public ICommand LoginCommand { get; }
    
    public ICommand ReturnToMainCommand { get; }

    public LoginViewModel(ApiManager apiManager, ICommand returnToMainCommand)
    {
        _apiManager = apiManager;
        LoginCommand = new RelayCommand(async o => await Login());
        ReturnToMainCommand = returnToMainCommand;
    }

    private async Task Login()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            MessageBox.Show("Fields cannot be empty");
            return;
        }

        try
        {
            var response = await _apiManager.AuthenticateUser(new User { Username = Username, Password = Password });
            MessageBox.Show(response);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }
}
