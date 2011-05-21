using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace OodHelper
{
    [Svn("$Id$")]
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        //private Hashtable _values;
        public Hashtable Values { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
