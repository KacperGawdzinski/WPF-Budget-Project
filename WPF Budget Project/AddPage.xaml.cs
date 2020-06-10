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

namespace WPF_Budget_Project
{
    public partial class AddPage : Page
    {
        bool TypeInsertBuilt = false;
        bool ExpendUsed = false;
        bool IncomeUsed = true;
        bool SaveInsideGrid = false;
        public AddPage()
        {
            InitializeComponent();
            /*DateTime x = new DateTime(2000,1,11);
            x = x.AddDays(50);
            Console.WriteLine(x.ToString("dd.MM.yyyy"));*/
        }

        void IncomeChecked(object sender, EventArgs e)
        {
            IncomeUsed = true;
            if (ExpendCheck.IsChecked == true)
            {
                ExpendCheck.IsChecked = false;
            }
            if (TypeInsertBuilt)
            {
                Stack.Children.RemoveAt(4);
                Stack.Children.RemoveAt(3);
                TypeInsertBuilt = false;
            }
            TypeCombo.Items.Clear();
            var sqLiteConn = new SQLiteConnection(@"Data Source=database.db;Version=3;");
            sqLiteConn.Open();
            string command = "select * from [gawdzinskikacper@gmail.com-income]";
            SQLiteCommand comm = new SQLiteCommand(command, sqLiteConn);
            comm.ExecuteNonQuery();
            ComboBoxItem x;
            SQLiteDataReader read = comm.ExecuteReader();
            for (var i = 0; i < read.FieldCount; i++)
            {
                string temp = read.GetName(i);
                if (temp == "Data" || temp == "id" || temp == "Repeatability")
                    continue;
                x = new ComboBoxItem();
                x.Content = temp;
                TypeCombo.Items.Add(x);
            }
            x = new ComboBoxItem();
            x.Content = "New type...";
            TypeCombo.Items.Add(x);
            read.Close();
            sqLiteConn.Close();
            if(ExpendUsed)
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

        void ExpendChecked(object sender, EventArgs e)
        {
            if (IncomeCheck.IsChecked == true)
            { 
                IncomeCheck.IsChecked = false;
            }
            if (TypeInsertBuilt)
            {
                Stack.Children.RemoveAt(4);
                Stack.Children.RemoveAt(3);
                TypeInsertBuilt = false;
            }
            TypeCombo.Items.Clear();
            ExpendUsed = true;
            var sqLiteConn = new SQLiteConnection(@"Data Source=database.db;Version=3;");
            sqLiteConn.Open();
            string command = "select * from [gawdzinskikacper@gmail.com-expend]";
            SQLiteCommand comm = new SQLiteCommand(command, sqLiteConn);
            comm.ExecuteNonQuery();
            ComboBoxItem x;
            SQLiteDataReader read = comm.ExecuteReader();
            for (var i = 0; i < read.FieldCount; i++)
            {
                string temp = read.GetName(i);
                if (temp == "Data" || temp == "id" || temp == "Repeatability" || temp == "MaxValue")
                    continue;
                x = new ComboBoxItem();
                x.Content = temp;
                TypeCombo.Items.Add(x);
            }
            x = new ComboBoxItem();
            x.Content = "New type...";
            TypeCombo.Items.Add(x);
            read.Close();
            sqLiteConn.Close();

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

        void TypeComboChanged(object sender, EventArgs e)
        {
            Console.WriteLine(TypeCombo.Text);
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
                if(PeriodicCheck.IsChecked == true && ExpendCheck.IsChecked == true && SaveInsideGrid == false)
                {
                    Stack.Children.RemoveAt(Stack.Children.Count - 1);
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
                    Save.Margin = new Thickness(250, 100, 0, 0);
                    Save.Click += SaveClick;
                    PeriodicGrid.Children.Add(Save);   //rewrite as function - duplicate of 233-253
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
            StartDate.Margin = new Thickness(140, 10, 0, 0);
            Calendar Date = new Calendar();
            Viewbox box = new Viewbox();
            box.HorizontalAlignment = HorizontalAlignment.Left;
            box.MaxWidth = 250;
            box.MaxHeight = 300;
            box.Margin = new Thickness(80, 35, 0, 0);

            // Date.DisplayDateStart = new DateTime(2009, 1, 10);
            Date.DisplayDate = new DateTime(2009, 3, 15);
            //Date.SelectedDate = new DateTime(2009, 2, 15);
            box.Child = Date;
            PeriodicGrid.Children.Add(PeriodicText);
            PeriodicGrid.Children.Add(PeriodicBox);
            PeriodicGrid.Children.Add(StartDate);
            PeriodicGrid.Children.Add(box);
            if(ExpendCheck.IsEnabled && TypeInsertBuilt)
            {
                Stack.Children.RemoveAt(Stack.Children.Count - 1);
                Button Save = new Button();
                TextBlock Txt = new TextBlock();
                Txt.Text = "Save";
                Txt.Margin = new Thickness(10, 0, 0, 0);
                StackPanel temp = new StackPanel();
                temp.Orientation = Orientation.Horizontal;
                temp.Children.Add(new MaterialDesignThemes.Wpf.PackIcon
                { Kind = MaterialDesignThemes.Wpf.PackIconKind.ContentSave });
                temp.Children.Add(Txt);
                Save.Content = temp;
                Save.Background = null;
                Save.BorderBrush = null;
                Save.Width = 100;
                Save.Height = 40;
                Save.Click += SaveClick;
                BrushConverter bc = new BrushConverter();
                Save.Background = (Brush)bc.ConvertFrom("#2e7d32");
                Save.BorderBrush = (Brush)bc.ConvertFrom("#2e7d32");
                Save.Margin = new Thickness(250, 100, 0, 0);
                PeriodicGrid.Children.Add(Save);
                SaveInsideGrid = true;
            }
        }
        void PeriodicUnchecked(object sender, EventArgs e)
        {
            PeriodicGrid.Children.RemoveAt(PeriodicGrid.Children.Count - 1);
            PeriodicGrid.Children.RemoveAt(PeriodicGrid.Children.Count - 1);
            PeriodicGrid.Children.RemoveAt(PeriodicGrid.Children.Count - 1);
            PeriodicGrid.Children.RemoveAt(PeriodicGrid.Children.Count - 1);
            if(SaveInsideGrid)
            {
                SaveInsideGrid = false;
                PeriodicGrid.Children.RemoveAt(PeriodicGrid.Children.Count - 1);
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
                Save.Margin = new Thickness(0, 20, 0, 0);
                Save.Click += SaveClick;
                Save.Name = "SaveButton";
                Stack.Children.Add(Save);   //rewrite as function - duplicate of 233-253
            }
        }

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

        void ShowError(string warning)
        {
            Window OK = new Notification(warning);
            OK.Show();
            return;
        }

        void SaveClick(object sender, EventArgs e)
        {
            int k = 0;
            if (IncomeCheck.IsChecked == true)
            {
                if (TypeCombo.Text == "New type...")
                {
                    foreach (var val in FindVisualChildren<TextBox>(MainGrid))
                    {
                        if (k == 1)
                            if (val.Text.Length == 0)
                            {
                                ShowError("Insert new type!");
                                return;
                            }

                        if (k == 2)
                        {
                            if (val.Text.Length == 0)
                            {
                                ShowError("Insert value!");
                                return;
                            }
                            for (int i = 0; i < val.Text.Length; i++)
                                if (Char.IsLetter(val.Text[i]))
                                {
                                    ShowError("Value must be a number!");
                                    return;
                                }
                        }
                        k++;
                    }
                }
                else if (TypeCombo.Text != "")
                {
                    foreach (var val in FindVisualChildren<TextBox>(this))
                    {
                        if (k == 1)
                        {
                            if (val.Text.Length == 0)
                            {
                                ShowError("Insert value!");
                                return;
                            }
                            for (int i = 0; i < val.Text.Length; i++)
                                if (Char.IsLetter(val.Text[i]))
                                {
                                    ShowError("Value must be a number!");
                                    return;
                                }
                        }
                        k++;
                    }
                }
                else
                {
                    ShowError("Choose type!");
                    return;
                }
            }

            else if (ExpendCheck.IsChecked == true)  //huge spaghetti - needs division into functions but I dont have much time due to exams
            {
                if (TypeCombo.Text == "New type...")
                {
                    foreach (var val in FindVisualChildren<TextBox>(MainGrid))
                    {
                        if (k == 1)
                            if (val.Text.Length == 0)
                            {
                                ShowError("Insert new type!");
                                return;
                            }

                        if (k == 2 || k == 3)
                        {
                            if (val.Text.Length == 0)
                            {
                                ShowError("Insert value!");
                                return;
                            }
                            for (int i = 0; i < val.Text.Length; i++)
                                if (Char.IsLetter(val.Text[i]))
                                {
                                    ShowError("Value must be a number!");
                                    return;
                                }
                        }
                        k++;
                    }
                }
                else if (TypeCombo.Text != "")
                {
                    foreach (var val in FindVisualChildren<TextBox>(this))
                    {
                        if (k == 1)
                        {
                            if (val.Text.Length == 0)
                            {
                                ShowError("Insert value!");
                                return;
                            }
                            for (int i = 0; i < val.Text.Length; i++)
                                if (Char.IsLetter(val.Text[i]))
                                {
                                    ShowError("Value must be a number!");
                                    return;
                                }
                        }
                        k++;
                    }
                }
                else
                {
                    ShowError("Choose type!");
                    return;
                }
            }

            else
            {
                ShowError("Choose category!");
                return;
            }

            k = 0;
            if (PeriodicCheck.IsChecked == true)
            {
               /* foreach (var val in FindVisualChildren<TextBox>(this))
                {
                    if (k == 2)
                        for (int i = 0; i < val.Text.Length; i++)
                            if (Char.IsLetter(val.Text[i]))
                                ShowError();
                    k++;
                }
                k = 0;*/
                foreach (var val in FindVisualChildren<Calendar>(this))
                {
                    try
                    {
                        Console.WriteLine(val.SelectedDate.Value.ToString("dd.MM.yyyy"));
                    }
                    catch(InvalidOperationException)
                    {
                        ShowError("Choose category!");
                        return;
                    }
                }

                foreach (var val in FindVisualChildren<ComboBox>(this))
                {
                    if (k == 0)
                    {
                        k++;
                        continue;
                    }
                    if(val.Text == "")
                    {
                        ShowError("Choose interval!");
                        return;
                    }
                }
            }
            Console.WriteLine("przeszlo");
        }
    }
}
