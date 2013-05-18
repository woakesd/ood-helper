using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OodHelper.Messaging
{
    public class EventStartChanged
    {
        public int Rid { get; set; }
        public DateTime? Start { get; set; }
    }
}
