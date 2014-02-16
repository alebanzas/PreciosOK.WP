using System;
using System.Device.Location;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using PreciosOK.Models;

namespace PreciosOK.Helpers
{
    public partial class ApplicationConfiguration
    {
        public ApplicationConfiguration()
        {
            Ubicacion = SetUbicacionDefault();
        }

        public ApplicationConfiguration(string name, string version)
        {
            Name = name;
            Version = version;
            Ubicacion = SetUbicacionDefault();
        }

        private static GeoPosition<GeoCoordinate> SetUbicacionDefault()
        {
            //obelisco de buenos aires
            return new GeoPosition<GeoCoordinate>(DateTimeOffset.UtcNow, new GeoCoordinate(-34.603722, -58.381594));
        }

        public void SetInitialConfiguration(string name, string version)
        {
            OpenCount++;
            Name = name;
            Version = version;

            if (IsInitialized) return;

            OpenCount = 1;
            Ubicacion = SetUbicacionDefault();
            InstallationId = Guid.NewGuid();
            IsLocationEnabledByPhone = true;
            IsLocationEnabledByAppConfig = true;
            IsRated = false;
            IsPromoted = false;
            IsInitialized = true;
            
            Config.Set(this);
        }

        public bool IsRated { get; set; }

        public bool IsPromoted { get; set; }

        public bool IsInitialized { get; set; }

        public bool IsLocationEnabled { get { return IsLocationEnabledByPhone && IsLocationEnabledByAppConfig; } }

        public bool IsLocationEnabledByPhone { get; set; }

        public bool IsLocationEnabledByAppConfig { get; set; }

        public IList<Product> GetProducts(string category, string text)
        {
            if (!string.IsNullOrWhiteSpace(category))
            {
                return products.Where(x => x.Market == SelectedMarket && x.Region == SelectedRegion && x.Category == category).ToList();
            }
            if (!string.IsNullOrWhiteSpace(text))
            {
                return products.Where(x => x.Market == SelectedMarket && x.Region == SelectedRegion && (x.Name.Contains(text.ToUpperInvariant()) || x.Brand.Contains(text.ToUpperInvariant()) || x.Provider.Contains(text.ToUpperInvariant()))).ToList();
            }
            return products.Where(x => x.Market == SelectedMarket && x.Region == SelectedRegion).ToList();
        }

        public Product GetByBarCode(long code)
        {
            return products.FirstOrDefault(x => x.Market == SelectedMarket 
                && x.Region == SelectedRegion
                && code.ToString(CultureInfo.InvariantCulture).StartsWith(x.BarCode.ToString(CultureInfo.InvariantCulture)));
        }

        public Product GetById(int id)
        {
            return products.FirstOrDefault(x => x.Market == SelectedMarket && x.Region == SelectedRegion && x.Id == id);
        }


        public int? SelectedRegion { get; set; }

        private readonly List<KeyValuePair<int, string>> _regions = new List<KeyValuePair<int, string>>
        {
            new KeyValuePair<int, string>(0, "Capital y GBA"),
            new KeyValuePair<int, string>(1, "Provincia de Bs. As."),
            new KeyValuePair<int, string>(2, "Cuyo y litoral"),
            new KeyValuePair<int, string>(3, "NEA y NOA"),
            new KeyValuePair<int, string>(4, "Patagonia"),
        };

        public int? SelectedMarket { get; set; }

        private readonly List<KeyValuePair<int, string>> _markets = new List<KeyValuePair<int, string>>
        {
            new KeyValuePair<int, string>(0, "Cadenas Nacionales"),
            new KeyValuePair<int, string>(1, "Supermercados de barrio"),
        };

        //relacion entre los mercados disponibles para una region
        public List<KeyValuePair<int, int>> RegionMarkets = new List<KeyValuePair<int, int>>
        {
            new KeyValuePair<int, int>(0, 0),
            new KeyValuePair<int, int>(0, 1),
            new KeyValuePair<int, int>(1, 0),
            new KeyValuePair<int, int>(1, 1),
            new KeyValuePair<int, int>(2, 0),
            new KeyValuePair<int, int>(2, 1),
            new KeyValuePair<int, int>(3, 0),
            new KeyValuePair<int, int>(3, 1),
            new KeyValuePair<int, int>(4, 0),
        };

        public List<KeyValuePair<int, string>> GetRegions()
        {
            return _regions;
        }

        public List<KeyValuePair<int, string>> GetMarkets()
        {
            return _markets;
        }

        public List<KeyValuePair<int, string>> GetMarketsByRegions(int region)
        {
            return RegionMarkets.Where(x => x.Key.Equals(region)).SelectMany(regionMarket => _markets.Where(x => x.Key.Equals(regionMarket.Value))).ToList();
        }

        public Guid InstallationId { get; set; }

        private GeoPosition<GeoCoordinate> _ubicacion;
        public GeoPosition<GeoCoordinate> Ubicacion
        {
            get { return _ubicacion ?? (_ubicacion = new GeoPosition<GeoCoordinate>()); }
            set { _ubicacion = value; }
        }

        public double MinDiffGeography = 0.0001;

        private string _version;
        public string Version
        {
            private set
            {
                var v = value;
#if DEBUG
                v += "d";
#endif
                _version = v;
            }

            get { return _version; }
        }

        public string Name { get; private set; }

        public int OpenCount { get; set; }

    }
}