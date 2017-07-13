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

        /// <summary>
        /// A horizontal table to display the amount of employees needed for each hour of a selected day
        /// </summary>
        public DataTable AssignmentTable;

        public static List<Employees> EmployeeList { get; set; }


        public LaborScheduling()
        {
            EmployeeList = FakeAPI.CreateEmployees();

            ThisWeek = new WorkWeek();

            //AssignmentTable = new DataTable();

            //string[] dateColumns = new string[8];

            //if (ThisWeek.LaborSchedule.Columns.Count > 0)
            //{
            //    for (int i = 0; i < 8; i++)
            //    {
            //        DataColumn c = ThisWeek.LaborSchedule.Columns[i];
            //        dateColumns[i] = c.Caption;
            //    }

            //    for (int j = 1; j < ThisWeek.LaborSchedule.Columns.Count; j++)
            //    {
            //        //dateColumns[j];
            //        AssignmentTable.Rows[j - 1][0] = dateColumns[j];

            //        for (int h = 0; h < ThisWeek.LaborSchedule.Rows.Count; h++)
            //        {

            //            if (j == 0)
            //            {
            //                //ThisWeek.LaborSchedule.Rows[h][j];
            //                AssignmentTable.Rows[h][j] = dateColumns[j];

            //            }
            //            else
            //            {
            //                //ThisWeek.LaborSchedule.Rows[h][j];
            //                AssignmentTable.Rows[h][j] = dateColumns[j];

            //            }
            //        }
            //    }
            //}
        }
    }
}