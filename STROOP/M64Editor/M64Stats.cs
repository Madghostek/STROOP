﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using STROOP.Structs;
using System.ComponentModel;
using STROOP.Utilities;
using System.Windows.Forms;
using STROOP.Forms;

namespace STROOP.M64Editor
{
    public class M64Stats
    {
        private readonly M64Header _header;
        private readonly BindingList<M64InputFrame> _inputs;

        [Category("\u200B\u200BMain Button Presses"), DisplayName("\u200B\u200B\u200B\u200BNum A Presses")]
        public int NumAPresses
        {
            get { return FindPresses(input => input.A).Count; }
            set { SetNumPreses(value, input => input.A = false); }
        }

        [Category("\u200B\u200BMain Button Presses"), DisplayName("\u200B\u200B\u200BNum B Presses")]
        public int NumBPresses
        {
            get { return FindPresses(input => input.B).Count; }
            set { SetNumPreses(value, input => input.B = false); }
        }

        [Category("\u200B\u200BMain Button Presses"), DisplayName("\u200B\u200BNum Z Presses")]
        public int NumZPresses
        {
            get { return FindPresses(input => input.Z).Count; }
            set { SetNumPreses(value, input => input.Z = false); }
        }

        [Category("\u200B\u200BMain Button Presses"), DisplayName("\u200BNum Start Presses")]
        public int NumStartPresses
        {
            get { return FindPresses(input => input.Start).Count; }
            set { SetNumPreses(value, input => input.Start = false); }
        }

        [Category("\u200B\u200BMain Button Presses"), DisplayName("Num R Presses")]
        public int NumRPresses
        {
            get { return FindPresses(input => input.R).Count; }
            set { SetNumPreses(value, input => input.R = false); }
        }

        [Category("\u200BC Button Presses"), DisplayName("\u200B\u200B\u200BNum C^ Presses")]
        public int NumCUpPresses
        {
            get { return FindPresses(input => input.C_Up).Count; }
            set { SetNumPreses(value, input => input.C_Up = false); }
        }

        [Category("\u200BC Button Presses"), DisplayName("\u200B\u200BNum Cv Presses")]
        public int NumCDownPresses
        {
            get { return FindPresses(input => input.C_Down).Count; }
            set { SetNumPreses(value, input => input.C_Down = false); }
        }

        [Category("\u200BC Button Presses"), DisplayName("\u200BNum C< Presses")]
        public int NumCLeftPresses
        {
            get { return FindPresses(input => input.C_Left).Count; }
            set { SetNumPreses(value, input => input.C_Left = false); }
        }

        [Category("\u200BC Button Presses"), DisplayName("Num C> Presses")]
        public int NumCRightPresses
        {
            get { return FindPresses(input => input.C_Right).Count; }
            set { SetNumPreses(value, input => input.C_Right = false); }
        }

        [Category("Noop Button Presses"), DisplayName("\u200B\u200B\u200B\u200BNum L Presses")]
        public int NumLPresses
        {
            get { return FindPresses(input => input.L).Count; }
            set { SetNumPreses(value, input => input.L = false); }
        }

        [Category("Noop Button Presses"), DisplayName("\u200B\u200B\u200BNum D^ Presses")]
        public int NumDUpPresses
        {
            get { return FindPresses(input => input.D_Up).Count; }
            set { SetNumPreses(value, input => input.D_Up = false); }
        }

        [Category("Noop Button Presses"), DisplayName("\u200B\u200BNum Dv Presses")]
        public int NumDDownPresses
        {
            get { return FindPresses(input => input.D_Down).Count; }
            set { SetNumPreses(value, input => input.D_Down = false); }
        }

        [Category("Noop Button Presses"), DisplayName("\u200BNum D< Presses")]
        public int NumDLeftPresses
        {
            get { return FindPresses(input => input.D_Left).Count; }
            set { SetNumPreses(value, input => input.D_Left = false); }
        }

        [Category("Noop Button Presses"), DisplayName("Num D> Presses")]
        public int NumDRightPresses
        {
            get { return FindPresses(input => input.D_Right).Count; }
            set { SetNumPreses(value, input => input.D_Right = false); }
        }

        public M64Stats(M64Header header, BindingList<M64InputFrame> inputs)
        {
            _header = header;
            _inputs = inputs;
        }

        private List<(int, int)> FindPresses(Func<M64InputFrame, bool> isPressedFunction)
        {
            List<(int, int)> pressList = new List<(int, int)>();
            int startFrame = 0;
            bool isAlreadyPressed = false;

            for (int i = 0; i <= _inputs.Count; i++)
            {
                bool isCurrentlyPressed = i == _inputs.Count ?
                    false : isPressedFunction(_inputs[i]);
                if (isAlreadyPressed)
                {
                    if (isCurrentlyPressed) // still pressing
                    {
                        // do nothing
                    }
                    else // just stopped pressing
                    {
                        int endFrame = i - 1;
                        pressList.Add((startFrame, endFrame));
                        isAlreadyPressed = false;
                    }
                }
                else
                {
                    if (isCurrentlyPressed) // just started pressing
                    {
                        startFrame = i;
                        isAlreadyPressed = true;
                    }
                    else // still not pressing
                    {
                        // do nothing
                    }
                }
            }

            return pressList;
        }

        private void SetNumPreses(int numPresses, Action<M64InputFrame> unpressFunction)
        {
            if (numPresses != 0) return;
            for (int i = 0; i < _inputs.Count; i++)
            {
                unpressFunction(_inputs[i]);
            }
        }

        private static readonly List<Func<M64InputFrame, bool>> isPressedFunctionList =
            new List<Func<M64InputFrame, bool>>()
            {
                input => input.A,
                input => input.B,
                input => input.Z,
                input => input.Start,
                input => input.R,
                input => input.C_Up,
                input => input.C_Down,
                input => input.C_Left,
                input => input.C_Right,
                input => input.L,
                input => input.D_Up,
                input => input.D_Down,
                input => input.D_Left,
                input => input.D_Right,
            };

        private static readonly List<string> buttonNameList =
            new List<string>()
            {
                "A", "B", "Z", "Start", "R",
                "C^", "Cv", "C<", "C>",
                "L", "D^", "Dv", "D<", "D>",
            };

        public ContextMenuStrip CreateContextMenuStrip()
        {
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            List<string> itemNames = buttonNameList.ConvertAll(
                buttonName => "Show all " + buttonName + " presses");
            List<ToolStripMenuItem> items = itemNames.ConvertAll(
                itemName => new ToolStripMenuItem(itemName));

            if (items.Count != isPressedFunctionList.Count)
                throw new ArgumentOutOfRangeException();

            for (int i = 0; i < items.Count; i++)
            {
                int index = i;
                items[index].Click += (sender, e) =>
                {
                    List<(int, int)> buttonPresses = FindPresses(isPressedFunctionList[index]);
                    string buttonPressesString =
                        FormatButtonPressesString(buttonPresses, buttonNameList[index]);
                    InfoForm.ShowText(
                        "Num Button Presses",
                        String.Format("Num {0} Presses", buttonNameList[index]),
                        buttonPressesString);
                };
            }

            return contextMenuStrip;
        }

        private string FormatButtonPressesString(List<(int, int)> buttonPresses, string buttonName)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format(
                "{0} {1} presses total:", buttonPresses.Count, buttonName));
            for (int i = 0; i < buttonPresses.Count; i++)
            {
                int countIndex = i + 1;
                (int startFrame, int endFrame) = buttonPresses[i];
                int frameSpan = endFrame - startFrame + 1;
                string pluralitySuffix = frameSpan == 1 ? "" : "s";
                builder.Append(String.Format(
                    "{0} press #{1}: frame {2} to frame {3} ({4} frame{5})",
                    buttonName, countIndex, startFrame, endFrame, frameSpan, pluralitySuffix));
            }
            return builder.ToString();
        }
    }
}
