using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;

namespace PreciosOK.Helpers
{
    public static class StatusChecker
    {
        public static void Check()
        {
            try
            {
                (new WebClient()).DownloadStringAsync("/status/check".ToApiCallUri());
            }
            catch (Exception)
            { }
        }

        public static void Check(string name)
        {
            try
            {
                var param = new Dictionary<string, object> {{"n", name}};
                var client = new WebClient();
                client.DownloadStringCompleted += client_DownloadStringCompleted;
                client.DownloadStringAsync("/status/check".ToApiCallUri(param));
            }
            catch (Exception)
            { }
        }

        static void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                var a = e.Result;
            }
            catch (WebException ex)
            {
                using (var response = (HttpWebResponse)ex.Response)
                {
                    if (response == null) return;
                    
                    HttpStatusCode status = response.StatusCode;
                    // Whatever
                    if (status.Equals(426))
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show(!string.IsNullOrWhiteSpace(response.StatusDescription) ? response.StatusDescription : "Hay una actualización disponible. Esta versión no será más soportada en poco tiempo. Diríjase al Store y actualícela."));
                    }
                }
            }
            catch (Exception)
            { }
        }
    }
}
