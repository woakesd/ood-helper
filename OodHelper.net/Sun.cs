using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.net
{
    class Sun
    {
        public static TimeSpan SunSet(DateTime dt)
        {
            const int twothou = 2451545;
            double lng = 3.40904;
            double lat = 55.99266;
            //long jd = (long)(dt.Date - new DateTime(-4713, 1, 1)).TotalDays;
            double jd = (dt.Date - new DateTime(2000, 1, 1)).TotalDays + twothou;
            long n = (long) Math.Round(jd - twothou - 0.0009 - lng / 360);
            double J = twothou + 0.0009 + lng / 360 + n;
            double M = (357.5291 + 0.98560028 * (J - twothou)) % 360;
            double C = 1.9148 * Sin(M) + .02 * Sin(2 * M) + .0003 * Math.Sin(3 * M);
            double eclLng = (M + 102.9372 + C + 180) % 360;
            double Jtrans = J + (0.0053 * Sin(M)) - (0.0069 * Sin(2 * eclLng));
            double declin = Asin(Sin(eclLng) * Sin(23.45));
            double hourAngle = Acos((Sin(-0.83) - Sin(lat) * Sin(declin)) / (Cos(lat) * Cos(declin)));
            double JSet = twothou + 0.0009 + ((hourAngle + lng) / 360 + n + 0.0053 * Sin(M)) - 0.0069 * Sin(2 * eclLng);
            DateTime x = new DateTime(2000, 1, 1).AddDays(JSet - twothou);
            return TimeSpan.Zero;
        }

        private static double Sin(double deg)
        {
            return Math.Sin(deg * Math.PI / 180);
        }

        private static double Asin(double v)
        {
            return Math.Asin(v) * 180 / Math.PI;
        }

        private static double Cos(double deg)
        {
            return Math.Cos(deg * Math.PI / 180);
        }

        private static double Acos(double v)
        {
            return Math.Acos(v) * 180 / Math.PI;
        }

    }
}
