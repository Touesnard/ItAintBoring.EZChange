﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ItAintBoring.EZChange.Common.Packaging
{
    public abstract class BaseSolution: BaseComponent
    {
        [XmlIgnore]
        public BaseChangePackage Package { get; set; }

        public List<BaseAction> BuildActions { get; set; }
        public List<BaseAction> DeployActions { get; set; }


        public BaseSolution(): base()
        {

            BuildActions = new List<BaseAction>();
            DeployActions = new List<BaseAction>();

        }

        [XmlIgnore]
        abstract public List<Type> SupportedPackageTypes { get; }

        public abstract void PrepareSolution(BaseComponent package);

        public virtual void SaveActionData(BaseAction action, string data)
        {

        }

        public virtual string LoadActionData(BaseAction action, string fileName)
        {
            return null;
        }
        public abstract void DeploySolution(BaseComponent package, BaseAction selectedAction);

        public void ProcessingStarted()
        {
            LogInfo("Solution: " + this.Name);
        }

        public void ProcessingCompleted()
        {
            //No need to log
        }

        public override void UpdateRuntimeData(Hashtable values)
        {
            foreach (var a in BuildActions)
            {
                a.UpdateRuntimeData(values);
            }
            foreach (var a in DeployActions)
            {
                a.UpdateRuntimeData(values);
            }
        }

        public BaseAction FindAction(Guid actionId)
        {
          
            if (Package != null)
            {
                return Package.FindAction(actionId);
            }
            return null;
        }

        public virtual void InitializeComponents()
        {
            foreach (var a in BuildActions)
            {
                a.Solution = this;
            }
            foreach (var a in DeployActions)
            {
                a.Solution = this;
            }
        }
    }
}
