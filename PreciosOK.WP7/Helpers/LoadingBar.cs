using System.ComponentModel;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PreciosOK.WP7.Helpers
{
    public class LoadingBar : INotifyPropertyChanged
    {
        private ProgressIndicator _mangoIndicator;

        private LoadingBar()
        {
        }

        public void Initialize(PhoneApplicationFrame frame)
        {
            _mangoIndicator = new ProgressIndicator();

            frame.Navigated += OnRootFrameNavigated;
        }

        private void OnRootFrameNavigated(object sender, NavigationEventArgs e)
        {
            // Use in Mango to share a single progress indicator instance.
            var ee = e.Content;
            var pp = ee as PhoneApplicationPage;
            if (pp != null)
            {
                pp.SetValue(SystemTray.ProgressIndicatorProperty, _mangoIndicator);
            }
        }
        
        private static LoadingBar _in;
        public static LoadingBar Instance
        {
            get { return _in ?? (_in = new LoadingBar()); }
        }

        public bool IsDataManagerLoading { get; set; }

        public bool ActualIsLoading
        {
            get
            {
                return IsLoading || IsDataManagerLoading;
            }
        }

        private int _loadingCount;

        public bool IsLoading
        {
            get
            {
                return _loadingCount > 0;
            }
            set
            {
                if (value)
                {
                    ++_loadingCount;
                }
                else
                {
                    --_loadingCount;
                }

                NotifyValueChanged();
            }
        }

        private void NotifyValueChanged()
        {
            if (_mangoIndicator == null) return;

            _mangoIndicator.IsIndeterminate = _loadingCount > 0 || IsDataManagerLoading;
            if (_mangoIndicator.IsVisible == false)
            {
                _mangoIndicator.IsVisible = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
