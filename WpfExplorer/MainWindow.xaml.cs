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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static WpfExplorer.db;
using static WpfExplorer.main;

namespace WpfExplorer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if(main.PingDB()) { TB_Ping.Text = "Connected"; }
            else { TB_Ping.Text = "Connection failed..."; ReportError(new Exception("Ping not successfull")); return; }
            DispatcherTimer dT = new System.Windows.Threading.DispatcherTimer();
            dT.Tick += new EventHandler(SetPing);
            dT.Interval = new TimeSpan(0, 0, 1);
            dT.Start();
        }

        private void SetPing(object sender, EventArgs e)
        {
            double PingTime = db.PingDB();
            TB_PingTime.Text = $"{PingTime}ms";
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<string> x = db.query("SELECT 1+1;");
            string res = "";
            for (int i = 0; i < x.Count; i++)
            {
                res += x[i];
                res += "\n\n";
            }
            MessageBox.Show(res);
        }
    }
}