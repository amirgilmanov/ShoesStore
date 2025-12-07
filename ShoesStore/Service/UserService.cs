using ShoesStore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoesStore.Service
{
    public class UserService
    {
        private readonly ShoesStoreContext _shoesStoreContext;
        public UserService(ShoesStoreContext shoesStoreContext)
        {
            _shoesStoreContext = shoesStoreContext;
        }

        public User LogIn(string login, string password) 
        {
            return _shoesStoreContext.User.Where(u => u.Login == login && u.Password == password).FirstOrDefault();
        }
    }
}
