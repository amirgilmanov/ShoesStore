using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ShoesStore.Helpers
{
   public class MessageHelper
    {
        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowWarn(string message)
        {
            MessageBox.Show(message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

    }
}
