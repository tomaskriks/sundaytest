using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BusinessLayer;
using DataAccess;

namespace TaxProvider.Controllers
{
    public class MunicipalityController : ApiController
    {
        [HttpPost]
        [ActionName("FromFile")]
        public void FromFile([FromBody]string filePath)
        {
            if(string.IsNullOrWhiteSpace(filePath))throw new HttpResponseException(HttpStatusCode.BadRequest);
            if (!filePath.Split('.').Last().Equals("txt", StringComparison.OrdinalIgnoreCase))
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Only .txt files are supported.")
                };
                throw new HttpResponseException(message);
            }

            try
            {
                using (var repo = new CommonRepository())
                {
                    repo.ImportMunicipalities(filePath);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("An expcetion was received when importing municipality data from file. (FilePath = {0}). {1} - {2}", filePath, ex.GetType(), ex.Message));
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("An error has occured while processing your request.")
                };
                throw new HttpResponseException(message);
            }
        }

        [HttpPost]
        public void FromJson([FromBody]List<string> names)
        {
            if (names == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            if (!names.Any()) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            try
            {
                using (var repo = new CommonRepository())
                {
                    repo.ImportMunicipalities(names);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("An expcetion was received when importing municipality data from file. (NamesGiven: {0}). {1} - {2}", names.Count, ex.GetType(), ex.Message));
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("An error has occured while processing your request.")
                };
                throw new HttpResponseException(message);
            }
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            try
            {
                using (var repo = new CommonRepository()) { 
                    var result = from munip in repo.GetAllMunicipalities() select munip.Name;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("An expcetion was received when getting municipalities. {0} - {1}", ex.GetType(), ex.Message));
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("An error has occured while processing your request.")
                };
                throw new HttpResponseException(message);
            }
        }


    }
}
