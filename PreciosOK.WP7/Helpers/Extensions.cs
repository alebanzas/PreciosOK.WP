using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace PreciosOK.Helpers
{
    public static class UriExtensions
    {
        private static Uri ToApiCallUri(this string source, string param, bool alwaysRefresh = false)
        {
            if (!string.IsNullOrWhiteSpace(param))
            {
                param = "&" + param;
            }
            else
            {
                param = string.Empty;
            }
            var refresh = string.Empty;
            if (alwaysRefresh)
            {
                refresh = string.Format("&__t={0}.{1}", DateTime.UtcNow.Hour, DateTime.UtcNow.Minute);
            }
            try
            {
                if (!param.Contains("&lat=") && !param.Contains("&lon="))
                {
                    var currentPosition = PositionService.GetCurrentLocation();
                    if (currentPosition != null)
                    {
                        param += "&lat=" + currentPosition.Location.Latitude.ToString(CultureInfo.InvariantCulture).Replace(",", ".");
                        param += "&lon=" + currentPosition.Location.Longitude.ToString(CultureInfo.InvariantCulture).Replace(",", ".");
                    }
                }
            }
            catch
            {
                //no me importa si pincha.
            }

            var apiCallUri = new Uri(string.Format("http://servicio.abhosting.com.ar{0}/?appId={1}&versionId={2}&installationId={3}{4}{5}",
                source,
                App.Configuration.Name,
                App.Configuration.Version,
                App.Configuration.InstallationId,
                refresh,
                param));

            Debug.WriteLine(apiCallUri);

            return apiCallUri;
        }

        public static Uri ToApiCallUri(this string source, Dictionary<string, object> param, bool alwaysRefresh = false)
        {
            var pp = string.Join("&", param.Select(x => x.Key + "=" + x.Value));

            return ToApiCallUri(source, pp, alwaysRefresh);
        }

        public static Uri ToApiCallUri(this string source, bool alwaysRefresh = false)
        {
            return ToApiCallUri(source, string.Empty, alwaysRefresh);
        }
    }
}
