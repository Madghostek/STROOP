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
using STROOP.Forms;
using System.Xml.Linq;

namespace STROOP.Map
{
    public class MapObjectLevelFloor : MapObjectFloor, MapObjectLevelTriangleInterface
    {
        private readonly List<uint> _triAddressList;
        private bool _removeCurrentTri;
        private TriangleListForm _triangleListForm;
        private bool _autoUpdate;
        private int _numLevelTris;
        private bool _useCurrentCellTris;

        private ToolStripMenuItem _itemUseCurrentCellTris;

        public MapObjectLevelFloor()
            : this(TriangleUtilities.GetLevelTriangles()
                .FindAll(tri => tri.IsFloor())
                .ConvertAll(tri => tri.Address))
        {
            _removeCurrentTri = false;
            _triangleListForm = null;
            _autoUpdate = true;
            _numLevelTris = _triAddressList.Count;
            _useCurrentCellTris = false;
        }

        public MapObjectLevelFloor(List<uint> triAddressList)
        {
            _triAddressList = triAddressList;
        }

        public static MapObjectLevelFloor Create(string text)
        {
            List<uint> triAddressList = MapUtilities.ParseCustomTris(text, null);
            if (triAddressList == null) return null;
            return new MapObjectLevelFloor(triAddressList);
        }

        protected override List<TriangleDataModel> GetUnfilteredTriangles()
        {
            if (_useCurrentCellTris)
            {
                return MapUtilities.GetTriangles(
                    CellUtilities.GetTriangleAddressesInMarioCell(true, TriangleClassification.Floor));
            }
            return MapUtilities.GetTriangles(_triAddressList);
        }

        public override ContextMenuStrip GetContextMenuStrip()
        {
            if (_contextMenuStrip == null)
            {
                ToolStripMenuItem itemAutoUpdate = new ToolStripMenuItem("Auto Update");
                itemAutoUpdate.Click += (sender, e) =>
                {
                    _autoUpdate = !_autoUpdate;
                    itemAutoUpdate.Checked = _autoUpdate;
                };
                itemAutoUpdate.Checked = _autoUpdate;

                ToolStripMenuItem itemReset = new ToolStripMenuItem("Reset");
                itemReset.Click += (sender, e) => ResetTriangles();

                ToolStripMenuItem itemRemoveCurrentTri = new ToolStripMenuItem("Remove Current Tri");
                itemRemoveCurrentTri.Click += (sender, e) =>
                {
                    _removeCurrentTri = !_removeCurrentTri;
                    itemRemoveCurrentTri.Checked = _removeCurrentTri;
                };

                ToolStripMenuItem itemShowTriData = new ToolStripMenuItem("Show Tri Data");
                itemShowTriData.Click += (sender, e) =>
                {
                    List<TriangleDataModel> tris = _triAddressList.ConvertAll(address => TriangleDataModel.Create(address));
                    TriangleUtilities.ShowTriangles(tris);
                };

                ToolStripMenuItem itemOpenForm = new ToolStripMenuItem("Open Form");
                itemOpenForm.Click += (sender, e) =>
                {
                    if (_triangleListForm != null) return;
                    _triangleListForm = new TriangleListForm(
                        this, TriangleClassification.Floor, _triAddressList);
                    _triangleListForm.Show();
                };

                _itemUseCurrentCellTris = new ToolStripMenuItem("Use Current Cell Tris");
                _itemUseCurrentCellTris.Click += (sender, e) =>
                {
                    MapObjectSettings settings = new MapObjectSettings(
                        changeUseCurrentCellTris: true, newUseCurrentCellTris: !_useCurrentCellTris);
                    GetParentMapTracker().ApplySettings(settings);
                };

                _contextMenuStrip = new ContextMenuStrip();
                _contextMenuStrip.Items.Add(itemAutoUpdate);
                _contextMenuStrip.Items.Add(itemReset);
                _contextMenuStrip.Items.Add(itemRemoveCurrentTri);
                _contextMenuStrip.Items.Add(itemShowTriData);
                _contextMenuStrip.Items.Add(itemOpenForm);
                _contextMenuStrip.Items.Add(_itemUseCurrentCellTris);
                _contextMenuStrip.Items.Add(new ToolStripSeparator());
                GetFloorToolStripMenuItems().ForEach(item => _contextMenuStrip.Items.Add(item));
                _contextMenuStrip.Items.Add(new ToolStripSeparator());
                GetHorizontalTriangleToolStripMenuItems().ForEach(item => _contextMenuStrip.Items.Add(item));
                _contextMenuStrip.Items.Add(new ToolStripSeparator());
                GetTriangleToolStripMenuItems().ForEach(item => _contextMenuStrip.Items.Add(item));
            }

            return _contextMenuStrip;
        }

        private void ResetTriangles()
        {
            _triAddressList.Clear();
            _triAddressList.AddRange(TriangleUtilities.GetLevelTriangles()
                .FindAll(tri => tri.IsFloor())
                .ConvertAll(tri => tri.Address));
            _triangleListForm?.RefreshAndSort();
        }

        public void NullifyTriangleListForm()
        {
            _triangleListForm = null;
        }

        public override void Update()
        {
            if (_autoUpdate)
            {
                int numLevelTriangles = Config.Stream.GetInt(TriangleConfig.LevelTriangleCountAddress);
                if (_numLevelTris != numLevelTriangles)
                {
                    _numLevelTris = numLevelTriangles;
                    ResetTriangles();
                }
            }

            if (_removeCurrentTri)
            {
                uint currentTriAddress = Config.Stream.GetUInt(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset);
                if (_triAddressList.Contains(currentTriAddress))
                {
                    _triAddressList.Remove(currentTriAddress);
                    _triangleListForm?.RefreshDataGridViewAfterRemoval();
                }
            }
        }

        public override string GetName()
        {
            return "Level Floor Tris";
        }

        public override Image GetInternalImage()
        {
            return Config.ObjectAssociations.TriangleFloorImage;
        }

        public override void ApplySettings(MapObjectSettings settings)
        {
            base.ApplySettings(settings);

            if (settings.ChangeUseCurrentCellTris)
            {
                _useCurrentCellTris = settings.NewUseCurrentCellTris;
                _itemUseCurrentCellTris.Checked = settings.NewUseCurrentCellTris;
            }
        }

        public override List<XAttribute> GetXAttributes()
        {
            List<string> hexList = _triAddressList.ConvertAll(triAddress => HexUtilities.FormatValue(triAddress));
            return new List<XAttribute>()
            {
                new XAttribute("triangles", string.Join(",", hexList)),
            };
        }
    }
}
