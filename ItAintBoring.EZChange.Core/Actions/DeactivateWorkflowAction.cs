﻿using ItAintBoring.EZChange.Common;
using ItAintBoring.EZChange.Common.Packaging;
using ItAintBoring.EZChange.Core.Packaging;
using ItAintBoring.EZChange.Core.UI;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ItAintBoring.EZChange.Core.Actions
{
    public class DeactivateWorkflowAction : BaseAction
    {
        public override string Version { get { return "1.0"; } }
        public override string Id { get { return "Deactivate Workflow Action"; } }
        public override string Description { get { return "Deactivate Workflow Action"; } }
        public string FetchXml { get; set; }
        public override string Name { get; set; }

        private List<Type> supportedSolutionTypes = null;

        [XmlIgnore]
        public override List<Type> SupportedSolutionTypes { get { return supportedSolutionTypes; } }

        public DeactivateWorkflowAction(): base()
        {
            supportedSolutionTypes = new List<Type>();
            supportedSolutionTypes.Add(typeof(DynamicsSolution));
        }

        public override void ApplyUIUpdates()
        {
            FetchXml = ((XMLEditor)uiControl).XML;
        }

        private UserControl uiControl = new XMLEditor();
        [XmlIgnore]
        public override UserControl UIControl
        {
            get
            {
                ((XMLEditor)uiControl).XML = FetchXml;
                return uiControl;
            }
        }

        public override void DoAction(BaseSolution solution)
        {
            ActionStarted();
            DynamicsSolution ds = (DynamicsSolution)solution;
            if (!String.IsNullOrEmpty(FetchXml) )
            {
                var results = ds.Service.Service.RetrieveMultiple(new FetchExpression(FetchXml));

                foreach (Entity r in results.Entities)
                {
                    var activateRequest = new SetStateRequest
                    {
                        EntityMoniker = r.ToEntityReference(),
                        State = new OptionSetValue(0),
                        Status = new OptionSetValue(1)
                    };
                    LogInfo("Deactivating process: " + r.Id.ToString());
                    ds.Service.Service.Execute(activateRequest);
                }
            }
            ActionCompleted();
        }

        public override void UpdateRuntimeData(System.Collections.Hashtable values)
        {
            FetchXml = ReplaceVariables(FetchXml, values);
        }
    }
}

