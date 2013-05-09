using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OodHelper.Helpers
{
    public class CommandListItem
    {
        public string Text { get; set; }
        public RelayCommand Command { get; set; }
        public object CommandParameter { get; set; }
    }
}
