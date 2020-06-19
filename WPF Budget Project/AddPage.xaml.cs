﻿using System;
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

namespace WPF_Budget_Project
{
    public partial class Pack
    {
        int sum;
        List<string> s;
        public Pack(int x, List<string> y)
        {
            sum = x;
            s = y;
        }
        public int Sum()
        {
            return sum;
        }
        public List<string> Input()
        {
            return s;
        }
    };


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
            string[] temp = ReadColumns(true);           //all columns from table
            for (int i=0;i<temp.Length;i++)
            {
                if (temp[i] == "DATE" || temp[i] == "ID" || temp[i] == "REPEATABILITY" || temp[i] == "NO")
                    continue;
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
            string[] temp = ReadColumns(false);
            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i] == "DATE" || temp[i] == "ID" || temp[i] == "REPEATABILITY" || temp[i] == "NO" || temp[i] == "MAXVALUE")
                    continue;
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


        string[] ReadColumns(bool income)   //might need slight modification to filter data,id etc in place
        {
            List<string> l = new List<string>();
            var sqLiteConn = new SQLiteConnection(@"Data Source=database.db;Version=3;");
            sqLiteConn.Open();
            SQLiteCommand comm;
            if(income)
                comm = new SQLiteCommand("select * from [" + UserMail + "-income]", sqLiteConn);
            else
                comm = new SQLiteCommand("select * from [" + UserMail + "-expend]", sqLiteConn);
            comm.ExecuteNonQuery();
            SQLiteDataReader read = comm.ExecuteReader();
            for (var i = 0; i < read.FieldCount; i++)
            {
                l.Add(read.GetName(i));
            }
            read.Close();
            sqLiteConn.Close();
            return l.ToArray();
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

        Pack CheckInputData()
        {
            int k = 0, sum = 0;
            List<string> l = new List<string>();
            if (IncomeCheck.IsChecked == true)//TODO can be done much shorter without checking maxvalue
            {
                if (TypeCombo.Text == "New type...")
                {
                    sum++;
                    foreach (var val in FindVisualChildren<TextBox>(MainGrid))
                    {
                        if (k == 1)
                        {
                            if (val.Text.Length == 0)
                            {
                                ShowError("Insert new type!");
                                return null;
                            }
                            string[] temp = ReadColumns(true);
                            for (int i = 0; i < temp.Length; i++)
                                if (temp[i] == val.Text)
                                {
                                    ShowError("This column already exists!");
                                    return null;
                                }
                            l.Add(val.Text);
                        }
                        if (k == 2)
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
                            l.Add(val.Text);
                        }
                        k++;
                    }
                }
                else if (TypeCombo.Text != "")
                {
                    l.Add(TypeCombo.Text);
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
                            l.Add(val.Text);
                        }
                        k++;
                    }
                }
                else
                {
                    ShowError("Choose type!");
                    return null;
                }
            }

            else if (ExpendCheck.IsChecked == true)  //huge spaghetti - needs division into functions but I dont have much time due to exams
            {
                sum = 4;
                if (TypeCombo.Text == "New type...")
                {
                    sum++;
                    foreach (var val in FindVisualChildren<TextBox>(MainGrid))
                    {
                        if (k == 1)
                        {
                            if (val.Text.Length == 0)
                            {
                                ShowError("Insert new type!");
                                return null;
                            }
                            string[] temp = ReadColumns(true);
                            for (int i = 0; i < temp.Length; i++)
                                if (temp[i] == val.Text)
                                {
                                    ShowError("This column already exists!");
                                    return null;
                                }
                            l.Add(val.Text);
                        }

                        if (k == 2 || k == 3)
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
                            l.Add(val.Text);
                        }
                        k++;
                    }
                }
                else if (TypeCombo.Text != "")
                {
                    l.Add(TypeCombo.Text);
                    foreach (var val in FindVisualChildren<TextBox>(this))
                    {
                        if (k == 1 || k== 2)
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
                            l.Add(val.Text);
                        }
                        k++;
                    }
                }
                else
                {
                    ShowError("Choose type!");
                    return null;
                }
            }

            else
            {
                ShowError("Choose category!");
                return null;
            }

            k = 0;
            if (PeriodicCheck.IsChecked == true)
            {
                sum = sum + 2;
                foreach (var val in FindVisualChildren<Calendar>(this))
                {
                    try
                    {
                        l.Add(val.SelectedDate.Value.ToString("yyyyMMdd"));
                    }
                    catch(InvalidOperationException)
                    {
                        ShowError("Choose date!");
                        return null;
                    }
                }

                foreach (var val in FindVisualChildren<ComboBox>(this))
                {
                    if (k == 1)
                    {
                        if (val.Text == "")
                        {
                            ShowError("Choose interval!");
                            return null;
                        }
                        l.Add(val.Text);
                        k++;
                        continue;
                    }
                    k++;
                }
            }
            Pack res = new Pack(sum,l);
            return res;
        }
        #endregion
        #region SaveData
        void SaveClick(object sender, EventArgs e)
        {
            Pack InputData = CheckInputData();
            if (InputData != null)
            {
                var sqLiteConn = new SQLiteConnection(@"Data Source=database.db;Version=3;");
                sqLiteConn.Open();
                SQLiteCommand comm;
                Guid guid = Guid.NewGuid();
                switch (InputData.Sum())
                {
                    case 0:
                        comm = new SQLiteCommand("INSERT INTO [" + UserMail +"-income] ("+InputData.Input()[0]+", DATE, ID) " +
                            "values('" + InputData.Input()[1] + "', '" + DateTime.Today.ToString("yyyyMMdd") + "', "+ guid.ToString() +")", sqLiteConn);
                        comm.ExecuteNonQuery();
                        break;
                    case 1:
                        comm = new SQLiteCommand("ALTER TABLE[" + UserMail + "-income] ADD[" + InputData.Input()[0] + "] REAL", sqLiteConn);
                        comm.ExecuteNonQuery();
                        comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-income] (" + InputData.Input()[0] + ", DATE, ID) " +
                            "values('" + InputData.Input()[1] + "', '" + DateTime.Today.ToString("yyyyMMdd") + "', '" + guid.ToString() + ")", sqLiteConn);
                        comm.ExecuteNonQuery();
                        break;
                    case 2:
                        comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-income] (" + InputData.Input()[0] + ", DATE, REPEATABILITY, ID) " +
                             "values('" + InputData.Input()[1] + "', '" + InputData.Input()[2] + "', '" + InputData.Input()[3] + "', '" + guid.ToString() + ")", sqLiteConn);
                        comm.ExecuteNonQuery();
                        break;
                    case 3:
                        comm = new SQLiteCommand("ALTER TABLE[" + UserMail + "-income] ADD[" + InputData.Input()[0] + "] REAL", sqLiteConn);
                        comm.ExecuteNonQuery();
                        comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-income] (" + InputData.Input()[0] + ", DATE, REPEATABILITY, ID) " +
                             "values('" + InputData.Input()[1] + "', '" + InputData.Input()[2] + "', '" + InputData.Input()[3] + "', '" + guid.ToString() + ")", sqLiteConn);
                        comm.ExecuteNonQuery();
                        break;
                    case 4:
                        comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-expend] (" + InputData.Input()[0] + ", DATE, MAXVALUE, ID) " +
                            "values('" + InputData.Input()[1] + "', '" + DateTime.Today.ToString("yyyyMMdd") + "', '" + InputData.Input()[2] + "', '" + guid.ToString() + "')", sqLiteConn);
                        comm.ExecuteNonQuery();
                        break;
                    case 5:
                        comm = new SQLiteCommand("ALTER TABLE[" + UserMail + "-expend] ADD[" + InputData.Input()[0] + "] REAL", sqLiteConn);
                        comm.ExecuteNonQuery();
                        comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-expend] (" + InputData.Input()[0] + ", DATE, MAXVALUE, ID) " +
                            "values('" + InputData.Input()[1] + "', '" + DateTime.Today.ToString("yyyyMMdd") + "', " + InputData.Input()[2] + "', '" + guid.ToString() + "')", sqLiteConn);
                        comm.ExecuteNonQuery();
                        break;
                    case 6:
                        comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-expend] (" + InputData.Input()[0] + ", DATE, REPEATABILITY, MAXVALUE, ID) " +
                             "values('" + InputData.Input()[1] + "', '" + InputData.Input()[3] + "', '" + InputData.Input()[4] + "', '" + InputData.Input()[2] + "', '" + guid.ToString() + "')", sqLiteConn);
                        comm.ExecuteNonQuery();
                        break;
                    case 7:
                        comm = new SQLiteCommand("ALTER TABLE[" + UserMail + "-expend] ADD[" + InputData.Input()[0] + "] REAL", sqLiteConn);
                        comm.ExecuteNonQuery();
                        comm = new SQLiteCommand("INSERT INTO [" + UserMail + "-expend] (" + InputData.Input()[0] + ", DATE, REPEATABILITY, MAXVALUE, ID) " +
                             "values('" + InputData.Input()[1] + "', '" + InputData.Input()[3] + "', '" + InputData.Input()[4] + "', '" + InputData.Input()[2] + "', '" + guid.ToString() + "')", sqLiteConn);
                        comm.ExecuteNonQuery();
                        break;
                }
                 Window OK = new Notification("Transaction added!");
                 OK.Show();
            }
        }
        #endregion
    }
}
