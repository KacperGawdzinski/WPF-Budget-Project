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
using System.Windows.Shapes;

namespace WPF_Budget_Project
{
    public partial class Notification : Window
    {
        public Notification(string x)
        {
            InitializeComponent();
            Notification_Text.Text = x;
            if(x.Length <= 30)
            {
                Height = 130;
            }
        }

        void OK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
