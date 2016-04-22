using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConsoleApp.Cassandra.Products.Models
{
    //@"productinstances(id,label,producttemplateid,type,version,document,bubbleid) " +

    public interface IProductInstance : IDocumentRecord
    {
        Guid Id { get; }
        string Label { get; }
        Guid ProductTemplateId { get; }
        Guid BubbleId { get; }
        string Json { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ProductInstance<T> : DocumentRecord<T>, IProductInstance where T : class
    {
        public ProductInstance(Guid id, T document, DocumentMetaData metaData, Guid productTemplateId, string label, Guid bubbleId) : base(document, metaData)
        {
            Id = id;
            ProductTemplateId = productTemplateId;
            Label = label;
            BubbleId = bubbleId;
        }
        public ProductInstance(T document) : this(Guid.Empty, document, new DocumentMetaData(), Guid.Empty, string.Empty, Guid.Empty)
        {
        }
        public ProductInstance(Guid id, T document) : this(id, document, new DocumentMetaData(), Guid.Empty, string.Empty, Guid.Empty)
        {
        }

        public ProductInstance(T document,DocumentMetaData metaData) : this(Guid.Empty,document, metaData,Guid.Empty,string.Empty,Guid.Empty)
        {
        }

        [JsonProperty]
        public Guid Id { get; set; }
        [JsonProperty]
        public string Label { get; set; }
        [JsonProperty]
        public Guid ProductTemplateId { get; set; }
        [JsonProperty]
        public Guid BubbleId { get; set; }

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
