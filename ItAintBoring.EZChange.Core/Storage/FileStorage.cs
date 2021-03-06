﻿using ItAintBoring.EZChange.Common.Packaging;
using ItAintBoring.EZChange.Common.Storage;
using ItAintBoring.EZChange.Core.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ItAintBoring.EZChange.Core.Storage
{
    public class FileStorage : IPackageStorage
    {

        public List<Type> KnownTypes { get; set; }

        public string Name { get { return "File System"; } }

        public string Description { get { return "File System Storage"; } }

        public string Version { get { return "1.0.0.0"; } }

        public object PackageFactory { get; private set; }

        public BaseChangePackage LoadPackage(string location = null)
        {
            BaseChangePackage result = null;
            if (location == null)
            {
                using (var fd = new System.Windows.Forms.OpenFileDialog())
                {
                    fd.DefaultExt = "ecp";
                    fd.Filter = "EZChange Files (*.ecp)|*.ecp|All files (*.*)|*.*";
                    fd.FilterIndex = 1;
                    if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        location = fd.FileName;
                    }
                    else return result;
                }
            }
            if(location != null)
            {
                XmlSerializer ser = new XmlSerializer(typeof(BaseChangePackage), KnownTypes.ToArray());
                TextReader reader = new StreamReader(location);
                result = (BaseChangePackage)ser.Deserialize(reader);
                result.PackageLocation = location;
                result.InitializeComponents();
                reader.Close();
            }
            if(result != null)
            {
                foreach(var s in result.Solutions)
                {
                    s.Package = result;
                }
            }

            return result;
        }

        public bool SavePackageAs(BaseChangePackage package)
        {
            if (package == null) return true;
            using (var fd = new System.Windows.Forms.SaveFileDialog())
            {
                fd.DefaultExt = "ecp";
                fd.Filter = "EZChange Files (*.ecp)|*.ecp|All files (*.*)|*.*";
                fd.FilterIndex = 1;
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    package.PackageLocation = fd.FileName;
                    SavePackage(package);
                    return true;
                }
                else return false;
            }

        }

        public void AddKnownTypes(List<Type> knownTypes)
        {
            KnownTypes = new List<Type>();
            KnownTypes.AddRange(knownTypes);
        }

        public bool SavePackage(BaseChangePackage package, string location = null)
        {
            if (package == null) return true;
            location = location == null ? package.PackageLocation : location;
            if (location == null)
            {
                return SavePackageAs(package);
            }
            else
            {
                XmlSerializer ser = new XmlSerializer(typeof(BaseChangePackage), KnownTypes.ToArray());
                TextWriter writer = new StreamWriter(location);
                ser.Serialize(writer, package);
                writer.Close();
                package.HasUnsavedChanges = false;
                return true;
            }
        }
    }
}
