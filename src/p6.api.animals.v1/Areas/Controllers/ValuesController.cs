using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace p6.api.animals.v1.Areas.Controllers
{
    [Area("api")]
    [Route("[controller]")]
    public class CustomersController : Controller
    {
        private List<Customer> _Customers = new List<Controllers.Customer>();

        public CustomersController()
        {
            _Customers.Add(new Customer() { ID = 1, Name = "Fred" });
            _Customers.Add(new Customer() { ID = 2, Name = "Bob" });
            _Customers.Add(new Customer() { ID = 3, Name = "Tim" });
        }

        [Route("[action]")]
        public Customer GetFirstCustomer()
        {
            return _Customers.First();
        }

        [HttpGet("{id:int}")]
        public Customer GetCustomer(int ID)
        {
            Customer Customer = _Customers.Find(c => c.ID == ID);
            return Customer;
        }
    }

    public class Customer
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }



}
