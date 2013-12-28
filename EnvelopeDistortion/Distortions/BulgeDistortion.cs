using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using ClipperLib;
using EnvelopeDistortion.Contracts;
using EnvelopeDistortion.Utility;

namespace EnvelopeDistortion.Distortions
{
    public class BulgeDistortion : DistortionBase, IDistortion
    {
        private GraphicsPath _distortionPath;
        private List<List<IntPoint>> _distortionPoints; 
        private RectangleF _distortionBounds;
        private RectangleF _sourceBounds;
        private PointF _upperLeft;
        private PointF _upperRight;
        private PointF _lowerLeft;
        private PointF _lowerRight;
        private readonly Dictionary<float, PointF[]> _boundCache = new Dictionary<float, PointF[]>(); 

        public BulgeDistortion()
        {
            
        }

        public PointF Distort(GraphicsPath source, PointF point)
        {
            if (_distortionPath == null)
            {
                BuildDistortion(source);
            }

            var ScaledPoint = point;

            PointF UpperBoundPoint;
            PointF LowerBoundPoint;
            
            GetBoundedPoints(out UpperBoundPoint, out LowerBoundPoint, point);
            var Y = UpperBoundPoint.Y + (((ScaledPoint.Y - _sourceBounds.Top) / _sourceBounds.Height) * Math.Abs(UpperBoundPoint.Y - LowerBoundPoint.Y));

            return new PointF(ScaledPoint.X, Y);
        }

        private void GetBoundedPoints(out PointF upperBoundPoint, out PointF lowerBoundPoint, PointF source)
        {

            if (_boundCache.ContainsKey(source.X))
            {
                upperBoundPoint = _boundCache[source.X][0];
                lowerBoundPoint = _boundCache[source.X][1];
                return;
            }

            var Path = new GraphicsPath();
            var UpperX = source.X * (_sourceBounds.Width / (_upperRight.X -_upperLeft.X));
            var LowerX = source.X * (_sourceBounds.Width / (_lowerRight.X -_lowerLeft.X));
            Path.AddPolygon(new PointF[]{
                new PointF(_distortionBounds.Left,_distortionBounds.Bottom),
                new PointF(_distortionBounds.Left, _distortionBounds.Top),
                new PointF(UpperX,  _distortionBounds.Top),
                new PointF(LowerX, _distortionBounds.Bottom), 
            });
            Path.CloseFigure();
            var ClippingPath = ClipperUtility.ConvertToClipperPolygons(Path);
            Path.Dispose();

            var ClippedPath = ClipperUtility.Clip(ClippingPath, _distortionPoints);
            if (Math.Abs(source.X - _sourceBounds.Left) < .1 || Math.Abs(source.X - _sourceBounds.Right) < .1)
            {
                upperBoundPoint = new PointF(_sourceBounds.Left, _sourceBounds.Top);
                lowerBoundPoint = new PointF(_sourceBounds.Left, _sourceBounds.Bottom );
            }
            else
            {
                var Points = ClippedPath.PathPoints;
                var QuickBounded = Points.Where(p => Math.Abs(p.X - LowerX) < .01);
                if (QuickBounded.Any())
                {
                    upperBoundPoint = Points.Where(p => Math.Abs(p.X - LowerX) < .01).OrderBy(p => p.Y).First();
                    lowerBoundPoint = Points.Where(p => Math.Abs(p.X - LowerX) < .01).OrderByDescending(p => p.Y).First();
                    _boundCache.Add(source.X, new PointF[] { upperBoundPoint, lowerBoundPoint });
                }
                else
                {
                    var RightMostPoints = Points.OrderByDescending(p => p.X).Take(2).ToList();
                    upperBoundPoint = RightMostPoints.OrderBy(p => p.Y).First();
                    lowerBoundPoint = RightMostPoints.OrderByDescending(p => p.Y).First();
                }
                ClippedPath.Dispose();
            }

        }

        private void BuildDistortion(GraphicsPath source)
        {
            _sourceBounds = source.GetBounds();

            _distortionPath = new GraphicsPath(source.FillMode);

            _lowerLeft = new PointF(_sourceBounds.Left, _sourceBounds.Bottom);
            _lowerRight = new PointF(_sourceBounds.Right, _sourceBounds.Bottom);
            _upperLeft = new PointF(_sourceBounds.Left, _sourceBounds.Top);
            _upperRight = new PointF(_sourceBounds.Right, _sourceBounds.Top);

            _distortionPath.AddLine(_lowerLeft, _upperLeft);

            _distortionPath.AddBezier(_upperLeft,
                                        new PointF(_sourceBounds.Left, _sourceBounds.Top + ((_sourceBounds.Height * (float)Intensity)) * -1),
                                        new PointF(_sourceBounds.Right, _sourceBounds.Top + ((_sourceBounds.Height * (float)Intensity)) * -1),
                                        _upperRight);

            _distortionPath.AddLine(_upperRight, _lowerRight);

            _distortionPath.AddBezier(_lowerRight,
                                        new PointF(_sourceBounds.Right, _sourceBounds.Bottom + (_sourceBounds.Height * (float)Intensity)),
                                        new PointF(_sourceBounds.Left, _sourceBounds.Bottom + (_sourceBounds.Height * (float)Intensity)),
                                    _lowerLeft);

            _distortionPath.Flatten();
            _distortionPoints = ClipperUtility.ConvertToClipperPolygons(_distortionPath);
            _distortionBounds = _distortionPath.GetBounds();
        }
    }
}
