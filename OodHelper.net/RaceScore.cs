using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.net
{
    [Svn("$Id$")]
    public interface IRaceScore
    {
        double StandardCorrectedTime { get; }

        void Calculate(int rid);

        int Finishers { get; set; }

        bool Calculated { get; set; }
    }
}
