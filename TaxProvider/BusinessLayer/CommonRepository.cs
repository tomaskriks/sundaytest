using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace BusinessLayer
{
    public class CommonRepository : ITaxDataSource, IDisposable
    {
        private LocalDbHelper DbHelper { get; set; }

        public CommonRepository()
        {
                DbHelper = new LocalDbHelper();
        }

        public double GetTaxRate(string municipalityName, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(municipalityName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(municipalityName));
            if (date == null)
            {
                throw new ArgumentNullException(nameof(date),"Value cannot be null");
            }


            var queryString = string.Format(@"Declare @municipalityName VARCHAR(255)
            SET @municipalityName = '{0}'

            Declare @myDate Date
            SET @myDate = '{1}'

            select top 1 t.Rate from Taxes t
            join Municipalities m on t.MunicipalityId = m.Id
            join TaxTypes tt on t.TypeId = tt.Id
            where @myDate >= t.DateFrom and @myDate <= t.DateTo and m.Name = @municipalityName
            order by tt.Priority asc",municipalityName.Replace("'","''"), date.Date.ToString("yyyy.MM.dd"));

            var resultDs = DbHelper.GetData(queryString);

            if (resultDs.Tables.Count != 1)
            {
                throw new ApplicationException("Query to get tax rate executed, however returned table count is not 1");
            }
            if (resultDs.Tables[0].Rows.Count == 0)
            {
                return 0; //If nothing is defined, then there are no taxes on that day.
            }
            if( resultDs.Tables[0].Rows.Count == 1)
            {
                var taxRate = (double) resultDs.Tables[0].Rows[0]["Rate"];
                return taxRate;
            }
            
            throw new ApplicationException("Query to get tax rate executed, however more than 1 tax rate matched the search, so we cannot provide a definitive answer.");
        }

        public void InsertNewTaxRecord(Tax record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if(!record.IsValid)throw new ApplicationException("Cannot insert this tax value, because it is not valid.");

            var availableMunicipalities = GetAllMunicipalities().ToList();

            if (record.MunicipalityId <= 0)
            {
                record.MunicipalityId = availableMunicipalities.First(m => m.Name == record.MunicipalityName).Id;
            }

            if (record.MunicipalityId > 0 && availableMunicipalities.Any(m => m.Id == record.MunicipalityId))
            {
                var procedure = "dbo.InsertTax";
                var parameters = new Dictionary<string, object>
                {
                    {"@municipalityId", record.MunicipalityId},
                    {"@typeId", (int) record.Type},
                    {"@rate", record.Rate},
                    {"@dateFrom", record.DateFrom},
                    {"@dateTo", record.DateTo}
                };
                var result = DbHelper.ExecuteNonQuery(procedure, true, parameters);
                if (result == 0)
                {
                    throw new ApplicationException(string.Format("New tax insertion affected {0} rows, when it should have affected 1",result));
                }
            }
            else
            {
                throw new ApplicationException("Provided tax record does not match any of supported municipalities.");
            }


            
        }

        public void UpdateTaxRecord(Tax record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (!record.IsValid) throw new ApplicationException("Cannot insert this tax value, because it is not valid.");
            if(record.Id <= 0)throw new ArgumentException("Tax record must have an Id to update",nameof(record));

            var availableMunicipalities = GetAllMunicipalities().ToList();

            if (record.MunicipalityId <= 0)
            {
                record.MunicipalityId = availableMunicipalities.First(m => m.Name == record.MunicipalityName).Id;
            }

            if (record.MunicipalityId > 0 && availableMunicipalities.Any(m => m.Id == record.MunicipalityId))
            {
                string command = "UPDATE dbo.Taxes SET MunicipalityId = @municipalityId, TypeId = @typeId, Rate = @rate, DateFrom = @dateFrom, DateTo = @dateTo WHERE Id = @id";

                var parameters = new Dictionary<string, object>
                {
                    {"@id", record.Id},
                    {"@municipalityId", record.MunicipalityId},
                    {"@typeId", (int) record.Type},
                    {"@rate", record.Rate},
                    {"@dateFrom", record.DateFrom},
                    {"@dateTo", record.DateTo}
                };

                var result= DbHelper.ExecuteNonQuery(command, false, parameters);
                if (result != 1)
                {
                    throw new ApplicationException(string.Format("Tax update affected {0} rows, when it should have affected 1", result));
                }
            }
            else
            {
                throw new ApplicationException("Provided tax record does not match any of supported municipalities.");
            }
        }

        public void ImportMunicipalities(List<string> municipalityNames)
        {
            if (municipalityNames == null) throw new ArgumentNullException(nameof(municipalityNames));

            if (!municipalityNames.Any())
            {
                return;
            }

            var query = municipalityNames.Aggregate("", (current, name) => current + string.Format("EXEC dbo.InsertMunicipality @name = '{0}'; \n", name.Replace("'","''")));

            DbHelper.ExecuteNonQuery(query);
        }

        public void ImportMunicipalities(IEnumerable<Municipality> municipalities)
        {
            if (municipalities == null) throw new ArgumentNullException(nameof(municipalities));

            var municipalityNames = municipalities.Select(m => m.Name);
            ImportMunicipalities(municipalityNames.ToList());
        }

        public void ImportMunicipalities(string filePath)
        {
            Encoding encoding;
            using (var reader = new StreamReader(filePath, Encoding.UTF8, true))
            {
                reader.Peek(); 
                encoding = reader.CurrentEncoding;
            }
            var lines = System.IO.File.ReadAllLines(filePath, encoding);

            var municipalities = lines.Select(line => new Municipality() {Name = line});

            ImportMunicipalities(municipalities);
        }

        public IEnumerable<Municipality> GetAllMunicipalities()
        {
            var queryString = @"select Id, Name from Municipalities";

            var resultDs = DbHelper.GetData(queryString);

            if (resultDs.Tables.Count != 1)
            {
                throw new ApplicationException("Query to get municipality executed, however returned table count is not 1");
            }
            if (resultDs.Tables[0].Rows.Count == 0)
            {
                return new List<Municipality>();
            }

            var output = from DataRow row in resultDs.Tables[0].Rows
                select new Municipality
                {
                    Id = (int) row["Id"],
                    Name = row["Name"].ToString()
                };
            return output;
            
        }

        public void Dispose()
        {
            DbHelper.Dispose();
        }
    }
}
