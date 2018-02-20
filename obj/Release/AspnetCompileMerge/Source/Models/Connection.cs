using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace LaborNeedsScheduling.Models
{
    public class Connection
    {

        public static string strSQLCon = ConfigurationManager.ConnectionStrings["SqlServerConnection"].ToString();


    }

}