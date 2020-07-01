using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clerk
{
    class LongToDateTime
    {
        public DateTime ConvertToClass(long date)
        {
            int a, b, c;
            a = (int)(date / 10000);
            b = (int)((date - a * 10000) / 100);
            c = (int)(date - a * 10000 - b * 100);
            DateTime x = new DateTime(a, b, c);
            return x;
        }
    };
}
