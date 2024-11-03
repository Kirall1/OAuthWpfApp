using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WpfApp.Core;
using WpfApp.Models;

namespace WpfApp.ViewModels;

public class UserDataViewModel : ObservableObject
{
    private readonly ApiManager _apiManager;
    private ObservableCollection<UserData> _users;
    public ICommand LoadUserDataCommand { get; }
    public ICommand ReturnToMainCommand { get; }

    
    public ObservableCollection<UserData> Users
    {
        get => _users;
        set
        {
            _users = value;
            OnPropertyChanged();
        }
    }
    public UserDataViewModel(ApiManager apiManager, ICommand returnToMainCommand)
    {
        _apiManager = apiManager;
        LoadUserDataCommand = new RelayCommand(async o => await LoadUserData());
        ReturnToMainCommand = returnToMainCommand;
    }

    private async Task LoadUserData()
    {
        try
        {
            var data = await _apiManager.GetUserData();
            Users = new ObservableCollection<UserData>(data);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }
}
