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
        public static Document NewDocument()
        {
            var res = new Document(Guid.NewGuid(), "New Job", true);

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

        public static void CloseDocument(Document document)
        {
            if (!document.Close())
                return;

            _documents.Remove(document);
        }

    }
}
