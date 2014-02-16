using System;
using System.Device.Location;
using System.Windows;

namespace PreciosOK.Helpers
{
    public class PositionService
    {
        public static void Initialize()
        {
            if (App.Configuration.IsLocationEnabledByAppConfig)
            {
                if (Ubicacion == null)
                {
                    Ubicacion = new GeoCoordinateWatcher();
                    Ubicacion.StatusChanged += Ubicacion_StatusChanged;
                    Ubicacion.PositionChanged += Ubicacion_PositionChanged;
                    Ubicacion.MovementThreshold = 200;
                    Ubicacion.Start();
                }
                else
                {
                    Ubicacion.TryStart(true, TimeSpan.FromSeconds(5));
                }
            }
            else
            {
                MessageBox.Show("El servicio de localización se encuentra deshabilitado. Por favor asegúrese de habilitarlo en las Opciones de la aplicación para utilizar las caracteristicas que lo requeran.");
            }
        }

        public static void Stop()
        {
            try
            {
                Ubicacion.Stop();
            }
            catch { }
        }

        public static void Destroy()
        {
            Ubicacion.Dispose();
        }

        private static GeoCoordinateWatcher Ubicacion { get; set; }
        private static void Ubicacion_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            var distance = ((e.Position.Location.Latitude - App.Configuration.Ubicacion.Location.Latitude)*
                            (e.Position.Location.Latitude - App.Configuration.Ubicacion.Location.Latitude) +
                            (e.Position.Location.Longitude - App.Configuration.Ubicacion.Location.Longitude) * (e.Position.Location.Longitude - App.Configuration.Ubicacion.Location.Longitude));

            if (distance < App.Configuration.MinDiffGeography)
            {
                return;
            }

            App.Configuration.Ubicacion = e.Position;
        }

        private static void Ubicacion_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    MessageBox.Show(Ubicacion.Permission == GeoPositionPermission.Denied
                                        ? "El servicio de localización se encuentra deshabilitado. Por favor asegúrese de habilitarlo en las Opciones del dispositivo para utilizar las caracteristicas que lo requeran."
                                        : "El servicio de localización se encuentra sin funcionamiento.");
                    App.Configuration.IsLocationEnabledByPhone = false;
                    Ubicacion.Stop();
                    break;

                case GeoPositionStatus.Initializing: //Estado: Inicializando

                    break;

                case GeoPositionStatus.NoData: //Estado: Datos no disponibles
                    MessageBox.Show("El servicio de localización no puede obtener su posición.");
                    Ubicacion.Stop();
                    break;

                case GeoPositionStatus.Ready: //Estado: Servicio de localización disponible
                    App.Configuration.IsLocationEnabledByPhone = true;
                    break;
            }
        }

        public static GeoPosition<GeoCoordinate> GetCurrentLocation()
        {
            if (Ubicacion == null ||
                Ubicacion.Position == null ||
                Ubicacion.Position.Location == null ||
                double.IsNaN(Ubicacion.Position.Location.Latitude) ||
                double.IsNaN(Ubicacion.Position.Location.Longitude))
            {
                return null;
            }

            return Ubicacion.Position;
        }
    }
}
