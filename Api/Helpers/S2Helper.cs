using Google.Common.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Api.Helpers
{
    static public class S2Helper
    {
        private const double EARTH_RADIUS = 6378137;
        public static S2LatLng ComputeOffset(S2LatLng from, double distance, double heading)
        {
            distance /= EARTH_RADIUS;
            heading = Deg2Rad(heading);
            var cosDistance = Math.Cos(distance);
            var sinDistance = Math.Sin(distance);
            var sinFromLat = Math.Sin(from.LatRadians);
            var cosFromLat = Math.Cos(from.LatRadians);
            var sc = cosDistance * sinFromLat + sinDistance * cosFromLat * Math.Cos(heading);

            var lat = Rad2Deg(Math.Asin(sc));
            var lng = Rad2Deg(from.LngRadians + Math.Atan2(sinDistance * cosFromLat * Math.Sin(heading), cosDistance - sinFromLat * sc));

            return S2LatLng.FromDegrees(lat, lng);
        }
        public static double ComputeHeading(S2LatLng from, S2LatLng to)
        {
            var lng = to.LngRadians - from.LngRadians;

            return wrapLongitude(Rad2Deg(Math.Atan2(Math.Sin(lng) * Math.Cos(to.LatRadians), Math.Cos(from.LatRadians)
                * Math.Sin(to.LatRadians) - Math.Sin(from.LatRadians) * Math.Cos(to.LatRadians) * Math.Cos(lng))));
        }
        public static double wrapLongitude(double lng)
        {
            return lng == 180 ? lng : ((((lng + 180) % 360) + 360) % 360) - 180;
        }
        static public double Deg2Rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }
        static public double Rad2Deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
        static public double ToKilometers(this S1Angle s1)
        {
            return s1.Degrees * 60 * 1.1515 * 1.609344;
        }
        public static List<ulong> GetNearbyCellIds(double latitude, double longitude)
        {
            var nearbyCellIds = new List<S2CellId>();

            var cellId = S2CellId.FromLatLng(S2LatLng.FromDegrees(latitude, longitude)).ParentForLevel(15);//.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent;

            nearbyCellIds.Add(cellId);
            for (int i = 0; i < 10; i++)
            {
                nearbyCellIds.Add(GetPrevious(cellId, i));
                nearbyCellIds.Add(GetNext(cellId, i));
            }
            return nearbyCellIds.Select(c => c.Id).OrderBy(c => c).ToList();
        }

        private static S2CellId GetPrevious(S2CellId cellId, int depth)
        {
            if (depth < 0)
                return cellId;

            depth--;

            return GetPrevious(cellId.Previous, depth);
        }

        private static S2CellId GetNext(S2CellId cellId, int depth)
        {
            if (depth < 0)
                return cellId;

            depth--;

            return GetNext(cellId.Next, depth);
        }

    }
}
