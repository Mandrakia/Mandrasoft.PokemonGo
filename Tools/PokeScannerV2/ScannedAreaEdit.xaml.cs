using MandraSoft.PokemonGo.Api.ScanningAlgorithm;
using MandraSoft.PokemonGo.Models.WebModels.Responses;
using MandraSoft.PokemonGo.Models.WPFViewModels;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PokeScannerV2
{
    /// <summary>
    /// Logique d'interaction pour ScannedAreaEdit.xaml
    /// </summary>
    public partial class ScannedAreaEdit : Window
    {
        public ScannedAreaEdit() : this(null)
        {
        }
        public ScannedAreaEdit(ScannedAreaViewModel context = null)
        {
            InitializeComponent();
            if (context != null) this.DataContext = context;
            else this.DataContext = new ScannedAreaViewModel();
            TypedContext.PropertyChanged += ScannedAreaRadiusChanged;
            map.MouseDoubleClick += LocationChangedMap;
            if (TypedContext.Coordinates.Longitude != default(double))
            {
                var location = new Location() { Latitude = TypedContext.Coordinates.Latitude, Longitude = TypedContext.Coordinates.Longitude };
                map.Center = location;
                map.ZoomLevel = 11.8;
                RedrawMapFromModel();
            }
        }
        private async void ScannedAreaRadiusChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ScannedAreaViewModel.HexNum))
            {
                await RedrawMapFromModel();
                //Display warning.
                await RefreshWarning();
            }
            else if (e.PropertyName == nameof(ScannedAreaViewModel.JobsCount))
            {
                await RefreshWarning();
            }
        }
        private async Task RefreshWarning()
        {
            var pointsToScan = Scanner.GetPointsToScan(new LatLng() { lat = TypedContext.Coordinates.Latitude, lng = TypedContext.Coordinates.Latitude }, TypedContext.HexNum);
            var countsPoint = pointsToScan.Count;
            var estimatedTime = ((double)countsPoint / TypedContext.JobsCount) * 150;
            TypedContext.Warning = (estimatedTime / 1000).ToString(".##") + " seconds";
        }

        private async void LocationChangedMap(object sender, MouseButtonEventArgs e)
        {
            // Disables the default mouse double-click action.
            e.Handled = true;
            Point mousePosition = e.GetPosition(map);
            Location pinLocation = map.ViewportPointToLocation(mousePosition);
            TypedContext.Coordinates.Latitude = pinLocation.Latitude;
            TypedContext.Coordinates.Longitude = pinLocation.Longitude;
            await RedrawMapFromModel();
            await RefreshWarning();
        }
        private async Task RedrawMapFromModel()
        {
            var location = new Location() { Latitude = TypedContext.Coordinates.Latitude, Longitude = TypedContext.Coordinates.Longitude };
            Pushpin pin = new Pushpin() { Location = location } ;
            map.Children.Clear();
            map.Children.Add(pin);
            var pointsToScan = Scanner.GetPointsToScan(new LatLng() { lat = location.Latitude, lng = location.Longitude }, TypedContext.HexNum);
            pointsToScan.Reverse();
            var outerEdge = pointsToScan.Take(TypedContext.HexNum * 6).ToList();
            MapPolygon polygon = new MapPolygon();
            polygon.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue);
            polygon.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            polygon.StrokeThickness = 5;
            polygon.Opacity = 0.7;
            polygon.Locations = new LocationCollection();
            outerEdge.Select(x => new Location(x.lat, x.lng)).ToList().ForEach(x => polygon.Locations.Add(x));
            map.Children.Add(polygon);
        }

        private ScannedAreaViewModel TypedContext
        {
            get
            {
                return (ScannedAreaViewModel)DataContext;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            TypedContext.AccountNames.Add(cbName.SelectedItem.ToString());
        }
        private void Escape_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            this.Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    [ComVisible(true)]
    public class ScriptingHelper
    {
        private ScannedAreaViewModel _model;
        public ScriptingHelper(ScannedAreaViewModel model)
        {
            _model = model;
        }
        public void PointChanged(string latLng)
        {
            var split = latLng.Split(',');
            _model.Coordinates.Latitude = double.Parse(split[0], System.Globalization.CultureInfo.InvariantCulture);
            _model.Coordinates.Longitude = double.Parse(split[1], System.Globalization.CultureInfo.InvariantCulture);
        }
        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }
    }
}
