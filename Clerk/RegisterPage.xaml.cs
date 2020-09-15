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
            if (Mail.Text == "" || Password.Password == "" || Username.Text == "" || Currency.SelectedItem == null)
            {
                Window OK = new Notification("Every field must be filled!");
                OK.Show();
                return;
            }
            if (!(Mail.Text.Contains("@")))
            {
                Window OK = new Notification("Incorrect user e-mail!");
                OK.Show();
                return;
            }
            if(Username.Text.Contains("@"))
            {
                Window OK = new Notification("Username can't contain '@'!");
                OK.Show();
                return;
            }

            SQLiteConnection sqLiteConn = new SQLiteConnection(dbConnectionString);
            sqLiteConn.Open();
            SQLiteCommand comm = new SQLiteCommand("SELECT * FROM USERINFO WHERE MAIL ='" + Mail.Text + "'", sqLiteConn);
            comm.ExecuteNonQuery();
            SQLiteDataReader read = comm.ExecuteReader();
            if (read.Read())
            {
                Window OK = new Notification("User e-mail is already taken. Type different one and try again");
                OK.Show();
            }
            else
            {
                comm = new SQLiteCommand("SELECT * FROM USERINFO WHERE USERNAME ='" + Username.Text + "'", sqLiteConn);
                comm.ExecuteNonQuery();
                read = comm.ExecuteReader();
                if (read.Read())
                {
                    Window OK = new Notification("Username is already taken. Type different user mail and password, and try again");
                    OK.Show();
                }
                else
                {
                    comm = new SQLiteCommand("INSERT INTO USERINFO ([MAIL], [PASSWORD], [USERNAME], [CURRENCY], [LATEST SIMULATION DATE]) VALUES('" + Mail.Text + "', '" + 
                        Password.Password + "', '" + Username.Text + "', '" + Currency.Text + "', '" + 
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')", sqLiteConn);
                    comm.ExecuteNonQuery();
                    NavigationService.Navigate(new LoginPage());
                    Window OK = new Notification("Registration completed");
                    OK.Show();
                }
            }
        }

        void Return_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
