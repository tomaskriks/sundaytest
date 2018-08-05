using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BusinessLayer;
using DataAccess;

namespace TaxProviderService.Controllers
{
    public class TaxController : ApiController
    {
        // GET: api/Tax/Municipality/Date
        [HttpGet]
        public double GetTaxRate(string municipalityName, string dateString)
        {
            if(string.IsNullOrWhiteSpace(municipalityName))throw new HttpResponseException(HttpStatusCode.BadRequest);
            if (string.IsNullOrWhiteSpace(dateString)) throw new HttpResponseException(HttpStatusCode.BadRequest);
            if (!DateTime.TryParse(dateString, out var date)) { throw new HttpResponseException(HttpStatusCode.BadRequest);}

            try
            {
                using (var repo = new CommonRepository())
                {

                    var taxRate = repo.GetTaxRate(municipalityName, date);

                    return taxRate;
                }
            }
            catch(Exception ex)
            {
                Logger.Log(string.Format("An expcetion was received when getting tax data for user. (MunicipalityName = {0}, date={1}). {2} - {3}",municipalityName,dateString,ex.GetType(),ex.Message));
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("An error has occured while processing your request.")
                };
                throw new HttpResponseException(message);
            }
        }


       // GET: api/Tax/5
        [HttpGet]
        public string Get(int id)
        {
            string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = (System.IO.Path.GetDirectoryName(executable));


            string Path = Environment.CurrentDirectory;
            string[] appPath = Path.Split(new string[] { "bin" }, StringSplitOptions.None);

            return path + "\r\n" + appPath[0];
        }
    

        // POST: api/Tax   //Insert
        [HttpPost]
        public void PostTaxRecord([FromBody]Tax value)
        {
            if(value == null || !value.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            try
            {
                using (var repo = new CommonRepository())
                {
                    repo.InsertNewTaxRecord(value);
                }
            }
            catch(Exception ex)
            {
                Logger.Log(string.Format("An expcetion was received when inserting tax data for user. (TaxValue = {0}). {1} - {2}", value.ToString(), ex.GetType(), ex.Message));
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("An error has occured while processing your request.")
                };
                throw new HttpResponseException(message);
            }
        }

        // PUT: api/Tax/5   //Update
        [HttpPut]
        public void Put(int id, [FromBody]Tax value)
        {
            if (value == null || !value.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            try
            {
                using (var repo = new CommonRepository())
                {
                    value.Id = id;
                    repo.UpdateTaxRecord(value);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("An expcetion was received when updating tax data for user. (TaxValue = {0}). {1} - {2}", value.ToString(), ex.GetType(), ex.Message));
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("An error has occured while processing your request.")
                };
                throw new HttpResponseException(message);
            }
        }


    }
}
