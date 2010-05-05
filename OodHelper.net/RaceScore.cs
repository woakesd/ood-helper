using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.net
{
    [Svn("$Id: RaceScore.cs 17583 2010-05-02 17:23:57Z david $")]
    public interface RaceScore
    {
        void Calculate(int rid);
    }
}
