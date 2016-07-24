using Google.Common.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Api.Helpers
{
    static public class Utils
    {

        private static double lat_gap_meters = 32.5;
        private static double lng_gap_meters = 21.5;


        private static double meters_per_degree = 111111;
        private static double lat_gap_degrees => lat_gap_meters / meters_per_degree;

        static private double calculate_lng_degrees(double lat)
        {
            return lng_gap_meters / (meters_per_degree * Math.Cos(S2Helper.Deg2Rad(lat)));
        }
        static public IEnumerable<S2LatLng> GetScanningPoints(S2LatLng origin, int numSteps)
        {
            int ring = 1;
            yield return origin;
            double lat = origin.LatDegrees, lng = origin.LngDegrees;
            while (ring < numSteps)
            {
                lat += lat_gap_degrees;
                lng -= calculate_lng_degrees(lat);

                for (var direction = 0; direction < 6; direction++)
                {
                    for (var i = 0; i < ring; i++)
                    {
                        if (direction == 0)
                            lng += calculate_lng_degrees(lat) * 2;


                        if (direction == 1)
                        {
                            lat -= lat_gap_degrees;
                            lng += calculate_lng_degrees(lat);
                        }
                        if (direction == 2)
                        {
                            lat -= lat_gap_degrees;
                            lng -= calculate_lng_degrees(lat);


                        }
                        if (direction == 3)
                        {
                            lng -= calculate_lng_degrees(lat) * 2;


                        }
                        if (direction == 4)
                        {
                            lat += lat_gap_degrees;
                            lng -= calculate_lng_degrees(lat);


                        }
                        if (direction == 5)
                        {
                            lat += lat_gap_degrees;
                            lng += calculate_lng_degrees(lat);
                        }
                        yield return S2LatLng.FromDegrees(lat, lng);
                    }
                    ring += 1;
                }
            }
        }
        public static ulong FloatAsUlong(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}
