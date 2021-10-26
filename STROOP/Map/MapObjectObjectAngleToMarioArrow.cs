﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using OpenTK;
using System.Windows.Forms;
using System.Xml.Linq;

namespace STROOP.Map
{
    public class MapObjectObjectAngleToMarioArrow : MapObjectArrow
    {
        private readonly PositionAngle _posAngle;

        public MapObjectObjectAngleToMarioArrow(PositionAngle posAngle)
            : base()
        {
            _posAngle = posAngle;
        }

        public override PositionAngle GetPositionAngle()
        {
            return _posAngle;
        }

        protected override double GetYaw()
        {
            return Config.Stream.GetUShort(_posAngle.GetObjAddress() + ObjectConfig.AngleToMarioOffset);
        }

        protected override double GetPitch()
        {
            return -1 * Config.Stream.GetShort(_posAngle.GetObjAddress() + ObjectConfig.PitchFacingOffset);
        }

        protected override double GetRecommendedSize()
        {
            return Config.Stream.GetFloat(_posAngle.GetObjAddress() + ObjectConfig.DistanceToMarioOffset);
        }

        public override string GetName()
        {
            return "Object Angle to Mario Arrow for " + _posAngle.GetMapName();
        }

        public override List<XAttribute> GetXAttributes()
        {
            return new List<XAttribute>()
            {
                new XAttribute("positionAngle", _posAngle),
            };
        }
    }
}
