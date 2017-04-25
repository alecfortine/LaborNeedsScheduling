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



        // get rid of everything else
        //public SqlConnection sqlCon = new SqlConnection(strSQLCon);

        ///// <summary>
        ///// SQL commands for getting data for the past six weeks from the source
        ///// </summary>
        //public SqlCommand CalculateWTG6 = new SqlCommand();
        //public SqlCommand CalculateWTG5 = new SqlCommand();
        //public SqlCommand CalculateWTG4 = new SqlCommand();
        //public SqlCommand CalculateWTG3 = new SqlCommand();
        //public SqlCommand CalculateWTG2 = new SqlCommand();
        //public SqlCommand CalculateWTG1 = new SqlCommand();

    }

}