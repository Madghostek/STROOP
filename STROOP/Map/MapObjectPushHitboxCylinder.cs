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
using System.Xml.Linq;
using System.Windows.Forms;

namespace STROOP.Map
{
    public class MapObjectPushHitboxCylinder : MapObjectCylinder
    {
        private readonly PositionAngle _posAngle;

        public MapObjectPushHitboxCylinder(PositionAngle posAngle)
            : base()
        {
            _posAngle = posAngle;

            Color = Color.Orange;
        }

        protected override List<(float centerX, float centerZ, float radius, float minY, float maxY, Color color)> Get3DDimensions()
        {
            uint objAddress = _posAngle.GetObjAddress();
            ObjectDataModel obj = new ObjectDataModel(objAddress);
            ObjectBehaviorAssociation assoc = Config.ObjectAssociations.FindObjectAssociation(obj.BehaviorCriteria);
            if (assoc == null || assoc.PushHitbox == null)
            {
                return new List<(float centerX, float centerZ, float radius, float minY, float maxY, Color color)>();
            }
            (float radius, float minY, float maxY) = assoc.PushHitbox.GetDetails(objAddress);
            return new List<(float centerX, float centerZ, float radius, float minY, float maxY, Color color)>()
            {
                ((float)_posAngle.X, (float)_posAngle.Z, radius, minY, maxY, Color)
            };
        }

        public override Image GetInternalImage()
        {
            return Config.ObjectAssociations.CylinderImage;
        }

        public override string GetName()
        {
            return "Push Hitbox Cylinder for " + _posAngle.GetMapName();
        }

        public override PositionAngle GetPositionAngle()
        {
            return _posAngle;
        }

        public override ContextMenuStrip GetContextMenuStrip()
        {
            if (_contextMenuStrip == null)
            {
                _contextMenuStrip = new ContextMenuStrip();
                GetCircleToolStripMenuItems().ForEach(item => _contextMenuStrip.Items.Add(item));
            }

            return _contextMenuStrip;
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
