
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
        public List<Employees> ListOfEmployees { get; set; }
        public List<EmployeeNotification> EmployeeNotifications { get; set; }
        public DataTable EmployeeSchedule { get; set; }
        public List<EmployeeTimeOffRequest> Requests { get; set; }

        public string employeeId { get; set; }

        public string[] startTimes = {"--", "12:00AM","12:30AM","1:00AM","1:30AM","2:00AM","2:30AM","3:00AM","3:30AM","4:00AM","4:30AM","5:00AM","5:30AM",
                                                  "6:00AM", "6:30AM", "7:00AM", "7:30AM","8:00AM", "8:30AM","9:00AM", "9:30AM","10:00AM", "10:30AM",
                                                  "11:00AM", "11:30AM","12:00PM","12:30PM", "1:00PM", "1:30PM", "2:00PM", "2:30PM", "3:00PM", "3:30PM",
                                                  "4:00PM", "4:30PM", "5:00PM", "5:30PM", "6:00PM", "6:30PM", "7:00PM", "7:30PM", "8:00PM", "8:30PM",
                                                  "9:00PM", "9:30PM", "10:00PM", "10:30PM", "11:00PM"};
        public string[] endTimes = {"--", "12:30AM","1:00AM","1:30AM","2:00AM","2:30AM","3:00AM","3:30AM","4:00AM","4:30AM","5:00AM","5:30AM",
                                                  "6:00AM", "6:30AM", "7:00AM", "7:30AM","8:00AM", "8:30AM","9:00AM", "9:30AM","10:00AM", "10:30AM",
                                                  "11:00AM", "11:30AM","12:00PM","12:30PM", "1:00PM", "1:30PM", "2:00PM", "2:30PM", "3:00PM", "3:30PM",
                                                  "4:00PM", "4:30PM", "5:00PM", "5:30PM", "6:00PM", "6:30PM", "7:00PM", "7:30PM", "8:00PM", "8:30PM",
                                                  "9:00PM", "9:30PM", "10:00PM", "10:30PM", "11:00PM", "11:30PM"};
        public List<string> startDates = new List<string>();
        public List<string> endDates = new List<string>();

        public string messageId { get; set; }
        public string created { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public bool allDayCheck { get; set; }
        public string message { get; set; }
        public DateTime[] CurrentWeekDates { get; set; }

        public EmployeeModel(string LocationCode, string EmployeeID, DateTime[] NextWeekDates)
        {
            //ListOfEmployees = FakeAPI.GetAllEmployees();

            EmployeeNotifications = FakeAPI.GetMessagesForEmployee(EmployeeID);

            EmployeeSchedule = FakeAPI.GetEmployeeSchedule(EmployeeID, NextWeekDates);
        }

        public EmployeeModel()
        {

        }

        /// <summary>
        /// Build the message string for a time off request
        /// </summary>
        /// <param name="name"></param>
        /// <param name="date"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="allDay"></param>
        /// <returns></returns>
        public string CreateMessage(string name, string startDate, string endDate, string startTime, string endTime)
        {
            string message = "";
            string startDay = Convert.ToDateTime(startDate).ToString("dddd");
            string endDay = Convert.ToDateTime(endDate).ToString("dddd");

            if (endDate != null)
            {
                message = name + " has requested the time off for " + startDate + " (" + startDay + ") - " + endDate + " (" + endDay + ")";

                return message;
            }
            else
            {
                // if condition to check if employee wants all day off
                if (startTime == "--" && endTime == "--")
                {
                    message = name + " has requested the day off on " + startDate + " (" + startDay + ")";
                }
                else
                {
                    message = name + " has requested time off on " + startDate + " (" + startDay + ") from " + startTime + " to " + endTime;
                }
                // more checks here to see if employee wanted multiple days off

                return message;
            }
        }

        /// <summary>
        /// Submit a time off request from an employee's home page
        /// </summary>
        /// <param name="created"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="Name"></param>
        /// <param name="Id"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="message"></param>
        public void submitTimeOffRequest(string created, string startDate, string endDate, string Name, string Id, string LocationCode, string startTime, string endTime, string message)
        {
            FakeAPI.SubmitTimeOffRequest(created, startDate, endDate, Name, Id, LocationCode, startTime, endTime, message);
        }

        /// <summary>
        /// Get an employee's messages to display on their home page
        /// </summary>
        /// <param name="EmployeeID"></param>
        public void getMessages(string EmployeeID)
        {
            EmployeeNotifications = FakeAPI.GetMessagesForEmployee(EmployeeID);
        }

    }

    /// <summary>
    /// Base for generating a time off request from the employee home page
    /// </summary>
    public class EmployeeTimeOffRequest
    {
        public string created { get; set; }
        public string Id { get; set; }
        public string LocationCode { get; set; }
        public string Name { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public bool allDayCheck { get; set; }
        public string message { get; set; }
    }

    /// <summary>
    /// Base for generating messages from the manager for the employee home page
    /// </summary>
    public class ManagerNotification
    {
        public int messageId { get; set; }
        public string created { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public string message { get; set; }
        //public string date { get; set; }
    }

    /// <summary>
    /// Base for generating messages from the manager for the employee home page
    /// </summary>
    public class EmployeeNotification
    {
        public int messageId { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public bool approved { get; set; }
        public string message { get; set; }
        public string employeeId { get; set; }
    }

    // managermessage
    //public class EmployeeNotification
    //{
    //    public string id { get; set; }
    //    public DateTime date { get; set; }
    //    public string message { get; set; }
    //    public bool accepted { get; set; }
    //}

    /// <summary>
    /// Employee object used when getting each employee for a store
    /// </summary>
    public class Employees
    {
        public string id { get; set; }
        public string storeCode { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string role { get; set; }
        public int rank { get; set; }
        public int hours { get; set; }
        public double hoursRemaining { get; set; }
        public double hoursScheduled { get; set; }
        public bool[] availableHours { get; set; }
        public bool[] scheduledHours { get; set; }
        public DataTable weeklySchedule { get; set; }
    }

    public class TimeOffRequest
    {
        public EmployeeTimeOffRequest Request { get; set; }
    }

}