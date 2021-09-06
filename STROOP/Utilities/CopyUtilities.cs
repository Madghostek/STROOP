﻿using STROOP.Controls;
using STROOP.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace STROOP.Utilities
{
    public static class CopyUtilities
    {
        public static void Copy(List<WatchVariableControl> vars, CopyTypeEnum copyType)
        {
            int index = EnumUtilities.GetEnumValues<CopyTypeEnum>(typeof(CopyTypeEnum)).IndexOf(copyType);
            GetCopyActions(() => vars)[index]();
        }

        public static void AddContextMenuStripFunctions(
            Control control, Func<List<WatchVariableControl>> getVars)
        {
            ControlUtilities.AddContextMenuStripFunctions(
                control,
                GetCopyNames(),
                GetCopyActions(getVars));
        }

        public static void AddDropDownItems(
            ToolStripMenuItem control, Func<List<WatchVariableControl>> getVars)
        {
            ControlUtilities.AddDropDownItems(
                control,
                GetCopyNames(),
                GetCopyActions(getVars));
        }

        public static List<string> GetCopyNames()
        {
            return new List<string>()
            {
                "Copy with Commas",
                "Copy with Spaces",
                "Copy with Tabs",
                "Copy with Line Breaks",
                "Copy with Commas and Spaces",
                "Copy with Names",
                "Copy as Table",
                "Copy for Code",
            };
        }

        public static List<Action> GetCopyActions(Func<List<WatchVariableControl>> getVars)
        {
            return new List<Action>()
            {
                () => CopyWithSeparator(getVars(), ","),
                () => CopyWithSeparator(getVars(), " "),
                () => CopyWithSeparator(getVars(), "\t"),
                () => CopyWithSeparator(getVars(), "\r\n"),
                () => CopyWithSeparator(getVars(), ", "),
                () => CopyWithNames(getVars()),
                () => CopyAsTable(getVars()),
                () => CopyForCode(getVars()),
            };
        }

        public static void CopyWithSeparator(
            List<WatchVariableControl> controls, string separator)
        {
            if (controls.Count == 0) return;
            Clipboard.SetText(
                string.Join(separator, controls.ConvertAll(
                    control => control.GetValue(
                        useRounding: false, handleFormatting: true))));
            controls.ForEach(control => control.FlashColor(WatchVariableControl.COPY_COLOR));
        }

        public static void CopyWithNames(List<WatchVariableControl> controls)
        {
            if (controls.Count == 0) return;
            List<string> lines = controls.ConvertAll(
                watchVar => watchVar.VarName + "\t" + watchVar.GetValue(false));
            Clipboard.SetText(string.Join("\r\n", lines));
            controls.ForEach(control => control.FlashColor(WatchVariableControl.COPY_COLOR));
        }

        public static void CopyAsTable(List<WatchVariableControl> controls)
        {
            if (controls.Count == 0) return;
            List<uint> addresses = controls[0].GetBaseAddresses();
            if (addresses.Count == 0) return;
            List<string> hexAddresses = addresses.ConvertAll(address => HexUtilities.FormatValue(address));
            string header = "Vars\t" + string.Join("\t", hexAddresses);

            List<string> names = controls.ConvertAll(control => control.VarName);
            List<List<object>> valuesTable = controls.ConvertAll(control => control.GetValues());
            List<string> valuesStrings = new List<string>();
            for (int i = 0; i < names.Count; i++)
            {
                string line = names[i] + "\t" + string.Join("\t", valuesTable[i]);
                valuesStrings.Add(line);
            }

            string output = header + "\r\n" + string.Join("\r\n", valuesStrings);
            Clipboard.SetText(output);
            controls.ForEach(control => control.FlashColor(WatchVariableControl.COPY_COLOR));
        }

        public static void CopyForCode(List<WatchVariableControl> controls, string dialogString = null)
        {
            if (controls.Count == 0) return;
            Func<string, string> varNameFunc;
            if (dialogString != null || KeyboardUtilities.IsCtrlHeld())
            {
                string template = dialogString ?? DialogUtilities.GetStringFromDialog("$");
                if (template == null) return;
                varNameFunc = varName => template.Replace("$", varName);
            }
            else
            {
                varNameFunc = varName => varName;
            }
            List<string> lines = new List<string>();
            foreach (WatchVariableControl watchVar in controls)
            {
                Type type = watchVar.GetMemoryType();
                string line = string.Format(
                    "{0} {1} = {2}{3};",
                    type != null ? TypeUtilities.TypeToString[type] : "double",
                    varNameFunc(watchVar.VarName.Replace(" ", "")),
                    watchVar.GetValue(false),
                    type == typeof(float) ? "f" : "");
                lines.Add(line);
            }
            if (lines.Count > 0)
            {
                Clipboard.SetText(string.Join("\r\n", lines));
                controls.ForEach(control => control.FlashColor(WatchVariableControl.COPY_COLOR));
            }
        }
    }
}
