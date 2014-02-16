using System;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PreciosOK.Models;

namespace PreciosOK.Views
{
    public partial class Denuncia : PhoneApplicationPage
    {
        private Product _estacion;

        public Denuncia()
        {
            InitializeComponent();
            Loaded += (sender, args) => PageLoad();
        }

        private void PageLoad()
        {
            SystemTray.BackgroundColor = new HexColor("#0071B7").GetColor();

            var bm = new BitmapImage(new Uri(string.Format("/Images/Products/product_{0}.jpg", _estacion.Id), UriKind.RelativeOrAbsolute));
            ProdImage.Source = bm;
            ProdName.Text = _estacion.Name;
            ProdBrand.Text = _estacion.Brand;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Al navegar a la página, busco el lugar en base al id pasado y luego lo muestro.
            var id = int.Parse(Uri.EscapeUriString(NavigationContext.QueryString["id"]));

            _estacion = App.Configuration.GetById(id);
            
            base.OnNavigatedTo(e);
        }
    }
}