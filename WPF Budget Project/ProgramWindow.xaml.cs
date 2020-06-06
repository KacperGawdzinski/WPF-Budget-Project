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
    /*public partial class Category
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
*/

    public partial class ProgramWindow : Window
    {
        string UserMail;
        public ProgramWindow(string x)
        {
            InitializeComponent();
            UserMail = x;
            Main.Navigate(new Home(UserMail));
        }

        void Home_Button_Click(object sender, RoutedEventArgs e)
        {
            Main.Navigate(new Home(UserMail));
        }

        void AddButton_Click(object sender, EventArgs e)
        {
            Main.Navigate(new AddPage());
        }

        private void Main_Navigated(object sender, EventArgs e)
        {

        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
          // // OpenButton.Visibility = Visibility.Visible;
          //  CloseButton.Visibility = Visibility.Collapsed;
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
           // OpenButton.Visibility = Visibility.Collapsed;
           // CloseButton.Visibility = Visibility.Visible;
            SidePanel.Focus();
            SidePanel.MouseLeave += CloseButton_Click;
        }
    }
}
