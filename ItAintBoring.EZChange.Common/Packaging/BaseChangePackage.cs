﻿using ItAintBoring.EZChange.Common.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ItAintBoring.EZChange.Common.Packaging
{
    public abstract class BaseChangePackage : BaseComponent
    {
        

        public string PackageLocation { get; set; } //Storage specific
        public List<BaseSolution> Solutions { get; set; }

        private List<Variable> variables = null;
        public virtual List<Variable> Variables
        {
            get
            {
                if(variables == null) variables = new List<Variable>();
                return variables;
            }
        }

        private bool hasUnsavedChanges = false;
        [XmlIgnore]
        virtual public bool HasUnsavedChanges
        {
            get
            {
                return hasUnsavedChanges;
            }
            set
            {
                hasUnsavedChanges = value;
            }
        }

        public BaseChangePackage(): base()
        {
            Solutions = new List<BaseSolution>();
            HasUnsavedChanges = false;
        }

        public override string GetDataFolder()
        {
            return System.IO.Path.Combine(System.IO.Path.GetFullPath(PackageLocation), "Solutions");
        }

        public virtual void Run()
        {
            foreach (var s in Solutions)
            {
                s.DeploySolution(this);
            }
        }

        public virtual void Build(IPackageStorage storage)
        {
                
            System.IO.Directory.CreateDirectory(GetDataFolder());
            System.IO.Directory.Delete(GetDataFolder(), true);
            System.IO.Directory.CreateDirectory(GetDataFolder());
            System.Threading.Thread.Sleep(500);//The files don't disappear right away it seems
            storage.SavePackage(this);//, System.IO.Path.Combine(GetDataFolder(), Name + ".ecp"));
            foreach (var s in Solutions)
            {
                s.PrepareSolution(this);
            }
            
        }

    }
}
