using MandraSoft.PokemonGo.Models.WebModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Api.ScanningAlgorithm
{
    static public class Scanner
    {
        static private double GetEarthRadius(double latrad)
        {
            return Math.Pow((1.0 / (Math.Pow(((Math.Cos(latrad)) / EARTH_Rmax), 2) + Math.Pow(((Math.Sin(latrad)) / EARTH_Rmin), 2))), (1.0 / 2));
        }
        static private double safety = 1.0;

        static private double EARTH_Rmax = 6378137.0;
        static private double EARTH_Rmin = 6356752.3;
        static private double HEX_R = 100.0;//range of detection for pokemon = 100m
        static private double HEX_M = Math.Pow(3.0, 0.5) / 2.0 * HEX_R;
        static private double ALT_C = 5;
        static public List<LatLng> GetPointsToScan(LatLng loc, int? hexNum = null)
        {
            var lat = loc.lat;
            var lng = loc.lng;
            double FLOAT_LAT = 0;
            double FLOAT_LNG = 0;
            var cellResponse = new CellResponse();
            int wID = 0;
            int HEX_NUM = hexNum.HasValue ? hexNum.Value : 20;
            var latrad = lat * Math.PI / 180;
            var ab = (HEX_NUM + 0.5);
            var x_un = 1.5 * HEX_R / GetEarthRadius(latrad) / Math.Cos(latrad) * safety * ab * 180 / Math.PI;
            var y_un = 3.0 * HEX_M / GetEarthRadius(latrad) * safety * ab * 180 / Math.PI;
            var xmod = new int[] { 0, 1, 2, 1, -1, -2, -1 };
            var ymod = new int[] { 0, -1, 0, 1, 1, 0, -1 };
            lat = lat + ymod[wID] * y_un;
            lng = lng + xmod[wID] * x_un;
            FLOAT_LAT = lat;
            FLOAT_LNG = lng;
            cellResponse.Centers.Add(new LatLng() { lat = lat, lng = lng });
            latrad = lat * Math.PI / 180;
            int maxR = 1;
            for (var a = 1; a < HEX_NUM + 1; a++)
                for (var i = 0; i < (a * 6); i++)
                {
                    x_un = 1.5 * HEX_R / GetEarthRadius(latrad) / Math.Cos(latrad) * safety * 180 / Math.PI;
                    y_un = 1.0 * HEX_M / GetEarthRadius(latrad) * safety * 180 / Math.PI;
                    if (i < a)
                    {
                        lat = FLOAT_LAT + y_un * (-2 * a + i);
                        lng = FLOAT_LNG + x_un * i;
                    }
                    else if (i < 2 * a)
                    {
                        lat = FLOAT_LAT + y_un * (-3 * a + 2 * i);
                        lng = FLOAT_LNG + x_un * a;
                    }
                    else if (i < 3 * a)
                    {
                        lat = FLOAT_LAT + y_un * (-a + i);
                        lng = FLOAT_LNG + x_un * (3 * a - i);
                    }
                    else if (i < 4 * a)
                    {
                        lat = FLOAT_LAT + y_un * (5 * a - i);
                        lng = FLOAT_LNG + x_un * (3 * a - i);
                    }
                    else if (i < 5 * a)
                    {
                        lat = FLOAT_LAT + y_un * (9 * a - 2 * i);
                        lng = FLOAT_LNG + x_un * -a;
                    }
                    else
                    {
                        lat = FLOAT_LAT + y_un * (4 * a - i);
                        lng = FLOAT_LNG + x_un * (-6 * a + i);
                    }
                    cellResponse.Centers.Add(new LatLng() { lat = lat, lng = lng });
                    maxR += 1;
                }
            return cellResponse.Centers;
        }
    }
}
