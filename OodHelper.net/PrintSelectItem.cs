﻿using System;
using System.ComponentModel;

namespace OodHelper
{
    public interface IPrintSelectItem: INotifyPropertyChanged
    {
        bool PrintIncludeAllVisible { get; set; }
        bool PrintIncludeAll { get; set; }
        bool PrintInclude { get; set; }
        int PrintIncludeCopies { get; set; }
        string PrintIncludeDescription { get; set; }
        int PrintIncludeGroup { get; set; }
        void OnPropertyChanged(string name);
    }
}
