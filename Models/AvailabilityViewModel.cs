using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LaborNeedsScheduling.Models
{
    public class AvailabilityViewModel
    {
        //public List<JakoEmployee> ListOfEmployees { get; set; }

        //public DataTable Availability { get; set; }

        public Dictionary<string, string> EmpsForStore; //<empid, empName>
        //public List<string> EmpsForStore;
        public Dictionary<string, DataTable> EmpAvailabilityTable; //<empid, empAvail>

        // list of employees for the dropdown
        public SelectList empList { get; set; }
        public List<Employees> EmployeeList { get; set; }

        public string[] updatedSchedule { get; set; }
        public string selectedEmployeeId { get; set; }
        public bool EmployeeStatus { get; set; }

        public AvailabilityViewModel(string LocationCode)
        {

            //get the employees for the store
            EmployeeList = FakeAPI.GetAllEmployees();

            // set of employee ids and names
            EmpsForStore = new Dictionary<string, string>();
            //EmpsForStore = new List<string>();

            // set of employee ids and their schedules
            EmpAvailabilityTable = new Dictionary<string, DataTable>();

            EmpsForStore.Add("--", "--");
            for (int i = 0; i < EmployeeList.Count; i++)
            {
                EmpsForStore.Add(EmployeeList[i].id, EmployeeList[i].firstName);

                EmpAvailabilityTable.Add(EmployeeList[i].id, FakeAPI.GetEmployeeAvailability(EmployeeList[i].id));
            }


            //empList = new SelectList(EmpAvailabilityTable, "name", "table");
            empList = new SelectList(EmpsForStore, "Name", "Id");

            //foreach (SelectListItem item in empList.Items)
            //{
            //    Debug.WriteLine(item);
            //    if (Convert.ToString(item) == "[--, --]")
            //    {
            //        item.Disabled = true;
            //    }
            //}
        }

        public AvailabilityViewModel()
        {

        }

        public void UpdateSchedule(AvailabilityViewModel avm, string[] schedule, string employeeId)
        {
            List<string> updatedSchedule = new List<string>();
            string s = schedule[0];

            // split the single element array into an element for each cell
            string[] values = s.Split(',');

            // remove whitespace
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Trim();
            }

            // remove timecells form the array and add the updated array to a list
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == "True" || values[i] == "False")
                {
                    updatedSchedule.Add(values[i]);
                }
            }

            FakeAPI.UpdateEmployeeAvailability(avm, updatedSchedule, employeeId);

        }

    }

    public class JakoEmployee
    {
        public string id { get; set; }
        public string name { get; set; }
        public string storeno { get; set; }
        public DataTable AvailabilityTable { get; set; }
        public bool[,] TimeCheck { get; set; }
    }

}