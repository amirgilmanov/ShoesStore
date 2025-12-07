using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ShoesStore.Helpers;
using ShoesStore.Model;
using ShoesStore.Service;
using ShoesStore.Statics;
using ShoesStore.ViewModel;

namespace ShoesStore
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ShoesStoreContext _context;
        private MessageHelper _messageHelper = new MessageHelper();
        private UserService _userService;
       
        public MainWindow()
        {
            InitializeComponent();
            _context = new ShoesStoreContext();
            _userService = new UserService(_context);
        }

        private void LogInButtonClick(object sender, RoutedEventArgs e)
        {
            string loginStr = loginEnter.Text;
            string passwordStr = passwordEnter.Password;

            var user = _userService.LogIn(loginStr, passwordStr);
            if (user == null)
            {
                _messageHelper.ShowError("Введен неверный логин или пароль! Проверьте введенные данные");
                return;
            }
            else
            {
                CurrentSession.CurrentUser = user;
                var userViewModel = new UserViewModel(user);
                new ProductWindow(user, userViewModel).Show();
                Close();
            }
        }

        private void TextBlockMouseDown(object sender, MouseButtonEventArgs e)
        {
           new ProductWindow().Show();
            Close();
        }
    }
}
