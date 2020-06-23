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
using System.Security.RightsManagement;
using System.Data.SqlTypes;
using System.Collections.Specialized;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Core;
using System.Net.Http.Headers;

//TODO : ADD VALUE INPUT WITH DOTS EX 59.99
//TODO : MAKE UNIVERSAL FUNCTION FOR SQLITECOMMAND

namespace WPF_Budget_Project
{
    public partial class AddPage : Page
    {
        #region Contructor & Variables
        string UserMail;
        bool ExpendUsed = false;
        bool IncomeUsed = true;
        bool TypeInsertBuilt = false;
        bool SaveInsideGrid = false;
        public AddPage(string x)
        {
            InitializeComponent();
            UserMail = x;
        }
        #endregion
        #region GUIAdapt
        void IncomeChecked(object sender, EventArgs e)
        {
            RemoveOldCategories(true);                   //cleaning main combo box
            ComboBoxItem x;
            string[] temp = ReadTypes(true);           //all columns from table
            for (int i=0;i<temp.Length;i++)
            {
                x = new ComboBoxItem();
                x.Content = temp[i];
                TypeCombo.Items.Add(x);
            }
            x = new ComboBoxItem();
            x.Content = "New type...";
            TypeCombo.Items.Add(x);
            RebuildAfterExpend();                       //clean maxvalue button
        }

        void ExpendChecked(object sender, EventArgs e)
        {
            RemoveOldCategories(false); 
            ComboBoxItem x;
            string[] temp = ReadTypes(false);
            for (int i = 0; i < temp.Length; i++)
            {
                x = new ComboBoxItem();
                x.Content = temp[i];
                TypeCombo.Items.Add(x);
            }
            x = new ComboBoxItem();
            x.Content = "New type...";
            TypeCombo.Items.Add(x);
            RebuildAfterIncome();   
        }

        void RebuildAfterExpend()
        {
            if (ExpendUsed)
            {
                ExpendUsed = false;
                Stack.Children.RemoveAt(3);
                TextBlock Val = new TextBlock();
                TextBox InsVal = new TextBox();
                Val.Text = "Insert Value";
                Val.Margin = new Thickness(0, 20, 0, 0);
                Val.HorizontalAlignment = HorizontalAlignment.Center;
                InsVal.HorizontalAlignment = HorizontalAlignment.Center;
                InsVal.MinWidth = 200;
                Stack.Children.Insert(3, Val);
                Stack.Children.Insert(4, InsVal);
            }
        }

        void RebuildAfterIncome()
        {
            if (IncomeUsed)
            {
                IncomeUsed = false;
                Stack.Children.RemoveAt(4);
                Stack.Children.RemoveAt(3);
                TextBlock Value = new TextBlock();
                TextBox InValue = new TextBox();
                TextBox InMaxValue = new TextBox();
                TextBlock MaxValue = new TextBlock();
                Grid Insert = new Grid();
                ColumnDefinition st = new ColumnDefinition();
                ColumnDefinition nd = new ColumnDefinition();
                st.Width = new GridLength(1, GridUnitType.Star);
                nd.Width = new GridLength(1, GridUnitType.Star);
                Insert.ColumnDefinitions.Add(st);
                Insert.ColumnDefinitions.Add(nd);
                Value.Text = "Insert value";
                Value.Margin = new Thickness(130, 20, 0, 0);
                MaxValue.Margin = new Thickness(20, 20, 0, 0);
                InValue.Margin = new Thickness(60, 50, 0, 0);
                InMaxValue.Margin = new Thickness(30, 50, 0, 0);
                InMaxValue.HorizontalAlignment = HorizontalAlignment.Left;
                InValue.MaxWidth = 150;
                InMaxValue.MinWidth = 150;
                MaxValue.Text = "Insert max monthly value";
                Grid.SetColumn(Value, 0);
                Grid.SetColumn(InValue, 0);
                Grid.SetColumn(InMaxValue, 1);
                Grid.SetColumn(MaxValue, 1);
                Insert.Children.Add(Value);
                Insert.Children.Add(MaxValue);
                Insert.Children.Add(InValue);
                Insert.Children.Add(InMaxValue);
                Stack.Children.Insert(3, Insert);
            }
        }

        string[] ReadTypes(bool income)
        {
            SQLiteConnection sqLiteConn = new SQLiteConnection(@"Data Source=database.db;Version=3;");
            sqLiteConn.Open();
            List<string> Types = new List<string>();
            SQLiteCommand comm;
            if(income)
                comm = new SQLiteCommand("SELECT DISTINCT TYPE FROM [" + UserMail + "-transactions] WHERE [CATEGORY]='Income'", sqLiteConn);
            else
                comm = new SQLiteCommand("SELECT DISTINCT TYPE FROM [" + UserMail + "-transactions] WHERE [CATEGORY]='Expend'", sqLiteConn);
            SQLiteDataReader read = comm.ExecuteReader();
            while (read.Read())
                Types.Add((string)read["Type"]);
            sqLiteConn.Close();
            return Types.ToArray();
        }

        void RemoveOldCategories(bool income)
        {
            if (income)
                IncomeUsed = true;
            else
                ExpendUsed = true;
            if (ExpendCheck.IsChecked == true && income)
                ExpendCheck.IsChecked = false;
            if (IncomeCheck.IsChecked == true && !income)
                IncomeCheck.IsChecked = false;
            if (TypeInsertBuilt)
            {
                Stack.Children.RemoveAt(4);
                Stack.Children.RemoveAt(3);
                TypeInsertBuilt = false;
            }
            TypeCombo.Items.Clear();
        }

        void TypeComboChanged(object sender, EventArgs e)
        {
            if (TypeCombo.Text == "New type..." && TypeInsertBuilt == false)
            {
                TypeInsertBuilt = true;
                TextBox TypeInsert = new TextBox();
                TextBlock TypeInsertText = new TextBlock();
                TypeInsertText.Width = 200;
                TypeInsertText.Text = "Insert new type";
                TypeInsertText.Margin = new Thickness(80, 20, 0, 0);
                TypeInsertText.HorizontalAlignment = HorizontalAlignment.Center;
                TypeInsert.MinWidth = 200;
                TypeInsert.HorizontalAlignment = HorizontalAlignment.Center;
                Stack.Children.Insert(3, TypeInsertText);
                Stack.Children.Insert(4, TypeInsert);
                if(PeriodicCheck.IsChecked == true && SaveInsideGrid == false)
                {
                    Stack.Children.RemoveAt(Stack.Children.Count - 1);
                    PeriodicGrid.Children.Add(MakeSaveButton(250, 100, 0, 0));
                    SaveInsideGrid = true;
                }
            }
            else
            {
                if (TypeInsertBuilt == true && TypeCombo.Text != "New type...")
                {
                    Stack.Children.RemoveAt(4);
                    Stack.Children.RemoveAt(3);
                    TypeInsertBuilt = false;
                }
            }
        }

        void PeriodicChecked(object sender, EventArgs e)
        {
            TextBlock PeriodicText = new TextBlock();
            PeriodicText.HorizontalAlignment = HorizontalAlignment.Right;
            PeriodicText.Text = "Repeatability";
            PeriodicText.Margin = new Thickness(0, 100, 130, 0);
            ComboBox PeriodicBox = new ComboBox();
            PeriodicBox.HorizontalAlignment = HorizontalAlignment.Right;
            PeriodicBox.VerticalAlignment = VerticalAlignment.Top;
            PeriodicBox.Margin = new Thickness(0, 130, 100, 0);
            PeriodicBox.Width = 150;
            ComboBoxItem Day = new ComboBoxItem();
            ComboBoxItem Week = new ComboBoxItem();
            ComboBoxItem Month = new ComboBoxItem();
            Day.Content = "Daily";
            Week.Content = "Weekly";
            Month.Content = "Monthly";
            PeriodicBox.Items.Add(Day);
            PeriodicBox.Items.Add(Week);
            PeriodicBox.Items.Add(Month);
            TextBlock StartDate = new TextBlock();
            StartDate.Text = "Start Date";
            StartDate.HorizontalAlignment = HorizontalAlignment.Left;
            StartDate.Width = 200;
            StartDate.Margin = new Thickness(155, 10, 0, 0);
            Calendar Date = new Calendar();
            Viewbox box = new Viewbox();
            box.HorizontalAlignment = HorizontalAlignment.Left;
            box.MaxWidth = 250;
            box.MaxHeight = 300;
            box.Margin = new Thickness(80, 35, 0, 0);
            string d = DateTime.Today.ToString("dd.MM.yyyy");
            string[] t = d.Split('.');
            Date.DisplayDate = new DateTime(Convert.ToInt32(t[2]), Convert.ToInt32(t[1]), Convert.ToInt32(t[0]));
            box.Child = Date;
            PeriodicGrid.Children.Add(PeriodicText);
            PeriodicGrid.Children.Add(PeriodicBox);
            PeriodicGrid.Children.Add(StartDate);
            PeriodicGrid.Children.Add(box);
            if(ExpendCheck.IsEnabled && TypeInsertBuilt)
            {
                Stack.Children.RemoveAt(Stack.Children.Count - 1);
                PeriodicGrid.Children.Add(MakeSaveButton(250,100,0,0));
                SaveInsideGrid = true;
            }
        }
        void PeriodicUnchecked(object sender, EventArgs e)
        {
            for(int i=0;i<4;i++)
                PeriodicGrid.Children.RemoveAt(PeriodicGrid.Children.Count - 1);

            if(SaveInsideGrid)
            {
                SaveInsideGrid = false;
                PeriodicGrid.Children.RemoveAt(PeriodicGrid.Children.Count - 1);
                Stack.Children.Add(MakeSaveButton(0, 20, 0, 0));
            }
        }

        Button MakeSaveButton(int a, int b, int c, int d)
        {
            Button Save = new Button();
            TextBlock Txt = new TextBlock();
            StackPanel temp = new StackPanel();
            Txt.Text = "Save";
            Txt.Margin = new Thickness(10, 0, 0, 0);
            temp.Orientation = Orientation.Horizontal;
            temp.Children.Add(new MaterialDesignThemes.Wpf.PackIcon
            { Kind = MaterialDesignThemes.Wpf.PackIconKind.ContentSave });
            temp.Children.Add(Txt);
            Save.Content = temp;
            Save.Background = null;
            Save.BorderBrush = null;
            Save.Width = 100;
            Save.Height = 40;
            BrushConverter bc = new BrushConverter();
            Save.Background = (Brush)bc.ConvertFrom("#2e7d32");
            Save.BorderBrush = (Brush)bc.ConvertFrom("#2e7d32");
            Save.Margin = new Thickness(a, b, c, d);
            Save.Click += SaveClick;
            Save.Name = "SaveButton";
            return Save;
        }

        #endregion
        #region UtilityFunctions
        public IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
        #endregion
        #region InputCheck
        void ShowError(string warning)
        {
            Window OK = new Notification(warning);
            OK.Show();
            return;
        }

        string[] CheckInputData()
        {
            int k = 0;
            string[] l = new string[6];
            if (IncomeCheck.IsChecked == true)
                l[2] = "Income";
            else if (ExpendCheck.IsChecked == true)
                l[2] = "Expend";
            else
            {
                ShowError("Choose Category!");
                return null;
            }
                
            if (TypeCombo.Text == "New type...")
            {
                foreach (var val in FindVisualChildren<TextBox>(MainGrid))
                {
                    if (k == 1)
                    {
                        if (val.Text.Length == 0)
                        {
                            ShowError("Insert new type!");
                            return null;
                        }
                        string[] temp = ReadTypes(true);
                        for (int i = 0; i < temp.Length; i++)
                            if (temp[i] == val.Text)
                            {
                                ShowError("This column already exists!");
                                return null;
                            }
                        l[3]=val.Text;
                    }
                    if (k == 2 || (k == 3 && ExpendCheck.IsChecked == true))
                    {
                        if (val.Text.Length == 0)
                        {
                            ShowError("Insert value!");
                            return null;
                        }
                        for (int i = 0; i < val.Text.Length; i++)
                        {
                            if (Char.IsLetter(val.Text[i]))
                            {
                                ShowError("Value must be a number!");
                                return null;
                            }
                        }
                        if(k == 2)
                            l[4]=val.Text;
                        if(k == 3)
                            l[5]=val.Text;
                    }
                    k++;
                }
            }
            else if (TypeCombo.Text != "")
            {
                l[3] = TypeCombo.Text;
                foreach (var val in FindVisualChildren<TextBox>(this))
                {
                    if (k == 1 || (k == 2 && ExpendCheck.IsChecked == true))
                    {
                        if (val.Text.Length == 0)
                        {
                            ShowError("Insert value!");
                            return null;
                        }
                        for (int i = 0; i < val.Text.Length; i++)
                            if (Char.IsLetter(val.Text[i]))
                            {
                                ShowError("Value must be a number!");
                                return null;
                            }
                        if (k == 1)
                            l[4] = val.Text;
                        if (k == 2)
                            l[5] = val.Text;
                    }
                    k++;
                }
            }
            else
            {
                ShowError("Choose type!");
                return null;
            }

            if (PeriodicCheck.IsChecked == true)
            {
                foreach (var val in FindVisualChildren<Calendar>(this))
                {
                    try
                    {
                        l[0] = val.SelectedDate.Value.ToString("yyyyMMdd");
                    }
                    catch (InvalidOperationException)
                    {
                        ShowError("Choose date!");
                        return null;
                    }
                }
                k = 0;
                foreach (var val in FindVisualChildren<ComboBox>(this))
                {
                    if (k == 1)
                    {
                        if (val.Text == "")
                        {
                            ShowError("Choose interval!");
                            return null;
                        }
                        l[1] = val.Text;
                        k++;
                        continue;
                    }
                    k++;
                }
            }
            else
                l[0] = DateTime.Today.ToString("yyyyMMdd");
            return l;
        }
        #endregion
        #region SaveData
        void SaveClick(object sender, EventArgs e)
        {
            string[] InputData = CheckInputData();
            if (InputData != null)
            {
                var sqLiteConn = new SQLiteConnection(@"Data Source=database.db;Version=3;");
                sqLiteConn.Open();
                SQLiteCommand comm;
                Guid guid = Guid.NewGuid();
                if (!(InputData[1] == null))
                    Simulate(InputData, guid, sqLiteConn);
                else
                {
                    comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-transactions] (ID, DATE, REPEATABILITY, CATEGORY, TYPE, VALUE, MAXVALUE) " +
                           "values('" + guid.ToString() + "', '" + InputData[0] + "', " + NullReturner(InputData[1]) + ", '" + InputData[2] + "', '" + InputData[3] + "', '" +
                           InputData[4] + "', " + NullReturner(InputData[5]) + ")", sqLiteConn);
                    comm.ExecuteNonQuery();
                }
                Window OK = new Notification("Transaction added!");
                OK.Show();
            }
        }

        string NullReturner(string s)
        {
            if (s == null)
                return "NULL";
            return ("'" + s + "'");
        }
        #endregion
        #region Simulation
        void Simulate(string[] InputData, Guid guid, SQLiteConnection sqLiteConn)
        {
            SQLiteCommand comm;
            int t = string.Compare(InputData[0], DateTime.Today.ToString("yyyyMMdd"));  //if transaction date is smaller than today's date we need to simulate
            if (t == -1 || t == 0)
            {
                comm = new SQLiteCommand("SELECT * FROM [" + UserMail + "-balance] ORDER BY DATE LIMIT 1", sqLiteConn);  //check if we need to add new rows to balance history
                SQLiteDataReader read = comm.ExecuteReader();
                read.Read();
                LongToDateTime x = new LongToDateTime();
                DateTime t1 = x.ConvertToClass(Convert.ToInt64(InputData[0]));
                DateTime t2 = x.ConvertToClass((long)read["Date"]);
                TimeSpan y = t2.Subtract(t1);
                if (Convert.ToInt64(InputData[0]) < (long)read["Date"]) //check if we need to add new rows in the history
                {
                    double InsertBalance = (double)read["Balance"];
                    double k = y.TotalDays;
                    while(k > 0)
                    {
                        comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-balance] (BALANCE, DATE) VALUES('"+ InsertBalance + "', '" + t1.ToString("yyyyMMdd") + "')", sqLiteConn);
                        comm.ExecuteNonQuery();
                        t1 = t1.AddDays(1);
                        k--;
                    }
                }
                //now we're sure that balance days were added so we have to modify their values
                comm = new SQLiteCommand("SELECT* FROM [" + UserMail + "-balance] ORDER BY DATE", sqLiteConn);
                read = comm.ExecuteReader();
                double val = Convert.ToDouble(InputData[4]);
                int l, temp = 7;
                if (InputData[1].Equals("Monthly"))
                    l = 1;
                else if (InputData[1].Equals("Weekly"))
                    l = 7;
                else
                    l = 2;
                while (read.Read())
                {
                    if(l == 7 && temp == 7)
                    {
                        comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-transactions] (ID, DATE, REPEATABILITY, CATEGORY, TYPE, VALUE, MAXVALUE) " +
                               "values('" + guid.ToString() + "', '" + ((long)read["Date"]).ToString() + "', " + NullReturner(InputData[1]) + ", '" + InputData[2] + "', '" + InputData[3] + "', '" +
                               InputData[4] + "', " + NullReturner(InputData[5]) + ")", sqLiteConn);
                        comm.ExecuteNonQuery();
                        temp = 0;
                        val = val + Convert.ToDouble(InputData[4]);
                    }
                    if(l == 1)
                    {
                        long d = ((long)read["Date"])%100;
                        if (d == Convert.ToInt64(InputData[0].Remove(0,6)))
                        {
                            comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-transactions] (ID, DATE, REPEATABILITY, CATEGORY, TYPE, VALUE, MAXVALUE) " +
                                   "values('" + guid.ToString() + "', '" + ((long)read["Date"]).ToString() + "', " + NullReturner(InputData[1]) + ", '" + InputData[2] + "', '" + InputData[3] + "', '" +
                                   InputData[4] + "', " + NullReturner(InputData[5]) + ")", sqLiteConn);
                            comm.ExecuteNonQuery();
                            val = val + Convert.ToDouble(InputData[4]);
                        }
                    }
                    if(l == 2)
                    {
                        comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-transactions] (ID, DATE, REPEATABILITY, CATEGORY, TYPE, VALUE, MAXVALUE) " +
                                   "values('" + guid.ToString() + "', '" + ((long)read["Date"]).ToString() + "', " + NullReturner(InputData[1]) + ", '" + InputData[2] + "', '" + InputData[3] + "', '" +
                                   InputData[4] + "', " + NullReturner(InputData[5]) + ")", sqLiteConn);
                        comm.ExecuteNonQuery();
                        val = val + Convert.ToDouble(InputData[4]);
                    }

                    if (InputData[3].Equals("Income"))
                        comm = new SQLiteCommand("UPDATE [" + UserMail + "-balance] SET BALANCE='"
                        + ((double)read["Balance"] + val).ToString() + "' WHERE DATE='" + ((long)read["Date"]).ToString() + "'", sqLiteConn);
                    else
                        comm = new SQLiteCommand("UPDATE [" + UserMail + "-balance] SET BALANCE='"
                        + ((double)read["Balance"] - val).ToString() + "' WHERE DATE='" + ((long)read["Date"]).ToString() + "'", sqLiteConn);
                    comm.ExecuteNonQuery();
                    temp++;
                }
            }
        }
        #endregion
    }
}
