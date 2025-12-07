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
using System.Windows.Shapes;
using ShoesStore.Model;
using ShoesStore.Service;
using ShoesStore.Statics;
using ShoesStore.ViewModel;

namespace ShoesStore
{
    /// <summary>
    /// Логика взаимодействия для ProductWindow.xaml
    /// </summary>
    public partial class ProductWindow : Window
    {
        private readonly ShoesStoreContext _context;
        private readonly GoodService _goodService;

        public UserViewModel UserViewModel;
        
        public List<Supplier> AllSupliers {  get; set; }

        private int _currentSortType = 0;
        private Supplier _currentSupplier;
        private readonly int _currentRoleId;
        private string _currentSearchStr;
        public Visibility AdminOrManagerVisibility { get; }
        public Visibility AdminVisibility { get; }

        /// <summary>
        /// Основной конструктор
        /// </summary>
        public ProductWindow()
        {
            InitializeComponent();
            _context = new ShoesStoreContext();
            _goodService = new GoodService(_context);

            int roleId = CurrentSession.CurrentUser?.RoleId ?? 0;
            _currentRoleId = roleId;

            this.DataContext = this;

            AdminOrManagerVisibility = (_currentRoleId == 1 || _currentRoleId == 2) ? Visibility.Visible : Visibility.Collapsed;
            AdminVisibility = (_currentRoleId == 1) ? Visibility.Visible : Visibility.Collapsed;

            LoadSuppliers();
            LoadProducts();
        }

        /// <summary>
        /// Конструктор для залогиненного пользователя
        /// </summary>    
        public ProductWindow(User user, UserViewModel userViewModel) : this()
        {
            UserViewModel = userViewModel;
            currentUserSurname.Text = userViewModel.Surname;
            currentUserName.Text = userViewModel.Name;            
            currentUserPatronymic.Text = userViewModel.Patronymic;            
        }

        private void LoadProducts()
        {
            List<GoodViewModel> goodViews = new List<GoodViewModel>();
            var goods = _context.Good.ToList();
            var goodsView = _goodService.ConvertToGoodView(goods);
            
            goodList.ItemsSource = goodsView;
        }

        private void LoadSuppliers()
        {
            AllSupliers =  _context.Supplier.OrderBy(s => s.Name).ToList();
            AllSupliers.Add(new Supplier { Id = 0, Name = "Все поставщики" });
            sortSupplier.ItemsSource = AllSupliers;
        }

        private void LoadProducts(List<GoodViewModel> sortedGoods)
        {
            goodList.ItemsSource = sortedGoods;
        }

        private void LogOutButtonClick(object sender, RoutedEventArgs e)
        {
            CurrentSession.CurrentUser = null;
            new MainWindow().Show();
            Close();
        }

        private void SortComboBoxClick(object sender, RoutedEventArgs e)
        {
            if (_goodService == null) return;

            _currentSortType = sortComboBox.SelectedIndex;
            SortData();

        }

        private void SortBySupplierSupplierClick(object sender, SelectionChangedEventArgs e)
        {
            
            if (_goodService == null || _context == null) return;
            var selectedSupplier = sortSupplier.SelectedItem as Supplier;
            if (selectedSupplier == null) { return; }

            List<Good> filteredGoods;

            if (selectedSupplier.Id == 0)
            {
                filteredGoods = _context.Good.ToList();
                List<GoodViewModel> goodViewModels = _goodService.ConvertToGoodView(filteredGoods);
                LoadProducts(goodViewModels);
            }
            else
            {
                filteredGoods = _goodService.SortBySupplier(selectedSupplier);
                List<GoodViewModel> goodViewModels = _goodService.ConvertToGoodView(filteredGoods);
                LoadProducts(goodViewModels);
            }
        }

        private void SortData()
        {
            var query = _context.Good.AsQueryable();
            if (_currentSupplier != null && _currentSupplier.Id != 0)
            {
                var selectedSupplier = sortSupplier.SelectedItem as Supplier;
                query = _goodService.SortBySupplier(selectedSupplier).AsQueryable();
            }

            switch (_currentSortType)
            {
                case 1: // По возрастанию количества
                    query = query.OrderBy(g => g.CountInStorage);
                    break;
                case 2: // По убыванию количества
                    query = query.OrderByDescending(g => g.CountInStorage);
                    break;
                case 0: // Без сортировки (или по ID по умолчанию)
                default:
                    query = query.OrderBy(g => g.Id); // Сортировка по умолчанию важна для стабильности
                    break;
            }

            if(!string.IsNullOrWhiteSpace(_currentSearchStr))
            {
                string searchStrLower = _currentSearchStr.ToLower();

                query = query.Where(g => 
                g.Article.ToLower().Contains(searchStrLower) ||
                g.Name.ToLower().Contains(searchStrLower) ||
                g.Description.ToLower().Contains(searchStrLower)
                );
            }

            var resultList = query.ToList();
            var viewModel = _goodService.ConvertToGoodView(resultList);

            goodList.ItemsSource = viewModel;
        }

        private void SearchTextByInputClick(object sender, TextChangedEventArgs e)
        {
            _currentSearchStr = search.Text;
            SortData();
        }

        // В классе ProductWindow.xaml.cs
        private void AddNewGoodClick(object sender, RoutedEventArgs e)
        {
            // 1. Создаем НОВЫЙ объект Good
            // Важно: По умолчанию Good.Id будет равен 0, что активирует режим "Добавление" в окне EditGood.
            var newGood = new Good();

            // 2. Инициализируем окно редактирования/добавления, передавая новый объект
            var editWindow = new EditGood(newGood, _context);

            // 3. Открываем окно как диалог и ждем результата
            var dialogResult = editWindow.ShowDialog();

            // 4. Проверяем, был ли результат положительным (сохранение или удаление)
            if (dialogResult == true)
            {
                // Предполагая, что у вас есть метод LoadProducts для обновления ItemsControl
                LoadProducts();
            }
        }

        private void EditItemButtonClick(object sender, RoutedEventArgs e)
        {
            // 1. Получаем объект Button, который вызвал событие
            var button = sender as Button;

            var selectedGoodViewModel = button.Tag as GoodViewModel;

            if (selectedGoodViewModel != null)
            {
                // 2. Используем ID из ViewModel, чтобы найти актуальный объект Good
                // в текущем контексте (_context). Find() - лучший способ найти сущность по PK.
                var goodToEdit = _context.Good.Find(selectedGoodViewModel.Id);

                if (goodToEdit != null)
                {
                    // 3. Передаем найденный объект Good в окно редактирования
                    var editWindow = new EditGood(goodToEdit, _context);

                    var dialogResult = editWindow.ShowDialog();

                    if (dialogResult == true)
                    {
                        LoadProducts();
                    }
                }
                else
                {
                    MessageBox.Show("Товар не найден в базе данных.", "Ошибка");
                }
            }
            else
            {
                // Эту проверку теперь можно оставить для отладки
                MessageBox.Show("Ошибка привязки: не удалось получить GoodViewModel.", "Ошибка");
            }
        }

        private void GoToOrdersClick(object sender, RoutedEventArgs e) 
        {
            new OrderWindow(_context).Show();
            Close();
        }
    }
}
