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
using System.Xml.Linq;

namespace STROOP.Map
{
    public class MapObjectSector : MapObject
    {
        private readonly PositionAngle _posAngle;
        private float _angleRadius;

        private ToolStripMenuItem _itemSetAngleRadius;

        private static readonly string SET_ANGLE_RADIUS_TEXT = "Set Angle Radius";

        public MapObjectSector(PositionAngle posAngle)
            : base()
        {
            _posAngle = posAngle;
            _angleRadius = 4096;

            Size = 1000;
            Opacity = 0.5;
            Color = Color.Yellow;
        }

        public override void DrawOn2DControlTopDownView(MapObjectHoverData hoverData)
        {
            List<(float centerX, float centerZ, float radius, float angle, float angleRadius)> dimenstionList = GetDimensions();

            foreach ((float centerX, float centerZ, float radius, float angle, float angleRadius) in dimenstionList)
            {
                (float controlCenterX, float controlCenterZ) = MapUtilities.ConvertCoordsForControlTopDownView(centerX, centerZ);
                float controlAngle = angle + 32768 - Config.CurrentMapGraphics.MapViewYawValue;
                float controlRadius = radius * Config.CurrentMapGraphics.MapViewScaleValue;
                List<(float pointX, float pointZ)> outerPoints = Enumerable.Range(0, SpecialConfig.MapCircleNumPoints2D + 1).ToList()
                    .ConvertAll(index => (index - SpecialConfig.MapCircleNumPoints2D / 2) / (float)(SpecialConfig.MapCircleNumPoints2D / 2))
                    .ConvertAll(proportion => controlAngle + proportion * angleRadius)
                    .ConvertAll(ang => ((float, float))MoreMath.AddVectorToPoint(controlRadius, ang, controlCenterX, controlCenterZ));

                GL.BindTexture(TextureTarget.Texture2D, -1);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();

                // Draw circle
                GL.Color4(Color.R, Color.G, Color.B, OpacityByte);
                GL.Begin(PrimitiveType.TriangleFan);
                GL.Vertex2(controlCenterX, controlCenterZ);
                foreach ((float x, float z) in outerPoints)
                {
                    GL.Vertex2(x, z);
                }
                GL.End();

                // Draw outline
                if (LineWidth != 0)
                {
                    GL.Color4(LineColor.R, LineColor.G, LineColor.B, (byte)255);
                    GL.LineWidth(LineWidth);
                    GL.Begin(PrimitiveType.LineLoop);
                    GL.Vertex2(controlCenterX, controlCenterZ);
                    foreach ((float x, float z) in outerPoints)
                    {
                        GL.Vertex2(x, z);
                    }
                    GL.Vertex2(controlCenterX, controlCenterZ);
                    GL.End();
                }
            }

            GL.Color4(1, 1, 1, 1.0f);
        }

        public override void DrawOn2DControlOrthographicView()
        {
            // do nothing
        }

        public override void DrawOn3DControl()
        {
            // do nothing
        }

        protected List<(float centerX, float centerZ, float radius, float angle, float angleRadius)> GetDimensions()
        {
            (double x, double y, double z, double angle) = _posAngle.GetValues();
            return new List<(float centerX, float centerZ, float radius, float angle, float angleRadius)>()
            {
                ((float)x, (float)z, Size, (float)angle, _angleRadius)
            };
        }

        public override MapDrawType GetDrawType()
        {
            return MapDrawType.Perspective;
        }

        public override Image GetInternalImage()
        {
            return Config.ObjectAssociations.SectorImage;
        }

        public override string GetName()
        {
            return "Sector for " + _posAngle.GetMapName();
        }

        public override ContextMenuStrip GetContextMenuStrip()
        {
            if (_contextMenuStrip == null)
            {
                string suffix = string.Format(" ({0})", _angleRadius);
                _itemSetAngleRadius = new ToolStripMenuItem(SET_ANGLE_RADIUS_TEXT + suffix);
                _itemSetAngleRadius.Click += (sender, e) =>
                {
                    string text = DialogUtilities.GetStringFromDialog(labelText: "Enter the angle radius for sector:");
                    float? angleRadius = ParsingUtilities.ParseFloatNullable(text);
                    if (!angleRadius.HasValue) return;
                    MapObjectSettings settings = new MapObjectSettings(
                        changeSectorAngleRadius: true, newSectorAngleRadius: angleRadius.Value);
                    GetParentMapTracker().ApplySettings(settings);
                };

                _contextMenuStrip = new ContextMenuStrip();
                _contextMenuStrip.Items.Add(_itemSetAngleRadius);
            }

            return _contextMenuStrip;
        }

        public override void ApplySettings(MapObjectSettings settings)
        {
            base.ApplySettings(settings);

            if (settings.ChangeSectorAngleRadius)
            {
                _angleRadius = settings.NewSectorAngleRadius;
                string suffix = string.Format(" ({0})", _angleRadius);
                _itemSetAngleRadius.Text = SET_ANGLE_RADIUS_TEXT + suffix;
            }
        }

        public override PositionAngle GetPositionAngle()
        {
            return _posAngle;
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
