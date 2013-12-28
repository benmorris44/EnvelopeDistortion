using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using EnvelopeDistortion.Distortions;

namespace EnvelopeDistortionWinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            var _source = new GraphicsPath();
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; 
            var stringFormat = new StringFormat
                                        {
                                            Alignment = StringAlignment.Near,
                                            LineAlignment = StringAlignment.Near,
                                            Trimming = StringTrimming.Character
                                        };

            _source.AddString("LoneTechie", new FontFamily("Arial"), (int)FontStyle.Regular, 200, new Point(100, 150), stringFormat);
            //e.Graphics.FillPath(Brushes.Black, _source);

            var _sourceBounds = _source.GetBounds();

            var _distortionPath = new GraphicsPath();
            const double intensity = 1;

            //*************
            //this portion is purely for debugging purposes and is copied directly out of BulgeDistortion()
            //allows you to visually verify the envelope distortion that is generated from the given intensity
            var _lowerLeft = new PointF(_sourceBounds.Left, _sourceBounds.Bottom);
            var _lowerRight = new PointF(_sourceBounds.Right, _sourceBounds.Bottom);
            var _upperLeft = new PointF(_sourceBounds.Left, _sourceBounds.Top);
            var _upperRight = new PointF(_sourceBounds.Right, _sourceBounds.Top);

            _distortionPath.AddLine(_lowerLeft, _upperLeft);

            _distortionPath.AddBezier(_upperLeft,
                                        new PointF(_sourceBounds.Left, _sourceBounds.Top + ((_sourceBounds.Height * (float)intensity)) * -1),
                                        new PointF(_sourceBounds.Right, _sourceBounds.Top + ((_sourceBounds.Height * (float)intensity)) * -1),
                                        _upperRight);

            _distortionPath.AddLine(_upperRight, _lowerRight);

            _distortionPath.AddBezier(_lowerRight,
                                        new PointF(_sourceBounds.Right, _sourceBounds.Bottom + (_sourceBounds.Height * (float)intensity)),
                                        new PointF(_sourceBounds.Left, _sourceBounds.Bottom + (_sourceBounds.Height * (float)intensity)),
                                    _lowerLeft);

            _distortionPath.Flatten();
            e.Graphics.DrawPath(new Pen(Brushes.Green), _distortionPath );
            //*************

            //simply draw a rectangle around the source
            e.Graphics.DrawRectangle(new Pen(Brushes.HotPink), Rectangle.Truncate(_sourceBounds) );

            //create an instance of the distortion service and feed in the source path
            var Distort = new EnvelopeDistortion.DistortionService(new BulgeDistortion() { Intensity = intensity }, _source);
            
            //draw a grid displaying the transformed points
            const int stepX = 10;
            const int stepY = 5;
            for (int i = 0; i <= _sourceBounds.Width / stepX; i++)
            {
                var X = i * stepX;

                for (int j = 0; j <= _sourceBounds.Height / stepY; j++)
                {
                    var Y = j * stepY;
                    var Point = Distort.DistortPoint(new PointF(X + _sourceBounds.Left, Y + _sourceBounds.Top));
                    e.Graphics.FillEllipse(new SolidBrush(Color.Red), Point.X - 1, Point.Y - 1, 2, 2);
                }
            }

            //draw the new transformed path
            e.Graphics.FillPath(Brushes.Black, Distort.ApplyDistortion());
        }
    }
}
