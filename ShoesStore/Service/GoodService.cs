using ShoesStore.Model;
using ShoesStore.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoesStore.Service
{
    //Менеджер и администратор должны иметь возможность отсортировать товары (по возрастанию и убыванию) по количеству на складе.
    public class GoodService
    {
        private readonly ShoesStoreContext _context;

        public GoodService(ShoesStoreContext context)
        {
            _context = context;
        }

        public List<Good> SortByQuantityAcs()
        {
           return _context.Good
                .OrderBy(good => good.CountInStorage)
                .ToList();
        }

        public List<Good> SortByQuantityDesc()
        {
            return _context.Good
                .OrderByDescending(good => good.CountInStorage)
                .ToList();
        }

        public List<Good> SortBySupplier(Supplier supplier)
        {
            return _context.Good
                .Where(g => g.Supplier.Id == supplier.Id)
                .ToList();
        }

        public List<GoodViewModel> ConvertToGoodView(List<Good> goods)
        {
            List<GoodViewModel> goodViews = new List<GoodViewModel>();

            foreach (var good in goods)
            {
                goodViews.Add(new GoodViewModel(good));
            }

            return goodViews;
        }
    }
}
