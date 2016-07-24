using Google.Common.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGoApi.ConsoleTest
{
    static public class GeoUtils
    {
        static public double ToKilometers(this S1Angle s1)
        {
            return  s1.Degrees * 60 * 1.1515 *1.609344;       
        }
        static public double Distance(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(Deg2Rad(lat1)) * Math.Sin(Deg2Rad(lat2)) + Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) * Math.Cos(Deg2Rad(theta));
            dist = Math.Acos(dist);
            dist = Rad2Deg(dist);
            dist = dist * 60 * 1.1515;
            if (unit == 'K')
            {
                dist = dist * 1.609344;
            }
            else if (unit == 'N')
            {
                dist = dist * 0.8684;
            }
            return (dist);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts decimal degrees to radians             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        static public double Deg2Rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts radians to decimal degrees             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        static public double Rad2Deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
    }
}
