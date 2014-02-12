using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using PreciosOK.Helpers;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Shell;
using PreciosOK.Models;

namespace PreciosOK.Views
{
    public partial class Puntos
    {
        private static EstacionesStatusViewModel _viewModel = new EstacionesStatusViewModel();
        public static EstacionesStatusViewModel ViewModel
        {
            get { return _viewModel ?? (_viewModel = new EstacionesStatusViewModel()); }
        }

        Pushpin _posicionActual;
        private string _categoria;
        private bool _isSearching;
        private bool _isChanging;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (NavigationContext.QueryString.ContainsKey("cat"))
                _categoria = Uri.EscapeUriString(NavigationContext.QueryString["cat"]);
            
            base.OnNavigatedTo(e);
        }
        
        public Puntos()
        {
            InitializeComponent();

            TxtVersion.Text = App.Configuration.Version;

            MobFoxAdControl.PublisherID = App.Configuration.MobFoxID;
            MobFoxAdControl.TestMode = App.Configuration.MobFoxInTestMode;


            RegionList.ItemsSource = App.Configuration.GetRegions().Select(x => x.Value);
            RegionList.SelectedIndex = App.Configuration.SelectedRegion.HasValue ? App.Configuration.SelectedRegion.Value : 0;
            MarketList.ItemsSource = App.Configuration.GetMarketsByRegions(RegionList.SelectedIndex).Select(x => x.Value);
            MarketList.SelectedIndex = App.Configuration.SelectedMarket.HasValue ? App.Configuration.SelectedMarket.Value : 0;

            if (!App.Configuration.SelectedMarket.HasValue ||
                !App.Configuration.SelectedRegion.HasValue)
            {
                SetApplicationBarEnabled(false);
                OptionSelectionPanel.Visibility = Visibility.Visible;
            }
            
            StatusChecker.Check("Home");

            DataContext = ViewModel;
            Loaded += Page_Loaded;
        }

        void SetApplicationBarEnabled(bool isEnabled)
        {
            var applicationBarIconButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            if (applicationBarIconButton != null)
                applicationBarIconButton.IsEnabled = isEnabled;

            applicationBarIconButton = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
            if (applicationBarIconButton != null)
                applicationBarIconButton.IsEnabled = isEnabled;
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.BackgroundColor = new HexColor("#0071B7").GetColor();
            SystemTray.ForegroundColor = new HexColor("#FFFFFF").GetColor();

            if (!CategoryList.Items.Any())
            {

                CategoryList.ItemsSource = new List<CategoryItem>
                    {
                        new CategoryItem { Name = "todos", Id = ""},
                        new CategoryItem { Name = "ALMACÉN", Id = "ALMACÉN"},
                        new CategoryItem { Name = "BEBIDAS", Id = "BEBIDAS"},
                        new CategoryItem { Name = "PERFUMERÍA", Id = "PERFUMERÍA"},
                        new CategoryItem { Name = "LIMPIEZA", Id = "LIMPIEZA"},
                        new CategoryItem { Name = "PANIFICADOS", Id = "PANIFICADOS"},
                        new CategoryItem { Name = "CARNES", Id = "CARNES"},
                        new CategoryItem { Name = "LÁCTEOS", Id = "LÁCTEOS"},
                        new CategoryItem { Name = "VERDULERÍA", Id = "VERDULERÍA"},
                        new CategoryItem { Name = "CANASTA ESCOLAR", Id = "CANASTA ESCOLAR"},
                    };
                
                MostrarLugares();   
            }
        }

        private void MostrarLugares()
        {
            ViewModel.Estaciones.Clear();

            var list = App.Configuration.GetProducts(_categoria);

            foreach (var product in list)
            {
                ViewModel.AddEstacion(product);
            }

            ScrollViewer sv = List.Descendents().OfType<ScrollViewer>().FirstOrDefault();
            if (sv != null) sv.ScrollToVerticalOffset(0);
        }
        
        private void LstLugares_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;

            if (listBox == null || listBox.SelectedIndex == -1) return;

            var estacion = (Product)listBox.SelectedItem;

            var uri = new Uri(string.Format("/Views/LugarDetalles.xaml?id={0}", estacion.Id), UriKind.Relative);
            NavigationService.Navigate(uri);

            //Vuelvo el indice del item seleccionado a -1 para que pueda hacer tap en el mismo item y navegarlo
            listBox.SelectedIndex = -1;
        }

        private void Opciones_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Opciones.xaml", UriKind.Relative));
        }

        private void Acerca_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Acerca.xaml", UriKind.Relative));
        }

        private void ButtonSearch_Click(object sender, EventArgs e)
        {
            CategoryList.Open();
            _isSearching = true;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (_isChanging)
            {
                _isChanging = false;
                OptionSelectionPanel.Visibility = Visibility.Collapsed;
                e.Cancel = true;
            }

            if (_isSearching)
            {
                _isSearching = false;
                ProcesarBusqueda(null);
                e.Cancel = true;
            }
        }

        private void ProcesarBusqueda(string category)
        {
            _categoria = category;
            MostrarLugares();
        }

        private void CategoryList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var list = sender as ListPicker;
            if (list == null) return;
            if (list.SelectedIndex == -1) return;

            var item = list.SelectedItem as CategoryItem;
            if (item == null) return;

            ProcesarBusqueda(item.Id);
        }

        private void PinToStart_Click(object sender, RoutedEventArgs e)
        {
            if (List.ItemContainerGenerator == null) return;
            var selectedListBoxItem = List.ItemContainerGenerator.ContainerFromItem(((MenuItem)sender).DataContext) as ListBoxItem;
            if (selectedListBoxItem == null) return;
            var selectedIndex = List.ItemContainerGenerator.IndexFromContainer(selectedListBoxItem);
            var _estacion = (Product)List.Items[selectedIndex];
            TileManager.Set(new Uri(string.Format("/Views/LugarDetalles.xaml?id={0}", _estacion.Id), UriKind.Relative), "", new Uri(string.Format("/Images/Products/product_{0}.jpg", _estacion.Id), UriKind.Relative));
        }

        private void CodeSearch_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Scan.xaml", UriKind.Relative));
        }

        private void CambiarZona_Click(object sender, EventArgs e)
        {
            _isChanging = true;
            OptionSelectionPanel.Visibility = Visibility.Visible;
        }
        
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            App.Configuration.SelectedRegion = RegionList.SelectedIndex;
            App.Configuration.SelectedMarket = MarketList.SelectedIndex;

            MostrarLugares();

            _isChanging = false;
            SetApplicationBarEnabled(true);
            OptionSelectionPanel.Visibility = Visibility.Collapsed;
        }

        private void RegionList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MarketList.ItemsSource = App.Configuration.GetMarketsByRegions(RegionList.SelectedIndex).Select(x => x.Value);
        }
    }

    public static class VisualTreeEnumeration
    {
        public static IEnumerable<DependencyObject> Descendents(this DependencyObject root, int depth)
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                yield return child;
                if (depth > 0)
                {
                    foreach (var descendent in Descendents(child, --depth))
                        yield return descendent;
                }
            }
        }

        public static IEnumerable<DependencyObject> Descendents(this DependencyObject root)
        {
            return Descendents(root, Int32.MaxValue);
        }

        public static IEnumerable<DependencyObject> Ancestors(this DependencyObject root)
        {
            DependencyObject current = VisualTreeHelper.GetParent(root);
            while (current != null)
            {
                yield return current;
                current = VisualTreeHelper.GetParent(current);
            }
        }
    }

    internal class HexColor
    {
        public string Pattern { get; set; }

        public HexColor(string pattern)
        {
            Pattern = pattern;
        }

        public Color GetColor()
        {
            //remove the # at the front
            Pattern = Pattern.Replace("#", "");

            byte a = 255;
            byte r = 255;
            byte g = 255;
            byte b = 255;

            int start = 0;

            //handle ARGB strings (8 characters long)
            if (Pattern.Length == 8)
            {
                a = byte.Parse(Pattern.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                start = 2;
            }

            //convert RGB characters to bytes
            r = byte.Parse(Pattern.Substring(start, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(Pattern.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(Pattern.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }
    }

    public class CategoryItem
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class EstacionesStatusViewModel: INotifyPropertyChanged
    {
        public EstacionesStatusViewModel()
        {
            Estaciones = new ObservableCollection<Product>();
        }

        public void AddEstacion(Product estacion)
        {
            Estaciones.Add(estacion);
        }

        private ObservableCollection<Product> _estaciones;
        public ObservableCollection<Product> Estaciones
        {
            get { return _estaciones; }
            private set { 
                _estaciones = value;
                NotifyPropertyChanged("Estaciones");
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}