using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OodHelper.Maintain.Models;

namespace OodHelper.Messaging
{
    public class UpdatedBoatMessage
    {
        public IBoatModel Boat { get; set; }
    }
}
