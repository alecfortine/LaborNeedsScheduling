
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
    public class Dashboard
    {

        public bool DMCurrentUser = false;
        public bool DMStoreSelect = false;
        public List<string> DistrictStores { get; set; }
        public string SelectedStore { get; set; }

        public string[] ScheduleHalfHourSlots = { "6:00AM", "6:30AM", "7:00AM", "7:30AM","8:00AM", "8:30AM","9:00AM", "9:30AM","10:00AM", "10:30AM",
                                                  "11:00AM", "11:30AM","12:00PM","12:30PM", "1:00PM", "1:30PM", "2:00PM", "2:30PM", "3:00PM", "3:30PM",
                                                  "4:00PM", "4:30PM", "5:00PM", "5:30PM", "6:00PM", "6:30PM", "7:00PM", "7:30PM", "8:00PM", "8:30PM",
                                                  "9:00PM", "9:30PM", "10:00PM", "10:30PM", "11:00PM", "11:30PM", "12:00AM"};
        public string[] SQLHours = {"06:00:00", "06:30:00", "07:00:00", "07:30:00", "08:00:00", "08:30:00", "09:00:00", "09:30:00", "10:00:00", "10:30:00", "11:00:00", "11:30:00", "12:00:00", "12:30:00", "13:00:00", "13:30:00", "14:00:00", "14:30:00",
                                    "15:00:00", "15:30:00", "16:00:00", "16:30:00", "17:00:00", "17:30:00", "18:00:00", "18:30:00", "19:00:00", "19:30:00", "20:00:00", "20:30:00", "21:00:00", "21:30:00", "22:00:00", "22:30:00", "23:00:00", "23:30:00", "00:00:00"};

        public DateTime[] NextWeekDates { get; set; }

        public DataTable AssignedEmployeesRequestedWeek = new DataTable();

        public DataTable AssignedEmployeesNextWeek = new DataTable();
        public List<Employees> EmployeeListStore { get; set; }

        public DateTime[] CurrentWeekDates = new DateTime[7];
        public DateTime[] OneWeekFromNowDates = new DateTime[7];
        public DateTime[] TwoWeeksFromNowDates = new DateTime[7];
        public DateTime[] ThreeWeeksFromNowDates = new DateTime[7];
        public DateTime[] RequestedDates = new DateTime[7];
        public string startdateCurrentWeek { get; set; }
        public string enddateCurrentWeek { get; set; }
        public string startdateOneWeek { get; set; }
        public string enddateOneWeek { get; set; }
        public string startdateTwoWeeks { get; set; }
        public string enddateTwoWeeks { get; set; }
        public string startdateThreeWeeks { get; set; }
        public string enddateThreeWeeks { get; set; }
        public string startdateRequested { get; set; }
        public string enddateRequested { get; set; }
    }
}
