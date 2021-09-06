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
using STROOP.Map.Map3D;

namespace STROOP.Map
{
    public class MapObjectAggregatedPath : MapObject
    {
        public MapObjectAggregatedPath()
            : base()
        {
        }

        public override void DrawOn2DControlTopDownView(MapObjectHoverData hoverData)
        {
            List<MapObjectPath> paths = new List<MapObjectPath>();
            foreach (MapTracker mapTracker in Config.MapGui.flowLayoutPanelMapTrackers.Controls)
            {
                paths.AddRange(mapTracker.GetMapPathObjects());
            }
            List<List<MapObjectPathSegment>> segmentLists = paths.ConvertAll(path => path.GetSegments());
            if (segmentLists.Count == 0) return;
            int maxCount = segmentLists.Max(list => list.Count);

            GL.BindTexture(TextureTarget.Texture2D, -1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            for (int i = 0; i < maxCount; i++)
            {
                foreach (List<MapObjectPathSegment> segmentList in segmentLists)
                {
                    if (i >= segmentList.Count) continue;
                    MapObjectPathSegment segment = segmentList[i];
                    GL.LineWidth(segment.LineWidth);
                    GL.Color4(segment.Color.R, segment.Color.G, segment.Color.B, segment.Opacity);
                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex2(segment.StartX, segment.StartZ);
                    GL.Vertex2(segment.EndX, segment.EndZ);
                    GL.End();
                }
            }
            GL.Color4(1, 1, 1, 1.0f);
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
            return "Aggregated Path";
        }

        public override Image GetInternalImage()
        {
            return Config.ObjectAssociations.AggregatedPathImage;
        }

        public override MapDrawType GetDrawType()
        {
            return MapDrawType.Perspective;
        }
    }
}
