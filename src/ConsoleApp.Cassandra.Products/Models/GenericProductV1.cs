using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp.Cassandra.Products.Models
{
    public class GenericProductV1
    {
        private Dictionary<string, string> _services;
        public Dictionary<string, string> Services => _services ?? (_services = new Dictionary<string, string>());
    }
}
