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

namespace Clerk
{
    public partial class History : Page
    {
        public History(string Mail)
        {
            InitializeComponent();
            SQLiteConnection sqLiteConn = new SQLiteConnection(@"Data Source=database.db;Version=3;");
            sqLiteConn.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM [" + Mail + "-transactions] ORDER BY DATETIME([DATE]) DESC", sqLiteConn);
            SQLiteDataReader read = command.ExecuteReader();
            while (read.Read())
            {
                List.Items.Add(new MyItem
                {
                    Category = (string)read["CATEGORY"],
                    Type = (string)read["TYPE"],
                    Value = ((double)read["VALUE"]).ToString() + "$",
                    Date = (string)read["DATE"],
                    ID = (string)read["ID"],
                    Period = DBNull.Value.Equals(read["REPEATABILITY"]) ? "None" : (string)read["REPEATABILITY"],
                    MaxValue = DBNull.Value.Equals(read["REPEATABILITY"]) ? "None" : ((double)read["MAXVALUE"]).ToString() + "$"
                });
            }
        }
    }
}