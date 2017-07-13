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

        public static DataTable EmployeeSchedule { get; set; }

        public List<JakoEmployee> ListOfEmployees { get; set; }

        public List<EmployeeTimeOffRequest> Requests { get; set; }

        public static string[] ScheduleHours = { "9AM","10AM","11AM","12PM", "1PM", "2PM", "3PM",
                                          "4PM", "5PM", "6PM", "7PM", "8PM", "9PM", "10PM"};

        //public static string StartHourID { get; set; }
        //public static string EndHourID { get; set; }
        //public static bool IsAllDay { get; set; }



        public EmployeeModel(string LocationCode, string EmployeeID)
        {
            ListOfEmployees = FakeAPI.GetEmpsForStore(LocationCode);

            EmployeeNotifications = FakeAPI.CreateResponses(EmployeeID);

            EmployeeSchedule = FakeAPI.GetEmployeeSchedule(EmployeeID);

            //foreach (JakoEmployee j in ListOfEmployees)
            //{
            //    if (j.id == EmployeeID)
            //    {
            //        EmployeeSchedule = j.AvailabilityTable;
            //    }
            //}
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

    public class Employees
    {
        public string id { get; set; }
        public string name { get; set; }
        public string role { get; set; }
        public string rank { get; set; }
        public string hours { get; set; }
        public bool[] availableHours { get; set; }
        public bool[] scheduledHours { get; set; }
        public DataTable weeklySchedule { get; set; }
        public Dictionary<int, string[]> startEndTimes { get; set; }
    }

    public class TimeOffRequest
    {
        public EmployeeTimeOffRequest Request { get; set; }
    }

}