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
//TODO : TRANSACTIONS FROM THE SAME DAY ARE NOT SORTED PROPERLY EVERY TIME
//TEST PERIODIC TRANSACTIONS IN FUTURE

namespace Clerk
{
    public partial class AddPage : Page
    {
        #region Contructor
        string UserMail;
        public AddPage(string x)
        {
            InitializeComponent();
            UserMail = x;
        }
        #endregion
        #region GUIAdapt
        bool IncomeChecked;
        bool MaxValueBuilt = false;
        bool TypeInsertBuilt = false;
        bool SaveInsideGrid = false;
        void CategoryChecked(object sender, EventArgs e)
        {
            IncomeChecked = ((CheckBox)sender).Name.Equals("IncomeCheck") ? true : false;
            RemoveOldCategories(IncomeChecked);
            ComboBoxItem x;
            string[] temp = ReadTypes(IncomeChecked);
            for (int i = 0; i < temp.Length; i++)
            {
                x = new ComboBoxItem(){
                    Content = temp[i]
                };
                TypeCombo.Items.Add(x);
            }
            x = new ComboBoxItem(){
                Content = "New type..."
            };
            TypeCombo.Items.Add(x);
        }

        string[] ReadTypes(bool income)
        {
            SQLiteConnection sqLiteConn = new SQLiteConnection(@"Data Source=database.db;Version=3;");
            sqLiteConn.Open();
            List<string> Types = new List<string>();
            SQLiteCommand comm = income ? new SQLiteCommand("SELECT DISTINCT TYPE FROM [" + UserMail + "-transactions] WHERE [CATEGORY]='Income'", sqLiteConn) :
                new SQLiteCommand("SELECT DISTINCT TYPE FROM [" + UserMail + "-transactions] WHERE [CATEGORY]='Expend'", sqLiteConn);
            SQLiteDataReader read = comm.ExecuteReader();
            while (read.Read())
                Types.Add((string)read["Type"]);
            sqLiteConn.Close();
            return Types.ToArray();
        }

        void RemoveOldCategories(bool income)
        {
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
            if (MaxValueBuilt)
                RemoveMaxValue();
        }

        void TypeComboChanged(object sender, EventArgs e)
        {
            if (TypeCombo.Text == "New type..." && TypeInsertBuilt == false)
            {
                TypeInsertBuilt = true;
                TextBox TypeInsert = new TextBox() {
                    MinWidth = 200,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                TextBlock TypeInsertText = new TextBlock(){
                    Width = 300,
                    Text = "Insert new type (Max 20 signs)",
                    Margin = new Thickness(80, 20, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Stack.Children.Insert(3, TypeInsertText);
                Stack.Children.Insert(4, TypeInsert); 

                if(PeriodicCheck.IsChecked == true && SaveInsideGrid == false)
                {
                    Stack.Children.RemoveAt(Stack.Children.Count - 1);
                    PeriodicGrid.Children.Add(MakeSaveButton(250, 100, 0, 0));
                    SaveInsideGrid = true;
                }
                if(!IncomeChecked)
                {
                    MaxValueBuilt = true;
                    Stack.Children.RemoveAt(6);
                    Stack.Children.RemoveAt(5);
                    TextBlock Value = new TextBlock(){
                        Text = "Insert value",
                        Margin = new Thickness(140, 20, 0, 0)
                    };
                    TextBlock MaxValue = new TextBlock(){
                        Margin = new Thickness(18, 20, 0, 0),
                        Text = "Insert max monthly value"
                    };
                    TextBox InMaxValue = new TextBox(){
                        Margin = new Thickness(30, 40, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        MinWidth = 150,
                        TextAlignment = TextAlignment.Center
                    };
                    TextBox InValue = new TextBox(){
                        Margin = new Thickness(60, 40, 0, 0),
                        MaxWidth = 150,
                        TextAlignment = TextAlignment.Center
                    };
                    Grid Insert = new Grid();
                    ColumnDefinition st = new ColumnDefinition();
                    ColumnDefinition nd = new ColumnDefinition();
                    st.Width = new GridLength(1, GridUnitType.Star);
                    nd.Width = new GridLength(1, GridUnitType.Star);
                    Insert.ColumnDefinitions.Add(st);
                    Insert.ColumnDefinitions.Add(nd);
                    Grid.SetColumn(MaxValue, 1);
                    Grid.SetColumn(InMaxValue, 1);
                    Grid.SetColumn(Value, 0);
                    Grid.SetColumn(InValue, 0);
                    Insert.Children.Add(MaxValue);
                    Insert.Children.Add(InMaxValue);
                    Insert.Children.Add(Value);
                    Insert.Children.Add(InValue);
                    Stack.Children.Insert(5, Insert);
                }
            }
            else
            {
                if (TypeInsertBuilt == true && TypeCombo.Text != "New type...")
                {
                    Stack.Children.RemoveAt(4);
                    Stack.Children.RemoveAt(3);
                    TypeInsertBuilt = false;
                    if(MaxValueBuilt)
                    {
                        MaxValueBuilt = false;
                        RemoveMaxValue();
                    }
                }
            }
        }

        void RemoveMaxValue()
        {
            MaxValueBuilt = false;
            Stack.Children.RemoveAt(3);
            TextBlock Val = new TextBlock(){
                Text = "Insert Value",
                Margin = new Thickness(0, 20, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            TextBox InsVal = new TextBox(){
                HorizontalAlignment = HorizontalAlignment.Center,
                MinWidth = 200,
                TextAlignment = TextAlignment.Center
            };
            Stack.Children.Insert(3, Val);
            Stack.Children.Insert(4, InsVal);
        }

        void PeriodicChecked(object sender, EventArgs e)
        {
            TextBlock PeriodicText = new TextBlock(){
                HorizontalAlignment = HorizontalAlignment.Right,
                Text = "Repeatability",
                Margin = new Thickness(0, 100, 130, 0)
            };
            ComboBox PeriodicBox = new ComboBox(){
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 130, 100, 0),
                Width = 150
            };
            ComboBoxItem Day = new ComboBoxItem(){
                Content = "Daily"
            };
            ComboBoxItem Week = new ComboBoxItem(){
                Content = "Weekly"
            };
            ComboBoxItem Month = new ComboBoxItem(){
                Content = "Monthly"
            };
            PeriodicBox.Items.Add(Day);
            PeriodicBox.Items.Add(Week);
            PeriodicBox.Items.Add(Month);
            TextBlock StartDateText = new TextBlock(){
                Text = "Start Date",
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 200,
                Margin = new Thickness(155, 10, 0, 0)
            };
            string d = DateTime.Today.ToString("yyyy.MM.dd");
            string[] t = d.Split('.');
            Calendar Date = new Calendar(){
                DisplayDate = new DateTime(Convert.ToInt32(t[0]), Convert.ToInt32(t[1]), Convert.ToInt32(t[2]))
            };
            Viewbox box = new Viewbox(){
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 250,
                MaxHeight = 300,
                Margin = new Thickness(80, 35, 0, 0),
                Child = Date
            };
            PeriodicGrid.Children.Add(PeriodicText);
            PeriodicGrid.Children.Add(PeriodicBox);
            PeriodicGrid.Children.Add(StartDateText);
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
            for(int i = 0; i < 4; i++)
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
            BrushConverter bc = new BrushConverter();
            TextBlock Txt = new TextBlock() {
                Text = "Save",
                Margin = new Thickness(10, 0, 0, 0)
            };
            StackPanel temp = new StackPanel(){
                Orientation = Orientation.Horizontal,
            };
            temp.Children.Add(new MaterialDesignThemes.Wpf.PackIcon{ Kind = MaterialDesignThemes.Wpf.PackIconKind.ContentSave});
            temp.Children.Add(Txt);
            Button Save = new Button(){
                Content = temp,
                Width = 100,
                Height = 40,
                Background = (Brush)bc.ConvertFrom("#2e7d32"),
                BorderBrush = (Brush)bc.ConvertFrom("#2e7d32"),
                Margin = new Thickness(a, b, c, d),
                Name = "SaveButton"
            };
            Save.Click += SaveClick;
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
                        if (val.Text.Length > 20)
                        {
                            ShowError("Type name is too long!");
                            return null;
                        }
                        string[] temp = l[2].Equals("Income") ? ReadTypes(true) : ReadTypes(false);
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
                    if (k == 1)
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
                        l[4] = val.Text;
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
                    comm = new SQLiteCommand("SELECT * FROM [" + UserMail + "-balance] ORDER BY DATE DESC LIMIT 1", sqLiteConn);
                    SQLiteDataReader read = comm.ExecuteReader();
                    read.Read();
                    if (InputData[2].Equals("Income"))
                        comm = new SQLiteCommand("UPDATE [" + UserMail + "-balance] SET BALANCE='"
                        + ((double)read["Balance"] + Convert.ToDouble(InputData[4])).ToString() + "' WHERE DATE = '" + ((long)read["Date"]).ToString() + "'", sqLiteConn);
                    else
                        comm = new SQLiteCommand("UPDATE [" + UserMail + "-balance] SET BALANCE='"
                        + ((double)read["Balance"] - Convert.ToDouble(InputData[4])).ToString() + "' WHERE DATE = '" + ((long)read["Date"]).ToString() + "'", sqLiteConn);
                    comm.ExecuteNonQuery();
                }
                Window OK = new Notification("Transaction added!");
                OK.Show();
            }
        }
        #endregion
        #region Simulation
        void Simulate(string[] InputData, Guid guid, SQLiteConnection sqLiteConn)
        {
            SQLiteCommand comm;
            int t = string.Compare(InputData[0], DateTime.Today.ToString("yyyyMMdd"));  //if transaction date is smaller or equal to today's date we need to simulate
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
                comm = new SQLiteCommand("SELECT* FROM [" + UserMail + "-balance] WHERE DATE >= '" + InputData[0] + "' ORDER BY DATE", sqLiteConn);
                read = comm.ExecuteReader();
                double val = 0;
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

                    if (InputData[2].Equals("Income"))
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

        string NullReturner(string s)
        {
            if (s == null)
                return "NULL";
            return ("'" + s + "'");
        }
        #endregion
    }
}
