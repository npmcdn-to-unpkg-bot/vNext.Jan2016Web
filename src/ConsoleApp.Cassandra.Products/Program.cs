using System;
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
                var productTemplate = new ProductTemplate<GenericProductV1>(
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
                    DocumentMetaData = new DocumentMetaData()
                    {
                        Type = typeof (GenericProductV1).FullName,
                        TypeId = Guid.NewGuid(),
                        Version = "1.0"
                    }
                };

                string output = productTemplate.DocumentJson;

                // Product Templates
                var ptRes = await CassandraDAO.CreateProductTemplateAsync(productTemplate);
                var ptRead = await CassandraDAO.FindProductTemplateByIdAsync(productTemplate.MetaData.TypeId);
                Console.WriteLine("");
                Console.WriteLine("ProductTemplate");
                Console.WriteLine(ptRead.MetaDataJson);
                Console.WriteLine(ptRead.DocumentJson);



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


                // Product Instance
                var productInstance = new ProductInstance<GenericProductV1>((GenericProductV1) productTemplate.Document)
                {
                    BubbleId = bubbleRecord.Id,
                    DocumentMetaData = productTemplate.DocumentMetaData
                };

                var piRes = await CassandraDAO.CreateProductInstanceAsync(productInstance);
                var piRead = await CassandraDAO.FindProductInstanceByIdAsync(productInstance.MetaData.TypeId);

                Console.WriteLine("");
                Console.WriteLine("ProductInstance");
                Console.WriteLine(piRead.MetaDataJson);
                Console.WriteLine(piRead.DocumentJson);

            }
            catch (Exception e)
            {

            }

        }



    }
}
