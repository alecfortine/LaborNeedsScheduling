using LSAData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace LaborNeedsScheduling.Models
{
    public class EmployeeModel
    {

        public List<EmployeeNotification> EmployeeNotifications { get; set; }

        public static DataTable EmpSchedule { get; set; }

        public List<JakoEmployee> ListOfEmployees { get; set; }


        //public static string StartHourID { get; set; }
        //public static string EndHourID { get; set; }
        //public static bool IsAllDay { get; set; }



        public EmployeeModel(string LocationCode, string EmployeeID)
        {
            //WorkWeek w = new WorkWeek();
            //w.FillDatatables();

            //notifications = new List<EmployeeNotification>();

            ListOfEmployees = FakeAPI.GetEmpsForStore(LocationCode);

            EmployeeNotifications = FakeAPI.CreateResponses(EmployeeID);

            //foreach (EmployeeNotification e in EmployeeNotifications)
            //{
            //    EmployeeNotifications.Add(e);
            //}

            foreach (JakoEmployee j in ListOfEmployees)
            {
                if (j.id == EmployeeID)
                {
                    EmpSchedule = j.AvailabilityTable;
                }
            }
        }
    }

    public class EmployeeTimeOffRequest
    {
        public static DateTime Date { get; set; }
        public static bool IsAllDay { get; set; }
        public static List<string> Hours { get; set; }
        public static string StartHourID { get; set; }
        public static string EndHourID { get; set; }

    }

    public class EmployeeNotification
    {
        public string id { get; set; }
        public DateTime date { get; set; }
        public string message { get; set; }
        public bool accepted { get; set; }
    }

    //public class EmployeeNotification
    //{
    //    List<EmployeeTimeOffRequest> Response { get; set; }
    //}

    //public class EmployeeSchedule
    //{
    //    DataTable EmpSchedule { get; set; }
    //}

    
}