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
using System.Globalization;
using System.Data;

namespace WPF_Budget_Project
{
    public partial class Home : Page
    {
        string UserMail;
        public Home(string mail)
        {
            InitializeComponent();
            Date.Text = "Today is: " + DateTime.Today.ToString("dd.MM.yyyy");
            Rounded = new SeriesCollection();
            DataContext = this;

            UserMail = mail;
            Balance.Text = "1";
            List<string> ColumnNames = new List<string>();
            var sqLiteConn = new SQLiteConnection(@"Data Source=database.db;Version=3;");
            sqLiteConn.Open();
            string command = "select * from [gawdzinskikacper@gmail.com-expend]";
            SQLiteCommand conn = new SQLiteCommand(command, sqLiteConn);
            conn.ExecuteNonQuery();
            SQLiteDataReader read = conn.ExecuteReader();
            for (int i = 0; i < read.FieldCount; i++)
            {
                string temp = read.GetName(i);
                if (temp == "Date" || temp == "id" || temp == "Repeatability" || temp == "MaxValue")
                    continue;
                ColumnNames.Add(temp);
            }

            for (int i = 0; i < ColumnNames.Count(); i++)
            {
                SQLiteCommand SumCommand = new SQLiteCommand("SELECT SUM(" + ColumnNames[i] + ") FROM [gawdzinskikacper@gmail.com-expend]", sqLiteConn);
                double val = (double)SumCommand.ExecuteScalar();
                var AddValue = new ChartValues<ObservableValue>();
                AddValue.Add(new ObservableValue(val));
                Rounded.Add(new PieSeries
                {
                    Values = AddValue,
                    Title = ColumnNames[i]
                });
            }
        
            Basic = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Series 1",
                    Values = new ChartValues<double> { 4, 6, 5, 2 ,4 }
                },
                /*new LineSeries
                {
                    Title = "Series 2",
                    Values = new ChartValues<double> { 6, 7, 3, 4 ,6 },
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Series 3",
                    Values = new ChartValues<double> { 4,2,7,2,7 },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 15
                }*/
            };

            Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
            YFormatter = value => value.ToString("C", CultureInfo.CreateSpecificCulture("en-US"));


            //modifying the series collection will animate and update the chart
            /*Basic.Add(new LineSeries
            {
                Title = "Series 4",
                Values = new ChartValues<double> { 5, 3, 2, 4 },
                LineSmoothness = 0, //0: straight lines, 1: really smooth lines
                PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                PointGeometrySize = 50,
                PointForeground = Brushes.Gray
            });

            //modifying any series values will also animate and update the chart
            Basic[3].Values.Add(5d);*/

            DataContext = this;
        }

        public SeriesCollection Rounded { get; set; }
        public SeriesCollection Basic { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        private void RemoveSeriesOnClick(object sender, RoutedEventArgs e)
        {
            if (Rounded.Count > 0)
                Rounded.RemoveAt(0);
        }

        private void RestartOnClick(object sender, RoutedEventArgs e)
        {
            Chart.Update(true, true);
        }
    }
}
