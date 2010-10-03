using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.LoadTide
{
    struct TideData
    {
        public DateTime date;
        public double height;
        public double current;

        public TideData(DateTime d, double h)
        {
            date = d;
            height = h;
            current = 0;
        }
    }
}
