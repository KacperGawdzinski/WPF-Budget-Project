﻿using System;
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
    public partial class LoginPage : Page
    {
        string dbConnectionString = @"Data Source=database.db;Version=3;";
        public LoginPage()
        {
            InitializeComponent();
            SQLiteConnection sqLiteConn = new SQLiteConnection(@"Data Source=database.db;Version=3;");
            sqLiteConn.Open();
            SQLiteCommand comm = new SQLiteCommand("CREATE TABLE IF NOT EXISTS USERINFO (MAIL TEXT, PASSWORD TEXT, USERNAME TEXT, IMAGE TEXT, CURRENCY TEXT, LSD TEXT)", sqLiteConn); //LSD - latest simulation date
            comm.ExecuteNonQuery();
        }

        void Register_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }

        void Login_Click(object sender, EventArgs e)
        {
            if(Mail.Text == "" || Password.Password=="")
            {
                Window OK = new Notification("Every field must be filled!");
                OK.Show();
                return;
            }
            SQLiteConnection sqLiteConn = new SQLiteConnection(dbConnectionString);
            sqLiteConn.Open();
            string command = "SELECT * FROM USERINFO WHERE MAIL ='" + Mail.Text + "' AND PASSWORD ='" + Password.Password + "'";
            SQLiteCommand comm = new SQLiteCommand(command, sqLiteConn);
            comm.ExecuteNonQuery();
            SQLiteDataReader read = comm.ExecuteReader();
            if (read.Read())
            {
                read.Close();
                sqLiteConn.Close();
                Window Program = new ProgramWindow(Mail.Text);
                Program.Show();
                App.Current.MainWindow.Close();
            }
            else
            {
                Window OK = new Notification("Incorrect user e-mail or password. Type the correct user mail and password, and try again");
                OK.Show();
            }
        }
    }
}
