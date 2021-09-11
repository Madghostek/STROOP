﻿using STROOP.Controls;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace STROOP.Forms
{
    public partial class SelectionForm : Form
    {
        public static int? WIDTH = null;
        public static int? HEIGHT = null;

        public object Selection;

        public SelectionForm()
        {
            InitializeComponent();
            if (WIDTH.HasValue) Width = WIDTH.Value;
            if (HEIGHT.HasValue) Height = HEIGHT.Value;
            Resize += (sender, e) =>
            {
                WIDTH = Width;
                HEIGHT = Height;
            };
        }

        public void Initialize<T>(
            string selectionText,
            string buttonText,
            List<T> items,
            Action<T> selectionAction)
        {
            textBoxSelect.Text = selectionText;
            buttonSet.Text = buttonText;
            listBoxSelections.DataSource = items;
                
            Action enterAction = () =>
            {
                T selection = (T)listBoxSelections.SelectedItem;
                selectionAction(selection);
                Selection = selection;
                DialogResult = DialogResult.OK;
                Close();
            };
            buttonSet.Click += (sender, e) => enterAction();
            listBoxSelections.DoubleClick += (sender, e) => enterAction();
        }

        public static void ShowActionDescriptionSelectionForm()
        {
            SelectionForm selectionForm = new SelectionForm();
            selectionForm.Initialize(
                "Select an Action",
                "Set Action",
                TableConfig.MarioActions.GetActionNameList(),
                actionName =>
                {
                    uint? action = TableConfig.MarioActions.GetActionFromName(actionName);
                    if (action.HasValue)
                        Config.Stream.SetValue(action.Value, MarioConfig.StructAddress + MarioConfig.ActionOffset);
                });
            selectionForm.Show();
        }

        public static void ShowPreviousActionDescriptionSelectionForm()
        {
            SelectionForm selectionForm = new SelectionForm();
            selectionForm.Initialize(
                "Select a Previous Action",
                "Set Previous Action",
                TableConfig.MarioActions.GetActionNameList(),
                actionName =>
                {
                    uint? action = TableConfig.MarioActions.GetActionFromName(actionName);
                    if (action.HasValue)
                        Config.Stream.SetValue(action.Value, MarioConfig.StructAddress + MarioConfig.PrevActionOffset);
                });
            selectionForm.Show();
        }

        public static void ShowAnimationDescriptionSelectionForm()
        {
            SelectionForm selectionForm = new SelectionForm();
            selectionForm.Initialize(
                "Select an Animation",
                "Set Animation",
                TableConfig.MarioAnimations.GetAnimationNameList(),
                animationName =>
                {
                    int? animation = TableConfig.MarioAnimations.GetAnimationFromName(animationName);
                    if (animation.HasValue)
                    {
                        uint marioObjRef = Config.Stream.GetUInt(MarioObjectConfig.PointerAddress);
                        Config.Stream.SetValue((short)animation.Value, marioObjRef + MarioObjectConfig.AnimationOffset);
                    }
                });
            selectionForm.Show();
        }

        public static int? GetAnimation(string firstText, string secondText)
        {
            SelectionForm selectionForm = new SelectionForm();
            selectionForm.Initialize(
                firstText,
                secondText,
                TableConfig.MarioAnimations.GetAnimationNameList(),
                animationName => { });
            if (selectionForm.ShowDialog() == DialogResult.OK)
            {
                string animationName = selectionForm.Selection as string;
                int? animationIndex = TableConfig.MarioAnimations.GetAnimationFromName(animationName);
                return animationIndex;
            }
            return null;
        }

        public static void ShowTriangleTypeDescriptionSelectionForm()
        {
            SelectionForm selectionForm = new SelectionForm();
            selectionForm.Initialize(
                "Select a Triangle Type",
                "Set Triangle Type",
                TableConfig.TriangleInfo.GetAllDescriptions(),
                triangleTypeDescription =>
                {
                    short? triangleType = TableConfig.TriangleInfo.GetType(triangleTypeDescription);
                    if (triangleType.HasValue)
                    {
                        foreach (uint triangleAddress in Config.TriangleManager.TriangleAddresses)
                        {
                            Config.Stream.SetValue(
                                triangleType.Value,
                                triangleAddress + TriangleOffsetsConfig.SurfaceType);
                        }
                    }
                });
            selectionForm.Show();
        }

        public static void ShowDemoCounterDescriptionSelectionForm()
        {
            SelectionForm selectionForm = new SelectionForm();
            selectionForm.Initialize(
                "Select a Demo Counter",
                "Set Demo Counter",
                DemoCounterUtilities.GetDescriptions(),
                demoCounterDescription =>
                {
                    short? demoCounter = DemoCounterUtilities.GetDemoCounter(demoCounterDescription);
                    if (demoCounter.HasValue)
                    {
                        Config.Stream.SetValue(demoCounter.Value, MiscConfig.DemoCounterAddress);
                    }
                });
            selectionForm.Show();
        }

        public static void ShowTtcSpeedSettingDescriptionSelectionForm()
        {
            SelectionForm selectionForm = new SelectionForm();
            selectionForm.Initialize(
                "Select a TTC Speed Setting",
                "Set TTC Speed Setting",
                TtcSpeedSettingUtilities.GetDescriptions(),
                ttcSpeedSettingDescription =>
                {
                    short? ttcSpeedSetting = TtcSpeedSettingUtilities.GetTtcSpeedSetting(ttcSpeedSettingDescription);
                    if (ttcSpeedSetting.HasValue)
                    {
                        Config.Stream.SetValue(ttcSpeedSetting.Value, MiscConfig.TtcSpeedSettingAddress);
                    }
                });
            selectionForm.Show();
        }

        public static void ShowAreaTerrainDescriptionSelectionForm()
        {
            SelectionForm selectionForm = new SelectionForm();
            selectionForm.Initialize(
                "Select a Terrain Type",
                "Set Terrain Type",
                AreaUtilities.GetDescriptions(),
                terrainTypeDescription =>
                {
                    short? terrainType = AreaUtilities.GetTerrainType(terrainTypeDescription);
                    if (terrainType.HasValue)
                    {
                        Config.Stream.SetValue(
                            terrainType.Value,
                            Config.AreaManager.SelectedAreaAddress + AreaConfig.TerrainTypeOffset);
                    }
                });
            selectionForm.Show();
        }

        public static void ShowMap3DModeSelectionForm()
        {
            SelectionForm selectionForm = new SelectionForm();
            selectionForm.Initialize(
                "Select a Mode",
                "Set Mode",
                EnumUtilities.GetEnumStrings<Map3DCameraMode>(typeof(Map3DCameraMode)),
                modeString =>
                {
                    try
                    {
                        MapConfig.Map3DMode = (Map3DCameraMode)Enum.Parse(typeof(Map3DCameraMode), modeString, true);
                    }
                    catch (Exception) { }
                });
            selectionForm.Show();
        }

        public static void ShowCompassPositionSelectionForm()
        {
            SelectionForm selectionForm = new SelectionForm();
            selectionForm.Initialize(
                "Select a Compass Position",
                "Set Compass Position",
                EnumUtilities.GetEnumStrings<CompassPosition>(typeof(CompassPosition)),
                stringValue =>
                {
                    try
                    {
                        MapConfig.CompassPosition = (CompassPosition)Enum.Parse(typeof(CompassPosition), stringValue, true);
                    }
                    catch (Exception) { }
                });
            selectionForm.Show();
        }

        public static void ShowDataManagerSelectionForm(List<WatchVariableControl> controls, AddToTabTypeEnum addToTabType)
        {
            SelectionForm selectionForm = new SelectionForm();
            selectionForm.Initialize(
                "Select a Tab",
                "Add Variable(s) to Tab",
                Config.GetVariableAdders(),
                variableAdder => WatchVariableControl.AddVarsToTab(controls, variableAdder, addToTabType));
            selectionForm.Show();
        }
    }
}
