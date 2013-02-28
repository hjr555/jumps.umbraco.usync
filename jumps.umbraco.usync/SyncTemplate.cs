﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml ;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.template;

using umbraco.BusinessLogic; 

using System.IO; 
using Umbraco.Core.IO; 

//  Check list
// ====================
//  SaveOne         X
//  SaveAll         X
//  OnSave          X
//  OnDelete        X
//  ReadFromDisk    X

namespace jumps.umbraco.usync
{
    public class SyncTemplate
    {
        public static void SaveToDisk(Template item)
        {
            if (item != null)
            {
                try
                {
                    XmlDocument xmlDoc = helpers.XmlDoc.CreateDoc();
                    xmlDoc.AppendChild(item.ToXml(xmlDoc));
                    helpers.XmlDoc.SaveXmlDoc(
                        item.GetType().ToString() + GetDocPath(item),
                        item.Text,
                        xmlDoc);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("error saving template {0}", item.Text), ex);
                }
            }
        }

        public static void SaveAllToDisk()
        {
            foreach (Template item in Template.GetAllAsList().ToArray())
            {
                SaveToDisk(item);                
            }
        }

        private static string GetDocPath(Template item)
        {
            string path = "";
            if (item != null)
            {
                if (item.MasterTemplate != 0)
                {
                    path = GetDocPath(new Template(item.MasterTemplate));
                }

                path = string.Format("{0}//{1}", path, helpers.XmlDoc.ScrubFile(item.Text));
            }
            return path;
        }

        public static void ReadAllFromDisk()
        {
            string path = IOHelper.MapPath(string.Format("{0}{1}",
                helpers.uSyncIO.RootFolder,
                "umbraco.cms.businesslogic.template.Template"));

            ReadFromDisk(path);
        }

        public static void ReadFromDisk(string path)
        {
            if (Directory.Exists(path))
            {
                User user = new User(0); 

                foreach (string file in Directory.GetFiles(path, "*.config"))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(file);

                    XmlNode node = xmlDoc.SelectSingleNode("//Template");

                    if (node != null)
                    {
                       Template.Import(node,user); 
                    }
                }

                foreach (string folder in Directory.GetDirectories(path))
                {
                    ReadFromDisk(folder);
                }
            }
        }

        public static void AttachEvents()
        {
            Template.AfterSave += Template_AfterSave;
            Template.AfterDelete += Template_AfterDelete;

        }

        static void Template_AfterDelete(Template sender, DeleteEventArgs e)
        {
            helpers.XmlDoc.ArchiveFile(sender.GetType().ToString() + GetDocPath(sender), sender.Text);

            e.Cancel = false; 
        }

        static void Template_AfterSave(Template sender, SaveEventArgs e)
        {
            // save
            SaveToDisk(sender);
        }
        
    }
}