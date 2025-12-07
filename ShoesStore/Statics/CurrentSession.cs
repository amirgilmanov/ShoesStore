using System;
using System.Collections.Generic;
using System.Text;
using ShoesStore.Model;

namespace ShoesStore.Statics
{
   public static class CurrentSession
    {
      public static User CurrentUser { get; set; }
    }
}
