using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApp.Cassandra.Products.DAO;
using ConsoleApp.Cassandra.Products.Models;

namespace ConsoleApp.Cassandra.Products
{
    public class Program
    {

        static void Main(string[] args)
        {
            string m1 = "Type a string of text then press Enter. " +
                        "Type '+' anywhere in the text to quit:\n";
            Console.WriteLine(m1);
            int writes = 0;
            char ch;

            do
            {
                var x = Console.ReadLine();
                if (string.Compare(x, "x", StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    break;

                }
                Task t = MainAsync(args);
                t.Wait();
                Console.WriteLine(++writes);


            } while (true);

            Console.WriteLine("Bye.....................");

        }

        static async Task MainAsync(string[] args)
        {
            try
            {
                var productTemplate = new ProductTemplate<GenericProductV1>(Guid.NewGuid(),
                    new GenericProductV1()
                    {
                        Services =
                        {
                            {"OxygenId", Guid.NewGuid().ToString()},
                            {"StoratePlatformId", Guid.NewGuid().ToString()}
                        }
                    }
                    )
                {
                    DocumentMetaData = new DocumentMetaData(typeof(GenericProductV1).FullName,"1.0")
                };

                string output = productTemplate.DocumentJson;

                List<Guid> productGuids = new List<Guid>();
                productGuids.Add(productTemplate.Id);
                // Product Templates
                var ptRes = await CassandraDAO.CreateProductTemplateAsync(productTemplate);
                // make a double to test insert by type.
                productTemplate.DocumentMetaData.Version = "1.1";
                productTemplate.Id = Guid.NewGuid();
                productGuids.Add(productTemplate.Id);

                ptRes = await CassandraDAO.CreateProductTemplateAsync(productTemplate);

                Console.WriteLine("-----------------------------------------------");
                foreach (var id in productGuids)
                {
                    var ptRead = await CassandraDAO.FindProductTemplateByIdAsync(id);
                    Console.WriteLine("");
                    Console.WriteLine("ProductTemplate");
                    Console.WriteLine(ptRead.Json);
                    Console.WriteLine(ptRead.MetaDataJson);
                    Console.WriteLine(ptRead.DocumentJson);
                }
                var ptRecords = await CassandraDAO.FindProductTemplateByTypeAsync(productTemplate.MetaData.Type);
                Console.WriteLine("-----------------------------------------------");
                foreach (var ptRecord in ptRecords)
                {
                    Console.WriteLine("");
                    Console.WriteLine("ProductTemplate");
                    Console.WriteLine(ptRecord.Json);
                    Console.WriteLine(ptRecord.MetaDataJson);
                    Console.WriteLine(ptRecord.DocumentJson);
                }
                Console.WriteLine("-----------------------------------------------");

                productGuids.Clear();

                // Bubble
                var bid = Guid.NewGuid();
                var bubbleRecord = new BubbleRecord()
                {
                    Id = bid,
                    BubbleChainId = Guid.Empty,
                    DeviceId = Guid.Empty,
                    DeviceIdText = null,
                    Name = "Nameof:" + bid
                };
                var bidRes = await CassandraDAO.CreateBubbleRecordAsync(bubbleRecord);
                var bidRead = await CassandraDAO.FindBubbleRecordByIdAsync(bubbleRecord.Id);
                Console.WriteLine("");
                Console.WriteLine("Bubbles");
                Console.WriteLine(bidRead.Json);

                var label = "Label:" + Guid.NewGuid();
                // Product Instance
                var doc = new GenericProductV1()
                {
                    Services =
                    {
                        {"OxygenId", Guid.NewGuid().ToString()},
                        {"StoratePlatformId", Guid.NewGuid().ToString()}
                    }
                };

                var productInstance = new ProductInstance<GenericProductV1>(
                    Guid.NewGuid(), doc, productTemplate.DocumentMetaData, productTemplate.Id, label, bubbleRecord.Id);
                productGuids.Add(productInstance.Id);

                var piRes = await CassandraDAO.CreateProductInstanceAsync(productInstance);

                productInstance.Id = Guid.NewGuid();
                productInstance.BubbleId = Guid.NewGuid();
                productGuids.Add(productInstance.Id);
                piRes = await CassandraDAO.CreateProductInstanceAsync(productInstance);

                Console.WriteLine("-----------------------------------------------");
                foreach (var id in productGuids)
                {
                    var piRead = await CassandraDAO.FindProductInstanceByIdAsync(id);

                    Console.WriteLine("");
                    Console.WriteLine("ProductInstance");

                    Console.WriteLine(piRead.Json);
                    Console.WriteLine(piRead.MetaDataJson);
                    Console.WriteLine(piRead.DocumentJson);

                }
                Console.WriteLine("-----------------------------------------------");

                var piRecords = await CassandraDAO.FindProductInstanceByLabelAsync(productInstance.Label);
                foreach (var record in piRecords)
                {
                    Console.WriteLine("");
                    Console.WriteLine("ProductInstance");
                    Console.WriteLine(record.Json);
                    Console.WriteLine(record.MetaDataJson);
                    Console.WriteLine(record.DocumentJson);
                }
                Console.WriteLine("-----------------------------------------------");

            }
            catch (Exception e)
            {

            }

        }



    }
}
