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

namespace STROOP.Map
{
    public abstract class MapObjectIconRectangle : MapObjectIcon
    {
        public MapObjectIconRectangle()
            : base()
        {
        }

        public override void DrawOn2DControlTopDownView(MapObjectHoverData hoverData)
        {
            List<(PointF loc, SizeF size)> dimensions = GetDimensions();
            float angle = InternalRotates ? MapUtilities.ConvertAngleForControl(0) : 0; 
            foreach ((PointF loc, SizeF size) in dimensions)
            {
                MapUtilities.DrawTexture(TextureId, loc, size, angle, Opacity);
            }
        }

        public override void DrawOn2DControlOrthographicView()
        {
            List<(PointF loc, SizeF size)> dimensions = GetDimensions();
            if (InternalRotates) return;
            foreach ((PointF loc, SizeF size) in dimensions)
            {
                MapUtilities.DrawTexture(TextureId, loc, size, 0, Opacity);
            }
        }

        protected abstract List<(PointF loc, SizeF size)> GetDimensions();
    }
}
