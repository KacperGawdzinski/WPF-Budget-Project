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

//TODO - MAXVALUE MUST APPEAR ONLY ONCE WHEN USER INSERTS NEW TYPE!
//TODO - SIMULATION AT ADD
namespace WPF_Budget_Project
{
    public partial class ArrayOfPacked
    {
        public TextBlock val;
        public TextBlock cat;
        public TextBlock dat;
        public bool income;
        public ArrayOfPacked(string x, string y, long z, bool k)
        {
            val = new TextBlock();
            val.Text = x;
            cat = new TextBlock();
            cat.Text = y;
            dat = new TextBlock();
            dat.Text = z.ToString();
            income = k;
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

            var sqLiteConn = new SQLiteConnection(@"Data Source=database.db;Version=3;");
            sqLiteConn.Open();
            string IncomeDatabase = "[" + UserMail + "-income]";
            string ExpendDatabase = "[" + UserMail + "-expend]";
            string BalanceDatabase = "[" + UserMail + "-balance]";
            List<string> IncomeColumns = new List<string>(ColumnNames(true,sqLiteConn,IncomeDatabase));
            List<string> ExpendColumns = new List<string>(ColumnNames(false, sqLiteConn,ExpendDatabase));
            int length = TableLength(sqLiteConn, ExpendDatabase);
            int length2 = TableLength(sqLiteConn, IncomeDatabase);

            ArrayOfPacked[] p1 = GetFiveLast(sqLiteConn, length, ExpendColumns, "select * from " + ExpendDatabase, false);
            ArrayOfPacked[] p2 = GetFiveLast(sqLiteConn, length2, IncomeColumns, "select * from "+ IncomeDatabase, true);
            SimulateBalance(sqLiteConn, BalanceDatabase, IncomeDatabase, ExpendDatabase);
            BuildLastTransactions(sqLiteConn, length, length2, p1, p2);
            BuildLineChart(sqLiteConn, BalanceDatabase);
            BuildPieChart(ExpendColumns, sqLiteConn, ExpendDatabase);
            sqLiteConn.Close();
        }
        #endregion
        #region Charts
        void BuildPieChart(List<string> ColumnNamesList, SQLiteConnection sqLiteConn, string db)
        {
            Rounded = new SeriesCollection();
            double val;
            for (int i = 0; i < ColumnNamesList.Count(); i++)
            {
                SQLiteCommand SumCommand = new SQLiteCommand("SELECT SUM(" + ColumnNamesList[i] + ") FROM "+ db, sqLiteConn);
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
                    Title = ColumnNamesList[i]
                });
            }
        }

        void BuildLineChart(SQLiteConnection sqLiteConn, string db)
        {
            Basic = new SeriesCollection();
            SQLiteCommand conn = new SQLiteCommand("SELECT * FROM " + db + "ORDER BY DATE", sqLiteConn);
            conn.ExecuteNonQuery();
            SQLiteDataReader read = conn.ExecuteReader();

            List<string> Data = new List<string>();
            var AddNewValue = new ChartValues<ObservableValue>();
            double balance = 0;
            while (read.Read())
            {
                AddNewValue.Add(new ObservableValue((double)read["Balance"]));
                balance = (double)read["Balance"];
                Data.Add(((long)read["Date"]).ToString());
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
        void BuildLastTransactions(SQLiteConnection sqLiteConn, int length, int length2, ArrayOfPacked[] p1, ArrayOfPacked[] p2)
        {//TODO - fix bug if dates aren't sorted 
            length--; length2--;
            if (length > 4)
                length = 4;
            if (length2 > 4)
                length2 = 4;
            int size = 5;

            while (size != 0 && (length != -1 || length2 != -1))
            {
                if (length == -1)
                {
                    LatestTransactions.Children.Add(MakeGrid(p2[length2]));
                    length2--; size--;
                    continue;
                }
                if (length2 == -1)
                {
                    LatestTransactions.Children.Add(MakeGrid(p1[length]));
                    length--; size--;
                    continue;
                }
                int t = string.Compare(p1[length].dat.Text, p2[length2].dat.Text);
                if (t == -1)
                {
                    LatestTransactions.Children.Add(MakeGrid(p2[length2]));
                    length2--; size--;
                }
                else if (t == 1)
                {
                    LatestTransactions.Children.Add(MakeGrid(p1[length]));
                    length--; size--;
                }
                else
                {
                    LatestTransactions.Children.Add(MakeGrid(p1[length]));
                    length--; size--;
                }
            }
        }

        List<string> ColumnNames(bool income, SQLiteConnection sqLiteConn, string db)
        {
            List<string> ColumnNamesList = new List<string>();
            SQLiteCommand conn;
            if (income)
                conn = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " + db +" (NO INTEGER PRIMARY KEY AUTOINCREMENT, ID TEXT, DATE INTEGER, REPEATABILITY TEXT)", sqLiteConn);
            else
                conn = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " + db + " (NO INTEGER PRIMARY KEY AUTOINCREMENT, ID TEXT, DATE INTEGER, REPEATABILITY TEXT, MAXVALUE REAL)", sqLiteConn);
            conn.ExecuteNonQuery();
            conn = new SQLiteCommand("select * from " + db, sqLiteConn);
            conn.ExecuteNonQuery();
            SQLiteDataReader read = conn.ExecuteReader();
            for (int i = 0; i < read.FieldCount; i++)
            {
                string temp = read.GetName(i);
                if (income)
                {
                    if (temp == "DATE" || temp == "ID" || temp == "REPEATABILITY" || temp == "NO")
                        continue;
                }
                else
                {
                    if (temp == "DATE" || temp == "ID" || temp == "REPEATABILITY" || temp == "NO" || temp == "MAXVALUE")
                        continue;
                }
                ColumnNamesList.Add(temp);
            }
            read.Close();
            return ColumnNamesList;
        }

        int TableLength(SQLiteConnection sqLiteConn, string db)
        { 
            SQLiteCommand conn = new SQLiteCommand("select count(*) from "+db, sqLiteConn);
            return Convert.ToInt32(conn.ExecuteScalar());
        }

        ArrayOfPacked[] GetFiveLast(SQLiteConnection x, int length, List<string> ColumnNames, string command, bool income)
        {
            if (length == 0)
                return null;
            SQLiteCommand conn = new SQLiteCommand(command, x);
            BrushConverter bc = new BrushConverter();
            conn.ExecuteNonQuery();
            SQLiteDataReader read = conn.ExecuteReader();
            ArrayOfPacked[] ArrayOfPacked;
            int k = 0; double r; long d; string c;
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
                d = (long)read["Date"];
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
