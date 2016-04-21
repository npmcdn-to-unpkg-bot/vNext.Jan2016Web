using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp.Cassandra.Products.Models
{
    public interface IProductInstance : IDocumentRecord
    {
        Guid BubbleId { get; }
    }

    public class ProductInstance<T> : DocumentRecord<T>, IProductInstance where T : class
    {
        public ProductInstance(T document) : base(document)
        {
        }
        public Guid BubbleId { get; set; }
    }
}
