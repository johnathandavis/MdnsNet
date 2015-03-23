using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MdnsNet.DNS;

namespace MdnsNet.DNS
{
    public class QueryQuestion
    {
        public QueryQuestion(DomainName name, DnsRecordType type, DnsClass cls)
        {
            this.Name = name;
            this.RecordType = type;
            this.QueryClass = cls;
        }

        public DomainName Name { get; private set; }
        public DnsRecordType RecordType { get; private set; }
        public DnsClass QueryClass { get; private set; }

        public override string ToString()
        {
            return RecordType.ToString() + " - \"" + Name.ToString() + "\" (" + QueryClass.ToString() + ")";
        }
    }
}
