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

namespace Clerk
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
            sqLiteConn.Close();
            sqLiteConn.Open();
            SQLiteCommand comm = new SQLiteCommand("SELECT * FROM USERINFO WHERE MAIL = '" + UserMail + "'", sqLiteConn);
            SQLiteDataReader read = comm.ExecuteReader();
            read.Read();
            Hello.Text = "Hello " + (string)read["USERNAME"];
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
            SQLiteCommand comm = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " + dbb + " (BALANCE REAL, DATE TEXT)", x);
            comm.ExecuteNonQuery();
            comm = new SQLiteCommand("SELECT COUNT(*) FROM " + dbb, x);  //check how fast it will be with hundreds of rows
            int row_sum = Convert.ToInt32(comm.ExecuteScalar());
            if (row_sum == 0)
            {
                comm = new SQLiteCommand("INSERT INTO " + dbb + " (BALANCE,DATE) values('0.0','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')",x);
                comm.ExecuteNonQuery();
            }

            //check if transaction table exists
            comm = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " + dbt + " (ID TEXT, DATE TEXT, REPEATABILITY TEXT, CATEGORY TEXT, TYPE TEXT, VALUE REAL, MAXVALUE REAL)", x);
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
                comm = new SQLiteCommand("SELECT COALESCE (SUM(VALUE), 0.0) FROM " + db + " WHERE DATE([DATE]) >='" + DateTime.Today.AddMonths(-1).ToString("yyyy-MM-dd") 
                                          + "' AND [CATEGORY] = 'Expend' AND [TYPE] = '"+ ExpendTypes[i]+"'", sqLiteConn);
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
            SQLiteCommand comm = new SQLiteCommand("SELECT * FROM " + db + "WHERE DATETIME([DATE]) >= '" + DateTime.Today.AddMonths(-1).ToString("yyyy-MM-dd") + "' ORDER BY DATE([DATE])", sqLiteConn);
            comm.ExecuteNonQuery();
            SQLiteDataReader read = comm.ExecuteReader();

            List<string> Data = new List<string>();
            var AddNewValue = new ChartValues<ObservableValue>();
            double balance = 0;
            while (read.Read())
            {
                AddNewValue.Add(new ObservableValue((double)read["Balance"]));
                balance = (double)read["Balance"];
                Data.Add(((string)read["Date"]).Remove(0, 5).Remove(5,9));
            }
            AdjustBalanceFont(balance, sqLiteConn);
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

        void AdjustBalanceFont(double balance, SQLiteConnection sqLiteConn)
        {
            SQLiteCommand comm = new SQLiteCommand("SELECT * FROM USERINFO WHERE MAIL = '" + UserMail + "'", sqLiteConn);
            comm.ExecuteNonQuery();
            SQLiteDataReader read = comm.ExecuteReader();
            read.Read();
            Balance.Text = balance.ToString() + " " +(string)read["CURRENCY"];
            int x = Balance.Text.Length;
            if (x > 11)
            {
                Balance.FontSize = 26;
                Balance.Margin = new Thickness(0, 0, 0, 10);
                return;
            }
            if (x > 10)
            {
                Balance.FontSize = 28;
                Balance.Margin = new Thickness(0,0,0,10);
                return;
            }
            if (x > 9)
            {
                Balance.FontSize = 30;
                Balance.Margin = new Thickness(0, 0, 0, 15);
                return;
            }
        }
        #endregion
        #region LastTransactionList
        void BuildLastTransactions(SQLiteConnection x, string db)
        {
            SQLiteCommand comm = new SQLiteCommand("SELECT COUNT (*) FROM (SELECT * FROM " + db + " LIMIT 5)", x);
            long length = (long)comm.ExecuteScalar();
            comm = new SQLiteCommand("SELECT * FROM " + db + "ORDER BY DATETIME([DATE]) DESC LIMIT 5", x);
            SQLiteDataReader read = comm.ExecuteReader();
            ArrayOfPacked[] ArrayOfPacked = new ArrayOfPacked[length];
            int k = 0;
            while (read.Read())
            {
                ArrayOfPacked[k] = new ArrayOfPacked(((double)read["VALUE"]).ToString(), (string)read["TYPE"], ((string)read["DATE"]).Remove(10,9), (string)read["CATEGORY"]);
                LatestTransactions.Children.Add(MakeGrid(ArrayOfPacked[k]));
                k++;
            }
        }

        private Grid MakeGrid(ArrayOfPacked x)
        {
            TextBlock val = new TextBlock()
            {
                Text = (x.income ? "+ " : "- ") + x.val.Text + "$",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };
            TextBlock cat = new TextBlock()
            {
                Text = x.cat.Text,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            TextBlock dat = new TextBlock()
            {
                Text = x.dat.Text,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            BrushConverter bc = new BrushConverter();
            string c = x.income ? "#0fd628" : "#ff3445";
            Grid make = new Grid
            {
                Background = (Brush)bc.ConvertFrom(c),
            };
            make.Children.Add(val);
            make.Children.Add(cat);
            make.Children.Add(dat);
            return make;
        }
        #endregion
        #region Simulation
        void SimulateBalance(SQLiteConnection sqLiteConn, string dbb, string dbt)
        {
            //take latest known balance and date to simulate period between then and now
            SQLiteCommand comm = new SQLiteCommand("SELECT * FROM " + dbb + "ORDER BY DATETIME([DATE]) DESC LIMIT 1", sqLiteConn);
            comm.ExecuteNonQuery();
            SQLiteDataReader read = comm.ExecuteReader();
            read.Read();
            string LastDate = ((string)read["DATE"]);
            double v = (double)read["BALANCE"];
            DateTime AddRowToBalance = Convert.ToDateTime(LastDate.Remove(10, 9));
            DateTime NND = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); //NewestNewDate
            TimeSpan y = NND.Subtract(AddRowToBalance);
            int k = (int)y.TotalDays;
            while (k > 0) {
                AddRowToBalance = AddRowToBalance.AddDays(1);
                if (k != 1) {
                    comm = new SQLiteCommand("INSERT INTO " + dbb + " (BALANCE, DATE) VALUES('" + v + "', '" + AddRowToBalance.ToString("yyyy-MM-dd HH:mm:ss") + "')", sqLiteConn);
                    comm.ExecuteNonQuery();
                }
                else {
                    comm = new SQLiteCommand("INSERT INTO " + dbb + " (BALANCE, DATE) VALUES('" + v + "', '" + NND.ToString("yyyy-MM-dd HH:mm:ss") + "')", sqLiteConn);
                    comm.ExecuteNonQuery();
                }
                k--;
            }
            //find and simulate last month's periodic transactions
            List<string> codes = new List<string>();
            comm = new SQLiteCommand("SELECT LSD FROM USERINFO WHERE MAIL = '" + UserMail + "'", sqLiteConn);
            string LatestSimulationDate = (string)comm.ExecuteScalar();
            DateTime LSD = Convert.ToDateTime(LatestSimulationDate);
            comm = new SQLiteCommand("SELECT * FROM " + dbt + " WHERE [REPEATABILITY] IS NOT NULL ORDER BY DATETIME([DATE]) DESC", sqLiteConn); //make better query in future & USE DATA ADAPTER
            read = comm.ExecuteReader();
            double val = 0;
            string LaterHour;
            while (read.Read())
            {
                string s = (string)read["ID"];
                if (!codes.Contains(s))
                {
                    string[] p = new string[6];
                    p[0] = (string)read["REPEATABILITY"];
                    p[1] = (string)read["CATEGORY"];
                    p[2] = (string)read["TYPE"];
                    p[3] = ((double)read["VALUE"]).ToString();
                    p[4] = DBNull.Value.Equals(read["MAXVALUE"]) ? null : ((double)read["MAXVALUE"]).ToString();
                    p[5] = (string)read["ID"];
                    DateTime TransactionDate = Convert.ToDateTime((string)read["DATE"]);
                    codes.Add(s);

                    if (TransactionDate.ToString("yyyy-MM-dd") == NND.ToString("yyyy-MM-dd"))
                        break;
                    if (TransactionDate.ToString("yyyy-MM-dd") == LSD.ToString("yyyy-MM-dd"))
                        LSD = LSD.AddDays(1);
                    while (TransactionDate <= LSD) { //move transaction date to period we're updating
                        if (p[0].Equals("Monthly"))
                            TransactionDate = TransactionDate.AddMonths(1);
                        if (p[0].Equals("Weekly"))
                            TransactionDate = TransactionDate.AddDays(7);
                        if (p[0].Equals("Daily"))
                            TransactionDate = TransactionDate.AddDays(1);
                    }
                    
                    if (TransactionDate > NND)  //if transactiondate is greater than NND, then return
                        break;
                    DateTime Head = TransactionDate;
                    do {    //if it's within this period, update table
                        if(Head == TransactionDate && p[0].Equals("Weekly")) {
                            val += Convert.ToDouble(p[3]);
                            Head = Head.AddDays(7);
                            comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-transactions] (ID, DATE, REPEATABILITY, CATEGORY, TYPE, VALUE, MAXVALUE) " +
                                                      "values('" + p[5] + "', '" + TransactionDate.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + p[0] + "', '" + p[1] +
                                                      "', '" + p[2] + "', '" + p[3] + "', " + NullReturner(p[4]) + ")", sqLiteConn);
                            comm.ExecuteNonQuery();
                        }
                        if (Head == TransactionDate && p[0].Equals("Monthly")) {
                            val += Convert.ToDouble(p[3]);
                            Head = Head.AddMonths(1);
                            comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-transactions] (ID, DATE, REPEATABILITY, CATEGORY, TYPE, VALUE, MAXVALUE) " +
                                    "values('" + p[5] + "', '" + TransactionDate.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + p[0] + "', '" + p[1] +
                                     "', '" + p[2] + "', '" + p[3] + "', " + NullReturner(p[4]) + ")", sqLiteConn);
                            comm.ExecuteNonQuery();
                        }
                        if (Head == TransactionDate && p[0].Equals("Daily")) {
                            val += Convert.ToDouble(p[3]);
                            Head = Head.AddDays(1);
                            comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-transactions] (ID, DATE, REPEATABILITY, CATEGORY, TYPE, VALUE, MAXVALUE) " +
                                    "values('" + p[5] + "', '" + TransactionDate.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + p[0] + "', '" + p[1] +
                                    "', '" + p[2] + "', '" + p[3] + "', " + NullReturner(p[4]) + ")", sqLiteConn);
                            comm.ExecuteNonQuery();
                        }
                        if (Head > NND) {
                            comm = new SQLiteCommand("SELECT * FROM [" + UserMail + "-balance] WHERE DATE LIKE '"
                                + TransactionDate.ToString("yyyy-MM-dd") + "%'", sqLiteConn);
                            SQLiteDataReader read2 = comm.ExecuteReader();
                            read2.Read();
                            LaterHour = string.Compare(TransactionDate.ToString("yyyy-MM-dd HH:mm:ss"), ((string)read2["Date"])) == 1 ?
                                TransactionDate.ToString("yyyy-MM-dd HH:mm:ss") : ((string)read2["Date"]);
                            comm = new SQLiteCommand("UPDATE [" + UserMail + "-balance] SET BALANCE = BALANCE " + SymbolReturner(p[1]) + " '"
                                            + val.ToString() + "', DATE = '" + LaterHour +
                                            "' WHERE DATE([DATE]) LIKE '" + TransactionDate.ToString("yyyy-MM-dd") + "%'", sqLiteConn);
                            comm.ExecuteNonQuery();
                            break;
                        }
                        comm = new SQLiteCommand("UPDATE [" + UserMail + "-balance] SET BALANCE = BALANCE " + SymbolReturner(p[1]) + " '"
                                            + val.ToString() + "', DATE = '" + TransactionDate.ToString("yyyy-MM-dd HH:mm:ss") +
                                            "' WHERE DATE([DATE]) LIKE '" + TransactionDate.ToString("yyyy-MM-dd") + "%'", sqLiteConn);
                        comm.ExecuteNonQuery();
                        TransactionDate.AddDays(1);
                    }
                    while (true);
                }
            }
            comm = new SQLiteCommand("UPDATE [USERINFO] SET LSD = '" + NND.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE MAIL = '" + UserMail + "'", sqLiteConn);
            comm.ExecuteNonQuery();
        }

        string SymbolReturner(string category) {
            if (category == "Income")
                return "+";
            return "-";
        }

        string NullReturner(string s) {
            if (s == null)
                return "NULL";
            return ("'" + s + "'");
        }
        #endregion
        #region ResizeControls
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            LatestTransactions.Width = TransactionGrid.ActualWidth;
            foreach (Grid child in LatestTransactions.Children)
            {
                child.Height = TransactionGrid.ActualHeight / 5;
            }
        }

        private void OnSizeChanged(object sender, RoutedEventArgs e)
        {
            LatestTransactions.Width = TransactionGrid.ActualWidth;
            foreach (Grid child in LatestTransactions.Children)
            {
                child.Height = TransactionGrid.ActualHeight / 5;
            }
        }
        #endregion
    }
}
