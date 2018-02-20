using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace LaborNeedsScheduling.Models
{
    public class ActiveRequests
    {
        public DataTable TimeOffRequests = new DataTable();
    }
}