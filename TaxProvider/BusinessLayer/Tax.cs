using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class Tax
    {
        public int Id { get; set; }
        public int MunicipalityId { get; set; }
        public string MunicipalityName { get; set; }
        public TaxType Type { get; set; }
        public double Rate { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public bool IsValid{
            get
            {
                return this.ValidateTypeDates() && Rate >= 0 && ValidateDates() &&
                       (!string.IsNullOrWhiteSpace(MunicipalityName) || MunicipalityId > 0);
            }
        }

        private bool ValidateTypeDates()
        {
            switch (this.Type)
            {
                case TaxType.Daily:
                    return DateFrom.Date == DateTo.Date;
                case TaxType.Weekly:
                    var timeDiff = DateTo.Date - DateFrom.Date;
                    return timeDiff == TimeSpan.FromDays(6); //Lets say that a week is any 7 days in a row. Doesn't have to be monday-sunday
                case TaxType.Monthly:
                    return (DateFrom.Day == 1 && DateFrom.Month == DateTo.Month && DateTo.Day == DateTime.DaysInMonth(DateTo.Year, DateTo.Month));
                case TaxType.Yearly:
                    return (DateFrom.Month == 1 && DateFrom.Day == 1 && DateTo.Month == 12 && DateTo.Day ==31 && DateTo.Year == DateFrom.Year);
                default:
                    return false;
            }
        }

        private bool ValidateDates()
        {
            var valid = DateFrom.Date <= DateTo.Date;
            valid = valid && DateFrom >= new DateTime(1753, 1, 1);
            valid = valid && DateTo >= new DateTime(1753, 1, 1);
            valid = valid && DateFrom <= new DateTime(9999, 12, 31);
            valid = valid && DateTo <= new DateTime(9999, 12, 31);
            return valid;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1} {2}% rate in {3} (Id {4}), Type {5}", DateFrom.ToString("yyyy.MM.dd"), DateTo.ToString("yyyy.MM.dd"),
                Rate, MunicipalityName, MunicipalityId, Type);
        }
    }

    public enum TaxType
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Yearly = 4
    }
}
