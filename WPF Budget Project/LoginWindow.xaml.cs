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
using System.Data.SQLite;

namespace WPF_Budget_Project
{
    public partial class LoginWindow : Window
    {
        string dbConnectionString = @"Data Source=database.db;Version=3;";
        public LoginWindow()
        {
            InitializeComponent();
        }

        void baza(object sender, EventArgs e)
        {
           /* SQLiteConnection sqLiteConn = new SQLiteConnection(dbConnectionString);
            sqLiteConn.Open();
            //string command = "select * from userinfo where mail='" + Mail.Text + "' and password='" + Password.Password + "'";
            SQLiteCommand comm = new SQLiteCommand(command, sqLiteConn);
            comm.ExecuteNonQuery();
            SQLiteDataReader read = comm.ExecuteReader();
            if (read.Read())
            {

            }
            else
            {
               // test.Text = "KUTAS";
            }*/
        }
    }
}
