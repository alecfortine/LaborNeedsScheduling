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

    public class LaborScheduling
    {
        /// <summary>
        /// The current week the schedule is being generated for
        /// </summary>
        public WorkWeek ThisWeek { get; set; }

        public AvailabilityViewModel Employee { get; set; }

        /// <summary>
        /// Set of previous work weeks of data
        /// </summary>
        public Dictionary<int, WorkWeek> PastWorkWeeks { get; set; }
    
        public static List<Employees> EmployeeListAll { get; set; }
        public static List<Employees> EmployeeListStore { get; set; }
        public List<ManagerNotification> ManagerMessageList { get; set; }

        public string selectedEmployeeId { get; set; }
        public string selectedEmployeeName { get; set; }
        public string[] ExcludedDates { get; set; }
        public string[] UnassignTimes { get; set; }
        public string AssignStartTime { get; set; }
        public string AssignEndTime { get; set; }

        public string[] ExcludeDates(string[] dates)
        {
            List<string> updatedSchedule = new List<string>();
            string s = dates[0];

            // split the single element array into an element for each cell
            string[] values = s.Split(',');

            // remove whitespace
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Trim();
            }

            return values;
        }


        public LaborScheduling(/*string storeCode*/)
        {
            EmployeeListAll = FakeAPI.GetAllEmployees();

            //EmployeeListStore = FakeAPI.GetEmployeesForStore(storeCode);

            ManagerMessageList = FakeAPI.GetMessagesForManager();

            ThisWeek = new WorkWeek();

            /// <summary>
            /// A horizontal table to display the amount of employees needed for each hour of a selected day
            /// </summary>
            DataTable AssignmentTable = new DataTable();
        }
    }
}