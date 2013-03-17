using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace OodHelper
{
    public static class GuidExtension
    {
        [DllImport("rpcrt4.dll", SetLastError = true)]
        static extern int UuidCreateSequential(out Guid guid);

        public static Guid SequentialGuid(this Guid _guid)
        {
            UuidCreateSequential(out _guid);
            return _guid;
        }
    }
}
