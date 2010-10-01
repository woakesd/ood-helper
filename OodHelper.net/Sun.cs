using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper
{
    /**
     * This class is derived from javascript found on http://www.esrl.noaa.gov/gmd/grad/solcalc/
     */
    [Svn("$Id$")]
    public class Sun
    {
        public DateTime? SunRise { get; private set; }
        public DateTime? SunSet { get; private set; }

        private double Longitude { get; set; }
        private double Lattitude { get; set; }

        public Sun(DateTime dt, double lat, double lng)
        {
            double jday = getJD(dt.Date);
            double tl = getTimeLocal(dt);
            int tz = (dt.IsDaylightSavingTime()) ? -1 : 0;
            bool dst = dt.IsDaylightSavingTime();
            double total = jday + tl / 1440 - tz / 24.0;
            double T = calcTimeJulianCent(jday);
            Longitude = lng;
            Lattitude = lat;
            double azimuth = calcAzEl(T, tl, Lattitude, Longitude, tz);
            double solNoonLocal = calcSolNoon(jday, Longitude, tz, dst);
            double rise = calcSunriseSet(true, jday, Lattitude, Longitude, tz, dst);
            double set = calcSunriseSet(false, jday, Lattitude, Longitude, tz, dst);
            if (!double.IsNaN(rise)) SunRise = dt.Date + new TimeSpan((long)(rise * 600000000));
            if (!double.IsNaN(set)) SunSet = dt.Date + new TimeSpan((long)(set * 600000000));
        }

        private double calcSunriseSetUTC(bool rise, double JD, double latitude, double longitude)
        {
            double t = calcTimeJulianCent(JD);
            double eqTime = calcEquationOfTime(t);
            double solarDec = calcSunDeclination(t);
            double hourAngle = calcHourAngleSunrise(latitude, solarDec);
            if (!rise) hourAngle = -hourAngle;
            double delta = longitude + hourAngle;
            double timeUTC = 720 - (4.0 * delta) - eqTime;	// in minutes
            return timeUTC;
        }

        private double calcHourAngleSunrise(double lat, double solarDec)
        {
            var HAarg = Cos(90.833) / (Cos(lat) * Cos(solarDec)) - Tan(lat) * Tan(solarDec);
            var HA = Acos(HAarg);
            return HA; // in radians (for sunset, use -HA)
        }

        private double calcSunriseSet(bool rise, double JD, double latitude, double longitude, int timezone, bool dst)
        {
            double timeUTC = calcSunriseSetUTC(rise, JD, latitude, longitude);
            double newTimeUTC = calcSunriseSetUTC(rise, JD + timeUTC / 1440.0, latitude, longitude);
            return newTimeUTC;
        }
 
        private double calcSolNoon(double jd, double longitude, int timezone, bool dst)
        {
            var tnoon = calcTimeJulianCent(jd - longitude / 360.0);
            var eqTime = calcEquationOfTime(tnoon);
            var solNoonOffset = 720.0 - (longitude * 4) - eqTime; // in minutes
            var newt = calcTimeJulianCent(jd + solNoonOffset / 1440.0);
            eqTime = calcEquationOfTime(newt);
            double solNoonLocal = 720 - (longitude * 4) - eqTime + (timezone * 60.0); // in minutes
            if (dst) solNoonLocal += 60.0;
            return solNoonLocal;
        }
 
        private double calcTimeJulianCent(double jd)
        {
            return (jd - 2451545.0) / 36525.0;
        }

        private double getJD(DateTime dt)
        {
            int docmonth = dt.Month;
            int docday = dt.Day;
            int docyear = dt.Year;
            if (docmonth <= 2)
            {
                docyear -= 1;
                docmonth += 12;
            }
            double A = Math.Floor(docyear / 100.0);
            double B = 2 - A + Math.Floor(A / 4);
            return Math.Floor(365.25 * (docyear + 4716)) + Math.Floor(30.6001 * (docmonth + 1)) + docday + B - 1524.5;
        }

        private double getTimeLocal(DateTime dt)
        {
            int dochr = dt.Hour;
            int docmn = dt.Minute;
            int docsc = dt.Second;
            if (dt.IsDaylightSavingTime())
            {
                dochr -= 1;
            }
            return dochr * 60 + docmn + docsc / 60.0;
        }

        private double calcAzEl(double T, double localtime, double latitude, double longitude, int zone)
        {
            double eqTime = calcEquationOfTime(T);
            double theta = calcSunDeclination(T);

            double solarTimeFix = eqTime + 4.0 * longitude - 60.0 * zone;
            double earthRadVec = calcSunRadVector(T);
            double trueSolarTime = localtime + solarTimeFix;
            while (trueSolarTime > 1440)
            {
                trueSolarTime -= 1440;
            }
            double hourAngle = trueSolarTime / 4.0 - 180.0;
            if (hourAngle < -180)
            {
                hourAngle += 360.0;
            }

            double csz = Sin(latitude) * Sin(theta) + Cos(latitude) * Cos(theta) * Cos(hourAngle);
            if (csz > 1.0)
            {
                csz = 1.0;
            }
            else if (csz < -1.0)
            {
                csz = -1.0;
            }
            double zenith = Acos(csz);
            double azDenom = Cos(latitude) * Sin(zenith);
            double azimuth = 0;
            if (Math.Abs(azDenom) > 0.001)
            {
                double azRad = (Sin(latitude) * Cos(zenith) - Sin(theta)) / azDenom;
                if (Math.Abs(azRad) > 1.0)
                {
                    if (azRad < 0)
                    {
                        azRad = -1.0;
                    }
                    else
                    {
                        azRad = 1.0;
                    }
                }

                azimuth = 180.0 - radToDeg(Math.Acos(azRad)); // questionable whether azRad is in Radians
                if (hourAngle > 0.0)
                {
                    azimuth = -azimuth;
                }
            }
            else
            {
                if (latitude > 0.0)
                {
                    azimuth = 180.0;
                }
                else
                {
                    azimuth = 0.0;
                }
            }
            if (azimuth < 0.0)
            {
                azimuth += 360.0;
            }
            double exoatmElevation = 90.0 - zenith;

            // Atmospheric Refraction correction

            double refractionCorrection;
            if (exoatmElevation > 85.0)
            {
                refractionCorrection = 0.0;
            }
            else
            {
                var te = Tan(exoatmElevation);
                if (exoatmElevation > 5.0)
                {
                    refractionCorrection = 58.1 / te - 0.07 / (te * te * te) + 0.000086 / (te * te * te * te * te);
                }
                else if (exoatmElevation > -0.575)
                {
                    refractionCorrection = 1735.0 + exoatmElevation * (-518.2 + exoatmElevation * (103.4 + exoatmElevation * (-12.79 + exoatmElevation * 0.711)));
                }
                else
                {
                    refractionCorrection = -20.774 / te;
                }
                refractionCorrection = refractionCorrection / 3600.0;
            }

            var solarZen = zenith - refractionCorrection;

            return azimuth;
        }

        private double calcSunDeclination(double t)
        {
            double e = calcObliquityCorrection(t);
            double lambda = calcSunApparentLong(t);

            double sint = Sin(e) * Sin(lambda);
            double theta = Asin(sint);
            return theta;		// in degrees
        }

        private double calcSunApparentLong(double t)
        {
            var o = calcSunTrueLong(t);
            var omega = 125.04 - 1934.136 * t;
            var lambda = o - 0.00569 - 0.00478 * Sin(omega);
            return lambda;		// in degrees
        }

        private double calcSunTrueLong(double t)
        {
            double l0 = calcGeomMeanLongSun(t);
            double c = calcSunEqOfCenter(t);
            double O = l0 + c;
            return O;		// in degrees
        }

        private double calcSunEqOfCenter(double t)
        {
            double m = calcGeomMeanAnomalySun(t);
            double sinm = Sin(m);
            double sin2m = Sin(m + m);
            double sin3m = Sin(m + m + m);
            double C = sinm * (1.914602 - t * (0.004817 + 0.000014 * t)) + sin2m * (0.019993 - 0.000101 * t) + sin3m * 0.000289;
            return C;		// in degrees
        }

        private double calcEquationOfTime(double t)
        {
            double epsilon = calcObliquityCorrection(t);
            double l0 = calcGeomMeanLongSun(t);
            double e = calcEccentricityEarthOrbit(t);
            double m = calcGeomMeanAnomalySun(t);

            double y = Tan(epsilon / 2.0);
            y *= y;

            double sin2l0 = Sin(2.0 * l0);
            double sinm = Sin(m);
            double cos2l0 = Cos(2.0 * l0);
            double sin4l0 = Sin(4.0 * l0);
            double sin2m = Sin(2.0 * m);

            double Etime = y * sin2l0 - 2.0 * e * sinm + 4.0 * e * y * sinm * cos2l0 - 0.5 * y * y * sin4l0 - 1.25 * e * e * sin2m;
            return radToDeg(Etime) * 4.0;	// in minutes of time
        }

        private double radToDeg(double rad)
        {
            return rad * 180 / Math.PI;
        }

        private double calcGeomMeanAnomalySun(double t)
        {
            double M = 357.52911 + t * (35999.05029 - 0.0001537 * t);
            return M;		// in degrees
        }

        private double calcMeanObliquityOfEcliptic(double t)
        {
            double seconds = 21.448 - t * (46.8150 + t * (0.00059 - t * (0.001813)));
            double e0 = 23.0 + (26.0 + (seconds / 60.0)) / 60.0;
            return e0;		// in degrees
        }

        private double calcObliquityCorrection(double t)
        {
            double e0 = calcMeanObliquityOfEcliptic(t);
            double omega = 125.04 - 1934.136 * t;
            double e = e0 + 0.00256 * Cos(omega);
            return e;		// in degrees
        }

        private double calcGeomMeanLongSun(double t)
        {
            double L0 = 280.46646 + t * (36000.76983 + t * (0.0003032));
            while (L0 > 360.0)
            {
                L0 -= 360.0;
            }
            while (L0 < 0.0)
            {
                L0 += 360.0;
            }
            return L0;		// in degrees
        }

        private double calcSunRadVector(double t)
        {
            var v = calcSunTrueAnomaly(t);
            var e = calcEccentricityEarthOrbit(t);
            var R = (1.000001018 * (1 - e * e)) / (1 + e * Cos(v));
            return R;		// in AUs
        }

        private double calcSunTrueAnomaly(double t)
        {
            var m = calcGeomMeanAnomalySun(t);
            var c = calcSunEqOfCenter(t);
            var v = m + c;
            return v;		// in degrees
        }

        private double calcEccentricityEarthOrbit(double t)
        {
            double e = 0.016708634 - t * (0.000042037 + 0.0000001267 * t);
            return e;		// unitless
        }

        private double Tan(double deg)
        {
            return Math.Tan(deg * Math.PI / 180);
        }

        private double Atan(double v)
        {
            return Math.Atan(v) * 180 / Math.PI;
        }

        private double Sin(double deg)
        {
            return Math.Sin(deg * Math.PI / 180);
        }

        private double Asin(double v)
        {
            return Math.Asin(v) * 180 / Math.PI;
        }

        private double Cos(double deg)
        {
            return Math.Cos(deg * Math.PI / 180);
        }

        private double Acos(double v)
        {
            return Math.Acos(v) * 180 / Math.PI;
        }

    }
}
