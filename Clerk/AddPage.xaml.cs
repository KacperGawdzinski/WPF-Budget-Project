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
using System.Globalization;

//TODO : MAKE UNIVERSAL FUNCTION FOR SQLITECOMMAND
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
        bool InsertTypeBuilt = false;
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
            if (InsertTypeBuilt)
            {
                Stack.Children.RemoveAt(4);
                Stack.Children.RemoveAt(3);
                InsertTypeBuilt = false;
            }
            TypeCombo.Items.Clear();
            if (MaxValueBuilt)
                RemoveMaxValue();
        }

        void TypeComboChanged(object sender, EventArgs e)
        {
            if (TypeCombo.Text == "New type..." && InsertTypeBuilt == false)
            {
                InsertTypeBuilt = true;
                TextBox TypeInsert = new TextBox() {
                    Name = "InsertedType",
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

                if(!IncomeChecked)
                {
                    MaxValueBuilt = true;
                    Stack.Children.RemoveAt(6);
                    Stack.Children.RemoveAt(5);
                    TextBlock Value = new TextBlock() {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Text = "Insert value",
                        Margin = new Thickness(50, 20, 0, 0)
                    };
                    TextBlock MaxValue = new TextBlock(){
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 20, 60, 0),
                        Text = "Insert max monthly value"
                    };
                    TextBox InMaxValue = new TextBox(){
                        Name = "InsertedMaxValue",
                        Margin = new Thickness(0, 40, 60, 0),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        MinWidth = 150,
                        TextAlignment = TextAlignment.Center
                    };
                    TextBox InValue = new TextBox(){
                        Name = "InsertedValue",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(50, 40, 0, 0),
                        MinWidth = 150,
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
                    Insert.Children.Add(Value);
                    Insert.Children.Add(InValue);
                    Insert.Children.Add(MaxValue);
                    Insert.Children.Add(InMaxValue);
                    Stack.Children.Insert(5, Insert);
                }
            }
            else
            {
                if (InsertTypeBuilt == true && TypeCombo.Text != "New type...")
                {
                    Stack.Children.RemoveAt(4);
                    Stack.Children.RemoveAt(3);
                    InsertTypeBuilt = false;
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
                Name = "InsertedValue",
                HorizontalAlignment = HorizontalAlignment.Center,
                MinWidth = 200,
                TextAlignment = TextAlignment.Center
            };
            Stack.Children.Insert(3, Val);
            Stack.Children.Insert(4, InsVal);
        }

        void PeriodicChecked(object sender, EventArgs e)
        {
            Grid PeriodicGrid = new Grid();
            ColumnDefinition c1 = new ColumnDefinition();
            c1.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition c2 = new ColumnDefinition();
            c2.Width = new GridLength(1, GridUnitType.Star);
            PeriodicGrid.ColumnDefinitions.Add(c1);
            PeriodicGrid.ColumnDefinitions.Add(c2);
            TextBlock PeriodicText = new TextBlock(){
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "Repeatability",
                Margin = new Thickness(0, 180, 40, 0)
            };
            TextBlock TimeText = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "Hour",
                Margin = new Thickness(0, 80, 40, 0)
            };
            ComboBox PeriodicBox = new ComboBox(){
                Name = "PeriodicBox",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 120, 30, 0),
                MinWidth = 150,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Bottom
            };
            ComboBox TimePicker = new ComboBox() {
                Name = "TimePicker",
                HorizontalContentAlignment = HorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 100, 30, 0),
                Width = 100,
                Height = 40,
                MaxDropDownHeight = 150
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
            DateTime x = new DateTime(1, 1, 1, 0, 0, 0);
            for (int i = 24; i > 0; i--) {
                TimePicker.Items.Add(new ComboBoxItem() { Content = x.ToString("HH:mm") });
                x = x.AddHours(1);
            }
            PeriodicBox.Items.Add(Day);
            PeriodicBox.Items.Add(Week);
            PeriodicBox.Items.Add(Month);
            TextBlock StartDateText = new TextBlock(){
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "Start Date",
                Margin = new Thickness(80, 10, 0, 0),
            };
            string d = DateTime.Today.ToString("yyyy.MM.dd");
            string[] t = d.Split('.');
            System.Windows.Controls.Calendar Date = new System.Windows.Controls.Calendar(){
                Name = "Calendar",
                DisplayDate = new DateTime(Convert.ToInt32(t[0]), Convert.ToInt32(t[1]), Convert.ToInt32(t[2]))
            };
            Viewbox box = new Viewbox(){
                HorizontalAlignment = HorizontalAlignment.Center,
                MaxWidth = 250,
                MaxHeight = 300,
                Margin = new Thickness(80, 35, 0, 0),
                Child = Date,
            };
            Grid.SetColumn(box, 0);
            Grid.SetColumn(StartDateText, 0);
            Grid.SetColumn(PeriodicText, 1);
            Grid.SetColumn(PeriodicBox, 1);
            Grid.SetColumn(TimePicker, 1);
            Grid.SetColumn(TimeText, 1);
            PeriodicGrid.Children.Add(StartDateText);
            PeriodicGrid.Children.Add(box);
            PeriodicGrid.Children.Add(PeriodicText);
            PeriodicGrid.Children.Add(PeriodicBox);
            PeriodicGrid.Children.Add(TimePicker);
            PeriodicGrid.Children.Add(TimeText);
            PeriodicGrid.Children.Add(MakeSaveButton(0, 300, 30, 0, 1));
            Stack.Children.RemoveAt(Stack.Children.Count - 1);
            Stack.Children.Add(PeriodicGrid);
        }
        void PeriodicUnchecked(object sender, EventArgs e)
        {
            Stack.Children.RemoveAt(Stack.Children.Count - 1);
            Stack.Children.Add(MakeSaveButton(0, 20, 0, 0, 2));
        }
        Button MakeSaveButton(int a, int b, int c, int d, int column)
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
            Button Save = new Button() {
                Content = temp,
                Width = 100,
                Height = 40,
                Background = (Brush)bc.ConvertFrom("#2e7d32"),
                BorderBrush = (Brush)bc.ConvertFrom("#2e7d32"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(a, b, c, d),
                Name = "SaveButton",
            };
            Save.Click += SaveClick;
            if(column != 0) {
                Grid.SetColumn(Save, 1);
                Grid.SetColumnSpan(Save, column);
            }
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
        void ShowError(string warning) {
            Window OK = new Notification(warning);
            OK.Show();
            return;
        }

        string[] CheckInputData() {
            string[] InputData = new string[6];
            if (IncomeCheck.IsChecked == true)
                InputData[2] = "Income";
            else if (ExpendCheck.IsChecked == true)
                InputData[2] = "Expend";
            else {
                ShowError("Choose Category!");
                return null;
            }

            if (TypeCombo.Text == "New type...") {
                InputData[3] = ((TextBox)LogicalTreeHelper.FindLogicalNode(MainGrid, "InsertedType")).Text;
                if (InputData[3].Length == 0) {
                    ShowError("Insert new type!");
                    return null;
                }
                if (InputData[3].Length > 20) {
                    ShowError("Insert shorter type!");
                    return null;
                }
                string[] temp = InputData[2] == "Income" ? ReadTypes(true) : ReadTypes(false);
                foreach (string x in temp)
                    if (x == InputData[3]) {
                        ShowError("Type already exists!");
                        return null;
                    }
            }
            else if (TypeCombo.Text != "") {
                InputData[3] = TypeCombo.Text;
            }
            else {
                ShowError("Choose type!");
                return null;
            }

            InputData[4] = ((TextBox)LogicalTreeHelper.FindLogicalNode(MainGrid, "InsertedValue")).Text;
            if (InputData[4].Length == 0) {
                ShowError("Insert value!");
                return null;
            }
            if (!double.TryParse(InputData[4], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out _)) { //discard the out parameter
                ShowError("Value must be a number!");
                return null;
            }

            if (ExpendCheck.IsChecked == true && InsertTypeBuilt == true) {
                InputData[5] = ((TextBox)LogicalTreeHelper.FindLogicalNode(MainGrid, "InsertedMaxValue")).Text;
                if (InputData[5].Length == 0) {
                    ShowError("Insert value!");
                    return null;
                }
                if (!double.TryParse(InputData[5], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out _)) {
                    ShowError("Value must be a number!");
                    return null;
                }
            }

            if (PeriodicCheck.IsChecked == true) {
                try {
                    InputData[0] = ((System.Windows.Controls.Calendar)LogicalTreeHelper.FindLogicalNode(MainGrid, "Calendar")).SelectedDate.Value.ToString("yyyy-MM-dd");
                }
                catch (InvalidOperationException) {
                    ShowError("Choose date!");
                    return null;
                }

                InputData[0] += " " + ((ComboBox)LogicalTreeHelper.FindLogicalNode(MainGrid, "TimePicker")).Text + ":00";
                InputData[1] = ((ComboBox)LogicalTreeHelper.FindLogicalNode(MainGrid, "PeriodicBox")).Text;
                if (InputData[1] == "") {
                    ShowError("Choose interval!");
                    return null;
                }
            }
            else
                InputData[0] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return InputData;
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
                           InputData[4].Replace(',','.') + "', " + NullReturner(InputData[5]) + ")", sqLiteConn);
                    comm.ExecuteNonQuery();
                    comm = new SQLiteCommand("SELECT * FROM [" + UserMail + "-balance] ORDER BY DATETIME([DATE]) DESC LIMIT 1", sqLiteConn);
                    SQLiteDataReader read = comm.ExecuteReader();
                    read.Read();
                    if (InputData[2].Equals("Income"))
                        comm = new SQLiteCommand("UPDATE [" + UserMail + "-balance] SET BALANCE = '"
                        + ((double)read["Balance"] + Convert.ToDouble(InputData[4].Replace('.', ','))).ToString().Replace(',', '.') + "', DATE = '" + 
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE DATETIME([DATE]) = '" + (string)read["Date"] + "'", sqLiteConn);
                    else
                        comm = new SQLiteCommand("UPDATE [" + UserMail + "-balance] SET BALANCE = '"
                        + ((double)read["Balance"] - Convert.ToDouble(InputData[4].Replace('.', ','))).ToString().Replace(',', '.') + "', DATE = '" +
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE DATETIME([DATE]) = '" + (string)read["Date"] + "'", sqLiteConn);
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
            int t = string.Compare(InputData[0].Remove(10,9), DateTime.Today.ToString("yyyy-MM-dd"));  //if transaction date is smaller or equal to today's date we need to simulate
            if (t == -1 || t == 0)
            {
                SQLiteCommand comm = new SQLiteCommand("SELECT * FROM [" + UserMail + "-balance] ORDER BY DATE([DATE]) LIMIT 1", sqLiteConn);   //CHECK IF IT'S NOT TOO SLOW
                SQLiteDataReader read = comm.ExecuteReader();
                read.Read();
                DateTime t1 = Convert.ToDateTime(InputData[0]);
                DateTime t2 = Convert.ToDateTime((string)read["Date"]);
                TimeSpan y = t2.Subtract(t1);
                int k = (int)y.TotalDays;
                double LastKnownBalanceValue = (double)read["Balance"];
                while(k > 0)
                {
                    comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-balance] (BALANCE, DATE) VALUES('" + LastKnownBalanceValue + "', '" + t1.ToString("yyyy-MM-dd HH:mm:ss") + "')", sqLiteConn);
                    comm.ExecuteNonQuery();
                    t1 = t1.AddDays(1);
                    k--;
                }
                comm = new SQLiteCommand("SELECT * FROM [" + UserMail + "-balance] ORDER BY DATETIME([DATE]) DESC LIMIT 1", sqLiteConn);
                read = comm.ExecuteReader();
                read.Read();
                comm = new SQLiteCommand("UPDATE [" + UserMail + "-balance] SET BALANCE = '" + ((double)read["BALANCE"]).ToString() + "', DATE = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                    "' WHERE DATE = '" + (string)read["DATE"] + "'", sqLiteConn);
                comm.ExecuteNonQuery();

                //now we're sure that balance days were added so we have to modify their values
                DateTime InputDate = Convert.ToDateTime(InputData[0].Remove(10,9));
                comm = new SQLiteCommand("SELECT * FROM [" + UserMail + "-balance] WHERE DATETIME([DATE]) > '" + InputDate.ToString("yyyy-MM-dd HH:mm:ss") + "' ORDER BY DATETIME([DATE])", sqLiteConn);
                read = comm.ExecuteReader();
                double val = 0;
                int l = 2, temp = 7;
                if (InputData[1] == "Monthly")
                    l = 1;
                if (InputData[1] == "Weekly")
                    l = 7;
                DateTime Head = Convert.ToDateTime(InputData[0]);
                while (read.Read())
                {
                    if (l == 7 && temp == 7)
                    {
                        val += Convert.ToDouble(InputData[4].Replace('.', ','));
                        comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-transactions] (ID, DATE, REPEATABILITY, CATEGORY, TYPE, VALUE, MAXVALUE) " +
                               "values('" + guid.ToString() + "', '" + Head.ToString("yyyy-MM-dd HH:mm:ss") + "', " + NullReturner(InputData[1]) + ", '" + InputData[2] + "', '" + InputData[3] + "', '" +
                               InputData[4].Replace(',', '.') + "', " + NullReturner(InputData[5]) + ")", sqLiteConn);
                        comm.ExecuteNonQuery();
                        temp = 0;
                    }
                    if(l == 1)
                    {
                        if (((string)read["Date"]).Remove(0, 8).Remove(2,9) == InputData[0].Remove(0, 8).Remove(2,9))   //may not be 100% accurate
                        {
                            val += Convert.ToDouble(InputData[4].Replace('.', ','));
                            comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-transactions] (ID, DATE, REPEATABILITY, CATEGORY, TYPE, VALUE, MAXVALUE) " +
                                   "values('" + guid.ToString() + "', '" + Head.ToString("yyyy-MM-dd HH:mm:ss") + "', " + NullReturner(InputData[1]) + ", '" + InputData[2] + "', '" + InputData[3] + "', '" +
                                   InputData[4].Replace(',', '.') + "', " + NullReturner(InputData[5]) + ")", sqLiteConn);
                            comm.ExecuteNonQuery();
                        }
                    }
                    if(l == 2)
                    {
                        val += Convert.ToDouble(InputData[4].Replace('.', ','));
                        comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-transactions] (ID, DATE, REPEATABILITY, CATEGORY, TYPE, VALUE, MAXVALUE) " +
                                   "values('" + guid.ToString() + "', '" + Head.ToString("yyyy-MM-dd HH:mm:ss") + "', " + NullReturner(InputData[1]) + ", '" + InputData[2] + "', '" + InputData[3] + "', '" +
                                   InputData[4].Replace(',', '.') + "', " + NullReturner(InputData[5]) + ")", sqLiteConn);
                        comm.ExecuteNonQuery();
                    }

                    if (InputData[2] == "Income")
                        comm = new SQLiteCommand("UPDATE [" + UserMail + "-balance] SET BALANCE = '"
                        + ((double)read["Balance"] + val).ToString().Replace(',', '.') + "' WHERE DATE = '" + (string)read["Date"] + "'", sqLiteConn);
                    else
                        comm = new SQLiteCommand("UPDATE [" + UserMail + "-balance] SET BALANCE = '"
                        + ((double)read["Balance"] - val).ToString().Replace(',', '.') + "' WHERE DATE = '" + (string)read["Date"] + "'", sqLiteConn);
                    comm.ExecuteNonQuery();
                    Head = Head.AddDays(1);
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
