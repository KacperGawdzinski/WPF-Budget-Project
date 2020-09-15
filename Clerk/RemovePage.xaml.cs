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
    //TODO: fix problem with adding removed transaction in simulation
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
        #region Read Transactions
        private void ReadTransactions(SQLiteConnection sqLiteConn)
        {
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM [" + Mail + "-transactions] ORDER BY DATETIME([DATE]) DESC",sqLiteConn);
            SQLiteDataReader read = command.ExecuteReader();
            while(read.Read())
            {
                string temp = (string)read["CATEGORY"];
                if (temp.Equals("Income"))
                    IncomeList.Items.Add(new MyItem { Type = (string)read["TYPE"], Value = ((double)read["VALUE"]).ToString() + "$",
                        Date = (string)read["DATE"], ID = (string)read["ID"], Period = DBNull.Value.Equals(read["REPEATABILITY"]) ? "None" : (string)read["REPEATABILITY"] });
                else
                    ExpendList.Items.Add(new MyItem { Type = (string)read["TYPE"], Value = ((double)read["VALUE"]).ToString() + "$",
                        Date = (string)read["DATE"], ID = (string)read["ID"], Period = DBNull.Value.Equals(read["REPEATABILITY"]) ? "None" : (string)read["REPEATABILITY"] });
            }
        }
        #endregion
        #region Selected Item
        private void SelectedItem(object sender, EventArgs e)
        {
            ListViewItem item = (ListViewItem)sender;
            ListView x = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            if (x.Name.Equals("IncomeList"))
                ExpendList.SelectedItem = null;
            else
                IncomeList.SelectedItem = null;
            if((((MyItem)((ListViewItem)sender).DataContext)).Period.Equals("None"))
            {
                RemoveSingleButton.IsEnabled = true;
                RemoveAllButton.IsEnabled = false;
            }
            else
            {
                RemoveAllButton.IsEnabled = true;
                RemoveSingleButton.IsEnabled = true;
            }
        }
        #endregion
        #region Remove Transaction
        private void RemoveButton_Click(object sender, EventArgs e)
        {
            SQLiteConnection sqLiteConn = new SQLiteConnection(@"Data Source=database.db;Version=3;");
            sqLiteConn.Open();
            MyItem source = IncomeList.SelectedItem == null ? ExpendList.SelectedItem as MyItem : IncomeList.SelectedItem as MyItem;
            SQLiteCommand command = ((Button)sender).Tag.Equals("Single") ? 
                new SQLiteCommand("DELETE FROM [" + Mail + "-transactions] WHERE [ID] = '" + source.ID.ToString() + "' AND [DATE] = '" + source.Date.ToString() + "'", sqLiteConn) :
                new SQLiteCommand("DELETE FROM [" + Mail + "-transactions] WHERE [ID] = '" + source.ID.ToString() + "'", sqLiteConn);
            command.ExecuteNonQuery();
            Window OK = new Notification("Transaction removed");
            OK.Show();
            IncomeList.Items.Clear();
            ExpendList.Items.Clear();
            RemoveAllButton.IsEnabled = false;
            RemoveSingleButton.IsEnabled = false;
            ReadTransactions(sqLiteConn);
        }
        #endregion 
        #region Resize ListView
        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /*ListView listView = sender as ListView;
            GridView gridView = listView.View as GridView;
            var width = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth;
            gridView.Columns[1].Width = width * 0.22;
            gridView.Columns[2].Width = width * 0.22;
            gridView.Columns[3].Width = width * 0.22;*/
        }
        #endregion
    }
}
