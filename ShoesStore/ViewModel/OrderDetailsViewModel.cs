using ShoesStore.Model;
using System;
using System.ComponentModel;

namespace ShoesStore.ViewModel
{
    public class OrderDetailsViewModel : INotifyPropertyChanged
    {
        private OrderDetails _orderDetails;

        public int Id
        {
            get { return _orderDetails.Id; }
        }

        public int? GoodId
        {
            get { return _orderDetails.GoodId; }
            set
            {
                if (_orderDetails.GoodId != value)
                {
                    _orderDetails.GoodId = value;
                    OnPropertyChanged(nameof(GoodId));
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }

        public int? Quantity
        {
            get { return _orderDetails.Quantity; }
            set
            {
                if (_orderDetails.Quantity != value)
                {
                    _orderDetails.Quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }

        public Good Good
        {
            get { return _orderDetails.Good; }
            set
            {
                if (_orderDetails.Good != value)
                {
                    _orderDetails.Good = value;
                    if (value != null)
                    {
                        _orderDetails.GoodId = value.Id;
                        GoodId = value.Id;
                    }
                    OnPropertyChanged(nameof(Good));
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }

        public Order Order
        {
            get { return _orderDetails.Order; }
            set
            {
                if (_orderDetails.Order != value)
                {
                    _orderDetails.Order = value;
                    OnPropertyChanged(nameof(Order));
                }
            }
        }

        public decimal Amount
        {
            get
            {
                if (Good != null && Quantity.HasValue && Quantity.Value > 0)
                {
                    return (decimal)Quantity.Value * Good.Price;
                }
                return 0;
            }
        }

        public OrderDetailsViewModel(OrderDetails orderDetails)
        {
            _orderDetails = orderDetails ?? throw new ArgumentNullException(nameof(orderDetails));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}