using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
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
            else { TB_Ping.Text = "Connection failed..."; main.ReportError(new Exception("Ping not successfull")); return; }
            System.Windows.Threading.DispatcherTimer dT = new System.Windows.Threading.DispatcherTimer();
            dT.Tick += new EventHandler(SetPing);
            dT.Interval = new TimeSpan(0, 0, 1);
            dT.Start();
            
            MessageBox.Show(string.Join("\n", fs.readDirSync("..\\..\\..\\..")));
            MessageBox.Show(string.Join("\n", fs.readDirSync("..\\..\\..\\..", true)));
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

        public void AddToGrid(string FileName, string FullPath)
        {

        }

        private void tb_Search_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            /**Es sollten zuerst die Dateinamen und DANN erst Dateien mit dem Inhalt durchsucht werden */

            main.isIndexerRunning = true;
            fs.CF_Ind _ = db.getConf<fs.CF_Ind>("index");
            for(int i = 0; i < _.Paths.Length; i++)
            {

                System.Windows.Controls.TextBox txt = new System.Windows.Controls.TextBox();
                main.FileStructure oof = fs.searchFile(_.Paths[i], false);
                txt.Text += $"\n\n{oof.Filename} in {oof.Path}";
            }
            

        }


    }
}