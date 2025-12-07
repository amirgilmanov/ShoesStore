using ShoesStore.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace ShoesStore.ViewModel
{
    public class EditOrderViewModel : INotifyPropertyChanged
    {
        private Order _order;
        private string _orderDateString;
        private string _deliveryDateString;
        private OrderStatus _orderStatus;
        private ObservableCollection<OrderDetailsViewModel> _orderDetailsViewModels;

        public Order Order
        {
            get { return _order; }
            set
            {
                if (_order != value)
                {
                    _order = value;
                    OnPropertyChanged(nameof(Order));
                }
            }
        }

        public int Id
        {
            get { return _order.Id; }
        }

        public int? Number
        {
            get { return _order.Number; }
            set
            {
                if (_order.Number != value)
                {
                    _order.Number = value;
                    OnPropertyChanged(nameof(Number));
                }
            }
        }

        public string PickUpPointAddress
        {
            get { return _order.PickUpPointAddress; }
            set
            {
                if (_order.PickUpPointAddress != value)
                {
                    _order.PickUpPointAddress = value;
                    OnPropertyChanged(nameof(PickUpPointAddress));
                }
            }
        }

        public string ReceiptCode
        {
            get { return _order.ReceiptCode; }
            set
            {
                if (_order.ReceiptCode != value)
                {
                    _order.ReceiptCode = value;
                    OnPropertyChanged(nameof(ReceiptCode));
                }
            }
        }

        public string OrderDateString
        {
            get { return _orderDateString; }
            set
            {
                if (_orderDateString != value)
                {
                    _orderDateString = value;
                    OnPropertyChanged(nameof(OrderDateString));
                }
            }
        }

        public string DeliveryDateString
        {
            get { return _deliveryDateString; }
            set
            {
                if (_deliveryDateString != value)
                {
                    _deliveryDateString = value;
                    OnPropertyChanged(nameof(DeliveryDateString));
                }
            }
        }

        public User User
        {
            get { return _order.User; }
            set
            {
                if (_order.User != value)
                {
                    _order.User = value;
                    OnPropertyChanged(nameof(User));
                }
            }
        }

        public ObservableCollection<OrderDetailsViewModel> OrderDetailsViewModels
        {
            get { return _orderDetailsViewModels; }
            set
            {
                if (_orderDetailsViewModels != value)
                {
                    _orderDetailsViewModels = value;
                    OnPropertyChanged(nameof(OrderDetailsViewModels));
                }
            }
        }

        // Свойство для доступа к оригинальным OrderDetails из Order
        public ICollection<OrderDetails> OrderDetails
        {
            get { return _order.OrderDetails; }
        }

        public OrderStatus OrderStatus
        {
            get { return _orderStatus; }
            set
            {
                if (_orderStatus != value)
                {
                    _orderStatus = value;
                    _order.OrderStatus = value;
                    _order.OrderStatusId = value?.Id;
                    OnPropertyChanged(nameof(OrderStatus));
                }
            }
        }

        public decimal TotalPrice
        {
            get
            {
                if (OrderDetailsViewModels == null || !OrderDetailsViewModels.Any())
                {
                    return 0;
                }
                return OrderDetailsViewModels.Sum(od => od.Amount);
            }
        }

        public EditOrderViewModel(Order order)
        {
            _order = order;
            _orderStatus = order.OrderStatus;

            // Инициализация строковых представлений дат
            if (order.Date.HasValue)
            {
                _orderDateString = order.Date.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                _orderDateString = DateTime.Now.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            if (order.DeliveryDate.HasValue)
            {
                _deliveryDateString = order.DeliveryDate.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                _deliveryDateString = string.Empty;
            }

            // Инициализация OrderDetailsViewModels
            InitializeOrderDetailsViewModels();
        }

        private void InitializeOrderDetailsViewModels()
        {
            _orderDetailsViewModels = new ObservableCollection<OrderDetailsViewModel>();

            if (_order.OrderDetails != null)
            {
                foreach (var detail in _order.OrderDetails)
                {
                    var viewModel = new OrderDetailsViewModel(detail);
                    _orderDetailsViewModels.Add(viewModel);
                }
            }

            // Подписываемся на изменения в коллекции для пересчета TotalPrice
            _orderDetailsViewModels.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(TotalPrice));
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}