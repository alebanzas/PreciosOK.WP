using System;
using System.Device.Location;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using PreciosOK.Helpers;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Controls.Maps.Core;
using Microsoft.Phone.Tasks;
using PreciosOK.Models;

namespace PreciosOK.Views
{
    public partial class LugarDetalles : PhoneApplicationPage
    {
        Product _estacion;
        
        // Constructor
        public LugarDetalles()
        {
            InitializeComponent();
            Unloaded += Page_UnLoaded;

            MobFoxAdControl.PublisherID = App.Configuration.MobFoxID;
            MobFoxAdControl.TestMode = App.Configuration.MobFoxInTestMode;
        }

        private void Page_UnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void UpdateLugar()
        {
            PageTitle.Text = _estacion.Name;
            Marca.Text = _estacion.Brand;
            Proveedor.Text = (_estacion.Provider == _estacion.Brand) ? "" : _estacion.Provider;
            Categoria.Text = _estacion.Category;
            Codigo.Text = (_estacion.BarCode == 0) ? "" : string.Format("({0})", _estacion.BarCode.ToString(CultureInfo.InvariantCulture));
            Precio.Text = string.Format("${0}", _estacion.Price);

            var bm = new BitmapImage(new Uri(string.Format("/Images/Products/product_{0}.jpg", _estacion.Id), UriKind.RelativeOrAbsolute));
            Image.Source = bm;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Al navegar a la página, busco el lugar en base al id pasado y luego lo muestro.
            var id = int.Parse(Uri.EscapeUriString(NavigationContext.QueryString["id"]));
            
            _estacion = App.Configuration.GetById(id);
            UpdateLugar();

            base.OnNavigatedTo(e);
        }
        
        private void Pin(object sender, EventArgs e)
        {
            TileManager.Set(new Uri(string.Format("/Views/LugarDetalles.xaml?id={0}", _estacion.Id), UriKind.Relative), "", new Uri(string.Format("/Images/Products/product_{0}.jpg", _estacion.Id), UriKind.Relative));
        }
        
        private void Share(object sender, EventArgs e)
        {
            var shareLinkTask = new ShareStatusTask()
                {
                    Status = string.Format("#preciosOKWP {0} ({1}) a solo ${2}", _estacion.Name, _estacion.Brand, _estacion.Price),
                };
            shareLinkTask.Show();
        }
    }
}