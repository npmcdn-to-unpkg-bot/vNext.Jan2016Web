using System;
using Newtonsoft.Json;

namespace ConsoleApp.Cassandra.Products.Models
{
    public interface IProductTemplate : IDocumentRecord
    {
        Guid Id { get; }
        string Json { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ProductTemplate<T> : DocumentRecord<T>, IProductTemplate where T : class
    {
        public ProductTemplate(Guid id,T document) : base(document)
        {
            Id = id;
        }
        public ProductTemplate(Guid id, T document, DocumentMetaData metaData) : base(document, metaData)
        {
            Id = id;
        }


        [JsonProperty]
        public Guid Id { get; set; }
        public string Json
        {
            get
            {
                string output = JsonConvert.SerializeObject(this);
                return output;
            }
        }
    }
}
