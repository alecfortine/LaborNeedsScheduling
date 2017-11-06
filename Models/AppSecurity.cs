using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace LaborNeedsScheduling.Models
{
    /// <summary>
    /// Summary description for AppSecurity
    /// </summary>
    public class AppSecurity
    {
        public const string AppId = "TransferWebsite";
        public string sqlcon = ConfigurationManager.ConnectionStrings["MovadoDB"].ToString();

        public string ApplicationId()
        {
            return AppId;
        }

        public DataTable GetUserSecurity(String UserName)
        {
            DataTable dtSecTable = new DataTable();
            SqlConnection cn = new SqlConnection(sqlcon);
            SqlCommand sqlcmd = new SqlCommand();

            try
            {
                if (System.Web.HttpContext.Current.Session["TransferWebsite_UserSecurity"].ToString() != "")
                {
                    System.Web.HttpContext.Current.Session["TransferWebsite_UserSecurity"] = dtSecTable;
                }

            }
            catch (Exception ex)
            {
                return dtSecTable;
            }

            if (dtSecTable.Rows.Count > 0)
            {
                return dtSecTable;
            }

            //Retrieve Security From Database
            try
            {
                cn.Open();
            }
            catch (Exception ex)
            {
                return dtSecTable;
            }

            try
            {
                sqlcmd.Connection = cn;
                sqlcmd.CommandType = CommandType.StoredProcedure;
                sqlcmd.CommandText = "UserSecurity_UserNameApplicationId";
                sqlcmd.Parameters.AddWithValue("@UserName", UserName);
                sqlcmd.Parameters.AddWithValue("@ApplicationId", AppId);

                dtSecTable.Load(sqlcmd.ExecuteReader());
                dtSecTable.AcceptChanges();
            }
            catch (Exception ex)
            {
                sqlcmd.Dispose();
                cn.Close();
                cn.Dispose();
                return dtSecTable;
            }

            sqlcmd.Dispose();
            cn.Close();
            cn.Dispose();

            if (dtSecTable.Rows.Count > 0)
            {
                System.Web.HttpContext.Current.Session["TransferWebsite_UserSecurity"] = dtSecTable;
            }

            return dtSecTable;
        }

        public Boolean UserHasAccess(String UserName, String SecurityOption)
        {
            DataTable dtSecurity = new DataTable();
            //DataRow Row;

            //UserHasAccess = false;


            dtSecurity = GetUserSecurity(UserName);

            foreach (DataRow row in dtSecurity.Rows)
            {
                string ColSecurityOption = row["SecurityOption"].ToString();
                if (ColSecurityOption.ToUpper() == SecurityOption.ToUpper())
                {
                    if (Convert.ToBoolean(row["GrantAccess"]) == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
    }
}