﻿using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;

namespace OodHelper
{
    [Svn("$Id$")]
    public partial class Results : DataContext
    {
        public Table<Calendar> Calendar;
        public Results() : base(Properties.Settings.Default.OodHelperConnectionString) { }
    }
}
