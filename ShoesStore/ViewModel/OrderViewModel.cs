using ShoesStore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoesStore.ViewModel
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public Nullable<int> Number { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string PickUpPointAddress { get; set; }
        public string ReceiptCode { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }

        public User User { get; set; }
        public ICollection<OrderDetails> OrderDetails { get; set; }
        public ICollection<OrderDetailsViewModel> OrderDetailsViewModels { get; set; }
        public OrderStatus OrderStatus { get; set; }

        public decimal TotalPrice
        {
            get
            {
                if (OrderDetails == null || !OrderDetails.Any())
                {
                    return 0;
                }
                // Используем OrderDetails для расчета. Примечание: Good должен быть загружен!
                return (decimal)OrderDetails.Sum(od => od.Quantity * od.Good.Price);
            }
        }

        public OrderViewModel(Order order) 
        { 
            Id = order.Id;
            Number = order.Number;
            Date = order.Date;
            PickUpPointAddress = order.PickUpPointAddress;
            ReceiptCode = order.ReceiptCode;
            DeliveryDate = order.DeliveryDate;
            User = order.User;
            OrderDetails = order.OrderDetails;
            OrderStatus = order.OrderStatus;
            OrderDetailsViewModels = convertDetails(order.OrderDetails.ToList());
        }

        private List<OrderDetailsViewModel> convertDetails(List<OrderDetails> details)
        {          
            List<OrderDetailsViewModel> convertedDetails = new List<OrderDetailsViewModel>();

            foreach (OrderDetails orderDetails in details) 
            {
                convertedDetails.Add(new OrderDetailsViewModel(orderDetails));
            }
            return convertedDetails;

        }

    }
}
