using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfExplorer.ViewModel;

namespace WpfExplorer.View
{
    /// <summary>
    /// Interaktionslogik für DialogWindow.xaml
    /// </summary>
    public partial class UserSettingsWindow : Window
    {
        public UserSettingsWindow()
        {
            InitializeComponent();
            this.Loaded += InitVM;
        }
        private void InitVM(object sender, RoutedEventArgs e)
        {
            
            fs.checkUserSettings(true);
        }
    }
}
