﻿using STROOP.Structs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using STROOP.Extensions;
using STROOP.Utilities;
using STROOP.Controls;
using STROOP.Structs.Configurations;

namespace STROOP.Forms
{
    public partial class VariableControllerForm : Form, IUpdatableForm
    {
        private static readonly Color COLOR_BLUE = Color.FromArgb(220, 255, 255);
        private static readonly Color COLOR_RED = Color.FromArgb(255, 220, 220);
        private static readonly Color COLOR_PURPLE = Color.FromArgb(200, 190, 230);

        private readonly List<string> _varNames;
        private readonly List<WatchVariableWrapper> _watchVarWrappers;
        private List<List<uint>> _fixedAddressLists;

        private readonly Action<bool, bool> _addAction;
        private bool _isDoingContinuousAdd;
        private bool _isDoingContinuousSubtract;

        public VariableControllerForm(
            string varName, WatchVariableWrapper watchVarWrapper, List<uint> fixedAddressList) :
                this (new List<string>() { varName },
                      new List<WatchVariableWrapper>() { watchVarWrapper },
                      new List<List<uint>>() { fixedAddressList })
        {

        }

        public VariableControllerForm(
            List<string> varNames, List<WatchVariableWrapper> watchVarWrappers, List<List<uint>> fixedAddressLists)
        {
            _varNames = varNames;
            _watchVarWrappers = watchVarWrappers;
            _fixedAddressLists = fixedAddressLists;

            _isDoingContinuousAdd = false;
            _isDoingContinuousSubtract = false;

            InitializeComponent();
            FormManager.AddForm(this);
            FormClosing += (sender, e) => FormManager.RemoveForm(this);

            _textBoxVarName.Text = String.Join(",", _varNames);

            _addAction = (bool add, bool allowToggle) =>
            {
                List<string> values = ParsingUtilities.ParseStringList(_textBoxAddSubtract.Text);
                if (values.Count == 0) return;
                for (int i = 0; i < _watchVarWrappers.Count; i++)
                    _watchVarWrappers[i].AddValue(values[i % values.Count], add, allowToggle, _fixedAddressLists[i]);
            };
            _buttonAdd.Click += (s, e) => _addAction(true, true);
            _buttonSubtract.Click += (s, e) => _addAction(false, true);

            Timer addTimer = new Timer { Interval = 30 };
            addTimer.Tick += (s, e) => { if (KeyboardUtilities.IsCtrlHeld()) _addAction(true, false); };
            _buttonAdd.MouseDown += (sender, e) => addTimer.Start();
            _buttonAdd.MouseUp += (sender, e) => addTimer.Stop();

            ControlUtilities.AddContextMenuStripFunctions(
                _buttonAdd,
                new List<string>() { "Start Continuous Add", "Stop Continuous Add" },
                new List<Action>() { () => _isDoingContinuousAdd = true, () => _isDoingContinuousAdd = false });

            Timer subtractTimer = new Timer { Interval = 30 };
            subtractTimer.Tick += (s, e) => { if (KeyboardUtilities.IsCtrlHeld()) _addAction(false, false); };
            _buttonSubtract.MouseDown += (sender, e) => subtractTimer.Start();
            _buttonSubtract.MouseUp += (sender, e) => subtractTimer.Stop();

            ControlUtilities.AddContextMenuStripFunctions(
                _buttonSubtract,
                new List<string>() { "Start Continuous Subtract", "Stop Continuous Subtract" },
                new List<Action>() { () => _isDoingContinuousSubtract = true, () => _isDoingContinuousSubtract = false });

            ToolStripMenuItem itemInvertedAdd = new ToolStripMenuItem("Inverted");
            ToolStripMenuItem itemInvertedSubtract = new ToolStripMenuItem("Inverted");
            Action<bool> setInverted = (bool inverted) =>
            {
                tableLayoutPanel1.Controls.Remove(_buttonAdd);
                tableLayoutPanel1.Controls.Remove(_buttonSubtract);
                if (inverted)
                {
                    tableLayoutPanel1.Controls.Add(_buttonAdd, 0, 2);
                    tableLayoutPanel1.Controls.Add(_buttonSubtract, 2, 2);
                }
                else
                {
                    tableLayoutPanel1.Controls.Add(_buttonAdd, 2, 2);
                    tableLayoutPanel1.Controls.Add(_buttonSubtract, 0, 2);
                }
                itemInvertedAdd.Checked = inverted;
                itemInvertedSubtract.Checked = inverted;
            };
            itemInvertedAdd.Click += (sender, e) => setInverted(!itemInvertedAdd.Checked);
            itemInvertedSubtract.Click += (sender, e) => setInverted(!itemInvertedSubtract.Checked);
            _buttonAdd.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            _buttonAdd.ContextMenuStrip.Items.Add(itemInvertedAdd);
            _buttonSubtract.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            _buttonSubtract.ContextMenuStrip.Items.Add(itemInvertedSubtract);

            _buttonGet.Click += (s, e) => _textBoxGetSet.Text = GetValues();

            _buttonSet.Click += (s, e) => SetValues(true);
            _textBoxGetSet.AddEnterAction(() => SetValues(true));

            _checkBoxFixAddress.Click += (s, e) => ToggleFixedAddress();

            _checkBoxLock.Click += (s, e) =>
            {
                List<bool> lockedBools = new List<bool>();
                for (int i = 0; i < _watchVarWrappers.Count; i++)
                    lockedBools.Add(_watchVarWrappers[i].GetLockedBool(_fixedAddressLists[i]));
                bool anyLocked = lockedBools.Any(b => b);
                for (int i = 0; i < _watchVarWrappers.Count; i++)
                    _watchVarWrappers[i].ToggleLocked(!anyLocked, _fixedAddressLists[i]);
            };

            _checkBoxFixAddress.CheckState = BoolUtilities.GetCheckState(
                fixedAddressLists.ConvertAll(fixedAddressList => fixedAddressList != null));

            _textBoxCurrentValue.BackColor = GetColorForCheckState(BoolUtilities.GetCheckState(
                fixedAddressLists.ConvertAll(fixedAddressList => fixedAddressList != null)));
        }

        private string GetValues()
        {
            List<object> values = new List<object>();
            for (int i = 0; i < _watchVarWrappers.Count; i++)
                values.Add(_watchVarWrappers[i].GetValue(true, true, _fixedAddressLists[i]));
            return String.Join(",", values);
        }

        private void SetValues(bool allowToggle)
        {
            List<string> values = ParsingUtilities.ParseStringList(_textBoxGetSet.Text);
            if (values.Count == 0) return;

            bool streamAlreadySuspended = Config.Stream.IsSuspended;
            if (!streamAlreadySuspended) Config.Stream.Suspend();
            for (int i = 0; i < _watchVarWrappers.Count; i++)
                _watchVarWrappers[i].SetValue(values[i % values.Count], allowToggle, _fixedAddressLists[i]);
            if (!streamAlreadySuspended) Config.Stream.Resume();
        }

        private Color GetColorForCheckState(CheckState checkState)
        {
            switch (checkState)
            {
                case CheckState.Unchecked:
                    return COLOR_BLUE;
                case CheckState.Checked:
                    return COLOR_RED;
                case CheckState.Indeterminate:
                    return COLOR_PURPLE;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdateForm()
        {
            _textBoxCurrentValue.Text = GetValues();
            List<bool> lockedBools = new List<bool>();
            for (int i = 0; i < _watchVarWrappers.Count; i++)
                lockedBools.Add(_watchVarWrappers[i].GetLockedBool(_fixedAddressLists[i]));
            _checkBoxLock.CheckState = BoolUtilities.GetCheckState(lockedBools);
            if (_isDoingContinuousAdd) _addAction(true, false);
            if (_isDoingContinuousSubtract) _addAction(false, false);
        }
        
        public void ToggleFixedAddress()
        {
            bool fixedAddress = _checkBoxFixAddress.Checked;
            if (fixedAddress)
            {
                _textBoxCurrentValue.BackColor = COLOR_RED;
                _fixedAddressLists = _watchVarWrappers.ConvertAll(
                    watchVarWrapper => watchVarWrapper.GetCurrentAddressesToFix());
            }
            else
            {
                _textBoxCurrentValue.BackColor = COLOR_BLUE;
                _fixedAddressLists = _watchVarWrappers.ConvertAll(
                    watchVarWrapper => (List<uint>)null);
            }
        }
    }
}
