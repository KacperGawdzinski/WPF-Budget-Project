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
    public partial class Category
    {
        string name;
        double max;
        public double Max()
        {
            return max;
        }
        public void Max(double x)
        {
            max = x;
        }
        public string Name()
        {
            return name;
        }
        public void Name(string x)
        {
            name = x;
        }
        public Category()
        {
            name = "";
            max = 0;
        }
        public Category(double x, string s)
        {
            name = s;
            max = x;
        }
    }

    public partial class Spending
    {
        Category c = new Category();
    }

    public partial class Budget
    {
        public DateTime time = DateTime.Now;
    }

    public partial class ProgramWindow : Window
    {
        //string dbConnectionString = @"Data Source=database.db;Version=3;";
        public ProgramWindow()
        {
            //Content = new LoginPage();
             InitializeComponent();
            //Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            // Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220));
            // Main.Content = new LoginPage();
           // Window login = new LoginWindow();
           // Close();
           // login.Show();
        }

        void Home_Button_Click(object sender, RoutedEventArgs e)
        {
            //Main.Content = new Home();
        }

        void Add_Spending_Button_Click(object sender, EventArgs e)
        {
            Main.Content = new Add_Spending();
        }
    }
}
