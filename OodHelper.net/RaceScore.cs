using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace OodHelper
{
    [Svn("$Id$")]
    public interface IRaceScore
    {
        double StandardCorrectedTime { get; }

        void Calculate(int rid);

        void Calculate(object sender, DoWorkEventArgs e);

        int Finishers { get; }

        bool Calculated { get; }
    }
}
