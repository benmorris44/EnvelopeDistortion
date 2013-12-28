using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EnvelopeDistortion.Contracts
{
    public interface IDistortion
    {
        PointF Distort(GraphicsPath source, PointF point);

    }
}
