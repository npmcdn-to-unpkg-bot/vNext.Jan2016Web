using System;
using Newtonsoft.Json;

namespace ConsoleApp.Cassandra.Products.Models
{
    public interface IDocumentRecord
    {
        IDocumentMetaData MetaData { get; }
        object Document { get; }
        string DocumentJson { get; }

        string MetaDataJson { get; }
    }

    public class DocumentRecord<T> : IDocumentRecord where T : class
    {
        private T _document;

        public DocumentRecord(T document)
        {
            _document = document;
        }
        public DocumentRecord(T document, DocumentMetaData metaData)
        {
            _document = document;
            DocumentMetaData = metaData;
        }


        public DocumentMetaData DocumentMetaData { get; set; }

        public IDocumentMetaData MetaData => DocumentMetaData;

        public object Document => _document;

        public string DocumentJson
        {
            get
            {
                string output = JsonConvert.SerializeObject(_document);
                return output;
            }
        }


        public string MetaDataJson
        {
            get
            {
                string output = JsonConvert.SerializeObject(DocumentMetaData);
                return output;
            }
        }
    }

}
