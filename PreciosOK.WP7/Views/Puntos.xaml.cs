﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Phone.Tasks;
using PreciosOK.WP7.Helpers;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PreciosOK.WP7.Models;

namespace PreciosOK.WP7.Views
{
    public partial class Puntos
    {
        private static EstacionesStatusViewModel _viewModel = new EstacionesStatusViewModel();
        private static EstacionesStatusViewModel ViewModel
        {
            get { return _viewModel ?? (_viewModel = new EstacionesStatusViewModel()); }
        }

        private string _categoria;
        private bool _isRefining;
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

            TxtVersion.Text = "Versión " + App.Configuration.Version;

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
            else
            {
                if (!App.Configuration.IsRated && App.Configuration.OpenCount > 2)
                {
                    if (MessageBox.Show("Queres calificar la aplicación?", "Ayudanos a mejorar", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        App.Configuration.IsRated = true;
                        Config.Set(App.Configuration);

                        ShowReviewTask();
                    }
                }
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

            applicationBarIconButton = ApplicationBar.Buttons[2] as ApplicationBarIconButton;
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
                        new CategoryItem { Name = "ALMACÉN", Id = "Almacen"},
                        new CategoryItem { Name = "BEBIDAS", Id = "Bebidas"},
                        new CategoryItem { Name = "PERFUMERÍA", Id = "Perfumeria"},
                        new CategoryItem { Name = "LIMPIEZA", Id = "Limpieza"},
                        new CategoryItem { Name = "PANIFICADOS", Id = "Panificados"},
                        new CategoryItem { Name = "CARNES Y PESCADOS", Id = "Carnes Y Procesados"},
                        new CategoryItem { Name = "CONSTRUCCIÓN", Id = "Construccion"},
                        new CategoryItem { Name = "LÁCTEOS", Id = "Lacteos"},
                        new CategoryItem { Name = "VERDULERÍA", Id = "Verduleria"},
                        new CategoryItem { Name = "LIBRERÍA", Id = "Libreria"},
                    };
                
                MostrarLugares(null);   
            }

            if (!App.Configuration.IsPromoted)
            {
                DispatcherTimer dt = new DispatcherTimer();
                dt.Interval = TimeSpan.FromSeconds(4);
                dt.Tick += delegate
                {
                    if (MessageBox.Show("Probá la guía de transporte para Windows Phone!", "Vivis en Buenos Aires?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        App.Configuration.IsPromoted = true;
                        Config.Set(App.Configuration);

                        //Show an application, using the default ContentType.
                        var marketplaceDetailTask = new MarketplaceDetailTask
                        {
                            ContentIdentifier = "98250785-9804-4439-af3f-63ef88c5998c",
                            ContentType = MarketplaceContentType.Applications
                        };

                        marketplaceDetailTask.Show();
                    }

                    dt.Stop();
                };
                dt.Start();
            }
        }

        private void MostrarLugares(string text)
        {
            ViewModel.Estaciones.Clear();

            var list = App.Configuration.GetProducts(_categoria, text);

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

        private void ButtonRefine_Click(object sender, EventArgs e)
        {
            CategoryList.Open();
            _isRefining = true;
            _isSearching = false;
            SearchPanel.Margin = new Thickness(0, -850, 0, 0);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (_isChanging)
            {
                _isChanging = false;
                OptionSelectionPanel.Visibility = Visibility.Collapsed;
                e.Cancel = true;
            }

            if (_isRefining)
            {
                _isRefining = false;
                ProcesarBusqueda(null);
                e.Cancel = true;
            }

            if (_isSearching)
            {
                SearchPanel.Margin = new Thickness(0, -850, 0, 0);
                _isSearching = false;
                ProcesarBusqueda(null);
                e.Cancel = true;
            }
        }

        private void ProcesarBusqueda(string category, string text = "")
        {
            _categoria = "".Equals(text) ? category : null;
            MostrarLugares(text);
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
            var estacion = (Product)List.Items[selectedIndex];
            TileManager.Set(new Uri(string.Format("/Views/LugarDetalles.xaml?id={0}", estacion.Id), UriKind.Relative), "", new Uri(string.Format("/Images/Products/product_{0}.jpg", estacion.Id), UriKind.Relative));
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

            MostrarLugares(null);

            _isChanging = false;
            SetApplicationBarEnabled(true);
            OptionSelectionPanel.Visibility = Visibility.Collapsed;
        }

        private void RegionList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MarketList.ItemsSource = App.Configuration.GetMarketsByRegions(RegionList.SelectedIndex).Select(x => x.Value);
        }

        private void RateReview_Click(object sender, EventArgs e)
        {
            ShowReviewTask();
        }

        private static void ShowReviewTask()
        {
            var marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        private void AcBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            AcBox.Text = string.Empty;
        }

        private void AcBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcesarBusqueda(_categoria, AcBox.Text);
            }
        }

        private void ButtonBuscar_OnClick(object sender, RoutedEventArgs e)
        {
            ProcesarBusqueda(_categoria, AcBox.Text);
        }

        private void ButtonSearch_Click(object sender, EventArgs e)
        {
            SearchPanel.Margin = new Thickness(0, -450, 0, 0);
            AcBox.Focus();
            _isSearching = true;
            _isRefining = false;
        }

        private void Denuncia_Click(object sender, RoutedEventArgs e)
        {
            if (List.ItemContainerGenerator == null) return;
            var selectedListBoxItem = List.ItemContainerGenerator.ContainerFromItem(((MenuItem) sender).DataContext) as ListBoxItem;
            if (selectedListBoxItem == null) return;
            int selectedIndex = List.ItemContainerGenerator.IndexFromContainer(selectedListBoxItem);
            var estacion = (Product) List.Items[selectedIndex];
            var uri = new Uri(string.Format("/Views/Denuncia.xaml?id={0}", estacion.Id), UriKind.Relative);
            NavigationService.Navigate(uri);
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

            int start = 0;

            //handle ARGB strings (8 characters long)
            if (Pattern.Length == 8)
            {
                a = byte.Parse(Pattern.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                start = 2;
            }

            //convert RGB characters to bytes
            byte r = byte.Parse(Pattern.Substring(start, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(Pattern.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(Pattern.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber);

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