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
using STROOP.Models;
using System.Windows.Forms;
using System.Xml.Linq;

namespace STROOP.Map
{
    public class MapObjectObjectFloor : MapObjectFloor
    {
        private readonly PositionAngle _posAngle;
        private bool _autoUpdate;
        private List<TriangleDataModel> _tris;
        private ToolStripMenuItem _itemAutoUpdate;

        public MapObjectObjectFloor(PositionAngle posAngle)
            : base()
        {
            _posAngle = posAngle;
            _autoUpdate = true;
            _tris = new List<TriangleDataModel>();
        }

        protected override List<TriangleDataModel> GetUnfilteredTriangles()
        {
            if (_autoUpdate)
            {
                _tris = TriangleUtilities.GetObjectTrianglesForObject(_posAngle.GetObjAddress())
                    .FindAll(tri => tri.IsFloor());
            }
            return _tris;
        }

        public override string GetName()
        {
            return "Floor Tris for " + _posAngle.GetMapName();
        }

        public override Image GetInternalImage()
        {
            return Config.ObjectAssociations.TriangleFloorImage;
        }

        public override ContextMenuStrip GetContextMenuStrip()
        {
            if (_contextMenuStrip == null)
            {
                _contextMenuStrip = new ContextMenuStrip();

                _itemAutoUpdate = new ToolStripMenuItem("Auto Update");
                _itemAutoUpdate.Click += (sender, e) =>
                {
                    MapObjectSettings settings = new MapObjectSettings(
                        changeAutoUpdate: true, newAutoUpdate: !_autoUpdate);
                    GetParentMapTracker().ApplySettings(settings);
                };
                _itemAutoUpdate.Checked = _autoUpdate;
                _contextMenuStrip.Items.Add(_itemAutoUpdate);
                _contextMenuStrip.Items.Add(new ToolStripSeparator());

                GetFloorToolStripMenuItems().ForEach(item => _contextMenuStrip.Items.Add(item));
                _contextMenuStrip.Items.Add(new ToolStripSeparator());
                GetHorizontalTriangleToolStripMenuItems().ForEach(item => _contextMenuStrip.Items.Add(item));
                _contextMenuStrip.Items.Add(new ToolStripSeparator());
                GetTriangleToolStripMenuItems().ForEach(item => _contextMenuStrip.Items.Add(item));
            }

            return _contextMenuStrip;
        }

        public override void ApplySettings(MapObjectSettings settings)
        {
            base.ApplySettings(settings);

            if (settings.ChangeAutoUpdate)
            {
                _autoUpdate = settings.NewAutoUpdate;
                _itemAutoUpdate.Checked = settings.NewAutoUpdate;
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
