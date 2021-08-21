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
using STROOP.Models;
using System.Xml.Linq;
using System.Windows.Forms;
using static STROOP.Managers.ObjectSlotsManager;

namespace STROOP.Map
{
    public class MapObjectObject : MapObjectIconPoint
    {
        private readonly ObjectDataModel _obj;
        private readonly PositionAngle _posAngle;

        public MapObjectObject(PositionAngle posAngle)
            : base()
        {
            _obj = new ObjectDataModel(posAngle.GetObjAddress());
            _posAngle = PositionAngle.Obj(posAngle.GetObjAddress());
        }

        public override Image GetInternalImage()
        {
            _obj.Update();
            if (_obj.BehaviorAssociation == null)
            {
                return Config.ObjectAssociations.DefaultImage;
            }
            return _iconType == MapTrackerIconType.ObjectSlotImage ?
                _obj.BehaviorAssociation.Image.Image :
                _obj.BehaviorAssociation.MapImage.Image;
        }

        public override PositionAngle GetPositionAngle()
        {
            return _posAngle;
        }

        public override string GetName()
        {
            return _posAngle.GetMapName();
        }

        public override void Update()
        {
            base.Update();
            _obj.Update();
            InternalRotates = Config.ObjectAssociations.GetObjectMapRotates(_obj.BehaviorCriteria);
        }

        public override List<ToolStripItem> GetHoverContextMenuStripItems(MapObjectHoverData hoverData)
        {
            List<ToolStripItem> output = base.GetHoverContextMenuStripItems(hoverData);

            ToolStripMenuItem selectObjectItem = new ToolStripMenuItem("Select Object in Object Tab");
            selectObjectItem.Click += (sender, e) => Config.ObjectSlotsManager.SelectSlotByAddress(_posAngle.GetObjAddress());
            output.Insert(0, selectObjectItem);

            ToolStripMenuItem copyAddressItem = new ToolStripMenuItem("Copy Address");
            copyAddressItem.Click += (sender, e) => Clipboard.SetText(HexUtilities.FormatValue(_posAngle.GetObjAddress()));
            output.Insert(1, copyAddressItem);

            return output;
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
