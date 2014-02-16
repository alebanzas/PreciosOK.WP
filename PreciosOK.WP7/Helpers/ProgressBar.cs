using System.Windows;
using Microsoft.Phone.Shell;

namespace PreciosOK.WP7.Helpers
{
    public static class ProgressBar
    {
        private static readonly ProgressIndicator Progress = new ProgressIndicator();
        public static UIElement UIElement;
        public static void Initialize()
        {
            Progress.IsVisible = true;
            Progress.IsIndeterminate = true;
        }

        public static void Show(string msj, bool showProgress = true)
        {
            Progress.Text = msj;
            Progress.IsIndeterminate = showProgress;
            SystemTray.SetIsVisible(UIElement, true);
            SystemTray.SetProgressIndicator(UIElement, Progress);
        }

        public static void Hide()
        {
            SystemTray.SetProgressIndicator(UIElement, null);
        }
    }
}
