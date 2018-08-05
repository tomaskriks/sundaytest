
using System;
using System.Collections.Generic;

namespace BusinessLayer
{
    public interface ITaxDataSource
    {
        double GetTaxRate(string municipalityName, DateTime date);

        void InsertNewTaxRecord(Tax record);

        void UpdateTaxRecord(Tax record);

        void ImportMunicipalities(IEnumerable<Municipality> municipalities);

        void ImportMunicipalities(List<string> municipalityNames);

        void ImportMunicipalities(string filePath);

        IEnumerable<Municipality> GetAllMunicipalities();
    }
}