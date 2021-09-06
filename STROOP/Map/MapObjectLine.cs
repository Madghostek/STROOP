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
using System.Drawing.Imaging;
using STROOP.Map.Map3D;
using System.Windows.Forms;

namespace STROOP.Map
{
    public abstract class MapObjectLine : MapObject
    {
        public MapObjectLine()
            : base()
        {
        }

        public override void DrawOn2DControlTopDownView(MapObjectHoverData hoverData)
        {
            MapUtilities.DrawLinesOn2DControlTopDownView(GetVerticesTopDownView(), LineWidth, LineColor, OpacityByte);
        }

        public override void DrawOn2DControlOrthographicView(MapObjectHoverData hoverData)
        {
            MapUtilities.DrawLinesOn2DControlOrthographicView(GetVerticesOrthographicView(), LineWidth, LineColor, OpacityByte);
        }

        public override void DrawOn3DControl()
        {
            MapUtilities.DrawLinesOn3DControl(GetVertices3D(), LineWidth, LineColor, OpacityByte, GetModelMatrix());
        }

        protected abstract List<(float x, float y, float z)> GetVerticesTopDownView();

        protected virtual List<(float x, float y, float z)> GetVerticesOrthographicView()
        {
            return GetVerticesTopDownView();
        }

        protected virtual List<(float x, float y, float z)> GetVertices3D()
        {
            return GetVerticesTopDownView();
        }

        public override MapDrawType GetDrawType()
        {
            return MapDrawType.Perspective;
        }
    }
}
