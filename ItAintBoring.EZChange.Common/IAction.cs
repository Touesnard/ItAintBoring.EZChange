﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ItAintBoring.EZChange.Common
{
    public interface IAction
    {
                
        void DoAction();
        List<Type> SupportedSolutionTypes { get; }

    }
}
