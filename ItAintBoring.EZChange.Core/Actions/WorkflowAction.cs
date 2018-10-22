﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using ItAintBoring.EZChange.Common;
using ItAintBoring.EZChange.Core.Packaging;
using ItAintBoring.EZChange.Core.UI;

namespace ItAintBoring.EZChange.Core.Actions
{
    public class WorkflowAction : BaseAction
    {
        public override string Version { get { return "1.0"; } }
        public override string Id { get { return "Workflow Action"; } }
        public override string Description { get { return "Workflow Action"; } }

        public override string Name { get; set; }

        private List<Type> supportedSolutionTypes = null;
        [XmlIgnore]
        public override List<Type> SupportedSolutionTypes { get { return supportedSolutionTypes; } }

        public WorkflowAction(): base()
        {
            supportedSolutionTypes = new List<Type>();
            supportedSolutionTypes.Add(typeof(DynamicsSolution));
        }

        
        public string Title { get; set; }


        public override void ApplyUIUpdates()
        {
            XML = ((XMLEditor)uiControl).XML;
        }

        private UserControl uiControl = new XMLEditor();
        [XmlIgnore]
        public override UserControl UIControl
        {
            get
            {
                ((XMLEditor)uiControl).XML = XML;
                return uiControl;
            }
        }



        public override void DoAction()
        {
            throw new NotImplementedException();
        }
    
    }
}
