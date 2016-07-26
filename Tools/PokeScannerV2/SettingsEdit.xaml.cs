using MandraSoft.PokemonGo.Models;
using MandraSoft.PokemonGo.Models.WPFViewModels;
using Newtonsoft.Json;
using PokeScannerV2.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Logique d'interaction pour SettingsEdit.xaml
    /// </summary>
    public partial class SettingsEdit : Window
    {
        public SettingsEdit()
        {
            InitializeComponent();
            this.DataContext = Configuration.Settings;
        }

        private async void Close_Click(object sender, RoutedEventArgs e)
        {
            await FileExtensions.WriteAllTextAsync("settings.json",JsonConvert.SerializeObject(Configuration.Settings));
            this.Close();
        }

        private async void EditAccount_Click(object sender, RoutedEventArgs e)
        {
            var window = new AccountEdit();
            window.DataContext = (AccountsViewModel)((Button)sender).DataContext;
            window.Escape.Content = "Delete";
            await window.ShowDialogAsync();
            if (window.DataContext == null)
            {
                Configuration.Settings.Accounts.Remove((AccountsViewModel)((Button)sender).DataContext);
            }
        }

        private async void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            var window = new AccountEdit();
            window.DataContext = new AccountsViewModel();
            window.Escape.Content = "Cancel";
            await window.ShowDialogAsync();
            if (window.DataContext != null)
                ((ScannerSettingsViewModel)this.DataContext).Accounts.Add((AccountsViewModel)window.DataContext);

        }

        private async void AddScanArea_Click(object sender, RoutedEventArgs e)
        {
            var window = new ScannedAreaEdit(new MandraSoft.PokemonGo.Models.WPFViewModels.ScannedAreaViewModel());
            window.Escape.Content = "Cancel";
            await window.ShowDialogAsync();
            if(window.DataContext != null)
                ((ScannerSettingsViewModel)this.DataContext).ScannedAreas.Add((ScannedAreaViewModel)window.DataContext);
        }

        private async void EditScannedArea_Click(object sender, RoutedEventArgs e)
        {
            var window = new ScannedAreaEdit((ScannedAreaViewModel)((Button)sender).DataContext);
            window.Escape.Content = "Delete";
            await window.ShowDialogAsync();
            if (window.DataContext == null)
            {
                Configuration.Settings.ScannedAreas.Remove((ScannedAreaViewModel)((Button)sender).DataContext);
            }
        }
    }
}
