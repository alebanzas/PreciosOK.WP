using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PreciosOK.Views
{
    public partial class Acerca : PhoneApplicationPage
    {
        public Acerca()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                SystemTray.BackgroundColor = new HexColor("#0071B7").GetColor();
            };
        }
    }
}