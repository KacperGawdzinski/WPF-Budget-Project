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

namespace Clerk
{
    public partial class RegisterPage : Page
    {
        string dbConnectionString = @"Data Source=database.db;Version=3;";
        public RegisterPage()
        {
            InitializeComponent();
            SQLiteConnection sqLiteConn = new SQLiteConnection(@"Data Source=database.db;Version=3;");
            sqLiteConn.Open();
            SQLiteCommand comm = new SQLiteCommand("CREATE TABLE IF NOT EXISTS USERNAME (MAIL TEXT, PASSWORD TEXT, USERNAME TEXT, IMAGE TEXT, CURRENCY TEXT, LATEST SIMULATION DATE TEXT)", sqLiteConn);
            comm.ExecuteNonQuery();
        }

        void Register_Click(object sender, EventArgs e)
        {
            if (Mail.Text == "" || Password.Password == "")
            {
                Window OK = new Notification("Every field must be filled!");
                OK.Show();
                return;
            }
            if (!(Mail.Text.Contains("@")))
            {
                Window OK = new Notification("Incorrect user e-mail");
                OK.Show();
                return;
            }

            SQLiteConnection sqLiteConn = new SQLiteConnection(dbConnectionString);
            sqLiteConn.Open();
            string command = "select * from userinfo where mail='" + Mail.Text + "'";
            SQLiteCommand comm = new SQLiteCommand(command, sqLiteConn);
            comm.ExecuteNonQuery();
            SQLiteDataReader read = comm.ExecuteReader();
            if (read.Read())
            {
                Window OK = new Notification("User e-mail is already taken. Type the different user mail and password, and try again");
                OK.Show();
            }
            else
            {
                command = "insert into userinfo (mail,password) values('" + Mail.Text + "','" + Password.Password + "')";
                comm = new SQLiteCommand(command, sqLiteConn);
                comm.ExecuteNonQuery();
                read = comm.ExecuteReader();
                NavigationService.Navigate(new LoginPage());
                Window OK = new Notification("Registration completed");
                OK.Show();
            }
        }

        void Return_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
