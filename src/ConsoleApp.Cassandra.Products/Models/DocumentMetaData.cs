using System;

namespace ConsoleApp.Cassandra.Products.Models
{
    public interface IDocumentMetaData
    {
        string Type { get; }
        string Version { get; }
    }

    public class DocumentMetaData : IDocumentMetaData
    {
        public DocumentMetaData()
        {
            Type = String.Empty;
            Version = String.Empty;
        }
        public DocumentMetaData(string type,string version)
        {
            Type = type;
            Version = version;
        }

        public string Type { get; set; }
        public string Version { get; set; }
    }
}