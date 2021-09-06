﻿using STROOP.Controls;
using STROOP.Forms;
using STROOP.Models;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace STROOP.Managers
{
    public class SearchManager : DataManager
    {
        private enum ValueRelationship
        {
            EqualTo,
            NotEqualTo,

            GreaterThan,
            LessThan,
            GreaterThanOrEqualTo,
            LessThanOrEqualTo,

            Changed,
            DidNotChange,
            Increased,
            Decreased,
            IncreasedBy,
            DecreasedBy,

            BetweenExclusive,
            BetweenInclusive,

            HasSensibleMagnitude,

            EverythingPasses,
        };

        public class Result
        {
            public readonly List<object> Values;

            public Result(List<object> values)
            {
                Values = values;
            }

            public string Format(bool useHex)
            {
                List<string> strings = Values.ConvertAll(
                    value => useHex ? HexUtilities.FormatValue(value) : value.ToString());
                return string.Join(",", strings);
            }
        }

        private readonly Dictionary<uint, Result> _dictionary;
        private readonly Dictionary<uint, Result> _undoDictionary;

        private Type _memoryType;
        private int _memoryTypeSize;
        private bool _useHex;
        private int _numValues;

        private readonly ComboBox _comboBoxSearchMemoryType;
        private readonly ComboBox _comboBoxSearchValueRelationship;
        private readonly BetterTextbox _textBoxSearchValue;
        private readonly Button _buttonSearchFirstScan;
        private readonly Button _buttonSearchNextScan;
        private readonly Label _labelSearchNumResults;
        private readonly Button _buttonSearchAddSelectedAsVars;
        private readonly Button _buttonSearchAddAllAsVars;
        private readonly Button _buttonSearchUndoScan;
        private readonly Button _buttonSearchClearResults;
        private readonly ProgressBar _progressBarSearch;
        private readonly Label _labelSearchProgress;
        private readonly DataGridView _dataGridViewSearch;

        public SearchManager(TabPage tabControl, WatchVariableFlowLayoutPanel watchVariablePanel)
            : base(null, watchVariablePanel)
        {
            _dictionary = new Dictionary<uint, Result>();
            _undoDictionary = new Dictionary<uint, Result>();
            _memoryType = typeof(byte);
            _memoryTypeSize = 1;
            _useHex = false;
            _numValues = 1;

            SplitContainer splitContainerSearch = tabControl.Controls["splitContainerSearch"] as SplitContainer;
            SplitContainer splitContainerSearchOptions = splitContainerSearch.Panel1.Controls["splitContainerSearchOptions"] as SplitContainer;

            _comboBoxSearchMemoryType = splitContainerSearchOptions.Panel1.Controls["comboBoxSearchMemoryType"] as ComboBox;
            _comboBoxSearchMemoryType.DataSource = TypeUtilities.InGameTypeList;
            _comboBoxSearchMemoryType.SelectedItem = "short";

            _comboBoxSearchValueRelationship = splitContainerSearchOptions.Panel1.Controls["comboBoxSearchValueRelationship"] as ComboBox;
            _comboBoxSearchValueRelationship.DataSource = Enum.GetValues(typeof(ValueRelationship));

            _textBoxSearchValue = splitContainerSearchOptions.Panel1.Controls["textBoxSearchValue"] as BetterTextbox;

            _buttonSearchFirstScan = splitContainerSearchOptions.Panel1.Controls["buttonSearchFirstScan"] as Button;
            _buttonSearchFirstScan.Click += (sender, e) => DoFirstScan();

            _buttonSearchNextScan = splitContainerSearchOptions.Panel1.Controls["buttonSearchNextScan"] as Button;
            _buttonSearchNextScan.Click += (sender, e) => DoNextScan();

            _labelSearchNumResults = splitContainerSearchOptions.Panel1.Controls["labelSearchNumResults"] as Label;

            _buttonSearchAddSelectedAsVars = splitContainerSearchOptions.Panel1.Controls["buttonSearchAddSelectedAsVars"] as Button;
            _buttonSearchAddSelectedAsVars.Click += (sender, e) => AddTableRowsAsVars(ControlUtilities.GetTableSelectedRows(_dataGridViewSearch));

            _buttonSearchAddAllAsVars = splitContainerSearchOptions.Panel1.Controls["buttonSearchAddAllAsVars"] as Button;
            _buttonSearchAddAllAsVars.Click += (sender, e) => AddTableRowsAsVars(ControlUtilities.GetTableAllRows(_dataGridViewSearch));

            _buttonSearchUndoScan = splitContainerSearchOptions.Panel1.Controls["buttonSearchUndoScan"] as Button;
            _buttonSearchUndoScan.Click += (sender, e) => UndoScan();

            _buttonSearchClearResults = splitContainerSearchOptions.Panel1.Controls["buttonSearchClearResults"] as Button;
            _buttonSearchClearResults.Click += (sender, e) => ClearResults();

            _progressBarSearch = splitContainerSearchOptions.Panel1.Controls["progressBarSearch"] as ProgressBar;

            _labelSearchProgress = splitContainerSearchOptions.Panel1.Controls["labelSearchProgress"] as Label;
            _labelSearchProgress.Visible = false;

            _dataGridViewSearch = splitContainerSearchOptions.Panel2.Controls["dataGridViewSearch"] as DataGridView;
        }

        private void AddTableRowsAsVars(List<DataGridViewRow> rows)
        {
            List<WatchVariableControl> controls = new List<WatchVariableControl>();
            foreach (DataGridViewRow row in rows)
            {
                uint? addressNullable = ParsingUtilities.ParseHexNullable(row.Cells[0].Value);
                if (!addressNullable.HasValue) continue;
                uint startAddress = addressNullable.Value;
                for (int i = 0; i < _numValues; i++)
                {
                    uint address = startAddress + (uint)(i * _memoryTypeSize);
                    string typeString = TypeUtilities.TypeToString[_memoryType];
                    string name = typeString + " " + HexUtilities.FormatValue(address);
                    WatchVariable watchVar = new WatchVariable(
                        name: name,
                        memoryTypeName: typeString,
                        specialType: null,
                        baseAddressType: BaseAddressTypeEnum.Relative,
                        offsetUS: address,
                        offsetJP: address,
                        offsetSH: address,
                        offsetEU: address,
                        offsetDefault: null,
                        mask: null,
                        shift: null,
                        handleMapping: true);
                    WatchVariableControlPrecursor precursor = new WatchVariableControlPrecursor(
                        name: name,
                        watchVar: watchVar,
                        subclass: WatchVariableSubclass.Number,
                        backgroundColor: null,
                        displayType: null,
                        roundingLimit: null,
                        useHex: _useHex ? true : (bool?)null,
                        invertBool: null,
                        isYaw: null,
                        coordinate: null,
                        groupList: new List<VariableGroup>() { VariableGroup.Custom });
                    WatchVariableControl control = precursor.CreateWatchVariableControl();
                    controls.Add(control);
                }
            }
            AddVariables(controls);
        }

        private void DoFirstScan()
        {
            string memoryTypeString = (string)_comboBoxSearchMemoryType.SelectedItem;
            _memoryType = TypeUtilities.StringToType[memoryTypeString];
            _memoryTypeSize = TypeUtilities.TypeSize[_memoryType];
            _useHex = _textBoxSearchValue.Text.StartsWith("0x");

            List<(object searchValue1, object searchValue2)> searchValues = ParseSearchValueAll(_textBoxSearchValue.Text, _memoryType);
            _numValues = searchValues.Count;

            List<object> oldMemoryValues = Enumerable.Range(0, _numValues).ToList().ConvertAll(i => (object)null);

            DictionaryUtilities.TransferDictionary(_dictionary, _undoDictionary);
            _dictionary.Clear();
            StartProgressBar();
            for (uint address = 0x80000000; address < 0x80000000 + Config.RamSize - _memoryTypeSize * _numValues; address += (uint)_memoryTypeSize)
            {
                List<object> memoryValues = Enumerable.Range(0, _numValues).ToList()
                    .ConvertAll(i => Config.Stream.GetValue(_memoryType, address + (uint)(i * _memoryTypeSize)));

                bool valuesQualify = true;
                for (int i = 0; i < _numValues; i++)
                {
                    if (!ValueQualifies(memoryValues[i], oldMemoryValues[i], searchValues[i].searchValue1, searchValues[i].searchValue2, _memoryType))
                    {
                        valuesQualify = false;
                        break;
                    }
                }
                if (valuesQualify)
                {
                    _dictionary[address] = new Result(memoryValues);
                }

                int offset = (int)(address - 0x80000000);
                if (offset % 1024 == 0)
                {
                    SetProgressCount(offset, (int)Config.RamSize);
                }
            }
            StopProgressBar();

            UpdateControlsBasedOnDictionary();
        }

        private void DoNextScan()
        {
            List<(object searchValue1, object searchValue2)> searchValues = ParseSearchValueAll(_textBoxSearchValue.Text, _memoryType);

            List<KeyValuePair<uint, Result>> pairs = _dictionary.ToList();
            DictionaryUtilities.TransferDictionary(_dictionary, _undoDictionary);
            _dictionary.Clear();
            StartProgressBar();
            for (int i = 0; i < pairs.Count; i++)
            {
                KeyValuePair<uint, Result> pair = pairs[i];
                uint startAddress = pair.Key;
                Result oldResult = pair.Value;
                List<object> oldMemoryValues = oldResult.Values;
                List<object> memoryValues = Enumerable.Range(0, _numValues).ToList()
                    .ConvertAll(j => Config.Stream.GetValue(_memoryType, startAddress + (uint)(j * _memoryTypeSize)));

                bool valuesQualify = true;
                for (int j = 0; j < _numValues; j++)
                {
                    if (!ValueQualifies(memoryValues[j], oldMemoryValues[j], searchValues[j].searchValue1, searchValues[j].searchValue2, _memoryType))
                    {
                        valuesQualify = false;
                        break;
                    }
                }
                if (valuesQualify)
                {
                    _dictionary[startAddress] = new Result(memoryValues);
                }

                if (pairs.Count > 10000)
                {
                    if (i % 1024 == 0)
                    {
                        SetProgressCount(i, pairs.Count);
                    }
                }
                else
                {
                    SetProgressCount(i, pairs.Count);
                }
            }
            StopProgressBar();

            UpdateControlsBasedOnDictionary();
        }

        private List<(object searchValue1, object searchValue2)> ParseSearchValueAll(string text, Type type)
        {
            return text.Split(';').ToList().ConvertAll(s => ParseSearchValueSpecific(s.Trim(), type));
        }

        private (object searchValue1, object searchValue2) ParseSearchValueSpecific(string text, Type type)
        {
            List<string> stringValues = ParsingUtilities.ParseStringList(text);
            string stringValue1 = stringValues.Count >= 1 ? stringValues[0] : null;
            string stringValue2 = stringValues.Count >= 2 ? stringValues[1] : null;
            return (ParsingUtilities.ParseValueNullable(stringValue1, type),
                ParsingUtilities.ParseValueNullable(stringValue2, type));
        }

        private void ClearResults()
        {
            _dictionary.Clear();
            UpdateControlsBasedOnDictionary();
        }

        private void UpdateControlsBasedOnDictionary()
        {
            _labelSearchNumResults.Text = _dictionary.Count.ToString() + " Results";
            _dataGridViewSearch.Rows.Clear();
            if (_dictionary.Count > 10000) return;
            _dictionary.Keys.ToList().ForEach(key =>
            {
                _dataGridViewSearch.Rows.Add(
                    HexUtilities.FormatValue(key),
                    _dictionary[key].Format(_useHex));
            });
        }

        private void StartProgressBar()
        {
            _labelSearchProgress.Visible = true;
            _labelSearchProgress.Update();
        }

        private void StopProgressBar()
        {
            _labelSearchProgress.Visible = false;
            _labelSearchProgress.Update();
            _progressBarSearch.Value = 0;
            _progressBarSearch.Update();
        }

        private void SetProgressCount(int value, int maximum)
        {
            string maximumString = maximum.ToString();
            string valueString = string.Format("{0:D" + maximumString.Length + "}", value);
            double percent = Math.Round(100d * value / maximum, 1);
            string percentString = percent.ToString("N1");
            _labelSearchProgress.Text = string.Format(
                "{0}% ({1} / {2})", percentString, valueString, maximumString);
            _labelSearchProgress.Update();
            _progressBarSearch.Maximum = maximum;
            _progressBarSearch.Value = value;
            _progressBarSearch.Update();
        }

        private void UndoScan()
        {
            DictionaryUtilities.TransferDictionary(_undoDictionary, _dictionary);
            UpdateControlsBasedOnDictionary();
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;

            base.Update(updateView);
        }

        private bool ValueQualifies(object memoryObject, object oldMemoryObject, object searchObject1, object searchObject2, Type type)
        {
            if (type == typeof(byte))
            {
                byte? memoryValue = ParsingUtilities.ParseByteNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                byte? oldMemoryValue = ParsingUtilities.ParseByteNullable(oldMemoryObject);
                byte? searchValue1 = ParsingUtilities.ParseByteNullable(searchObject1);
                byte? searchValue2 = ParsingUtilities.ParseByteNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)_comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.HasSensibleMagnitude:
                        return true;
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (type == typeof(sbyte))
            {
                sbyte? memoryValue = ParsingUtilities.ParseSByteNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                sbyte? oldMemoryValue = ParsingUtilities.ParseSByteNullable(oldMemoryObject);
                sbyte? searchValue1 = ParsingUtilities.ParseSByteNullable(searchObject1);
                sbyte? searchValue2 = ParsingUtilities.ParseSByteNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)_comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.HasSensibleMagnitude:
                        return true;
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (type == typeof(short))
            {
                short? memoryValue = ParsingUtilities.ParseShortNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                short? oldMemoryValue = ParsingUtilities.ParseShortNullable(oldMemoryObject);
                short? searchValue1 = ParsingUtilities.ParseShortNullable(searchObject1);
                short? searchValue2 = ParsingUtilities.ParseShortNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)_comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.HasSensibleMagnitude:
                        return true;
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (type == typeof(ushort))
            {
                ushort? memoryValue = ParsingUtilities.ParseUShortNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                ushort? oldMemoryValue = ParsingUtilities.ParseUShortNullable(oldMemoryObject);
                ushort? searchValue1 = ParsingUtilities.ParseUShortNullable(searchObject1);
                ushort? searchValue2 = ParsingUtilities.ParseUShortNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)_comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.HasSensibleMagnitude:
                        return true;
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (type == typeof(int))
            {
                int? memoryValue = ParsingUtilities.ParseIntNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                int? oldMemoryValue = ParsingUtilities.ParseIntNullable(oldMemoryObject);
                int? searchValue1 = ParsingUtilities.ParseIntNullable(searchObject1);
                int? searchValue2 = ParsingUtilities.ParseIntNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)_comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.HasSensibleMagnitude:
                        return true;
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (type == typeof(uint))
            {
                uint? memoryValue = ParsingUtilities.ParseUIntNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                uint? oldMemoryValue = ParsingUtilities.ParseUIntNullable(oldMemoryObject);
                uint? searchValue1 = ParsingUtilities.ParseUIntNullable(searchObject1);
                uint? searchValue2 = ParsingUtilities.ParseUIntNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)_comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.HasSensibleMagnitude:
                        return true;
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (type == typeof(float))
            {
                float? memoryValue = ParsingUtilities.ParseFloatNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                float? oldMemoryValue = ParsingUtilities.ParseFloatNullable(oldMemoryObject);
                float? searchValue1 = ParsingUtilities.ParseFloatNullable(searchObject1);
                float? searchValue2 = ParsingUtilities.ParseFloatNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)_comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.HasSensibleMagnitude:
                        return HasSensibleMagnitude(memoryValue.Value);
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (type == typeof(double))
            {
                double? memoryValue = ParsingUtilities.ParseDoubleNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                double? oldMemoryValue = ParsingUtilities.ParseDoubleNullable(oldMemoryObject);
                double? searchValue1 = ParsingUtilities.ParseDoubleNullable(searchObject1);
                double? searchValue2 = ParsingUtilities.ParseDoubleNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)_comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.HasSensibleMagnitude:
                        return HasSensibleMagnitude(memoryValue.Value);
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            throw new ArgumentOutOfRangeException();
        }

        private static bool HasSensibleMagnitude(double value)
        {
            double abs = Math.Abs(value);
            return abs > 1E-3 && abs < 1E6;
        }
    }
}
