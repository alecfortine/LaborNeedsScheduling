using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LaborNeedsScheduling.Models
{
    public class AvailabilityViewModel
    {
        public List<JakoEmployee> ListOfEmployees { get; set; }

        //public DataTable Availability { get; set; }

        public Dictionary<string, string> EmpsForStore; //<empid, empName>
        public Dictionary<string, DataTable> EmpAvail; //<empid, empAvail>

        public SelectList empList { get; set; }

        public AvailabilityViewModel(string LocationCode)
        {
            ListOfEmployees = FakeAPI.GetEmpsForStore(LocationCode);

            //emp list
            EmpsForStore = new Dictionary<string, string>();
            //avail tables
            EmpAvail = new Dictionary<string, DataTable>();

            //get emps from API
            foreach (JakoEmployee e in ListOfEmployees)
            {
                EmpsForStore.Add(e.id, e.name);
                EmpAvail.Add(e.id, e.AvailabilityTable);
            }

            empList = new SelectList(EmpsForStore, "id", "name");

            //WorkWeek w = new WorkWeek();
            //w.FillDatatables();
            //Availability = w.TimeSelectionTable.Clone();

            //foreach (DataRow dr in Availability.Rows)
            //{
            //    for (int i = 1; i < dr.ItemArray.Length; i++)
            //    {
            //        dr.ItemArray[i] = true;
            //    }
            //}
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