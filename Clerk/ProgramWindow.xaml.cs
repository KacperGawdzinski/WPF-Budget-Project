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

        private void Main_Navigated(object sender, EventArgs e)
        {

        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {/*
            myCanvas.Width = e.NewSize.Width;
            myCanvas.Height = e.NewSize.Height;

            double xChange = 1, yChange = 1;

            if (e.PreviousSize.Width != 0)
                xChange = (e.NewSize.Width / e.PreviousSize.Width);

            if (e.PreviousSize.Height != 0)
                yChange = (e.NewSize.Height / e.PreviousSize.Height);

            ScaleTransform scale = new ScaleTransform(myCanvas.LayoutTransform.Value.M11 * xChange, myCanvas.LayoutTransform.Value.M22 * yChange);
            myCanvas.LayoutTransform = scale;
            myCanvas.UpdateLayout();*/
        }
    }
}
