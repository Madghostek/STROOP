﻿using STROOP.Forms;
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
using System.Xml.Linq;

namespace STROOP.Controls
{
    public class ObjectSlotFlowLayoutPanel : NoTearFlowLayoutPanel
    {
        public ObjectSlotFlowLayoutPanel()
        {
            ToolStripMenuItem itemSelectMarkedSlots = new ToolStripMenuItem("Select Marked Slots");
            itemSelectMarkedSlots.Click += (sender, e) =>
            {
                Config.ObjectSlotsManager.SelectedSlotsAddresses.Clear();
                Config.ObjectSlotsManager.SelectedSlotsAddresses.AddRange(Config.ObjectSlotsManager.MarkedSlotsAddresses);
            };

            ToolStripMenuItem itemSelectCopiedAddress = new ToolStripMenuItem("Select Copied Address");
            itemSelectCopiedAddress.Click += (sender, e) =>
            {
                uint? address = ParsingUtilities.ParseHexNullable(Clipboard.GetText());
                if (address.HasValue) Config.ObjectSlotsManager.SelectSlotByAddress(address.Value);
            };

            ToolStripMenuItem itemClearMarkedSlots = new ToolStripMenuItem("Clear Marked Slots");
            itemClearMarkedSlots.Click += (sender, e) =>
            {
                Config.ObjectSlotsManager.MarkedSlotsAddresses.Clear();
                Config.ObjectSlotsManager.MarkedSlotsAddressesDictionary.Clear();
            };

            ToolStripMenuItem itemClearSelectedSlots = new ToolStripMenuItem("Clear Selected Slots");
            itemClearSelectedSlots.Click += (sender, e) =>
            {
                Config.ObjectSlotsManager.SelectedSlotsAddresses.Clear();
            };

            ToolStripMenuItem itemUnloadAllButMarkedSlots = new ToolStripMenuItem("Unload All but Marked Slots");
            itemUnloadAllButMarkedSlots.Click += (sender, e) =>
            {
                List<ObjectDataModel> objsToUnload =
                    DataModels.ObjectProcessor.Objects.ToList().FindAll(
                        obj => !Config.ObjectSlotsManager.MarkedSlotsAddresses.Contains(obj.Address));
                ButtonUtilities.UnloadObject(objsToUnload);
            };

            ToolStripMenuItem itemDisplayAsRow = new ToolStripMenuItem("Display as Row");
            itemDisplayAsRow.Click += (sender, e) =>
            {
                WrapContents = !WrapContents;
                itemDisplayAsRow.Checked = !WrapContents;
                ResetSlots();
            };

            ContextMenuStrip = new ContextMenuStrip();
            ContextMenuStrip.Items.Add(itemSelectMarkedSlots);
            ContextMenuStrip.Items.Add(itemSelectCopiedAddress);
            ContextMenuStrip.Items.Add(itemClearMarkedSlots);
            ContextMenuStrip.Items.Add(itemClearSelectedSlots);
            ContextMenuStrip.Items.Add(itemUnloadAllButMarkedSlots);
            ContextMenuStrip.Items.Add(itemDisplayAsRow);
        }

        private void ResetSlots()
        {
            List<Control> controls = new List<Control>();
            foreach (Control control in Controls)
            {
                controls.Add(control);
            }
            while (Controls.Count > 0)
            {
                Controls.RemoveAt(0);
            }
            foreach (Control control in controls)
            {
                Controls.Add(control);
            }
        }
    }
}
