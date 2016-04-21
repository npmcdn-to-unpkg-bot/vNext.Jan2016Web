using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp.Cassandra.Products.Models
{
    public interface IProductTemplate : IDocumentRecord
    {
    }

    public class ProductTemplate<T> : DocumentRecord<T>, IProductTemplate where T : class
    {
        public ProductTemplate(T document) : base(document)
        {
        }
    }
}
