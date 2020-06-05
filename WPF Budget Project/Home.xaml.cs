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
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.Data.SQLite;
namespace WPF_Budget_Project
{
    public partial class Home : Page
    {
        string UserMail;
        string dbConnectionString;
        public Home(string x, string y)
        {
            UserMail = x;
            InitializeComponent();
            Balance.Text = "1";
            /*dbConnectionString = y;
            string sql = "create table xd (name TEXT, score TEXT)";
            SQLiteConnection m_dbConnection = new SQLiteConnection(dbConnectionString);
            m_dbConnection.Open();
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            m_dbConnection.Close();*/

            SeriesCollection = new SeriesCollection
            {
                /*new PieSeries
                {
                    Title = "Food",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(20) },
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "Taxes",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(2) },
                   // DataLabels = true
                },
                new PieSeries
                {
                    Title = "",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(10) },
                   // DataLabels = true
                },
                new PieSeries
                {
                    Title = "Explorer",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(4) },
                  //  DataLabels = true
                }*/
            };
            DataContext = this;
        }

        public SeriesCollection SeriesCollection { get; set; }

        private void AddSeriesOnClick(object sender, RoutedEventArgs e)
        {
            var r = new Random();
            var c = SeriesCollection.Count > 0 ? SeriesCollection[0].Values.Count : 1;

            var vals = new ChartValues<ObservableValue>();

            for (var i = 0; i < c; i++)
            {
                vals.Add(new ObservableValue(r.Next(0, 10)));
            }

            SeriesCollection.Add(new PieSeries
            {
                Values = vals
            });
        }

        private void RemoveSeriesOnClick(object sender, RoutedEventArgs e)
        {
            if (SeriesCollection.Count > 0)
                SeriesCollection.RemoveAt(0);
        }

        private void RestartOnClick(object sender, RoutedEventArgs e)
        {
            Chart.Update(true, true);
        }
    }
}
