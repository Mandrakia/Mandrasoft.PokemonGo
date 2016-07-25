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

        //Extensions
        static public IEnumerable<S2LatLng> GetShape(this S2CellId cellId)
        {
            var cell = new S2Cell(cellId);
            S2LatLng firstElement = S2LatLng.Center;
            for (var i = 0; i < 4; i++)
            {
                var latLng = new S2LatLng(cell.GetVertex(i));
                if (i == 0) firstElement = latLng;
                yield return latLng;
            }
            yield return firstElement;
        }
        static public S2LatLng Center(this S2CellId cell)
        {
            return new S2LatLng(new S2Cell(cell).Center);
        }
        static public S2CellId NorthCell(this S2CellId origin)
        {
            var max_size = 1 << 30;
            var size = 1 << (30 - origin.Level);
            int i = 0, j = 0;
            int? orientation = null;
            var face = origin.ToFaceIjOrientation(ref i, ref j, ref orientation);
            return S2CellId.FromFaceIjSame(face, i + size, j, i + size < max_size).ParentForLevel(origin.Level);
        }
        static public S2CellId SouthCell(this S2CellId origin)
        {
            var size = 1 << (30 - origin.Level);
            int i = 0, j = 0;
            int? orientation = null;
            var face = origin.ToFaceIjOrientation(ref i, ref j, ref orientation);
            return S2CellId.FromFaceIjSame(face, i - size, j, i - size >= 0).ParentForLevel(origin.Level);
        }
        static public S2CellId WestCell(this S2CellId origin)
        {
            var max_size = 1 << 30;
            var size = 1 << (30 - origin.Level);
            int i = 0, j = 0;
            int? orientation = null;
            var face = origin.ToFaceIjOrientation(ref i, ref j, ref orientation);
            return S2CellId.FromFaceIjSame(face, i, j + size, j + size < max_size).ParentForLevel(origin.Level);
        }
        static public S2CellId EastCell(this S2CellId origin)
        {
            var size = 1 << (30 - origin.Level);
            int i = 0, j = 0;
            int? orientation = null;
            var face = origin.ToFaceIjOrientation(ref i, ref j, ref orientation);
            return S2CellId.FromFaceIjSame(face, i, j - size, j - size >= 0).ParentForLevel(origin.Level);
        }
        static public S2CellId SouthWestCell(this S2CellId origin)
        {
            var max_size = 1 << 30;
            var size = 1 << (30 - origin.Level);
            int i = 0, j = 0;
            int? orientation = null;
            var face = origin.ToFaceIjOrientation(ref i, ref j, ref orientation);
            return S2CellId.FromFaceIjSame(face, i - size, j + size, j + size < max_size && i - size >= 0).ParentForLevel(origin.Level);
        }
        static public S2CellId NorthWestCell(this S2CellId origin)
        {
            var max_size = 1 << 30;
            var size = 1 << (30 - origin.Level);
            int i = 0, j = 0;
            int? orientation = null;
            var face = origin.ToFaceIjOrientation(ref i, ref j, ref orientation);
            return S2CellId.FromFaceIjSame(face, i + size, j + size, j + size < max_size && i + size < max_size).ParentForLevel(origin.Level);
        }
        static public S2CellId SouthEastCell(this S2CellId origin)
        {
            var size = 1 << (30 - origin.Level);
            int i = 0, j = 0;
            int? orientation = null;
            var face = origin.ToFaceIjOrientation(ref i, ref j, ref orientation);
            return S2CellId.FromFaceIjSame(face, i - size, j - size, j - size >= 0 && i - size >= 0).ParentForLevel(origin.Level);
        }
        static public S2CellId NorthEastCell(this S2CellId origin)
        {
            var max_size = 1 << 30;
            var size = 1 << (30 - origin.Level);
            int i = 0, j = 0;
            int? orientation = null;
            var face = origin.ToFaceIjOrientation(ref i, ref j, ref orientation);
            return S2CellId.FromFaceIjSame(face, i + size, j - size, j - size >= 0 && i + size < max_size).ParentForLevel(origin.Level);
        }






        static public List<S2CellId> GetListOfCellsToCheck(IEnumerable<S2CellId> cellGrid)
        {
            var dict = cellGrid.ToDictionary(x => x.Id, x => x);
            var alreadyVisited = new HashSet<ulong>();
            var result = new List<S2CellId>();
            //Center start on curve (not on the shape).
            var start = cellGrid.OrderBy(x => x.Id).Skip(cellGrid.Count() / 2).First();
            var candidates = new Queue<S2CellId>();
            candidates.Enqueue(start);
            while (candidates.Any())
            {
                var cellToCover = candidates.Dequeue();
                var tmpResult = CoverAreaAroundCell(cellToCover, dict, alreadyVisited);
                result.AddRange(tmpResult);
                foreach (var itm in tmpResult)
                    candidates.Enqueue(itm);
            }
            return result;
        }
        static public List<S2CellId> CoverAreaAroundCell(S2CellId cell, Dictionary<ulong, S2CellId> allCells, HashSet<ulong> alreadyVisited)
        {
            var result = new List<S2CellId>();
            foreach (var nCell in GetNearbyCellIds(cell).Take(5))
            {
                if (!alreadyVisited.Contains(nCell.Id) && allCells.ContainsKey(nCell.Id))
                {
                    result.Add(nCell);
                    alreadyVisited.Add(nCell.Id);
                }
            }
            return result;
        }




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

        //    def getNeighbors():
        //origin = CellId.from_lat_lng(LatLng.from_degrees(FLOAT_LAT, FLOAT_LNG)).parent(15)

        //level = 15
        //max_size = 1 << 30
        //size = origin.get_size_ij(level)

        //face, i, j = origin.to_face_ij_orientation()[0:3]

        //    walk=  [origin.Id,
        //            S2CellId.FromFaceIjSame(face, i, j - size, j - size >= 0).ParentForLevel(level).Id,
        //            S2CellId.FromFaceIjSame(face, i, j + size, j + size < max_size).ParentForLevel(level).Id,
        //            S2CellId.FromFaceIjSame(face, i - size, j, i - size >= 0).ParentForLevel(level).Id,
        //            S2CellId.FromFaceIjSame(face, i + size, j, i + size < max_size).ParentForLevel(level).Id,
        //            S2CellId.FromFaceIjSame(face, i - size, j - size, j - size >= 0 and i - size >= 0).ParentForLevel(level).Id,
        //            S2CellId.FromFaceIjSame(face, i + size, j - size, j - size >= 0 and i + size < max_size).ParentForLevel(level).Id,
        //            S2CellId.FromFaceIjSame(face, i - size, j + size, j + size < max_size and i - size >= 0).ParentForLevel(level).Id,
        //            S2CellId.FromFaceIjSame(face, i + size, j + size, j + size < max_size and i + size < max_size).ParentForLevel(level).Id]
        //        #S2CellId.FromFaceIjSame(face, i, j - 2*size, j - 2*size >= 0).ParentForLevel(level).Id,
        //        #S2CellId.FromFaceIjSame(face, i - size, j - 2*size, j - 2*size >= 0 and i - size >=0).ParentForLevel(level).Id,
        //        #S2CellId.FromFaceIjSame(face, i + size, j - 2*size, j - 2*size >= 0 and i + size < max_size).ParentForLevel(level).Id,
        //        #S2CellId.FromFaceIjSame(face, i, j + 2*size, j + 2*size < max_size).ParentForLevel(level).Id,
        //        #S2CellId.FromFaceIjSame(face, i - size, j + 2*size, j + 2*size < max_size and i - size >=0).ParentForLevel(level).Id,
        //        #S2CellId.FromFaceIjSame(face, i + size, j + 2*size, j + 2*size < max_size and i + size < max_size).ParentForLevel(level).Id,
        //        #S2CellId.FromFaceIjSame(face, i + 2*size, j, i + 2*size < max_size).ParentForLevel(level).Id,
        //        #S2CellId.FromFaceIjSame(face, i + 2*size, j - size, j - size >= 0 and i + 2*size < max_size).ParentForLevel(level).Id,
        //        #S2CellId.FromFaceIjSame(face, i + 2*size, j + size, j + size < max_size and i + 2*size < max_size).ParentForLevel(level).Id,
        //        #S2CellId.FromFaceIjSame(face, i - 2*size, j, i - 2*size >= 0).ParentForLevel(level).Id,
        //        #S2CellId.FromFaceIjSame(face, i - 2*size, j - size, j - size >= 0 and i - 2*size >=0).ParentForLevel(level).Id,
        //        #S2CellId.FromFaceIjSame(face, i - 2*size, j + size, j + size < max_size and i - 2*size >=0).ParentForLevel(level).Id]

        //return walk

        static public List<S2CellId> GetNearbyCellIds(S2CellId origin)
        {
            return new List<S2CellId>()
            {
                origin,
                origin.NorthEastCell(),
                origin.SouthEastCell(),
                origin.SouthWestCell(),
                origin.NorthWestCell(),
                origin.NorthCell(),
                origin.EastCell(),
                origin.SouthCell(),
                origin.WestCell()};
        }
        public static List<ulong> GetNearbyCellIds(double latitude, double longitude)
        {
            var origin = S2CellId.FromLatLng(S2LatLng.FromDegrees(latitude, longitude)).ParentForLevel(15);
            return GetNearbyCellIds(origin).Select(x => x.Id).ToList();
        }
        //          ]
        //# S2CellId.FromFaceIjSame(face, i, j - 2*size, j - 2*size >= 0).ParentForLevel(level).Id,
        //# S2CellId.FromFaceIjSame(face, i - size, j - 2*size, j - 2*size >= 0 and i - size >=0).ParentForLevel(level).Id,
        //# S2CellId.FromFaceIjSame(face, i + size, j - 2*size, j - 2*size >= 0 and i + size < max_size).ParentForLevel(level).Id,
        //# S2CellId.FromFaceIjSame(face, i, j + 2*size, j + 2*size < max_size).ParentForLevel(level).Id,
        //# S2CellId.FromFaceIjSame(face, i - size, j + 2*size, j + 2*size < max_size and i - size >=0).ParentForLevel(level).Id,
        //# S2CellId.FromFaceIjSame(face, i + size, j + 2*size, j + 2*size < max_size and i + size < max_size).ParentForLevel(level).Id,
        //# S2CellId.FromFaceIjSame(face, i + 2*size, j, i + 2*size < max_size).ParentForLevel(level).Id,
        //# S2CellId.FromFaceIjSame(face, i + 2*size, j - size, j - size >= 0 and i + 2*size < max_size).ParentForLevel(level).Id,
        //# S2CellId.FromFaceIjSame(face, i + 2*size, j + size, j + size < max_size and i + 2*size < max_size).ParentForLevel(level).Id,
        //# S2CellId.FromFaceIjSame(face, i - 2*size, j, i - 2*size >= 0).ParentForLevel(level).Id,
        //# S2CellId.FromFaceIjSame(face, i - 2*size, j - size, j - size >= 0 and i - 2*size >=0).ParentForLevel(level).Id,
        //# S2CellId.FromFaceIjSame(face, i - 2*size, j + size, j + size < max_size and i - 2*size >=0).ParentForLevel(level).Id
        //            };


        public static List<ulong> GetNearbyCellIdsOld(double latitude, double longitude)
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
