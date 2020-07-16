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
using LiveCharts.Wpf.Charts.Base;

namespace Clerk
{
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
            Main.Navigate(new AddPage(UserMail));
        }

        void RemoveButton_Click(object sender, EventArgs e)
        {
            Main.Navigate(new RemovePage());
        }

        private void Main_Navigated(object sender, EventArgs e)
        {

        }
    }
}
