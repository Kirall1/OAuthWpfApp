using System.Windows;
using System.Windows.Controls;
using WpfApp.ViewModels;

namespace WpfApp.Views;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel vm)
        {
            vm.Password = ((PasswordBox)sender).Password;
        }
    }
}