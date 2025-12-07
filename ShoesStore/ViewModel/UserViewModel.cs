using ShoesStore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoesStore.ViewModel
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public Nullable<int> RoleId { get; set; }
        public virtual ICollection<Order> Order { get; set; }
        public virtual Role Role { get; set; }
        public UserViewModel(User user) 
        {
            Id = user.Id;
            Surname = user.Surname;
            Name = user.Name;
            Patronymic = user.Patronymic;
            Login = user.Login;
            Password = user.Password;
            RoleId = user.RoleId;
            Order = user.Order;
            Role = user.Role;
        }
    }
}
