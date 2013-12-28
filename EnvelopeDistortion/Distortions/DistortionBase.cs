using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvelopeDistortion.Enums;

namespace EnvelopeDistortion.Distortions
{
    public class DistortionBase
    {
        /// <summary>
        /// The direction the effect will be applied, for example Buldge vertical will apply to top and bottom only
        /// </summary>
        public DistortionDirection Direction { get; set; }
        /// <summary>
        /// The intensity of the effect can be positive or negative
        /// intensity factor is based on the relative size of the source
        /// </summary>
        public double Intensity { get; set; }
    }
}
