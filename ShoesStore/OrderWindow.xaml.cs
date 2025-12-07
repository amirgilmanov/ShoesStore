using ShoesStore.Model;
using ShoesStore.Statics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ShoesStore.ViewModel;

namespace ShoesStore
{
    public partial class OrderWindow : Window
    {
        private readonly ShoesStoreContext _context;
        public List<OrderViewModel> AllOrderViewModels { get; set; }

        public Visibility AdminOrManagerVisibility { get; }
        public Visibility AdminVisibility { get; }

        private int _currentRoleId = CurrentSession.CurrentUser?.RoleId ?? 0;

        public OrderWindow(ShoesStoreContext context)
        {
            InitializeComponent();
            _context = context;

            AdminOrManagerVisibility = (_currentRoleId == 1 || _currentRoleId == 2) ? Visibility.Visible : Visibility.Collapsed;
            AdminVisibility = (_currentRoleId == 1) ? Visibility.Visible : Visibility.Collapsed;

            this.DataContext = this;
            LoadOrders();
        }

        private void LoadOrders()
        {
            List<Order> AllOrders = _context.Order
                .Include("OrderDetails.Good")
                .Include("OrderStatus")
                .ToList();

            AllOrderViewModels = AllOrders.Select(o => new OrderViewModel(o)).ToList();

            if (orderList != null)
            {
                orderList.ItemsSource = AllOrderViewModels;
            }
        }

        private void LogOutButtonClick(object sender, RoutedEventArgs e)
        {
            CurrentSession.CurrentUser = null;
            new MainWindow().Show();
            Close();
        }

        public void AddNewOrderClick(object sender, RoutedEventArgs e)
        {
            var newOrder = new Order();
            newOrder.OrderDetails = new List<OrderDetails>();

            var editOrderWindow = new EditOrderWindow(newOrder, _context);
            var dialogResult = editOrderWindow.ShowDialog();

            if (dialogResult == true)
            {
                LoadOrders();
            }
        }

        public void EditOrderClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedOrder = button?.Tag as OrderViewModel;

            if (selectedOrder != null)
            {
                // Используем новый контекст для редактирования
                var editContext = new ShoesStoreContext();

                try
                {
                    var orderToEdit = editContext.Order
                        .Include("OrderDetails.Good")
                        .Include("OrderStatus")
                        .AsNoTracking()
                        .FirstOrDefault(o => o.Id == selectedOrder.Id);

                    if (orderToEdit != null)
                    {
                        // Присоединяем объект к новому контексту для редактирования
                        var attachedOrder = editContext.Order.Attach(orderToEdit);
                        editContext.Entry(attachedOrder).Collection(o => o.OrderDetails).Load();
                        editContext.Entry(attachedOrder).Reference(o => o.OrderStatus).Load();

                        var editWindow = new EditOrderWindow(attachedOrder, editContext);
                        var dialogResult = editWindow.ShowDialog();

                        if (dialogResult == true)
                        {
                            editContext.Dispose();
                            LoadOrders();
                        }
                        else
                        {
                            editContext.Dispose();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Заказ не найден в базе данных.", "Ошибка");
                        editContext.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке заказа: {ex.Message}", "Ошибка");
                    editContext.Dispose();
                }
            }
        }
    }
}