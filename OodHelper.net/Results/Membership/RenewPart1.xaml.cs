﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OodHelper.Membership
{
    /// <summary>
    /// Interaction logic for Renew_part1.xaml
    /// </summary>
    public partial class RenewPart1 : Page
    {
        public RenewPart1()
        {
            InitializeComponent();
            using (Db _conn = new Db(""))
            {
            }
        }
    }
}
