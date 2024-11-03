using System.Windows.Input;
using WpfApp.Core;
using WpfApp.Models;
using WpfApp.Views;

namespace WpfApp.ViewModels;

public class MainViewModel : ObservableObject
{

    public ICommand ChangeCurrentViewCommand;
    public ICommand OpenRegistrationCommand { get; }
    public ICommand OpenLoginCommand { get; }
    public ICommand OpenUserDataCommand { get; }
    public ICommand ReturnToMainCommand { get; }
    private ApiManager _apiManager;

    public MainViewModel(ICommand changeCurrentViewCommand)
    {
        OpenRegistrationCommand = new RelayCommand(o => OpenRegistration());
        OpenLoginCommand = new RelayCommand(o => OpenLogin());
        OpenUserDataCommand = new RelayCommand(o => OpenUserData());
        _apiManager = new ApiManager();
        ReturnToMainCommand = new RelayCommand(_ => ReturnToMain());
        ChangeCurrentViewCommand = changeCurrentViewCommand;
    }

    private void OpenRegistration()
    {
        var viewModel = new RegisterViewModel(_apiManager, ReturnToMainCommand);
        ChangeCurrentViewCommand.Execute(new RegisterView { DataContext = viewModel });
    }

    private void OpenLogin()
    {
        var viewModel = new LoginViewModel(_apiManager, ReturnToMainCommand);
        ChangeCurrentViewCommand.Execute(new LoginView { DataContext = viewModel });
    }

    private void OpenUserData()
    {
        var viewModel = new UserDataViewModel(_apiManager, ReturnToMainCommand);
        ChangeCurrentViewCommand.Execute(new DataView { DataContext = viewModel });
    }
    
    private void ReturnToMain()
    {
        ChangeCurrentViewCommand.Execute(new MainView { DataContext = this });
    }
}
