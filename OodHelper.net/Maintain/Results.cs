﻿using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;

namespace OodHelper.Maintain
{
    public partial class Results : DataContext
    {
        public Table<Calendar> Calendar;
        public Results() : base(Db.DatabaseConstr) { }
    }
}
