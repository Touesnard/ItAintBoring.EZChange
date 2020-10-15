using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using ItAintBoring.EZChange.Common;
using ItAintBoring.EZChange.Common.Packaging;
using ItAintBoring.EZChange.Core.Packaging;
using ItAintBoring.EZChange.Core.UI;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace ItAintBoring.EZChange.Core.Actions
{
    public class WorkflowAction : BaseAction
    {
        public override string Version { get { return "1.0"; } }
        public override string Id { get { return "Workflow Action"; } }
        public override string Description { get { return "Workflow Action"; } }
        
        public string WorkflowId { get; set; }
        public string FetchXml { get; set; }

        

        public override string Name { get; set; }

        private List<Type> supportedSolutionTypes = null;
        [XmlIgnore]
        public override List<Type> SupportedSolutionTypes { get { return supportedSolutionTypes; } }

        public WorkflowAction(): base()
        {
            supportedSolutionTypes = new List<Type>();
            supportedSolutionTypes.Add(typeof(DynamicsSolution));
        }

        
        public override void ApplyUIUpdates()
        {
            WorkflowId = ((WorkflowActionEditor)uiControl).WorkflowId;
            FetchXml = ((WorkflowActionEditor)uiControl).FetchXml;
        }

        private UserControl uiControl = new WorkflowActionEditor();
        [XmlIgnore]
        public override UserControl UIControl
        {
            get
            {
                ((WorkflowActionEditor)uiControl).WorkflowId = WorkflowId;
                ((WorkflowActionEditor)uiControl).FetchXml = FetchXml;
                return uiControl;
            }
        }



        public override void DoAction(BaseSolution solution)
        {
            ActionStarted();
            DynamicsSolution ds = (DynamicsSolution)solution;
            ds.Service.PublishAll();
            if (!String.IsNullOrEmpty(FetchXml) && !String.IsNullOrEmpty(WorkflowId))
            {
                // get all lines of fetch
                var fetchLines = FetchXml.Split('<');

                //replace first line <fetch.... with <fetch {0}>
                for (int i = 0; i < fetchLines.Length; i++)
                {
                    if(fetchLines[i].Contains("fetch"))
                    {
                        fetchLines[i] = "fetch {0}>";

                        break;
                    }
                }

                FetchXml = String.Join("<", fetchLines);

                bool moreRecords;
                int page = 1;
                string cookie = string.Empty;
                int totalRecords = 0;

                int batch = 1000;//int.Parse(ConfigurationManager.AppSettings["batch"]);

                ExecuteMultipleRequest emr = new ExecuteMultipleRequest()
                {
                    Requests = new OrganizationRequestCollection(),
                    Settings = new ExecuteMultipleSettings()
                    {
                        ContinueOnError = false,
                        ReturnResponses = false
                    }
                };

                do
                {
                    var xml = string.Format(FetchXml, cookie);
                    var results = ds.Service.Service.RetrieveMultiple(new FetchExpression(xml));
                    totalRecords += results.Entities.Count;

                    foreach (var r in results.Entities)
                    {
                        if (emr.Requests.Count == batch)
                        {
                            ds.Service.Service.Execute(emr);

                            emr = new ExecuteMultipleRequest()
                            {
                                Requests = new OrganizationRequestCollection(),
                                Settings = new ExecuteMultipleSettings()
                                {
                                    ContinueOnError = false,
                                    ReturnResponses = false
                                }
                            };
                        }

                        emr.Requests.Add(new ExecuteWorkflowRequest
                        {
                            EntityId = r.Id,
                            WorkflowId = Guid.Parse(WorkflowId)
                        });
                    }

                    if(emr.Requests.Any())
                    {
                        ds.Service.Service.Execute(emr);
                        emr = new ExecuteMultipleRequest()
                        {
                            Requests = new OrganizationRequestCollection(),
                            Settings = new ExecuteMultipleSettings()
                            {
                                ContinueOnError = false,
                                ReturnResponses = false
                            }
                        };
                    }

                    moreRecords = results.MoreRecords;
                    if (moreRecords)
                    {
                        page++;
                        cookie = string.Format("paging-cookie='{0}' page='{1}'", System.Security.SecurityElement.Escape(results.PagingCookie), page);
                    }
                }
                while (moreRecords);

                LogInfo($"Processed {totalRecords} records");
            }
            ActionCompleted();
        }

        public static List<List<T>> SplitList<T>(List<T> items, int size)
        {
            List<List<T>> list = new List<List<T>>();
            for (int i = 0; i < items.Count; i += size)
            {
                list.Add(items.GetRange(i, Math.Min(size, items.Count - i)));
            }
            return list;
        }

        public override void UpdateRuntimeData(System.Collections.Hashtable values)
        {
            FetchXml = ReplaceVariables(FetchXml, values);
            WorkflowId = ReplaceVariables(WorkflowId, values);
        }
    }
}

