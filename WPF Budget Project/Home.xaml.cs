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
using System.IO;

namespace WPF_Budget_Project
{
    public partial class ArrayOfPacked
    {
        public TextBlock val;
        public TextBlock cat;
        public TextBlock dat;
        public bool income;
        public ArrayOfPacked(string x, string y, string z, bool k)
        {
            val = new TextBlock();
            val.Text = x;
            cat = new TextBlock();
            cat.Text = y;
            dat = new TextBlock();
            dat.Text = z;
            income = k;
        }
    };

    public partial class Home : Page
    {
        string UserMail;
        public Home(string mail)
        {
            InitializeComponent();
            Date.Text = "Today is: " + DateTime.Today.ToString("dd.MM.yyyy");
            Rounded = new SeriesCollection();
            Basic = new SeriesCollection();

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

            command = "select * from [gawdzinskikacper@gmail.com-balance]";
            conn = new SQLiteCommand(command, sqLiteConn);
            conn.ExecuteNonQuery();
            read = conn.ExecuteReader();

            List<string> Data = new List<string>();
            var AddNewValue = new ChartValues<ObservableValue>();
            double balance = 0;
            while (read.Read())
            {
                AddNewValue.Add(new ObservableValue((double)read["Balance"]));
                balance = (double)read["Balance"];
                Data.Add((string)read["Date"]);
            }
            Balance.Text = balance.ToString() + "$";
            Basic.Add(new LineSeries
            {
                Title = "Balance",
                Values = AddNewValue,
                LineSmoothness = 1,
            });
            Labels = Data.ToArray();
            for(int i=0;i<Labels.Length;i++)
            {
                Labels[i] = Labels[i].Remove(5, 5);
            }
            Console.WriteLine(Labels[0]);
            YFormatter = value => value.ToString("C", CultureInfo.CreateSpecificCulture("en-US"));
            DataContext = this;

            command = "select count(*) from [gawdzinskikacper@gmail.com-expend]";
            conn = new SQLiteCommand(command, sqLiteConn);
            int length = Convert.ToInt32(conn.ExecuteScalar());
            ArrayOfPacked[] ArrayOfPacked = GetFiveLast(sqLiteConn, length, ColumnNames, "select * from [gawdzinskikacper@gmail.com-expend]", false);

            ColumnNames = new List<string>();
            command = "select * from [gawdzinskikacper@gmail.com-income]";
            conn = new SQLiteCommand(command, sqLiteConn);
            conn.ExecuteNonQuery();
            read = conn.ExecuteReader();
            for (int i = 0; i < read.FieldCount; i++)
            {
                string temp = read.GetName(i);
                if (temp == "Date" || temp == "id" || temp == "Repeatability")
                    continue;
                ColumnNames.Add(temp);
            }

            command = "select count(*) from [gawdzinskikacper@gmail.com-income]";
            conn = new SQLiteCommand(command, sqLiteConn);
            int length2 = Convert.ToInt32(conn.ExecuteScalar());
            ArrayOfPacked[] ArrayOfPacked2 = GetFiveLast(sqLiteConn, length2, ColumnNames, "select * from [gawdzinskikacper@gmail.com-income]", true);
            length--; length2--;
            if (length > 4)
                length = 4;
            if (length2 > 4)
                length2 = 4;
            int size = 5; 

            while(size != 0 || (length == -1 && length2 == -1))
            {
                if (length == -1)
                {
                    LatestTransactions.Children.Add(MakeGrid(ArrayOfPacked2[length2]));
                    length2--; size--;
                    continue;
                }
                if (length2 == -1)
                {
                    LatestTransactions.Children.Add(MakeGrid(ArrayOfPacked[length]));
                    length--; size--;
                    continue;
                }
                int t = string.Compare(ArrayOfPacked[length].dat.Text, ArrayOfPacked2[length2].dat.Text);
                if(t == -1)
                {
                    LatestTransactions.Children.Add(MakeGrid(ArrayOfPacked2[length2]));
                    length2--; size--;
                }
                else if (t == 1)
                {
                    LatestTransactions.Children.Add(MakeGrid(ArrayOfPacked[length]));
                    length--; size--;
                }
                else
                {
                    LatestTransactions.Children.Add(MakeGrid(ArrayOfPacked[length]));
                    length--; size--;
                }
            }
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

        private ArrayOfPacked[] GetFiveLast(SQLiteConnection x, int length, List<string> ColumnNames, string command, bool income)
        {
            if (length == 0)
                return null;
            SQLiteCommand conn = new SQLiteCommand(command, x);
            BrushConverter bc = new BrushConverter();
            conn.ExecuteNonQuery();
            SQLiteDataReader read = conn.ExecuteReader();
            ArrayOfPacked[] ArrayOfPacked;
            int k = 0; double r; string d; string c;
            if (length > 5)
            {
                ArrayOfPacked = new ArrayOfPacked[5];
            }
            else
            {
                ArrayOfPacked = new ArrayOfPacked[length];
            }

            if (length > 5)
            {
                for (int i = 0; i != length - 5; i++)
                    read.Read();
            }

            while (read.Read()) //due to the lack of sql classes (next year) this code is kinda inefficient
            {
                d = (string)read["Date"];
                for (int j = 0; j < ColumnNames.Count; j++)
                {
                    try
                    {
                        r = (double)read[ColumnNames[j]];
                    }
                    catch (InvalidCastException)
                    {
                        continue;
                    }
                    ArrayOfPacked[k] = new ArrayOfPacked(r.ToString(), ColumnNames[j], d, income);
                    k++;
                }
            }
            return ArrayOfPacked;
        }

        private Grid MakeGrid(ArrayOfPacked x)
        {
            string c;
            BrushConverter bc = new BrushConverter();
            if (x.income)
                c = "#0fd628";
            else
                c = "#ff3445";
            Grid make = new Grid
            {
                Background = (Brush)bc.ConvertFrom(c),
            };
            
            TextBlock val = new TextBlock();
            TextBlock cat = new TextBlock();
            TextBlock dat = new TextBlock();
            if (x.income)
                val.Text = "+ ";
            else
                val.Text = "- ";
            val.Text += x.val.Text;
            val.Text += "$";
            val.HorizontalAlignment = HorizontalAlignment.Left;
            val.VerticalAlignment = VerticalAlignment.Center;
            val.Margin = new Thickness(10, 0, 0, 0);
            cat.Text = x.cat.Text;
            cat.HorizontalAlignment = HorizontalAlignment.Center;
            cat.VerticalAlignment = VerticalAlignment.Center;
            dat.Text = x.dat.Text;
            dat.HorizontalAlignment = HorizontalAlignment.Right;
            dat.VerticalAlignment = VerticalAlignment.Center;
            dat.Margin = new Thickness(0, 0, 10, 0);
            make.Children.Add(val);
            make.Children.Add(cat);
            make.Children.Add(dat);
            make.Height = 50;
            return make;
        }
    }
}
