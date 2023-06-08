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
using System.Windows.Forms;

namespace STROOP.Map
{
    public class MapObjectBounds : MapObject
    {
        public static MapObjectBounds LAST_INSTANCE = null;

        private int _blueCircleTex = -1;
        private int _lastHoveredPointIndex = 0;

        private List<(float x, float z)> _points =
            new List<(float x, float z)>()
            {
                (-1000, -1000),
                (-1000, 1000),
                (1000, 1000),
                (1000, -1000),
            };

        public MapObjectBounds()
            : base()
        {
            Size = 15;
            Opacity = 0.25;
            Color = Color.Magenta;
            LineWidth = 3;

            LAST_INSTANCE = this;
        }

        public override void DrawOn2DControlTopDownView(MapObjectHoverData hoverData)
        {
            List<(float x, float z)> dataForControl =
                _points.ConvertAll(d => MapUtilities.ConvertCoordsForControlTopDownView(d.x, d.z, UseRelativeCoordinates));

            GL.BindTexture(TextureTarget.Texture2D, -1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Draw quad
            GL.Begin(PrimitiveType.Quads);
            foreach (var d in dataForControl)
            {
                GL.Color4(Color.R, Color.G, Color.B, OpacityByte);
                GL.Vertex2(d.x, d.z);
            }
            GL.End();

            // Draw outline
            if (LineWidth != 0)
            {
                GL.Color4(LineColor.R, LineColor.G, LineColor.B, (byte)255);
                GL.LineWidth(LineWidth);
                GL.Begin(PrimitiveType.LineLoop);
                foreach (var d in dataForControl)
                {
                    GL.Vertex2(d.x, d.z);
                }
                GL.End();
            }

            GL.Color4(1, 1, 1, 1.0f);

            for (int i = _points.Count - 1; i >= 0; i--)
            {
                var dataPoint = _points[i];
                (float x, float z) = dataPoint;
                (float x, float z) positionOnControl = MapUtilities.ConvertCoordsForControlTopDownView(x, z, UseRelativeCoordinates);
                float angleDegrees = 0;
                SizeF size = MapUtilities.ScaleImageSizeForControl(Config.ObjectAssociations.BlueCircleMapImage.Size, Size, Scales);
                PointF point = new PointF(positionOnControl.x, positionOnControl.z);
                double opacity = 1;
                if (this == hoverData?.MapObject && i == hoverData?.Index)
                {
                    _lastHoveredPointIndex = i;
                    opacity = MapUtilities.GetHoverOpacity();
                }
                MapUtilities.DrawTexture(_blueCircleTex, point, size, angleDegrees, opacity);
            }
        }

        public override (double x, double y, double z)? GetDragPosition()
        {
            var point = _points[_lastHoveredPointIndex];
            return (point.x, 0, point.z);
        }

        public override void SetDragPositionTopDownView(double? x = null, double? y = null, double? z = null)
        {
            if (x.HasValue)
            {
                if (_lastHoveredPointIndex == 0 || _lastHoveredPointIndex == 1)
                {
                    _points[0] = ((float)x.Value, _points[0].z);
                    _points[1] = ((float)x.Value, _points[1].z);
                }
                else
                {
                    _points[2] = ((float)x.Value, _points[2].z);
                    _points[3] = ((float)x.Value, _points[3].z);
                }
            }

            if (z.HasValue)
            {
                if (_lastHoveredPointIndex == 0 || _lastHoveredPointIndex == 3)
                {
                    _points[0] = (_points[0].x, (float)z.Value);
                    _points[3] = (_points[3].x, (float)z.Value);
                }
                else
                {
                    _points[1] = (_points[1].x, (float)z.Value);
                    _points[2] = (_points[2].x, (float)z.Value);
                }
            }
        }

        public override void DrawOn2DControlOrthographicView(MapObjectHoverData hoverData)
        {
            // do nothing
        }

        public override void DrawOn3DControl()
        {
            // do nothing
        }

        public override string GetName()
        {
            return "Bounds";
        }

        public override Image GetInternalImage()
        {
            return Config.ObjectAssociations.WatersImage;
        }

        public override MapDrawType GetDrawType()
        {
            return MapDrawType.Perspective;
        }

        public override void Update()
        {
            if (_blueCircleTex == -1)
            {
                _blueCircleTex = MapUtilities.LoadTexture(
                    Config.ObjectAssociations.BlueCircleMapImage as Bitmap);
            }
        }

        public override MapObjectHoverData GetHoverDataTopDownView(bool isForObjectDrag, bool forceCursorPosition)
        {
            Point? relPosMaybe = MapObjectHoverData.GetPositionMaybe(isForObjectDrag, forceCursorPosition);
            if (!relPosMaybe.HasValue) return null;
            Point relPos = relPosMaybe.Value;

            for (int i = _points.Count - 1; i >= 0; i--)
            {
                var point = _points[i];
                (float controlX, float controlZ) = MapUtilities.ConvertCoordsForControlTopDownView(point.x, point.z, UseRelativeCoordinates);
                double dist = MoreMath.GetDistanceBetween(controlX, controlZ, relPos.X, relPos.Y);
                double radius = Scales ? Size * Config.CurrentMapGraphics.MapViewScaleValue : Size;
                if (dist <= radius || (forceCursorPosition && _lastHoveredPointIndex == i))
                {
                    return new MapObjectHoverData(this, MapObjectHoverDataEnum.Icon, point.x, 0, point.z, index: i);
                }
            }
            return null;
        }

        public float GetXMin()
        {
            return _points.Min(p => p.x);
        }

        public float GetXMax()
        {
            return _points.Max(p => p.x);
        }

        public float GetZMin()
        {
            return _points.Min(p => p.z);
        }

        public float GetZMax()
        {
            return _points.Max(p => p.z);
        }
    }
}
