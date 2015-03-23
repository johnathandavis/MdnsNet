using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MdnsNet.MDNS;

namespace Server
{
    public class GlobalRecord
    {
        public GlobalRecord(MdnsRecord record)
        {
            this.ID = Guid.NewGuid();
            this.Record = record;
        }
        public GlobalRecord(MdnsRecord record, Guid id)
        {
            this.Record = record;
            this.ID = id;
        }

        public Guid ID { get; set; }
        public MdnsRecord Record { get; set; }
    }
}
