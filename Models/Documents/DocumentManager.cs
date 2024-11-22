using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FikusIn.Model.Documents
{
    internal static class DocumentManager
    {
        private static readonly ObservableCollection<Document> _documents = [new Document(new Guid(), "New Job", true)];

        private static void AddDocument(Document document)
        {
            if (document == null)
                return;

            if (document.IsActive)
                foreach (var item in _documents)
                    item.IsActive = false;

            _documents.Add(document);
        }

        private static void RemoveDocument(Document document)
        {
            if (document == null)
                return;

            _documents.Remove(document);

            if(document.IsActive && _documents.Count > 0)
                _documents.First().IsActive = true;
        }

        private static readonly string NewJobName = "New Job";

        private static int GetMaxJobNumber()
        {
            var jobNumbers = _documents
                .Where(d => d.Name.StartsWith($"{NewJobName} "))
                .Select(d => int.Parse(d.Name.Substring($"{NewJobName} ".Length)))
                .ToList();

            return jobNumbers.Count != 0 ? jobNumbers.Max() + 1: _documents.Any(d => d.Name == NewJobName)? 1: 0;
        }
        public static Document NewDocument()
        {
            int njc = GetMaxJobNumber();
            var res = new Document(Guid.NewGuid(), NewJobName + (njc > 0? $" {njc}": ""), true);

            AddDocument(res);

            return res;
        }

        public static Document? GetActiveDocument()
        {
            return _documents.FirstOrDefault(d => d.IsActive);
        }

        public static void SetActiveDocument(Document? document)
        { 
            if(document == null || !_documents.Contains(document))
                return;

            foreach (var item in _documents)
                item.IsActive = false;

            document.IsActive = true;
        }

        public static Document? GetDocumentById(Guid id)
        {
            return _documents.FirstOrDefault(d => d.Id == id);
        }

        public static ObservableCollection<Document> GetDocuments()
        {
            return _documents;
        }

        public static void CloseDocument(Document? document)
        {
            if(document == null || document is not Document) 
                return;

            RemoveDocument(document);

            if (!document.Close())
                return;
        }

    }
}
