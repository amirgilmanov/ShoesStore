using ShoesStore.Model;
using ShoesStore.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ShoesStore
{
    public partial class EditOrderWindow : Window
    {
        private readonly ShoesStoreContext _context;
        public EditOrderViewModel CurrentOrder { get; set; }
        public List<OrderStatus> AllOrderStatus { get; set; }
        public List<Good> AllGoods { get; set; }
        private Order _originalOrder;

        public EditOrderWindow(Order orderToEdit, ShoesStoreContext context)
        {
            InitializeComponent();
            _context = context;
            _originalOrder = orderToEdit;

            if (orderToEdit.Id != 0)
            {
                Title = $"Редактирование заказа №{orderToEdit.Id}";
            }
            else
            {
                Title = "Добавление нового заказа";
            }

            CurrentOrder = new EditOrderViewModel(orderToEdit);
            LoadRelatedData();

            this.DataContext = this;
        }

        public void LoadRelatedData()
        {
            // Загружаем связанные данные (статусы и товары)
            AllOrderStatus = _context.OrderStatus.ToList();
            AllGoods = _context.Good.ToList();

            // Устанавливаем выбранный статус
            if (CurrentOrder.OrderStatus != null)
            {
                selectedOrderStatus.SelectedItem = AllOrderStatus.FirstOrDefault(
                    s => s.Id == CurrentOrder.OrderStatus.Id);
            }
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Закрепляем изменения в DataGrid
                // Это важно, чтобы получить последние изменения из редактируемой ячейки
                orderDetailsDataGrid.CommitEdit();

                // 1. Проверка статуса
                if (CurrentOrder.OrderStatus == null && selectedOrderStatus.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, выберите статус заказа.", "Ошибка ввода");
                    return;
                }

                // Проверяем, если статус не был выбран через SelectedItem, но установлен через OrderStatus
                if (selectedOrderStatus.SelectedItem != null && CurrentOrder.OrderStatus == null)
                {
                    CurrentOrder.OrderStatus = selectedOrderStatus.SelectedItem as OrderStatus;
                }


                // 2. Проверка даты заказа
                DateTime orderDate = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(CurrentOrder.OrderDateString))
                {
                    if (!DateTime.TryParseExact(CurrentOrder.OrderDateString.Trim(), "dd.MM.yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out orderDate))
                    {
                        MessageBox.Show("Дата заказа введена неверно. Используйте формат DD.MM.yyyy (например: 19.10.2005)", "Ошибка ввода");
                        dateInput.Focus();
                        return;
                    }
                }

                // 3. Проверка даты доставки
                DateTime? deliveryDate = null;
                if (!string.IsNullOrWhiteSpace(CurrentOrder.DeliveryDateString))
                {
                    if (!DateTime.TryParseExact(CurrentOrder.DeliveryDateString.Trim(), "dd.MM.yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out DateTime parsedDeliveryDate))
                    {
                        MessageBox.Show("Дата доставки введена неверно. Используйте формат DD.MM.yyyy (например: 19.10.2005)", "Ошибка ввода");
                        deliveryDateInput.Focus();
                        return;
                    }
                    deliveryDate = parsedDeliveryDate;
                }

                // 4. Проверка OrderDetailsViewModels
                if (CurrentOrder.OrderDetailsViewModels.Count == 0)
                {
                    MessageBox.Show("Заказ должен содержать хотя бы один товар.", "Ошибка ввода");
                    return;
                }

                // 5. Проверка, что все OrderDetailsViewModels имеют товар и количество > 0
                foreach (var detailVM in CurrentOrder.OrderDetailsViewModels)
                {
                    if (detailVM.Good == null)
                    {
                        MessageBox.Show("Все товары в заказе должны иметь артикул. Пожалуйста, выберите товар для каждой строки.", "Ошибка ввода");
                        return;
                    }

                    if (detailVM.Quantity == null || detailVM.Quantity <= 0)
                    {
                        MessageBox.Show("Количество всех товаров должно быть больше 0.", "Ошибка ввода");
                        return;
                    }
                }

                // 6. Обновляем основные данные заказа
                _originalOrder.Date = orderDate;
                _originalOrder.DeliveryDate = deliveryDate;
                _originalOrder.OrderStatus = CurrentOrder.OrderStatus;
                _originalOrder.OrderStatusId = CurrentOrder.OrderStatus?.Id;
                _originalOrder.PickUpPointAddress = CurrentOrder.PickUpPointAddress;
                _originalOrder.ReceiptCode = CurrentOrder.ReceiptCode;

                // 7. Синхронизируем OrderDetails

                // Копируем список старых деталей для безопасного удаления
                var oldDetails = _originalOrder.OrderDetails.ToList();

                // Удаление: Ищем старые детали, которых нет в ViewModels
                foreach (var detail in oldDetails)
                {
                    // Ищем соответствующий ViewModel. OrderDetails, созданные для нового заказа (Id=0), 
                    // не будут найдены по Id, но мы их не отслеживаем через _context.OrderDetails.Remove().
                    var viewModelMatch = CurrentOrder.OrderDetailsViewModels
                        .FirstOrDefault(vm => vm.Id == detail.Id);

                    if (viewModelMatch == null && detail.Id != 0) // Только для существующих в БД деталей
                    {
                        _originalOrder.OrderDetails.Remove(detail);
                        _context.OrderDetails.Remove(detail);
                    }
                }

                // Добавление/Обновление: Проходим по ViewModels
                foreach (var detailVM in CurrentOrder.OrderDetailsViewModels)
                {
                    if (detailVM.Id == 0)
                    {
                        // Добавление новой детали
                        var newDetail = new OrderDetails
                        {
                            GoodId = detailVM.Good.Id, // Используем Id из выбранного Good
                            Quantity = detailVM.Quantity.Value,
                            OrderId = _originalOrder.Id // OrderId может быть 0, если это новый заказ
                        };

                        // Добавляем в коллекцию навигации заказа
                        _originalOrder.OrderDetails.Add(newDetail);

                        // При редактировании существующего заказа (Id != 0), явно добавляем деталь в контекст
                        // Если Id = 0, деталь будет добавлена автоматически при добавлении самого заказа
                        if (_originalOrder.Id != 0)
                        {
                            _context.OrderDetails.Add(newDetail);
                        }
                    }
                    else
                    {
                        // Обновление существующей детали
                        var existingDetail = _originalOrder.OrderDetails.FirstOrDefault(d => d.Id == detailVM.Id);

                        if (existingDetail != null)
                        {
                            // Обновляем свойства существующей, отслеживаемой сущности
                            existingDetail.GoodId = detailVM.Good.Id;
                            existingDetail.Quantity = detailVM.Quantity.Value;
                            // Другие поля, если они есть
                            // EF автоматически отследит изменения, если сущность была загружена
                        }
                    }
                }

                // 8. Если это новый заказ, добавляем его в контекст
                if (_originalOrder.Id == 0)
                {
                    _context.Order.Add(_originalOrder);
                }

                // 9. Сохраняем изменения
                _context.SaveChanges();
                MessageBox.Show("Данные успешно сохранены.", "Успех");
                this.DialogResult = true;
                Close();
            }
            catch (DbUpdateException ex)
            {
                string errorMessage = ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при сохранении данных:\n{errorMessage}", "Ошибка базы данных");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.GetBaseException().Message}\n\nStack: {ex.StackTrace}", "Ошибка");
            }
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ №{CurrentOrder.Id}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (CurrentOrder.Id == 0)
                    {
                        MessageBox.Show("Новый заказ не был сохранён, поэтому удалять нечего. Окно будет закрыто.", "Информация");
                        this.DialogResult = false;
                        Close();
                        return;
                    }

                    // Удаляем все детали заказа
                    var detailsToDelete = _originalOrder.OrderDetails.ToList();
                    foreach (var detail in detailsToDelete)
                    {
                        // Удаляем из контекста
                        _context.OrderDetails.Remove(detail);
                    }

                    // Удаляем сам заказ
                    _context.Order.Remove(_originalOrder);
                    _context.SaveChanges();

                    MessageBox.Show("Заказ успешно удален.", "Успех");
                    this.DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при удалении: {ex.Message}", "Ошибка");
                }
            }
        }

        private void OrderDetailsDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            // Проверка и установка значений после окончания редактирования строки
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Row.Item is OrderDetailsViewModel detailVM)
                    {
                        // Если товар выбран, но количество не задано, ставим 1
                        if (detailVM.Good != null && (detailVM.Quantity == null || detailVM.Quantity <= 0))
                        {
                            detailVM.Quantity = 1;
                        }

                        // Обновляем привязку для пересчета TotalPrice (на случай, если Quantity изменилось)
                        BindingExpression beTotal = totalPriceTextBlock.GetBindingExpression(TextBlock.TextProperty);
                        beTotal?.UpdateTarget();
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void AddOrderDetailClick(object sender, RoutedEventArgs e)
        {
            // Создаём новый OrderDetails (модель)
            var newOrderDetail = new OrderDetails
            {
                OrderId = _originalOrder.Id
            };

            // Создаём ViewModel для него
            var newDetailVM = new OrderDetailsViewModel(newOrderDetail);

            // Добавляем в коллекцию ViewModel, привязанную к DataGrid
            CurrentOrder.OrderDetailsViewModels.Add(newDetailVM);

            // Переводим на редактирование первой ячейки (артикула) новой строки
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (orderDetailsDataGrid.Items.Count > 0)
                {
                    var newRowIndex = orderDetailsDataGrid.Items.Count - 1;
                    orderDetailsDataGrid.SelectedIndex = newRowIndex;
                    orderDetailsDataGrid.ScrollIntoView(orderDetailsDataGrid.SelectedItem);

                    // Начинаем редактирование первой ячейки
                    DataGridRow row = (DataGridRow)orderDetailsDataGrid.ItemContainerGenerator.ContainerFromIndex(newRowIndex);
                    if (row != null)
                    {
                        DataGridCell cell = orderDetailsDataGrid.Columns[0].GetCellContent(row)?.Parent as DataGridCell;
                        if (cell != null)
                        {
                            cell.Focus();
                            orderDetailsDataGrid.BeginEdit(e);
                        }
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Input); // Более высокий приоритет для немедленного фокуса
        }
    }
}