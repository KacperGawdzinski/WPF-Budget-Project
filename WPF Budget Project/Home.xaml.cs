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
using System.Diagnostics;
using System.Data;
using System.IO;

//TODO - MAXVALUE MUST APPEAR ONLY ONCE WHEN USER INSERTS NEW TYPE!
//TODO - SIMULATION AT ADD
namespace WPF_Budget_Project
{
    public partial class ArrayOfPacked
    {
        public TextBlock val;
        public TextBlock cat;
        public TextBlock dat;
        public bool income = true;
        public ArrayOfPacked(string v, string type, long date, string category)
        {
            val = new TextBlock();
            val.Text = v;
            cat = new TextBlock();
            cat.Text = type;
            dat = new TextBlock();
            dat.Text = date.ToString();
            if (category.Equals("Expend"))
                income = false;
        }
    };

    public partial class Home : Page
    {
        #region Constructor & Variables
        string UserMail;
        public Home(string mail)
        {
            UserMail = mail;
            InitializeComponent();
            Date.Text = "Today is: " + DateTime.Today.ToString("dd.MM.yyyy");
            SQLiteConnection sqLiteConn = new SQLiteConnection(@"Data Source=database.db;Version=3;");
            sqLiteConn.Open();
            string TransactionDatabase = "[" + UserMail + "-transactions]";
            string BalanceDatabase = "[" + UserMail + "-balance]";
            BuildLastTransactions(sqLiteConn, TransactionDatabase);
            //SimulateBalance(sqLiteConn, BalanceDatabase, IncomeDatabase, ExpendDatabase);
            BuildLineChart(sqLiteConn, BalanceDatabase);
            BuildPieChart(sqLiteConn, TransactionDatabase);
            sqLiteConn.Close();
        }
        #endregion
        #region Charts
        void BuildPieChart(SQLiteConnection sqLiteConn, string db)
        {
            Rounded = new SeriesCollection();
            double val;

            List<string> ExpendTypes = new List<string>();
            SQLiteCommand comm = new SQLiteCommand("SELECT DISTINCT TYPE FROM " + db + "WHERE [CATEGORY]='Expend'", sqLiteConn);
            SQLiteDataReader read = comm.ExecuteReader();
            while (read.Read())
                ExpendTypes.Add((string)read["Type"]);
            for (int i = 0; i < ExpendTypes.Count(); i++)
            {
                SQLiteCommand SumCommand = new SQLiteCommand("SELECT SUM('" + ExpendTypes[i] + "') FROM "+ db + "WHERE DATE>='" + DateTime.Today.AddDays(-31).ToString("yyyMMdd") + "' AND WHERE [CATEGORY]='Expend'", sqLiteConn);
                try
                {
                    val = (double)SumCommand.ExecuteScalar();
                }
                catch(InvalidCastException)
                {
                    continue;
                }
                var AddValue = new ChartValues<ObservableValue>();
                AddValue.Add(new ObservableValue(val));
                Rounded.Add(new PieSeries
                {
                    Values = AddValue,
                    Title = ExpendTypes[i]
                });
            }
        }

        void BuildLineChart(SQLiteConnection sqLiteConn, string db)
        {
            Basic = new SeriesCollection();
            SQLiteCommand conn = new SQLiteCommand("SELECT * FROM " + db + "WHERE DATE>=" + DateTime.Today.AddDays(-31).ToString("yyyMMdd") + " ORDER BY DATE", sqLiteConn);
            conn.ExecuteNonQuery();
            SQLiteDataReader read = conn.ExecuteReader();

            List<string> Data = new List<string>();
            var AddNewValue = new ChartValues<ObservableValue>();
            double balance = 0;
            while (read.Read())
            {
                AddNewValue.Add(new ObservableValue((double)read["Balance"]));
                balance = (double)read["Balance"];
                Data.Add(((long)read["Date"]).ToString().Remove(0,4).Insert(2,"/"));
            }
            Balance.Text = balance.ToString() + "$";
            Basic.Add(new LineSeries
            {
                Title = "Balance history",
                Values = AddNewValue,
                LineSmoothness = 1,
            });
            Labels = Data.ToArray();/*
            for (int i = 0; i < Labels.Length; i++)?????????
            {
                Labels[i] = Labels[i].Remove(5, 5);
            }*/
            YFormatter = value => value.ToString("C", CultureInfo.CreateSpecificCulture("en-US"));
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
        #endregion
        #region LastTransactionList
        void BuildLastTransactions(SQLiteConnection x, string db)
        {
            SQLiteCommand comm = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " + db + " (ID TEXT, DATE INTEGER, REPEATABILITY TEXT, CATEGORY TEXT, TYPE TEXT, VALUE REAL, MAXVALUE REAL)", x);
            comm.ExecuteNonQuery();
            comm = new SQLiteCommand("SELECT COUNT(*) FROM (SELECT * FROM " + db + "ORDER BY DATE DESC LIMIT 5)", x);
            long length = (long)comm.ExecuteScalar();
            comm = new SQLiteCommand("SELECT * FROM " + db + "ORDER BY DATE DESC LIMIT 5", x);
            SQLiteDataReader read = comm.ExecuteReader();
            ArrayOfPacked[] ArrayOfPacked = new ArrayOfPacked[length];
            int k = 0;
            while (read.Read())
            {
                ArrayOfPacked[k] = new ArrayOfPacked(((double)read["VALUE"]).ToString(), (string)read["TYPE"], (long)read["DATE"], (string)read["CATEGORY"]);
                LatestTransactions.Children.Add(MakeGrid(ArrayOfPacked[k]));
                k++;
            }
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
        #endregion
        #region Simulation
        void SimulateBalance(SQLiteConnection sqLiteConn, string dbb, string dbi, string dbe)
        {
            //table creator
            SQLiteCommand comm = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " + dbb + " (NO INTEGER PRIMARY KEY AUTOINCREMENT, BALANCE REAL, DATE INTEGER)", sqLiteConn);
            comm.ExecuteNonQuery();
            comm = new SQLiteCommand("SELECT count(*) FROM " + dbb, sqLiteConn);
            int row_sum = Convert.ToInt32(comm.ExecuteScalar());
            if(row_sum == 0)
            {
                comm = new SQLiteCommand("INSERT INTO " + dbb +" (BALANCE,DATE) values('0','" + DateTime.Today.ToString("yyyyMMdd") + "')", sqLiteConn);
                comm.ExecuteNonQuery();
                return;
            }

            //checking latest date, if it's equal to today's return
            comm = new SQLiteCommand("SELECT* FROM "+ dbb + "ORDER BY DATE DESC LIMIT 1", sqLiteConn);
            comm.ExecuteNonQuery();
            SQLiteDataReader read = comm.ExecuteReader();
            read.Read();
            long LastDate = (long)read["DATE"];
            double v = (double)read["BALANCE"];
            if (Convert.ToInt64(DateTime.Today.ToString("yyyyMMdd")) == LastDate)
                return;

            //analysing periodic transactions
            /*
            List<double> vals = new List<double>();
            DateTime x = ConvertToClass(LastDate);
            DateTime LastDateClass = ConvertToClass(LastDate);
            //temp = temp.AddDays(1);
            TimeSpan number_between = DateTime.Today.Subtract(x);
            for(int i=0;i<number_between.TotalDays;i++)
            {
                vals.Add(v);
               // comm = new SQLiteCommand("INSERT INTO " + dbb + " (BALANCE,DATE) values('"+ v +"','" + temp.ToString("yyyyMMdd") + "')", sqLiteConn);
               // temp = temp.AddDays(1);
               // comm.ExecuteNonQuery();
            }

            x = x.AddMonths(-1);
            comm = new SQLiteCommand("SELECT * FROM "+ dbe +" WHERE DATE >= " + x.ToString("yyyyMMdd") + " AND REPEATABILITY IS NOT NULL ORDER BY DATE DESC", sqLiteConn);
            read = comm.ExecuteReader();
            List<string> id = new List<string>();
            while(read.Read())
            {
                    //Console.WriteLine(x.ToString());//wieloktornosci teraa
                    string code = (string)read["ID"];
                if (id.Contains(code))
                    continue;
                id.Add(code);
                vals = CountNewBalance((string)read["REPEATABILITY"], LastDateClass, ConvertToClass((long)read["DATE"]));

            }
        }

        List<double> CountNewBalance(string period, DateTime actual, DateTime temp)
        {
            TimeSpan between = actual.Subtract(temp);
            Console.WriteLine(between.TotalDays);
            return (new List<double>());

        }

        DateTime ConvertToClass(long date)
        {
            int a, b, c;
            a = (int)(date / 10000);
            b = (int)((date - a * 10000) / 100);
            c = (int)(date - a * 10000 - b * 100);
            DateTime x = new DateTime(a, b, c);
            return x;
        }*/
        }
        #endregion
    }
}
