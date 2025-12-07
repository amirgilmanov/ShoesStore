using ShoesStore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace ShoesStore
{
    public partial class EditGood : Window
    {
        private readonly ShoesStoreContext _context;
        public Good CurrentGood { get; set; }

        public List<GoodCategory> AllCategories { get; set; }
        public List<Manufacture> AllManufacturers { get; set; }
        public List<Supplier> AllSuppliers { get; set; }

        private string _newImagePath = null;

        /// <summary>
        /// Основной конструктор для добавления/редактирования товара
        /// </summary>
        /// <param name="goodToEdit"></param>
        public EditGood(Good goodToEdit, ShoesStoreContext context)
        {
            InitializeComponent();
            _context = context;

            if (goodToEdit.Id == 0)
            {
                _context.Good.Add(goodToEdit);
                Title = "Добавление нового товара";
            }
            else
            {
                _context.Good.Attach(goodToEdit);
                Title = $"Редактирование товара №{goodToEdit.Id}";
            }

            CurrentGood = goodToEdit;

            LoadRelatedData();
            this.DataContext = this;
        }

        private void LoadRelatedData()
        {
            // Загрузка списков
            AllCategories = _context.GoodCategory.ToList();
            AllManufacturers = _context.Manufacture.ToList();
            AllSuppliers = _context.Supplier.ToList();

            // ПРАВИЛЬНОЕ ПРИСВОЕНИЕ ИСТОЧНИКОВ ДАННЫХ
            selectedGoodCategory.ItemsSource = AllCategories;
            selectedManufacture.ItemsSource = AllManufacturers;
            selectedSupplier.ItemsSource = AllSuppliers;

            selectedGoodCategory.SelectedItem = AllCategories.FirstOrDefault(c => c.Id == CurrentGood.GoodCategoryId);
            selectedManufacture.SelectedItem = AllManufacturers.FirstOrDefault(m => m.Id == CurrentGood.ManufactureId);
            selectedSupplier.SelectedItem = AllSuppliers.FirstOrDefault(s => s.Id == CurrentGood.SupplierId);
        }

        private void ChooseImageClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("В разработке");
        }

        /// <summary>
        /// Реализация сохранения (Create и Update)
        /// </summary>
        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            // 1. ПАРСИНГ и ВАЛИДАЦИЯ ВВОДА

            if (!decimal.TryParse(priceInput.Text, out decimal price))
            {
                MessageBox.Show("Поле 'Цена' должно содержать корректное число.", "Ошибка ввода");
                return;
            }
            if (!int.TryParse(countInStorageInput.Text, out int countInStorage))
            {
                MessageBox.Show("Поле 'Количество' должно содержать корректное целое число.", "Ошибка ввода");
                return;
            }
            if (!int.TryParse(discountInput.Text, out int discount))
            {
                // Необязательно, но лучше использовать 0 при ошибке
                discount = 0;
            }

            // 2. ОБНОВЛЕНИЕ СВОЙСТВ _currentGood (СИНХРОНИЗАЦИЯ с UI)
            CurrentGood.Article = articleInput.Text;
            CurrentGood.Name = nameInput.Text;
            CurrentGood.Price = price;
            CurrentGood.CountInStorage = countInStorage;
            CurrentGood.Discount = discount;
            CurrentGood.UnitOfMeaserent = unitOfMeaserentInput.Text;
            CurrentGood.Description = descriptionInput.Text;

            // ОБНОВЛЕНИЕ ВНЕШНИХ КЛЮЧЕЙ
            CurrentGood.SupplierId = (selectedSupplier.SelectedItem as Supplier)?.Id ?? 0;
            CurrentGood.ManufactureId = (selectedManufacture.SelectedItem as Manufacture)?.Id ?? 0;
            CurrentGood.GoodCategoryId = (selectedGoodCategory.SelectedItem as GoodCategory)?.Id ?? 0;

            // 3. ПЕРЕПРОВЕРКА ВАЛИДАЦИИ (теперь валидируется актуальный объект)
            if (!ValidateInput()) return;

            if (string.IsNullOrEmpty(CurrentGood.PicturePass))
            {
                CurrentGood.PicturePass = "/Res/picture.png";
            }

            try
            {
                // _context.Good.Add() уже был вызван в конструкторе для нового товара (Id=0)
                // Если Id > 0, Entity Framework сам отследит изменения в _currentGood

                _context.SaveChanges();

                MessageBox.Show("Данные успешно сохранены.", "Успех");
                this.DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка сохранения данных: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nПодробности БД: {ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $"\nSQL-Ошибка: {ex.InnerException.InnerException.Message}";
                    }
                }
                MessageBox.Show(errorMessage, "Ошибка сохранения");
            }
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить товар: {CurrentGood.Name}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool isInOrder = _context.OrderDetails
                                             .Any(oi => oi.GoodId == CurrentGood.Id);

                    if (isInOrder)
                    {
                        MessageBox.Show(
                            "Товар нельзя удалить, так как он присутствует в одном или более заказе.",
                            "Ошибка удаления");
                        return;
                    }

                    _context.Good.Remove(CurrentGood);
                    _context.SaveChanges();

                    MessageBox.Show("Товар успешно удален.", "Успех");

                    this.DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при удалении: {ex.Message}", "Ошибка");
                }
            }
        }

        private bool ValidateInput()
        {
            if (CurrentGood.Price < 0)
            {
                MessageBox.Show("Цена не может быть отрицательной.", "Валидация");
                return false;
            }
            if (CurrentGood.CountInStorage < 0)
            {
                MessageBox.Show("Количество на складе не может быть отрицательным.", "Валидация");
                return false;
            }
            if (string.IsNullOrWhiteSpace(CurrentGood.Name) || CurrentGood.GoodCategoryId == 0)
            {
                MessageBox.Show("Заполните обязательные поля (Наименование, Категория).", "Валидация");
                return false;
            }

            // 2. ДОБАВИТЬ: Проверка других обязательных внешних ключей
            if (CurrentGood.ManufactureId == 0)
            {
                MessageBox.Show("Выберите Производителя.", "Валидация");
                return false;
            }

            if (CurrentGood.SupplierId == 0)
            {
                MessageBox.Show("Выберите Поставщика.", "Валидация");
                return false;
            }

            // 3. ДОБАВИТЬ: Проверка артикула
            if (string.IsNullOrWhiteSpace(CurrentGood.Article))
            {
                MessageBox.Show("Поле 'Артикул' не может быть пустым.", "Валидация");
                return false;
            }

            return true;
        }

        
    }
}