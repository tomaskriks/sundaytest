using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace TaxConsumer
{
    public class ApiConsumer
    {
        private readonly string _apiUrl = ConfigurationManager.AppSettings["ApiUrl"];

        public ApiConsumer()
        {
            if (string.IsNullOrWhiteSpace(_apiUrl))
            {
                _apiUrl = "http://localhost:50795/api/";
            }
        }

        public string GetTaxRate(string municipality, DateTime date)
        {
            var client = new RestClient(_apiUrl);
            var request = new RestRequest("Tax/"+municipality+"/"+date.ToString("yyyy-MM-dd"), Method.GET);
            request.AddHeader("Cache-Control", "no-cache");
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content;
            }
            else
            {
                throw new ApplicationException(string.Format("Bad response received from the API: {0} - {1}. Content {2}",response.StatusCode, response.StatusDescription, response.Content));
            }
        }

        public List<string> GetAllMunicipalities()
        {
            var client = new RestClient(_apiUrl);
            var request = new RestRequest("Municipality",Method.GET);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var deserializer =new JsonDeserializer();
                return deserializer.Deserialize<List<string>>(response);
            }
            else
            {
                throw new ApplicationException(string.Format("Bad response received from the API: {0} - {1}. Content {2}", response.StatusCode, response.StatusDescription, response.Content));
            }
        }

        public bool InsertNewTax(string municipalityName, string taxType, decimal rate, DateTime dateFrom,
            DateTime dateTo)
        {
            var client = new RestClient(_apiUrl);
            var request = new RestRequest("Tax", Method.POST);

            var taxInfo = new Dictionary<string, object>
            {
                {"MunicipalityName", municipalityName},
                {"Type", taxType},
                {"Rate", rate},
                {"DateFrom", dateFrom.ToString("yyyy-MM-dd")},
                {"DateTo", dateTo.ToString("yyyy-MM-dd")}
            };
            var serializer = new JsonSerializer();
            var taxString = serializer.Serialize(taxInfo);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddParameter("application/json", taxString, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ImportMunicipalities(List<string> municipalityNames)
        {
            var client = new RestClient(_apiUrl);
            var request = new RestRequest("Municipality/FromJson", Method.POST);
            request.AddHeader("Cache-Control", "no-cache");

            var serializer = new JsonSerializer();
            var municipalityNameJson = serializer.Serialize(municipalityNames);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", municipalityNameJson, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);

            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent;
        }

        public bool ImportMunicipalities(string filePath)
        {
            var client = new RestClient(_apiUrl);
            var request = new RestRequest("Municipality/FromFile", Method.POST);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Cache-Control", "no-cache");

            var serializer = new JsonSerializer();
            var escapedPath = serializer.Serialize(filePath);
            request.AddParameter("application/json", escapedPath, ParameterType.RequestBody);


            IRestResponse response = client.Execute(request);

            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent;
        }

    }
}
