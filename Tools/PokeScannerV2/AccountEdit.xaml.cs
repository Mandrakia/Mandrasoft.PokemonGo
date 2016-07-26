using MandraSoft.PokemonGo.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PokeScannerV2
{
    /// <summary>
    /// Logique d'interaction pour AccountEdit.xaml
    /// </summary>
    public partial class AccountEdit : Window
    {
        public AccountEdit()
        {
            InitializeComponent();
            comboBox.ItemsSource = Enum.GetValues(typeof(AuthType)).Cast<AuthType>();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Escape_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            this.Close();
        }
    }
}
