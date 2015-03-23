using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

using MdnsNet;
using MdnsNet.MDNS;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MdnsNet
{
    public class ManagementClient
    {
        public ManagementClient(IPAddress ip)
        {
            this.endpoint = FindManagementPort(ip);
        }

        public ManagementClient(string name)
        {
            this.endpoint = FindManagementPort(name);
        }

        private IPEndPoint endpoint;
        public string EndpointUrl
        {
            get
            {
                return "http://" + endpoint.Address.ToString() + ":" + endpoint.Port + "/";
            }
        }


        public string AddRecord(MdnsRecord record)
        {
            dynamic doc = new System.Dynamic.ExpandoObject();
            doc.name = record.Name;
            doc.port = record.Port.ToString();
            doc.ip = record.IP.ToString();
            doc.service = record.Domain;
            string json = JsonConvert.SerializeObject(doc);

            using (var client = new WebClient())
            {
                var response = client.UploadData(EndpointUrl + "AddRecord", Encoding.ASCII.GetBytes(json));

                return Encoding.Default.GetString(response);
            }

        }
        public List<MdnsRecord> GetAllRecords()
        {
            var ls = new List<MdnsRecord>();

            using (var client = new WebClient())
            {
                var response = client.DownloadString(EndpointUrl + "ListRecords");
                return JsonConvert.DeserializeObject<List<MdnsRecord>>(response, new JsonSerializerSettings() { Converters = new JsonConverter[1] { new Server.IPAddressConverter() }.ToList() });
            }
        }

        public const string MANAGEMENT_SERVICE = "_mdns._http";

        public static IPEndPoint FindManagementPort(string hostname, int timeout = 1)
        {
            var client = new MdnsClient();
            var results = client.QueryAll(MANAGEMENT_SERVICE, timeout);
            foreach (var result in results)
            {
                if (result.Name.Split('.')[0].ToLower() == hostname.ToLower()) return new IPEndPoint(result.IP, result.Port);
            }
            throw new Exception("No MDNS Server with the name (" + hostname + ") could be found.");
        }
        public static IPEndPoint FindManagementPort(IPAddress ip, int timeout = 1)
        {
            var client = new MdnsClient();
            var results = client.QueryAll(MANAGEMENT_SERVICE, timeout);
            foreach (var result in results)
            {
                if (result.IP.ToString() == ip.ToString()) return new IPEndPoint(ip, result.Port);
            }
            throw new Exception("No MDNS Server with the IP (" + ip.ToString() + ") could be found.");
        }
    }
}
