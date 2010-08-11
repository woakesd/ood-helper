using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.net
{
    class SvnRevision
    {
        public const string Rev = @"$WCREV$";
        public const string Date = @"$WCDATE$";
        public const string Range = @"$WCRANGE$";
        public const string BuildDate = @"$WCNOW$";
        public const string LocalMods = @"$WCMODS?Local modifications:No local modifications$";
    }
}
