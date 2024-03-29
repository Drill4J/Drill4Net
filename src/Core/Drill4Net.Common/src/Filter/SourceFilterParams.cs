﻿using System;
using System.Collections.Generic;

namespace Drill4Net.Common
{
    /// <summary>
    /// Parameters of Filter for the Source Target
    /// </summary>
    [Serializable]
    public class SourceFilterParams
    {
        public List<string> Directories { get; set; }
        public List<string> Folders { get; set; }
        public List<string> Files { get; set; }
        public List<string> Namespaces { get; set; }
        public List<string> Classes { get; set; }
        public List<string> Attributes { get; set; }

        /**********************************************************/

        public int GetRuleCount()
        {
            return GetCount(Directories) + GetCount(Folders) + GetCount(Files) + GetCount(Namespaces) + GetCount(Classes) + GetCount(Attributes);
        }

        private int GetCount(List<string> list)
        {
            return (list?.Count) ?? 0;
        }

        public void AddDirectory(string dir)
        {
            if (Directories == null)
                Directories = new();
            if (string.IsNullOrWhiteSpace(dir) || Directories.Contains(dir))
                return;
            Directories.Add(dir);
        }

        public void AddFolder(string fld)
        {
            if (Folders == null)
                Folders = new();
            if (string.IsNullOrWhiteSpace(fld) || Folders.Contains(fld))
                return;
            Folders.Add(fld);
        }

        public void AddFile(string name)
        {
            if (Files == null)
                Files = new();
            if (string.IsNullOrWhiteSpace(name) || Files.Contains(name))
                return;
            Files.Add(name);
        }

        public void AddNamespace(string ns)
        {
            if (Namespaces == null)
                Namespaces = new();
            if (string.IsNullOrWhiteSpace(ns) || Namespaces.Contains(ns))
                return;
            Namespaces.Add(ns);
        }

        public void AddClass(string fullName)
        {
            if (Classes == null)
                Classes = new();
            if (string.IsNullOrWhiteSpace(fullName) || Classes.Contains(fullName))
                return;
            Classes.Add(fullName);
        }

        public void AddAttribute(string fullName)
        {
            if (Attributes == null)
                Attributes = new();
            if (string.IsNullOrWhiteSpace(fullName) || Attributes.Contains(fullName))
                return;
            Attributes.Add(fullName);
        }

        public bool IsDirectoryListed(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            if (!path.EndsWith("\\"))
                path += "\\";
            return Directories?.Contains(path) == true;
        }

        public bool IsFolderListed(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
                return false;
            return Folders?.Contains(folderName) == true;
        }

        public bool IsFileListed(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            return Files?.Contains(name) == true;
        }

        public bool IsNamespaceListedExactly(string ns)
        {
            if (string.IsNullOrWhiteSpace(ns))
                return false;
            return Namespaces?.Contains(ns) == true;
        }

        public bool IsClassListed(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return false;
            return Classes?.Contains(fullName) == true;
        }

        public bool IsAttributeListed(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            return Attributes?.Contains(name) == true;
        }
    }
}
