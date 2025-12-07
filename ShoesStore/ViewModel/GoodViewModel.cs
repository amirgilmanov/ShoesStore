using ShoesStore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ShoesStore.ViewModel
{
    public class GoodViewModel
    {
        private const string PICTURE_PASS = "/Res/picture.png";
     
        public int Id { get; set; }
        public string Article { get; set; }
        public string Name { get; set; }
        public string UnitOfMeaserent { get; set; }
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public int CountInStorage { get; set; }
        public string Description { get; set; }
        public string PicturePass { get; set; }

        public GoodCategory GoodCategory { get; set; }
        public ICollection<OrderDetails> OrderDetails { get; set; }
        public Manufacture Manufacture { get; set; }
        public Supplier Supplier { get; set; }

        public Brush Background { get; set; }
        public decimal OldPrice {  get; set; }

        public GoodViewModel(Good good)
        {
            Id = good.Id;
            Article = good.Article;
            Name = good.Name;
            UnitOfMeaserent = good.UnitOfMeaserent;
            Price = good.Price;
            Discount = good.Discount;
            Description = good.Description;
            PicturePass = good.PicturePass;
            GoodCategory = good.GoodCategory;
            OrderDetails = good.OrderDetails;
            Manufacture = good.Manufacture;
            Supplier = good.Supplier;
            CountInStorage = good.CountInStorage;

            GetBackground();
            GetPicture();
            GetPrice();
        }

        private void GetBackground()
        {
            if (Discount >= 15)
            {
                Background = (Brush)new BrushConverter().ConvertFromString("#2e8b57");
                return;
            }
            else if (CountInStorage == 0)
            {
                Background = Brushes.LightBlue;
                return;
            }
            else
            {
                Background = (Brush)new BrushConverter().ConvertFromString("#7fff00");
                return;
            }

        }
        
        private void GetPicture()
        {
            if (!string.IsNullOrEmpty(PicturePass) || PicturePass != "")
            {
                return;
            }
            else
            {
                PicturePass = PICTURE_PASS;
            }

        }

        private void GetPrice()
        {
            if(Discount <= 0)
            {
                return;
            }
            else
            {
                OldPrice = Price;
                Price = OldPrice * (1m - (decimal)Discount / 100m);
            }
        }
    }
}
