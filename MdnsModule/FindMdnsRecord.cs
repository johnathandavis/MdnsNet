using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management;
using System.Management.Automation;

namespace MdnsModule
{
    [Cmdlet(VerbsCommon.Find, "MdnsRecord")]
    public class FindMdnsRecord : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var client = new MdnsNet.MdnsClient();
            var ls = client.QueryAll(Service, timeout);
            if (ls.Count == 1) WriteObject(ls[0]);
            else WriteObject(ls);
        }

        private string record;
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = false,
            ValueFromPipeline = false,
            Position = 0,
            HelpMessage = "Domain or Service name to search for.")]
        [Alias("Domain")]
        public string Service
        {
            get { return record; }
            set { record = value; }
        }

        private byte timeout = 1;
        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = false,
            ValueFromPipeline = false,
            Position = 0,
            HelpMessage = "The time to wait for responding queries.")]
        public byte Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }
    }
}
