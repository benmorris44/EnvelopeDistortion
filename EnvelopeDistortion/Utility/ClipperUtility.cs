using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using ClipperLib;

namespace EnvelopeDistortion.Utility
{
    internal class ClipperUtility
    {
        private const int Scale = 1000;

        public static GraphicsPath unionTwo(ref GraphicsPath p1, ref GraphicsPath p2)
        {
            List<List<IntPoint>> polygonB = ConvertToClipperPolygons(p1);
            var polygons = new List<List<IntPoint>>();
            List<List<IntPoint>> polygonA = ConvertToClipperPolygons(p2);

            var c = new Clipper();
            c.AddPolygons(polygonB, PolyType.ptSubject);
            c.AddPolygons(polygonA, PolyType.ptClip);
            c.Execute(ClipType.ctUnion, polygons, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            return ConvertClipperToGraphicsPath(polygons);
        }

        internal static GraphicsPath Clip(List<List<IntPoint>> clippingPath, List<List<IntPoint>> distortionPoints)
        {
            var c = new Clipper();
            c.AddPolygons(distortionPoints, PolyType.ptSubject);
            c.AddPolygons(clippingPath, PolyType.ptClip);
            var polygons = new List<List<IntPoint>>();
            c.Execute(ClipType.ctIntersection, polygons, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            return ConvertClipperToGraphicsPath(polygons);
        }

        public static GraphicsPath ConvertClipperToGraphicsPath(List<List<IntPoint>> path)
        {
            var returnPath = new GraphicsPath();

            foreach (List<IntPoint> intPoints in path)
            {
                returnPath.AddPolygon(ToPointList(intPoints).ToArray());
            }
            return returnPath;
        }

        private static List<PointF> ToPointList(IEnumerable<IntPoint> pointList)
        {
            return pointList.Select(pt => new PointF((pt.X/(float) Scale), (pt.Y/(float) Scale))).ToList();
        }

        public static List<List<IntPoint>> ConvertToClipperPolygons(GraphicsPath path)
        {
            var Polygon = new List<IntPoint>();
            var Polygons = new List<List<IntPoint>>();

            var it = new GraphicsPathIterator(path);
            it.Rewind();
            for (int i = 0; i < it.SubpathCount; i++)
            {
                bool isClosed;
                int startIndex;
                int endIndex;
                it.NextSubpath(out startIndex, out endIndex, out isClosed);
                Polygon.AddRange(
                    path.PathPoints
                        .Skip(startIndex)
                        .Take((endIndex - startIndex) + 1)
                        .Select(x => new IntPoint(Convert.ToInt64(x.X*Scale), Convert.ToInt64(x.Y*Scale)))
                    );
                Polygons.Add(new List<IntPoint>(Polygon));
                Polygon.Clear();
            }
            it.Dispose();
            return Polygons;
        }


    }
}