using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkingTools.Extensions;

namespace SL.EntityFramework.Extensions
{
    public static class Geo
    {
        public static DbGeography Point(double longitude, double latitude)
        {
            var strpoint = $"POINT ({longitude.ToString(CultureInfo.InvariantCulture)} {latitude.ToString(CultureInfo.InvariantCulture)})";
            var dbGeography = DbGeography.FromText(strpoint, DbGeography.DefaultCoordinateSystemId);//4326

            //using System.Data.Spatial;

            return dbGeography;
        }

        public static DbGeography Polygon(params Pnt[] points)
        {
            //TODO: оптимизировать, расчет площади довольно тяжелый
            var first = points.First();
            var last = points.Last();

            if (first != last) points = points.Union(first).ToArray();

            var polygon = ToPolygon(points);
            var invertpolygon = ToPolygon(points.Reverse().ToArray());

            bool polygonIsValid;
            bool invertpolygonIsValid;

            var polygonArea = polygon.Get(p => p.Area);
            var invertpolygonArea = invertpolygon.Get(i => i.Area);

            if (polygonArea == null && invertpolygonArea == null)
                invertpolygonIsValid = polygonIsValid = false;
            else if (polygonArea == null && invertpolygonArea != null)
                invertpolygonIsValid = !(polygonIsValid = false);
            else if (polygonArea != null && invertpolygonArea == null)
                invertpolygonIsValid = !(polygonIsValid = true);
            else if (polygonArea < invertpolygonArea)
                invertpolygonIsValid = !(polygonIsValid = true);
            else
                invertpolygonIsValid = !(polygonIsValid = false);

            if (polygonIsValid) return polygon;
            else if (invertpolygonIsValid) return invertpolygon;
            else return null;
        }

        private static DbGeography ToPolygon(Pnt[] points)
        {
            var polygon = $"POLYGON(({str.Join(", ", points.Select(a => $"{a.Longitude.ToString(CultureInfo.InvariantCulture)} {a.Latitude.ToString(CultureInfo.InvariantCulture)}"))}))";
            return DbGeography.PolygonFromText(polygon, DbGeography.DefaultCoordinateSystemId);
        }

        public static IEnumerable<Pnt> ToPoints(DbGeography geo, bool invertPointCoordinatesInArray = false)
        {
            if (geo == null || geo.Length <= 0) 
                yield break;

            for (var i = 1; i < geo.PointCount; i++)
            {
                var p = geo.PointAt(i);

                if (p.Longitude == null || p.Latitude == null)
                    throw new Exception("не удалось получить координаты точки");

                yield return new Pnt()
                {
                    Longitude = p.Longitude.Value,
                    Latitude = p.Latitude.Value
                };
            }
        }
    }

    public class Pnt
    {
        public Pnt() { }

        public Pnt(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        public Pnt(string longitude, string latitude)
        {
            Longitude = longitude.ToDouble();
            Latitude = latitude.ToDouble();
        }

        public Pnt(string point, char seporate = ' ')
        {
            var ps = point.Split(seporate);

            Longitude = ps.Gv(0).ToDouble();
            Latitude = ps.Gv(1).ToDouble();
        }

        //Санкт-Петербург
        // Долгота (longitude) (30.41221619)
        // Широта (latitude) (60.03398758) 

        /// <summary>
        /// Долгота
        /// </summary>
        /// <remarks>- лево/право +</remarks>
        public double Longitude { get; set; }

        /// <summary>
        /// Широта
        /// </summary>
        /// <remarks>- вниз/верх +</remarks>
        public double Latitude { get; set; }

        public override string ToString() => $"{Longitude.ToString(CultureInfo.InvariantCulture)} {Latitude.ToString(CultureInfo.InvariantCulture)}";

        public override bool Equals(object obj) => !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || obj.GetType() == this.GetType() && Equals((Pnt)obj));
        protected bool Equals(Pnt other) => Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
        public override int GetHashCode() { unchecked { return (Latitude.GetHashCode() * 397) ^ Longitude.GetHashCode(); } }
        public static bool operator ==(Pnt left, Pnt right) => Equals(left, right);
        public static bool operator !=(Pnt left, Pnt right) => !Equals(left, right);
    }
}
