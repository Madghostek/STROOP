﻿using STROOP.Managers;
using STROOP.Structs.Configurations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STROOP.Structs
{
    public static class CorkBoxUtilities
    {
        public static (float y, int numFrames) GetNumFrames(double x, double z)
        {
            return (1, (int)x);
        }
    }
}
