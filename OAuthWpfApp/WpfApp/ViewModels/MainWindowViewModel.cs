using System.Windows.Input;
using WpfApp.Core;
using WpfApp.Models;
using WpfApp.Views;

namespace WpfApp.ViewModels;

public class MainWindowViewModel : ObservableObject
{
    private object _currentView;
    public ICommand ChangeCurrentViewCommand { get; }

    public object CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            OnPropertyChanged();
        }
    }

    public MainWindowViewModel()
    {
        ChangeCurrentViewCommand = new RelayCommand(ChangeCurrentView);
        var mainView = new MainView();
        mainView.DataContext = new MainViewModel(ChangeCurrentViewCommand);
        CurrentView = mainView;
    }
    
    private void ChangeCurrentView(object obj)
    {
        CurrentView = obj;
    }
    
}