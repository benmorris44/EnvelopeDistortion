using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvelopeDistortion.Contracts;

namespace EnvelopeDistortion
{
    public class DistortionService
    {
        private readonly IDistortion _distortion;
        private readonly GraphicsPath _source;
        private readonly float _flatness;

        /// <summary>
        /// Creates a new instance of a Distortion Service
        /// </summary>
        /// <param name="distortion">The distortion to be performed</param>
        /// <param name="source">The graphics path to be distorted</param>
        /// <param name="flatness">The precision of the flattening operation (smaller = more points = slower)</param>
        public DistortionService(IDistortion distortion, GraphicsPath source, float flatness = .2f)
        {
            _distortion = distortion;
            _source = source;
            _flatness = flatness;
            _source.Flatten(null , flatness);
        }


        /// <summary>
        /// returns a newly created graphics path with points distorted
        /// </summary>
        /// <returns>The distorted Graphics Path</returns>
        public GraphicsPath ApplyDistortion()
        {
            var it = new GraphicsPathIterator(_source);
            it.Rewind();
            var Gp = new GraphicsPath(FillMode.Winding);
            var ReturnPath = new GraphicsPath(FillMode.Winding);

            for (var i = 0; i < it.SubpathCount; i++)
            {
                bool result;
                it.NextSubpath(Gp, out result);

                InjectPrecisionPoints(Gp);

                ReturnPath.AddPolygon(Gp.PathPoints.Select(p=>_distortion.Distort(_source, p)).ToArray());
                Gp.Reset();
            }
            it.Dispose();
            Gp.Dispose();

            return ReturnPath;
        }

        private void InjectPrecisionPoints(GraphicsPath gp)
        {
            var InsertDictionary = new Dictionary<int, PointF[]>();
            //inject points on vertical and horizontal runs to increase precision
            for (var j = 0; j < gp.PointCount; j++)
            {
                PointF CurrentPoint;
                PointF NextPoint;
                if (j != gp.PointCount - 1)
                {
                    CurrentPoint = gp.PathPoints[j];
                    NextPoint = gp.PathPoints[j + 1];
                }
                else
                {
                    CurrentPoint = gp.PathPoints[j];
                    NextPoint = gp.PathPoints[0];
                }
                if (Math.Abs(CurrentPoint.X - NextPoint.X) < .001 && Math.Abs(CurrentPoint.Y - NextPoint.Y) > _flatness)
                {
                    var Distance = CurrentPoint.Y - NextPoint.Y;
                    var Items = Enumerable.Range(1, Convert.ToInt32(Math.Floor(Math.Abs(Distance)/_flatness)))
                                           .Select(p => new PointF(CurrentPoint.X, Distance < 0 ? (CurrentPoint.Y + (_flatness * p)) : (CurrentPoint.Y - (_flatness * p))))
                                           .ToArray();
                    InsertDictionary.Add(j + 1, Items);
                }
                if (Math.Abs(CurrentPoint.Y - NextPoint.Y) < .001 && Math.Abs(CurrentPoint.X - NextPoint.X) > _flatness)
                {
                    var Distance = CurrentPoint.X - NextPoint.X;
                    var Items =  Enumerable.Range(1, Convert.ToInt32(Math.Floor(Math.Abs(Distance)/_flatness)))
                                           .Select(p => new PointF(Distance < 0 ? (CurrentPoint.X + (_flatness * p)) : (CurrentPoint.X - (_flatness * p)), CurrentPoint.Y))
                                           .ToArray();

                    InsertDictionary.Add(j + 1, Items);
                }
            }

            if (InsertDictionary.Count > 0)
            {
                var PointArray = gp.PathPoints.ToList();
                InsertDictionary.OrderByDescending(p => p.Key).ToList().ForEach(p => PointArray.InsertRange(p.Key, p.Value));

                gp.Reset();
                gp.AddPolygon(PointArray.ToArray());

                InsertDictionary.Clear();
            }
        }

        /// <summary>
        /// A debugging method - will return the corresponding distorted point for a given source
        /// </summary>
        /// <param name="point">The source point</param>
        /// <returns>The distorted point location</returns>
        public PointF DistortPoint(PointF point)
        {
            return _distortion.Distort(_source, point);
        }
    }
}
