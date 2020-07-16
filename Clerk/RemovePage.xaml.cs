using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

namespace Clerk
{
    public class MyItem
    {
        public string Type { get; set; }
        public double Value { get; set; }
        public string Date { get; set; }
    }

    public partial class RemovePage : Page
    {
        string Mail;
        public RemovePage(string mail)
        {
            Mail = mail;
            SQLiteConnection sqLiteConn = new SQLiteConnection(@"Data Source=database.db;Version=3;");
            sqLiteConn.Open();
            InitializeComponent();
            ReadTransactions(sqLiteConn);
        }
        #region ReadTransactions
        private void ReadTransactions(SQLiteConnection sqLiteConn)
        {
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM [" + Mail + "-transactions] ORDER BY DATE([DATE]) DESC",sqLiteConn);
            SQLiteDataReader read = command.ExecuteReader();
            while(read.Read())
            {
                string temp = (string)read["CATEGORY"];
                if(temp.Equals("Income"))
                    IncomeList.Items.Add(new MyItem { Type = (string)read["TYPE"], Value = (double)read["VALUE"], Date = (string)read["DATE"] });
                else
                    ExpendList.Items.Add(new MyItem { Type = (string)read["TYPE"], Value = (double)read["VALUE"], Date = (string)read["DATE"] });
            }
        }
        #endregion
    }
}
