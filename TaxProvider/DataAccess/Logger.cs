using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public static class Logger
    {
        public static void Log(string message)
        {
            try
            {
                var db = new LocalDbHelper();

                var command =
                    string.Format("INSERT INTO dbo.ErrorLog  (TimeStamp, ErrorMessage) Values(GETUTCDATE(), '{0}')",
                        message.Replace("'", "''"));

                db.ExecuteNonQuery(command);
            }
            catch(Exception ex)
            {
                //could have backup logging here if database is not accessible. But now just suppress further errors
            }
        }
    }
}
