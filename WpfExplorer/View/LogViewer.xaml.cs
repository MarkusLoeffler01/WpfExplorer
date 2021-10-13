using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfExplorer.ViewModel;

namespace WpfExplorer.View
{
    /// <summary>
    /// Interaktionslogik für LogViewer.xaml
    /// </summary>
    public partial class LogViewer : Window
    {
        public LogViewer()
        {
            InitializeComponent();
            this.Loaded += LogViewModel.instance.fillLogs;
        }

        private void MouseWheel(object sender, MouseWheelEventArgs e) => LogViewModel.instance.MouseWheel(sender, e);
    }
}
