﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using STROOP.Controls.Map;
using OpenTK.Graphics.OpenGL;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using OpenTK;
using System.Drawing.Imaging;
using STROOP.Models;
using System.Windows.Forms;
using STROOP.Forms;

namespace STROOP.Map3
{
    public class Map3LevelWallObject : Map3WallObject, Map3LevelTriangleObjectI
    {
        private readonly List<uint> _triAddressList;
        private bool _removeCurrentTri;
        private TriangleListForm _triangleListForm;

        public Map3LevelWallObject()
            : base()
        {
            _triAddressList = TriangleUtilities.GetLevelTriangles()
                .FindAll(tri => tri.IsWall())
                .ConvertAll(tri => tri.Address);
            _removeCurrentTri = false;
            _triangleListForm = null;
        }

        protected override List<TriangleDataModel> GetTriangles()
        {
            return Map3Utilities.GetTriangles(_triAddressList);
        }

        public override ContextMenuStrip GetContextMenuStrip()
        {
            if (_contextMenuStrip == null)
            {
                ToolStripMenuItem itemReset = new ToolStripMenuItem("Reset");
                itemReset.Click += (sender, e) =>
                {
                    _triAddressList.Clear();
                    _triAddressList.AddRange(TriangleUtilities.GetLevelTriangles()
                        .FindAll(tri => tri.IsWall())
                        .ConvertAll(tri => tri.Address));
                    _triangleListForm?.RefreshAndSort();
                };

                ToolStripMenuItem itemRemoveCurrentTri = new ToolStripMenuItem("Remove Current Tri");
                itemRemoveCurrentTri.Click += (sender, e) =>
                {
                    _removeCurrentTri = !_removeCurrentTri;
                    itemRemoveCurrentTri.Checked = _removeCurrentTri;
                };

                ToolStripMenuItem itemShowTriData = new ToolStripMenuItem("Show Tri Data");
                itemShowTriData.Click += (sender, e) =>
                {
                    List<TriangleDataModel> tris = _triAddressList.ConvertAll(address => new TriangleDataModel(address));
                    TriangleUtilities.ShowTriangles(tris);
                };

                ToolStripMenuItem itemOpenForm = new ToolStripMenuItem("Open Form");
                itemOpenForm.Click += (sender, e) =>
                {
                    if (_triangleListForm != null) return;
                    _triangleListForm = new TriangleListForm(
                        this, TriangleClassification.Wall, _triAddressList);
                    _triangleListForm.Show();
                };

                BetterContextMenuStrip contextMenuStrip = CreateWallContextMenuStrip();
                contextMenuStrip.AddToBeginningList(itemReset);
                contextMenuStrip.AddToBeginningList(itemRemoveCurrentTri);
                contextMenuStrip.AddToBeginningList(itemShowTriData);
                contextMenuStrip.AddToBeginningList(itemOpenForm);
                contextMenuStrip.AddToBeginningList(new ToolStripSeparator());
                _contextMenuStrip = contextMenuStrip;
            }

            return _contextMenuStrip;
        }

        public void NullifyTriangleListForm()
        {
            _triangleListForm = null;
        }

        public override void Update()
        {
            if (_removeCurrentTri)
            {
                uint currentTriAddress = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.WallTriangleOffset);
                if (_triAddressList.Contains(currentTriAddress))
                {
                    _triAddressList.Remove(currentTriAddress);
                    _triangleListForm?.RefreshDataGridViewAfterRemoval();
                }
            }
        }

        public override string GetName()
        {
            return "Level Wall Tris";
        }

        public override Image GetImage()
        {
            return Config.ObjectAssociations.TriangleWallImage;
        }
    }
}
