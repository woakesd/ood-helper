using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;

namespace OodHelper
{
    public partial class HandicapLinq : DataContext
    {
        public Table<HandicapRecord> portsmouth_numbers;
        public HandicapLinq() : base(HandicapDb.DatabaseConstr) { }
    }
}
