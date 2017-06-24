﻿using SM64_Diagnostic.Structs;
using SM64_Diagnostic.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SM64_Diagnostic.Controls;
using SM64_Diagnostic.Extensions;
using SM64_Diagnostic.Structs.Configurations;

namespace SM64_Diagnostic.Managers
{
    public class ObjectManager : DataManager
    {
        List<WatchVariableControl> _behaviorDataControls = new List<WatchVariableControl>();
        ObjectAssociations _objAssoc;
        ObjectDataGui _objGui;

        object _watchVarLocker = new object();

        string _slotIndex;
        string _slotPos;
        string _behavior;
        bool _unclone = false;
        bool _revive = false;

        #region Fields
        public void SetBehaviorWatchVariables(List<WatchVariable> value, Color color)
        {
            lock (_watchVarLocker)
            {
                // Remove old watchVars from list
                foreach (var watchVar in _behaviorDataControls)
                {
                    _dataControls.Remove(watchVar);
                    _objGui.ObjectFlowLayout.Controls.Remove(watchVar.Control);
                }
                _behaviorDataControls.Clear();

                // Add new watchVars
                foreach (var watchVar in value)
                {
                    var newWatchVarControl = new WatchVariableControl(_stream, watchVar);
                    newWatchVarControl.Color = color;
                    _behaviorDataControls.Add(newWatchVarControl);
                    _dataControls.Add(newWatchVarControl);
                    _objGui.ObjectFlowLayout.Controls.Add(newWatchVarControl.Control);
                }
                _behaviorDataControls.ForEach(w => w.OtherOffsets = _currentAddresses);
            }
        }

        List<uint> _currentAddresses = new List<uint>();
        public List<uint> CurrentAddresses
        {
            get
            {
                return _currentAddresses;
            }
            set
            {
                if (_currentAddresses.SequenceEqual(value))
                    return;

                _currentAddresses = value.ToList();

                if (_currentAddresses.Count > 1)
                    _objGui.ObjAddressLabelValue.Text = "";
                else if (_currentAddresses.Count > 0)
                    _objGui.ObjAddressLabelValue.Text = "0x" + _currentAddresses[0].ToString("X8");
                else
                    _objGui.ObjAddressLabelValue.Text = "";

                AddressChanged();

                foreach (WatchVariableControl watchVar in _dataControls)
                {
                    watchVar.OtherOffsets = _currentAddresses;
                }
            }
        }

        public string SlotIndex
        {
            get
            {
                return _slotIndex;
            }
            set
            {
                if (_slotIndex != value)
                {
                    _slotIndex = value;
                    _objGui.ObjSlotIndexLabel.Text = _slotIndex;
                }
            }
        }

        public string SlotPos
        {
            get
            {
                return _slotPos;
            }
            set
            {
                if (_slotPos != value)
                {
                    _slotPos = value;
                    _objGui.ObjSlotPositionLabel.Text = _slotPos;
                }
            }
        }

        public string Behavior
        {
            get
            {
                return _behavior;
            }
            set
            {
                if (_behavior != value)
                {
                    _behavior = value;
                    _objGui.ObjBehaviorLabel.Text = value;
                }
            }
        }

        public string Name
        {
            get
            {
                return _objGui.ObjectNameTextBox.Text;
            }
            set
            {
                if (_objGui.ObjectNameTextBox.Text != value)
                    _objGui.ObjectNameTextBox.Text = value;
            }
        }

        public Color BackColor
        {
            set
            {
                if (_objGui.ObjectBorderPanel.BackColor != value)
                {
                    _objGui.ObjectBorderPanel.BackColor = value;
                    _objGui.ObjectImagePictureBox.BackColor = value.Lighten(0.7);
                }
            }
            get
            {
                return _objGui.ObjectBorderPanel.BackColor;
            }
        }

        public Image Image
        {
            get
            {
                return _objGui.ObjectImagePictureBox.Image;
            }
            set
            {
                if (_objGui.ObjectImagePictureBox.Image != value)
                    _objGui.ObjectImagePictureBox.Image = value;
            }
        }

        #endregion

        protected override void InitializeSpecialVariables()
        {
            _specialWatchVars = new List<IDataContainer>()
            {
                new DataContainer("MarioDistanceToObject"),
                new DataContainer("MarioLateralDistanceToObject"),
                new DataContainer("MarioVerticalDistanceToObject"),
                new DataContainer("MarioDistanceToObjectHome"),
                new DataContainer("MarioLateralDistanceToObjectHome"),
                new DataContainer("MarioVerticalDistanceToObjectHome"),
                new AngleDataContainer("MarioAngleToObject"),
                new AngleDataContainer("MarioDeltaAngleToObject"),
                new AngleDataContainer("AngleToMario"),
                new AngleDataContainer("DeltaAngleToMario"),
                new DataContainer("ObjectDistanceToHome"),
                new DataContainer("LateralObjectDistanceToHome"),
                new DataContainer("VerticalObjectDistanceToHome"),
                new DataContainer("MarioHitboxAwayFromObject"),
                new DataContainer("MarioHitboxAboveObject"),
                new DataContainer("MarioHitboxBelowObject"),
                new DataContainer("MarioHitboxOverlapsObject"),
                new DataContainer("RngCallsPerFrame"),
            };
        }

        public ObjectManager(ProcessStream stream, ObjectAssociations objAssoc, List<WatchVariable> objectData, ObjectDataGui objectGui)
            : base(stream, objectData, objectGui.ObjectFlowLayout)
        { 
            _objGui = objectGui;
            _objAssoc = objAssoc;
            
            _objGui.ObjAddressLabelValue.Click += ObjAddressLabel_Click;
            _objGui.ObjAddressLabel.Click += ObjAddressLabel_Click;

            // Register buttons
            objectGui.CloneButton.Click += CloneButton_Click;
            objectGui.UnloadButton.Click += UnloadButton_Click;
            objectGui.GoToButton.Click += GoToButton_Click;
            objectGui.RetrieveButton.Click += RetreiveButton_Click;
            objectGui.GoToHomeButton.Click += GoToHomeButton_Click;
            objectGui.RetrieveHomeButton.Click += RetrieveHomeButton_Click;

            objectGui.PosXnZnButton.Click += (sender, e) => PosXZButton_Click(sender, e, -1, -1);
            objectGui.PosXnButton.Click += (sender, e) => PosXZButton_Click(sender, e, -1, 0);
            objectGui.PosXnZpButton.Click += (sender, e) => PosXZButton_Click(sender, e, -1, 1);
            objectGui.PosZnButton.Click += (sender, e) => PosXZButton_Click(sender, e, 0, -1);
            objectGui.PosZpButton.Click += (sender, e) => PosXZButton_Click(sender, e, 0, 1);
            objectGui.PosXpZnButton.Click += (sender, e) => PosXZButton_Click(sender, e, 1, -1);
            objectGui.PosXpButton.Click += (sender, e) => PosXZButton_Click(sender, e, 1, 0);
            objectGui.PosXpZpButton.Click += (sender, e) => PosXZButton_Click(sender, e, 1, 1);
            objectGui.PosYnButton.Click += (sender, e) =>  PosYButton_Click(sender, e, -1);
            objectGui.PosYpButton.Click += (sender, e) => PosYButton_Click(sender, e, 1);
        }

        private void PosYButton_Click(object sender, EventArgs e, int y)
        {
            if (_currentAddresses.Count == 0)
                return;

            float value;
            if (!float.TryParse(_objGui.PosYTextbox.Text, out value))
                return;

            MarioActions.MoveObjects(_stream, CurrentAddresses, 0, y * value, 0);
        }

        private void PosXZButton_Click(object sender, EventArgs e, int x, int z)
        {
            if (_currentAddresses.Count == 0)
                return;

            float value;
            if (!float.TryParse(_objGui.PosXZTextbox.Text, out value))
                return;

            MarioActions.MoveObjects(_stream, CurrentAddresses, x * value, 0, z * value);
        }

        private void AddressChanged()
        {
            var test = _dataControls.Where(d => d is WatchVariableControl);
            foreach (WatchVariableControl dataControl in test)
                dataControl.EditMode = false;

            if (CurrentAddresses.Count == 1)
            {
                _objGui.CloneButton.Enabled = true;
            }
            else
            {
                _objGui.CloneButton.Enabled = false;
            }
        }

        private void ObjAddressLabel_Click(object sender, EventArgs e)
        {
            if (_currentAddresses.Count == 0)
                return;

            var variableTitle = "Object Address" + (_currentAddresses.Count > 1 ? " (First of Multiple)" : ""); 
            var variableInfo = new VariableViewerForm(variableTitle, "Object",
                String.Format("0x{0:X8}", _currentAddresses[0]), String.Format("0x{0:X8}", (_currentAddresses[0] & 0x0FFFFFFF) + _stream.ProcessMemoryOffset));
            variableInfo.ShowDialog();
        }

        private void RetreiveButton_Click(object sender, EventArgs e)
        {
            if (CurrentAddresses.Count == 0)
                return;

            MarioActions.RetreiveObjects(_stream, CurrentAddresses);
        }

        private void GoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentAddresses.Count == 0)
                return;

            MarioActions.GoToObjects(_stream, CurrentAddresses);
        }


        private void GoToHomeButton_Click(object sender, EventArgs e)
        {
            if (CurrentAddresses.Count == 0)
                return;

            MarioActions.GoToObjectsHome(_stream, CurrentAddresses);
        }

        private void RetrieveHomeButton_Click(object sender, EventArgs e)
        {
            if (CurrentAddresses.Count == 0)
                return;

            MarioActions.RetreiveObjectsHome(_stream, CurrentAddresses);
        }

        private void UnloadButton_Click(object sender, EventArgs e)
        {
            if (CurrentAddresses.Count == 0)
                return;

            if (_revive)
                MarioActions.ReviveObject(_stream, CurrentAddresses);
            else
                MarioActions.UnloadObject(_stream, CurrentAddresses);
        }

        private void CloneButton_Click(object sender, EventArgs e)
        {
            if (CurrentAddresses.Count == 0)
                return;

            if (_unclone)
                MarioActions.UnCloneObject(_stream, CurrentAddresses[0]);
            else
                MarioActions.CloneObject(_stream, CurrentAddresses[0]);
        }

        private void ProcessSpecialVars()
        {
            // Get Mario position
            float mX, mY, mZ, mFacing;
            mX = _stream.GetSingle(Config.Mario.StructAddress + Config.Mario.XOffset);
            mY = _stream.GetSingle(Config.Mario.StructAddress + Config.Mario.YOffset);
            mZ = _stream.GetSingle(Config.Mario.StructAddress + Config.Mario.ZOffset);
            mFacing = (float)(((_stream.GetUInt32(Config.Mario.StructAddress + Config.Mario.RotationOffset) >> 16) % 65536) / 65536f * 2 * Math.PI);

            // Get Mario object position
            var marioObjRef = _stream.GetUInt32(Config.Mario.ObjectReferenceAddress);
            float mObjX, mObjY, mObjZ;
            mObjX = _stream.GetSingle(marioObjRef + Config.ObjectSlots.ObjectXOffset);
            mObjY = _stream.GetSingle(marioObjRef + Config.ObjectSlots.ObjectYOffset);
            mObjZ = _stream.GetSingle(marioObjRef + Config.ObjectSlots.ObjectZOffset);

            // Get Mario object hitbox variables
            float mObjHitboxRadius, mObjHitboxHeight, mObjHitboxDownOffset, mObjHitboxBottom, mObjHitboxTop;
            mObjHitboxRadius = _stream.GetSingle(marioObjRef + Config.ObjectSlots.HitboxRadius);
            mObjHitboxHeight = _stream.GetSingle(marioObjRef + Config.ObjectSlots.HitboxHeight);
            mObjHitboxDownOffset = _stream.GetSingle(marioObjRef + Config.ObjectSlots.HitboxDownOffset);
            mObjHitboxBottom = mObjY - mObjHitboxDownOffset;
            mObjHitboxTop = mObjY + mObjHitboxHeight - mObjHitboxDownOffset;

            bool firstObject = true;

            foreach (var objAddress in _currentAddresses)
            { 
                // Get object position
                float objX, objY, objZ, objFacing;
                objX = _stream.GetSingle(objAddress + Config.ObjectSlots.ObjectXOffset);
                objY = _stream.GetSingle(objAddress + Config.ObjectSlots.ObjectYOffset);
                objZ = _stream.GetSingle(objAddress + Config.ObjectSlots.ObjectZOffset);
                objFacing = (float)((UInt16)(_stream.GetUInt32(objAddress + Config.ObjectSlots.ObjectRotationOffset)) / 65536f * 2 * Math.PI);

                double angleToMario = Math.PI / 2 - MoreMath.AngleTo(objX, objZ, mX, mZ);

                // Get object position
                float objHomeX, objHomeY, objHomeZ;
                objHomeX = _stream.GetSingle(objAddress + Config.ObjectSlots.HomeXOffset);
                objHomeY = _stream.GetSingle(objAddress + Config.ObjectSlots.HomeYOffset);
                objHomeZ = _stream.GetSingle(objAddress + Config.ObjectSlots.HomeZOffset);

                // Get object hitbox variables
                float objHitboxRadius, objHitboxHeight, objHitboxDownOffset, objHitboxBottom, objHitboxTop;
                objHitboxRadius = _stream.GetSingle(objAddress + Config.ObjectSlots.HitboxRadius);
                objHitboxHeight = _stream.GetSingle(objAddress + Config.ObjectSlots.HitboxHeight);
                objHitboxDownOffset = _stream.GetSingle(objAddress + Config.ObjectSlots.HitboxDownOffset);
                objHitboxBottom = objY - objHitboxDownOffset;
                objHitboxTop = objY + objHitboxHeight - objHitboxDownOffset;

                // Compute hitbox distances between Mario obj and obj
                double marioHitboxAwayFromObject = MoreMath.DistanceTo(mObjX, mObjZ, objX, objZ) - mObjHitboxRadius - objHitboxRadius;
                double marioHitboxAboveObject = mObjHitboxBottom - objHitboxTop;
                double marioHitboxBelowObject = objHitboxBottom - mObjHitboxTop;

                foreach (IDataContainer specialVar in _specialWatchVars)
                {
                    var newText = "";
                    double? newAngle = null;
                    switch (specialVar.SpecialName)
                    {
                        case "MarioDistanceToObject":
                            newText = Math.Round(MoreMath.DistanceTo(mX, mY, mZ, objX, objY, objZ),3).ToString();
                            break;

                        case "MarioLateralDistanceToObject":
                            newText = Math.Round(MoreMath.DistanceTo(mX, mZ, objX, objZ), 3).ToString();
                            break;

                        case "MarioVerticalDistanceToObject":
                            newText = Math.Round(mY - objY, 3).ToString();
                            break;

                        case "MarioDistanceToObjectHome":
                            newText = Math.Round(MoreMath.DistanceTo(mX, mY, mZ, objHomeX, objHomeY, objHomeZ), 3).ToString();
                            break;

                        case "MarioLateralDistanceToObjectHome":
                            newText = Math.Round(MoreMath.DistanceTo(mX, mZ, objHomeX, objHomeZ), 3).ToString();
                            break;

                        case "MarioVerticalDistanceToObjectHome":
                            newText = Math.Round(mY - objHomeY, 3).ToString();
                            break;

                        case "ObjectDistanceToHome":
                            newText = Math.Round(MoreMath.DistanceTo(objX, objY, objZ, objHomeX, objHomeY, objHomeZ), 3).ToString();
                            break;

                        case "LateralObjectDistanceToHome":
                            newText = Math.Round(MoreMath.DistanceTo(objX, objZ, objHomeX, objHomeZ), 3).ToString();
                            break;

                        case "VerticalObjectDistanceToHome":
                            newText = Math.Round(objY - objHomeY, 3).ToString();
                            break;

                        case "MarioAngleToObject":
                            newAngle = angleToMario + Math.PI;
                            break;

                        case "MarioDeltaAngleToObject":
                            newAngle = mFacing - (angleToMario + Math.PI);
                            break;

                        case "AngleToMario":
                            newAngle = angleToMario;
                            break;

                        case "DeltaAngleToMario":
                            newAngle = objFacing - angleToMario;
                            break;

                        case "MarioHitboxAwayFromObject":
                            newText = Math.Round(marioHitboxAwayFromObject, 3).ToString();
                            break;

                       case "MarioHitboxAboveObject":
                            newText = Math.Round(marioHitboxAboveObject, 3).ToString();
                            break;

                        case "MarioHitboxBelowObject":
                            newText = Math.Round(marioHitboxBelowObject, 3).ToString();
                            break;

                        case "MarioHitboxOverlapsObject":
                            if (marioHitboxAwayFromObject < 0 &&
                                marioHitboxAboveObject <= 0 &&
                                marioHitboxBelowObject <= 0)
                            {
                                newText = "True";
                            }
                            else
                            {
                                newText = "False";
                            }
                            break;

                        case "RngCallsPerFrame":
                            newText = GetNumRngCalls(objAddress).ToString();
                            break;
                    }

                    if (specialVar is AngleDataContainer)
                    {
                        var angleContainer = specialVar as AngleDataContainer;
                        if (firstObject)
                        {
                            angleContainer.ValueExists = newAngle.HasValue;
                            if (newAngle.HasValue)
                                angleContainer.AngleValue = newAngle.Value;
                        }

                        newAngle %= Math.PI * 2;
                        if (newAngle < 0)
                            newAngle += Math.PI * 2;

                        // Check when multiple objects have different values
                        angleContainer.ValueExists &= newAngle == angleContainer.AngleValue;
                    }
                    else if (specialVar is DataContainer)
                    {
                        var dataContainer = specialVar as DataContainer;
                        if (firstObject)
                            dataContainer.Text = newText;
                        // Check when multiple objects have different values
                        else if (dataContainer.Text != newText)
                            dataContainer.Text = "";
                    }
                }

                firstObject = false;
            }
        }

        public override void Update(bool updateView)
        {
            if (!updateView)
                return;

            // Determine which object is being held
            uint holdingObj = _stream.GetUInt32(Config.Mario.StructAddress + Config.Mario.HoldingObjectPointerOffset);

            // Change to unclone if we are already holding the object
            if ((_currentAddresses.Contains(holdingObj)) != _unclone)
            {
                _unclone = !_unclone;

                // Update button text
                _objGui.CloneButton.Text = _unclone ? "UnClone" : "Clone";
            }

            // Determine load or unload
            bool anyActive = _currentAddresses.Any(address => _stream.GetUInt16(address + Config.ObjectSlots.ObjectActiveOffset) != 0x0000);
            if (anyActive == _revive)
            {
                _revive = !anyActive;

                // Update button text
                _objGui.UnloadButton.Text = _revive ? "Revive" : "Unload";
            }

            base.Update(updateView);
            ProcessSpecialVars();
        }

        private int GetNumRngCalls(uint objAddress)
        {
            var numberOfRngObjs = _stream.GetUInt32(Config.RngRecordingAreaAddress);

            int numOfCalls = 0;

            for (int i = 0; i < numberOfRngObjs; i++)
            {
                uint rngStructAdd = (uint)(Config.RngRecordingAreaAddress + 0x30 + 0x08 * i);
                var address = _stream.GetUInt32(rngStructAdd + 0x04);
                if (address != objAddress)
                    continue;

                var preRng = _stream.GetUInt16(rngStructAdd + 0x00);
                var postRng = _stream.GetUInt16(rngStructAdd + 0x02);

                numOfCalls = RngIndexer.GetRngIndexDiff(preRng, postRng);
                break;
            }

            return numOfCalls;
        }
    }
}
