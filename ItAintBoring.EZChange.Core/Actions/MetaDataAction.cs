﻿using ItAintBoring.EZChange.Common;
using ItAintBoring.EZChange.Common.Packaging;
using ItAintBoring.EZChange.Core.Packaging;
using ItAintBoring.EZChange.Core.UI;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace ItAintBoring.EZChange.Core.Actions
{
    public class MetaDataAction : BaseAction
    {

        public override string Version { get { return "1.0"; } }
        public override string Id { get { return "Delete Action"; } }
        public override string Description { get { return "Delete Action"; } }

        public override string Name { get; set; }

        private List<Type> supportedSolutionTypes = null;
        [XmlIgnore]
        public override List<Type> SupportedSolutionTypes { get { return supportedSolutionTypes; } }

        public MetaDataAction(): base()
        {
            supportedSolutionTypes = new List<Type>();
            supportedSolutionTypes.Add(typeof(DynamicsSolution));
            XML = @"<actions><action target=""entity/attribute/workflow/businessrule/webresource/record/globaloptionset/optionsetvalue/globaloptionsetvalue"" attribute="""" entity="""" plugin="""" recordid="""" name="""" value="""" errorIfMissing=""false""/></actions>";
        }

        public string XML { get; set; }
  
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


        public override void DoAction(BaseSolution solution)
        {
            ActionStarted();
            DynamicsSolution ds = (DynamicsSolution)solution;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(XML);
            var actions = doc.GetElementsByTagName("action");
            foreach (XmlNode a in actions)
            {
                try
                {
                    switch (a.Attributes["target"].Value)
                    {
                        case "attribute":
                            DeleteAttributeRequest dar = new DeleteAttributeRequest();
                            dar.EntityLogicalName = a.Attributes["entity"].Value;
                            dar.LogicalName = a.Attributes["attribute"].Value;
                            ds.Service.Service.Execute(dar);
                            break;
                        case "entity":
                            DeleteEntityRequest der = new DeleteEntityRequest();
                            der.LogicalName = a.Attributes["entity"].Value;
                            ds.Service.Service.Execute(der);
                            break;
                        case "pluginstep":
                            break;
                        case "plugin":
                            break;
                        case "businessrule": case "workflow":
                            try
                            {
                                SetStateRequest deactivateRequest = new SetStateRequest
                                {
                                    EntityMoniker =
                                            new EntityReference("workflow", Guid.Parse(a.Attributes["recordid"].Value)),
                                    State = new OptionSetValue(0),
                                    Status = new OptionSetValue(1)
                                };
                                ds.Service.Service.Execute(deactivateRequest);
                                ds.Service.Service.Delete("workflow", Guid.Parse(a.Attributes["recordid"].Value));
                            }
                            catch(Exception ex)
                            {
                                if(!ex.Message.ToUpper().Contains(a.Attributes["recordid"].Value.ToUpper()))
                                {
                                    throw;
                                }
                                //Ignore if the ID is there - likely "does not exist" error
                                //May need an extra attribute to decide if to ignore or not
                            }
                            break;
                        case "webresource":
                            ds.Service.Service.Delete("webresrouce", Guid.Parse(a.Attributes["recordid"].Value));
                            break;
                        case "record":
                            ds.Service.Service.Delete(a.Attributes["entity"].Value, Guid.Parse(a.Attributes["recordid"].Value));
                            break;
                        case "globaloptionset":
                            ds.Service.Service.Execute(new DeleteOptionSetRequest
                            {
                                Name = a.Attributes["name"].Value
                            });
                            break;
                        case "globaloptionsetvalue":
                            ds.Service.Service.Execute(new DeleteOptionValueRequest
                            {
                                OptionSetName = a.Attributes["name"].Value,
                                Value = int.Parse(a.Attributes["value"].Value)
                            });
                            break;
                        case "optionsetvalue":
                            ds.Service.Service.Execute(new DeleteOptionValueRequest
                            {
                                EntityLogicalName = a.Attributes["entity"].Value,
                                AttributeLogicalName = a.Attributes["attribute"].Value,
                                Value = int.Parse(a.Attributes["value"].Value)
                            });
                            break;
                    }
                }
                catch (Exception ex)
                {
                    if (!ex.Message.ToLower().Contains("could not find") ||
                        a.Attributes["errorIfMissing"].Value == "true") throw; //Ignore if the "artefact" does not exist
                }
            }
            ActionCompleted();
        }

        public override void UpdateRuntimeData(System.Collections.Hashtable values)
        {
            XML = ReplaceVariables(XML, values);
        }
    }
}
/*
 <action target="entity/attribute/workflow/pluginstep/plugin/businessrule/webresource/record" attribute="" entity="" plugin="" recordid=""/>
  * */
