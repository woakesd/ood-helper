﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Xps;

namespace OodHelper
{
    interface IResultsPage
    {
        bool PrintPage(VisualsToXpsDocument collator, int pageNo);
    }
}
