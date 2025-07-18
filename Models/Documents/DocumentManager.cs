﻿using FikusIn.Models.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FikusIn.Model.Documents
{
    internal static class DocumentManager
    {
        private static readonly ObservableCollection<Document> _documents = [];

        public static void SetActiveDocument(Document? document)
        {
            if (document == null || !_documents.Contains(document))
                return;

            foreach (var item in _documents)
                item.IsActive = false;

            document.IsActive = true;
        }

        private static void AddDocument(Document document)
        {
            _documents.Add(document);
            SetActiveDocument(document);

            if (document.Path != "" && _documents.Count == 2 && _documents[0].IsModified == false && _documents[0].Path == "")
            {
                _documents[0].Close();
                _documents.RemoveAt(0);
            }
        }

        private static void RemoveDocument(Document document)
        {
            int idx = _documents.IndexOf(document);
            if(!_documents.Remove(document))
                return;

            document.Close();

            if (!document.IsActive)
                return;

            if(_documents.Count == 0)
                SetActiveDocument(null);
            else if (idx < _documents.Count)
                SetActiveDocument(_documents[idx]);
            else
                SetActiveDocument(_documents[idx - 1]);
        }

        private static readonly string NewJobName = "New Job";

        private static int GetMaxJobNumber()
        {
            var jobNumbers = _documents
                .Where(d => d.Name.StartsWith($"{NewJobName} "))
                .Select(d => int.Parse(d.Name[$"{NewJobName} ".Length..]))
                .ToList();

            return jobNumbers.Count != 0 ? jobNumbers.Max() + 1: _documents.Any(d => d.Name == NewJobName)? 2: 0;
        }

        public static Document NewDocument(double windowScale, double graphicsQuality)
        {
            int njc = GetMaxJobNumber();
            var res = new Document(Guid.NewGuid(), NewJobName + (njc > 0? $" {njc}": ""), "", false, windowScale, graphicsQuality);

            AddDocument(res);

            return res;
        }

        public static Document? OpenDocument(string path, double windowScale, double graphicsQuality)
        {
            var res = new Document(Guid.NewGuid(), Path.GetFileNameWithoutExtension(path), path, false, windowScale, graphicsQuality);

            if(res.GetOCDocument() == null)
                return null;

            AddDocument(res);

            return res;
        }


        public static Document? GetActiveDocument()
        {
            return _documents.FirstOrDefault(d => d.IsActive);
        }

        public static Document? GetDocumentById(Guid id)
        {
            return _documents.FirstOrDefault(d => d.Id == id);
        }

        public static ObservableCollection<Document> GetDocuments()
        {
            return _documents;
        }

        public static ObservableCollection<DocumentInfo> GetRecentDocuments()
        {
            return RecentDocuments.Get();
        }

        public static void CloseDocument(Document? document)
        {
            if(document == null) 
                return;

            RemoveDocument(document);
        }
    }
}
