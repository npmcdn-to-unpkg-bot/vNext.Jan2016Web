using System;

namespace ConsoleApp.Cassandra.Products.Models
{
    public interface IDocumentMetaData
    {
        string Type { get; }
        string Version { get; }
        Guid TypeId { get; }

    }

    public class DocumentMetaData : IDocumentMetaData
    {
        public string Type { get; set; }
        public string Version { get; set; }
        public Guid TypeId { get; set; }
    }
}