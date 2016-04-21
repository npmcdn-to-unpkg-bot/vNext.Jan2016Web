using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConsoleApp.Cassandra.Products.Models
{
    /*
    Id uuid,
	bubbleChainId uuid,
	Name text,
	DeviceId uuid,
	DeviceIdText text,
   PRIMARY KEY (Id, DeviceId)*/

    public interface IBubbleRecord
    {
        Guid Id { get; }
        Guid BubbleChainId { get; }
        string Name { get; }
        Guid DeviceId { get; }
        string DeviceIdText { get; }
        string Json { get; }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class BubbleRecord: IBubbleRecord
    {
        [JsonProperty]
        public Guid Id { get; set; }
        [JsonProperty]
        public Guid BubbleChainId { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public Guid DeviceId { get; set; }
        [JsonProperty]
        public string DeviceIdText { get; set; }
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
