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
using System.CodeDom;
using System.Data.Entity.ModelConfiguration.Configuration;

//TODO - MAXVALUE MUST APPEAR ONLY ONCE WHEN USER INSERTS NEW TYPE!
namespace WPF_Budget_Project
{
    public partial class ArrayOfPacked
    {
        public TextBlock val;
        public TextBlock cat;
        public TextBlock dat;
        public bool income = true;
        public ArrayOfPacked(string v, string type, string date, string category)
        {
            val = new TextBlock();
            val.Text = v;
            cat = new TextBlock();
            cat.Text = type;
            dat = new TextBlock();
            dat.Text = date;
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
            BuildTables(sqLiteConn, BalanceDatabase, TransactionDatabase);
            SimulateBalance(sqLiteConn, BalanceDatabase, TransactionDatabase);
            BuildLastTransactions(sqLiteConn, TransactionDatabase);
            BuildLineChart(sqLiteConn, BalanceDatabase);
            BuildPieChart(sqLiteConn, TransactionDatabase);
            sqLiteConn.Close();
        }
        #endregion
        #region Build Tables
        void BuildTables(SQLiteConnection x, string dbb, string dbt)
        {
            //check if balance table exists
            SQLiteCommand comm = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " + dbb + " (BALANCE REAL, DATE INTEGER)", x);
            comm.ExecuteNonQuery();
            comm = new SQLiteCommand("SELECT COUNT(*) FROM " + dbb, x);  //check how fast it will be with hundreds of rows
            int row_sum = Convert.ToInt32(comm.ExecuteScalar());
            if (row_sum == 0)
            {
                comm = new SQLiteCommand("INSERT INTO " + dbb + " (BALANCE,DATE) values('0','" + DateTime.Today.ToString("yyyyMMdd") + "')",x);
                comm.ExecuteNonQuery();
            }

            //check if transaction table exists
            comm = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " + dbt + " (ID TEXT, DATE INTEGER, REPEATABILITY TEXT, CATEGORY TEXT, TYPE TEXT, VALUE REAL, MAXVALUE REAL)", x);
            comm.ExecuteNonQuery();
        }
        #endregion
        #region Charts
        void BuildPieChart(SQLiteConnection sqLiteConn, string db)
        {
            Rounded = new SeriesCollection();
            double val;
            List<string> ExpendTypes = new List<string>();  //TODO: make class of next 3 lines
            SQLiteCommand comm = new SQLiteCommand("SELECT DISTINCT TYPE FROM " + db + "WHERE [CATEGORY]='Expend'", sqLiteConn);
            SQLiteDataReader read = comm.ExecuteReader();
            while (read.Read())
                ExpendTypes.Add((string)read["Type"]);
            for (int i = 0; i < ExpendTypes.Count(); i++)
            {
                comm = new SQLiteCommand("SELECT COALESCE (SUM(VALUE),0.0) FROM " + db + " WHERE [DATE] >='" + DateTime.Today.AddDays(-31).ToString("yyyyMMdd") 
                                          + "' AND [CATEGORY]='Expend' AND [TYPE]='"+ ExpendTypes[i]+"'", sqLiteConn);
                val = (double)comm.ExecuteScalar();
                var AddValue = new ChartValues<ObservableValue>();
                if (val != 0)
                {
                    AddValue.Add(new ObservableValue(val));
                    Rounded.Add(new PieSeries
                    {
                        Values = AddValue,
                        Title = ExpendTypes[i]
                    });
                }
            }
        }

        void BuildLineChart(SQLiteConnection sqLiteConn, string db)
        {
            Basic = new SeriesCollection();
            SQLiteCommand comm = new SQLiteCommand("SELECT * FROM " + db + "WHERE DATE>=" + DateTime.Today.AddDays(-31).ToString("yyyMMdd") + " ORDER BY DATE", sqLiteConn);
            comm.ExecuteNonQuery();
            SQLiteDataReader read = comm.ExecuteReader();

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
                LineSmoothness = 0,
            });
            Labels = Data.ToArray();
            YFormatter = value => Math.Round(value,2).ToString() + "$";
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
            SQLiteCommand comm = new SQLiteCommand("SELECT COUNT(*) FROM (SELECT * FROM " + db + "ORDER BY DATE DESC LIMIT 5)", x);
            long length = (long)comm.ExecuteScalar();
            comm = new SQLiteCommand("SELECT * FROM " + db + "ORDER BY DATE DESC LIMIT 5", x);
            SQLiteDataReader read = comm.ExecuteReader();
            ArrayOfPacked[] ArrayOfPacked = new ArrayOfPacked[length];
            int k = 0;
            while (read.Read())
            {
                ArrayOfPacked[k] = new ArrayOfPacked(((double)read["VALUE"]).ToString(), (string)read["TYPE"], DivideDate((long)read["DATE"]), (string)read["CATEGORY"]);
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

        string DivideDate(long date)
        {
            string s = date.ToString();
            s = s.Insert(4, "/");
            return (s.Insert(7, "/"));
        }
        #endregion
        #region Simulation
        void SimulateBalance(SQLiteConnection sqLiteConn, string dbb, string dbt)
        {
            //checking latest date, if it's equal to today's then return
            SQLiteCommand comm = new SQLiteCommand("SELECT * FROM " + dbb + "ORDER BY DATE DESC LIMIT 1", sqLiteConn);
            comm.ExecuteNonQuery();
            SQLiteDataReader read = comm.ExecuteReader();
            read.Read();
            long LastDate = (long)read["DATE"];
            double v = (double)read["BALANCE"];
            if (Convert.ToInt64(DateTime.Today.ToString("yyyyMMdd")) == LastDate)
                return;

            //analysing periodic transactions
            LongToDateTime x = new LongToDateTime();
            DateTime t1 = x.ConvertToClass(LastDate);
            DateTime t2 = x.ConvertToClass(Convert.ToInt64(DateTime.Today.ToString("yyyyMMdd")));
            t1 = t1.AddDays(1);
            TimeSpan y = t2.Subtract(t1);
            double k = y.TotalDays;
            while (k >= 0)
            {
                comm = new SQLiteCommand("INSERT INTO " + dbb + " (BALANCE, DATE) VALUES('" + v + "', '" + t1.ToString("yyyyMMdd") + "')", sqLiteConn);
                comm.ExecuteNonQuery();
                t1 = t1.AddDays(1);
                k--;
            }
            List<string> codes = new List<string>();
            //find last month's periodic transactions
            comm = new SQLiteCommand("SELECT * FROM " + dbt + " WHERE DATE >= '" + x.ConvertToClass(LastDate).AddMonths(-1).ToString("yyyyMMdd") + "' AND [REPEATABILITY] IS NOT NULL ORDER BY DATE DESC", sqLiteConn);
            read = comm.ExecuteReader();
            while(read.Read())
            {
                string s = (string)read["ID"];
                if (!codes.Contains(s))
                {
                    string[] p = new string[6];
                    p[0] = ((long)read["DATE"]).ToString();
                    if (DBNull.Value.Equals(read["REPEATABILITY"]))
                        p[1] = null;
                    else
                        p[1] = (string)read["REPEATABILITY"];
                    p[2] = (string)read["CATEGORY"];
                    p[3] = (string)read["TYPE"];
                    p[4] = ((double)read["VALUE"]).ToString();
                    if (DBNull.Value.Equals(read["MAXVALUE"]))
                        p[5] = null;
                    else
                        p[5] = ((double)read["MAXVALUE"]).ToString();
                    codes.Add(s);
                    SimulateTransaction(sqLiteConn, p, s, dbb);
                }
            }
        }

        void SimulateTransaction(SQLiteConnection sqLiteConn, string[] InputData, string guid, string dbb)    //make a class of it - redef
        {
            SQLiteCommand comm = new SQLiteCommand("SELECT* FROM " + dbb + " WHERE DATE > '" + InputData[0] + "' ORDER BY DATE", sqLiteConn);
            SQLiteDataReader read = comm.ExecuteReader();
            double val = 0;
            int l, temp = 1;
            if (InputData[1].Equals("Monthly"))
                l = 1;
            else if (InputData[1].Equals("Weekly"))
                l = 7;
            else
                l = 2;
            while (read.Read())
            {
                if (l == 7 && temp == 7)
                {
                    comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-transactions] (ID, DATE, REPEATABILITY, CATEGORY, TYPE, VALUE, MAXVALUE) " +
                           "values('" + guid + "', '" + ((long)read["Date"]).ToString() + "', " + NullReturner(InputData[1]) + ", '" + InputData[2] + "', '" + InputData[3] + "', '" +
                           InputData[4] + "', " + NullReturner(InputData[5]) + ")", sqLiteConn);
                    comm.ExecuteNonQuery();
                    temp = 0;
                    val = val + Convert.ToDouble(InputData[4]);
                }
                if (l == 1)
                {
                    long d = ((long)read["Date"]) % 100;
                    if (d == Convert.ToInt64(InputData[0].Remove(0, 6)))
                    {
                        comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-transactions] (ID, DATE, REPEATABILITY, CATEGORY, TYPE, VALUE, MAXVALUE) " +
                               "values('" + guid + "', '" + ((long)read["Date"]).ToString() + "', " + NullReturner(InputData[1]) + ", '" + InputData[2] + "', '" + InputData[3] + "', '" +
                               InputData[4] + "', " + NullReturner(InputData[5]) + ")", sqLiteConn);
                        comm.ExecuteNonQuery();
                        val = val + Convert.ToDouble(InputData[4]);
                    }
                }
                if (l == 2)
                {
                    comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-transactions] (ID, DATE, REPEATABILITY, CATEGORY, TYPE, VALUE, MAXVALUE) " +
                               "values('" + guid + "', '" + ((long)read["Date"]).ToString() + "', " + NullReturner(InputData[1]) + ", '" + InputData[2] + "', '" + InputData[3] + "', '" +
                               InputData[4] + "', " + NullReturner(InputData[5]) + ")", sqLiteConn);
                    comm.ExecuteNonQuery();
                    val = val + Convert.ToDouble(InputData[4]);
                }

                if (InputData[2].Equals("Income"))
                    comm = new SQLiteCommand("UPDATE [" + UserMail + "-balance] SET BALANCE='"
                    + ((double)read["Balance"] + val).ToString() + "' WHERE DATE='" + ((long)read["Date"]).ToString() + "'", sqLiteConn);
                else
                    comm = new SQLiteCommand("UPDATE [" + UserMail + "-balance] SET BALANCE='"
                    + ((double)read["Balance"] - val).ToString() + "' WHERE DATE='" + ((long)read["Date"]).ToString() + "'", sqLiteConn);
                comm.ExecuteNonQuery();
                temp++;
            }
        }
        string NullReturner(string s)
        {
            if (s == null)
                return "NULL";
            return ("'" + s + "'");
        }
        #endregion
    }
}
