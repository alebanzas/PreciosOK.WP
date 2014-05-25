using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PreciosOK.WP7.Helpers;
using PreciosOK.WP7.Models;

namespace PreciosOK.WP7.Views
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
            Thanks.Visibility = Visibility.Collapsed;
            Loading.Visibility = Visibility.Collapsed;
            
            if (!MarketList.Items.Any())
            {
                MarketList.ItemsSource = new List<CategoryItem>
                {
                    new CategoryItem {Name = "Supermercado de barrio", Id = "barrio"},
                    new CategoryItem {Name = "Carrefour", Id = "carrefour"},
                    new CategoryItem {Name = "Changomas", Id = "changomas"},
                    new CategoryItem {Name = "Cooperativa Obrera Supermercados", Id = "cos"},
                    new CategoryItem {Name = "Coto", Id = "coto"},
                    new CategoryItem {Name = "Día", Id = "dia"},
                    new CategoryItem {Name = "Disco", Id = "disco"},
                    new CategoryItem {Name = "Josimar", Id = "josimar"},
                    new CategoryItem {Name = "Jumbo", Id = "jumbo"},
                    new CategoryItem {Name = "La anónima", Id = "anonima"},
                    new CategoryItem {Name = "Toledo", Id = "toledo"},
                    new CategoryItem {Name = "VEA", Id = "vea"},
                    new CategoryItem {Name = "Walmart", Id = "walmart"},
                };
            }

            var bm = new BitmapImage(new Uri(string.Format("/Images/Products/product_{0}.jpg", _estacion.Id), UriKind.RelativeOrAbsolute));
            ProdImage.Source = bm;
            ProdName.Text = _estacion.Name;
            ProdBrand.Text = _estacion.Brand;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Al navegar a la página, busco el lugar en base al id pasado y luego lo muestro.
            var id = long.Parse(Uri.EscapeUriString(NavigationContext.QueryString["id"]));

            _estacion = App.Configuration.GetById(id);
            
            base.OnNavigatedTo(e);
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            //el back vuelve a la home
            while (NavigationService.BackStack.Count() > 1)
            {
                NavigationService.RemoveBackEntry();
            }
            NavigationService.Navigate(new Uri("/Views/Puntos.xaml", UriKind.Relative));
        }

        private void DenunciaButton_OnClick(object sender, RoutedEventArgs e)
        {
            Loading.Visibility = Visibility.Visible;

            var position = PositionService.GetCurrentLocation();

            RequestModel = new DenunciaPreciosModel
            {
                Date = DateTime.UtcNow,
                InstallationId = App.Configuration.InstallationId.ToString(),
                AppId = App.Configuration.Name,
                AppVersion = App.Configuration.Version,
                Lat = position.Location.Latitude,
                Lon = position.Location.Longitude,
                MarketId = App.Configuration.SelectedMarket.Value,
                RegionId = App.Configuration.SelectedRegion.Value,
                Type = ((ListPickerItem)TypeList.SelectedItem).Content.ToString(),
                Address = ProdDireccion.Text,
                ProductId = _estacion.Id,
                Comment = ProdComentarios.Text,
                MarketName = ((CategoryItem)MarketList.SelectedItem).Id,
            };

            var webRequest = WebRequest.CreateHttp("/api/denunciaprecios".ToApiCallUri());
            webRequest.Method = "POST";
            webRequest.BeginGetRequestStream(GetRequestStreamCallback, webRequest);
        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            var webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
            var postStream = webRequest.EndGetRequestStream(asynchronousResult);

            string postData = GetReportData();

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();

            webRequest.ContentType = "application/json";
            webRequest.BeginGetResponse(ResponseCallback, webRequest);
        }
        
        private string GetReportData()
        {
            var ms = new MemoryStream();
            var ser = new DataContractJsonSerializer(typeof(DenunciaPreciosModel));
            ser.WriteObject(ms, RequestModel);
            var json = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(json, 0, json.Length);
        }

        private void ResponseCallback(IAsyncResult asyncResult)
        {
            try
            {
                var httpRequest = (HttpWebRequest)asyncResult.AsyncState;
                var response = (HttpWebResponse)httpRequest.EndGetResponse(asyncResult);

                Dispatcher.BeginInvoke(() =>
                {
                    //TxtErrorCode.Text = "ID reporte: " + response.StatusDescription;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    //TxtErrorCode.Text = "ID reporte: " + Guid.Empty;
                });
            }
            finally
            {
                Dispatcher.BeginInvoke(() =>
                {
                    Thanks.Visibility = Visibility.Visible;
                });
            }
        }

        public DenunciaPreciosModel RequestModel { get; set; }
    }

    public class DenunciaPreciosModel
    {
        public string AppId { get; set; }

        public string AppVersion { get; set; }

        public string InstallationId { get; set; }

        public double Lat { get; set; }

        public double Lon { get; set; }

        public DateTime Date { get; set; }

        public Guid TrackingId { get; set; }

        public int RegionId { get; set; }

        public int MarketId { get; set; }

        public long ProductId { get; set; }

        public string Type { get; set; }

        public string Address { get; set; }

        public string MarketName { get; set; }

        public string Comment { get; set; }
    }
}