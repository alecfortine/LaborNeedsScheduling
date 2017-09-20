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
    public class Dashboard
    {
        public string[] ScheduleHalfHourSlots = { "6:00AM", "6:30AM", "7:00AM", "7:30AM","8:00AM", "8:30AM","9:00AM", "9:30AM","10:00AM", "10:30AM",
                                                  "11:00AM", "11:30AM","12:00PM","12:30PM", "1:00PM", "1:30PM", "2:00PM", "2:30PM", "3:00PM", "3:30PM",
                                                  "4:00PM", "4:30PM", "5:00PM", "5:30PM", "6:00PM", "6:30PM", "7:00PM", "7:30PM", "8:00PM", "8:30PM",
                                                  "9:00PM", "9:30PM", "10:00PM", "10:30PM", "11:00PM", "11:30PM", "12:00AM"};

        public DateTime[] CurrentWeekDates { get; set; }

        public Dictionary<string, Dictionary<string, List<string>>> AssignedEmployeesForWeek = new Dictionary<string, Dictionary<string, List<string>>>();
    }
}
