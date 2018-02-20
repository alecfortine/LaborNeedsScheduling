using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LaborNeedsScheduling.Models;

using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Configuration;

namespace LaborNeedsScheduling.Controllers
{
    public class HomeController : Controller
    {
        public string user = System.Web.HttpContext.Current.User.Identity.Name;
        //public string user = "JAKOENT\\store1010";

        public string GetStoreCode()
        {
            string StoreCode;
            Session["DMCurrentUser"] = false;

            if (Session["DMStoreCode"] != null)
            {
                StoreCode = (string)Session["DMStoreCode"];
            }
            else
            {
                StoreCode = FakeAPI.GetStoreCode(user);
                if(StoreCode == "DM")
                {
                    Session["DMCurrentUser"] = true;
                }
            }

            return StoreCode;
        }

        //public ActionResult RedirectDMStoreSelect()
        //{

        //    string DistrictCode = FakeAPI.GetDistrictCode(user);
        //    List<string> DistrictStoreCodes = FakeAPI.GetStoresForDistrict(DistrictCode);

        //    return View("DMStoreSelect", DistrictStoreCodes);
        //}

        // import the previous week's schedule into the selected week
        public ActionResult ImportSchedule()
        {
            LaborScheduling ls = (LaborScheduling)Session["LaborSchedulingPartial"];

            ls.ThisWeek.currentStoreCode = GetStoreCode();
            DateTime[] RequestedDates = ls.ThisWeek.RequestedDates;
            DateTime[] PreviousWeekDates = new DateTime[7];
            for (int i = 0; i < 7; i++)
            {
                PreviousWeekDates[i] = RequestedDates[i].AddDays(-7);
            }
            List<string> employeeIds = new List<string>();
            foreach (Employees emp in ls.ThisWeek.employeeListStore)
            {
                employeeIds.Add(emp.id);
            }
            Dictionary<string, Dictionary<string, string[]>> EmployeeSchedulesLastWeek = FakeAPI.GetEmployeeScheduledTimes(employeeIds.ToArray(), GetStoreCode(), RequestedDates);

            ls.ThisWeek.EmployeeScheduledTimes = EmployeeSchedulesLastWeek;

            FakeAPI.ImportLastWeekSchedule(GetStoreCode(), employeeIds.ToArray(), RequestedDates, PreviousWeekDates);

            //ls.ThisWeek.EmployeeScheduledTimes = ls.ThisWeek.UpdateEmployees(ls.ThisWeek.employeeListStore);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateTotalHours();

            return PartialView("_LaborScheduleAssignmentView", ls);
        }

        // clear the selected week schedule
        public ActionResult ClearSchedule()
        {
            LaborScheduling ls = (LaborScheduling)Session["LaborSchedulingPartial"];

            List<string> employeeIds = new List<string>();
            foreach (Employees emp in ls.ThisWeek.employeeListStore)
            {
                employeeIds.Add(emp.id);
            }

            FakeAPI.ClearRequestedWeekSchedule(GetStoreCode(), employeeIds.ToArray(), ls.ThisWeek.RequestedDates);

            //ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            //ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            //ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            //ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            //ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            //ls.ThisWeek.GenerateTotalHours();

            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateTotalHours();

            return PartialView("_LaborScheduleAssignmentView", ls);
        }

        // schedule a default employee block based on rank
        public ActionResult AssignEmployeeBlock(string employeeId, int StartHour)
        {
            LaborScheduling ls = (LaborScheduling)Session["LaborSchedulingPartial"];

            string StartTime = ls.ThisWeek.AssignmentView.Columns[StartHour].ToString();
            ls.ThisWeek.startHour = StartTime;
            ls.ThisWeek.selectedEmployeeId = employeeId;
            ls.ThisWeek.employeeListStore = FakeAPI.GetEmployeesForStore(ls.ThisWeek.currentStoreCode);

            string endHour = ls.ThisWeek.AssignmentView.Columns[ls.ThisWeek.AssignmentView.Columns.Count - 1].Caption;
            int endhourPosition = 0;
            for (int i = 0; i < ls.ThisWeek.ScheduleHalfHourSlots.Length; i++)
            {
                if (ls.ThisWeek.ScheduleHalfHourSlots[i] == endHour)
                {
                    endhourPosition = i;
                }
            }
            foreach (Employees emp in ls.ThisWeek.employeeListStore)
            {
                if (emp.id == employeeId)
                {
                    if (emp.rank < 40)
                    {
                        for (int i = 0; i < ls.ThisWeek.ScheduleHalfHourSlots.Length; i++)
                        {
                            if (ls.ThisWeek.ScheduleHalfHourSlots[i] == StartTime)
                            {
                                if ((i + 8) > endhourPosition)
                                {
                                    ls.ThisWeek.endHour = ls.ThisWeek.ScheduleHalfHourSlots[endhourPosition];
                                }
                                else
                                {
                                    ls.ThisWeek.endHour = ls.ThisWeek.ScheduleHalfHourSlots[i + 7];
                                }
                            }
                        }
                    }
                    else if (emp.rank >= 40)
                    {
                        for (int i = 0; i < ls.ThisWeek.ScheduleHalfHourSlots.Length; i++)
                        {
                            if (ls.ThisWeek.ScheduleHalfHourSlots[i] == StartTime)
                            {
                                if ((i + 16) > endhourPosition)
                                {
                                    ls.ThisWeek.endHour = ls.ThisWeek.ScheduleHalfHourSlots[endhourPosition];
                                }
                                else
                                {
                                    ls.ThisWeek.endHour = ls.ThisWeek.ScheduleHalfHourSlots[i + 15];
                                }
                            }
                        }
                    }
                }
            }

            Dictionary<string, string[]> EmployeeAvailability = ls.ThisWeek.EmployeeAvailableTimes[employeeId];
            string[] AvailableTimes = EmployeeAvailability[ls.ThisWeek.weekdayNames[ls.ThisWeek.selectedWeekday]];
            bool block = false;
            int startposition = 0;
            for (int i = 0; i < ls.ThisWeek.ScheduleHalfHourSlots.Length; i++)
            {
                if (ls.ThisWeek.ScheduleHalfHourSlots[i] == StartTime)
                {
                    startposition = i;
                    break;
                }
            }
            int halfhouriterator = 0;
            for (int i = 0; i < AvailableTimes.Length - 1; i++)
            {
                if (AvailableTimes[i] == ls.ThisWeek.ScheduleHalfHourSlots[startposition])
                {
                    block = true;
                }
                if (AvailableTimes[i] == ls.ThisWeek.endHour)
                {
                    block = false;
                }
                if (block == true)
                {
                    halfhouriterator++;
                    if (AvailableTimes[i + 1] != ls.ThisWeek.ScheduleHalfHourSlots[startposition + halfhouriterator])
                    {
                        ls.ThisWeek.endHour = AvailableTimes[i];
                        block = false;
                    }
                }
            }

            ls.ThisWeek.EmployeeScheduledTimes = ls.ThisWeek.UpdateEmployees(ls.ThisWeek.employeeListStore);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.GenerateTotalHours();

            return PartialView("_LaborScheduleAssignmentView", ls);
        }

        // unassign an employee block based on start time
        public ActionResult UnassignEmployeeBlock(string employeeId, string StartHour)
        {
            LaborScheduling ls = (LaborScheduling)Session["LaborSchedulingPartial"];
            string StartTime = "";
            string EndTime = "";

            for (int i = 0; i < ls.ThisWeek.ScheduleHalfHourSlots.Length; i++)
            {
                if (ls.ThisWeek.ScheduleHalfHourSlots[i] == StartHour)
                {
                    StartTime = ls.ThisWeek.SQLHours[i];
                }
            }
            for (int i = 0; i < ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows.Count; i++)
            {
                if (ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][1].ToString() == employeeId &&
                    ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][4].ToString() == StartTime)
                {
                    StartTime = ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][4].ToString();
                    EndTime = ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][5].ToString();
                }
            }
            ls.ThisWeek.selectedEmployeeId = employeeId;
            for (int i = 0; i < ls.ThisWeek.SQLHours.Length; i++)
            {
                if (ls.ThisWeek.SQLHours[i] == StartTime)
                {
                    ls.ThisWeek.startHour = ls.ThisWeek.ScheduleHalfHourSlots[i];
                }

                if (ls.ThisWeek.SQLHours[i] == EndTime && i > 0)
                {
                    ls.ThisWeek.endHour = ls.ThisWeek.ScheduleHalfHourSlots[i];
                }
            }


            string unassignTimes = "";
            bool block = false;
            for (int i = 0; i < ls.ThisWeek.ScheduleHalfHourSlots.Length; i++)
            {
                if (ls.ThisWeek.ScheduleHalfHourSlots[i] == ls.ThisWeek.startHour)
                {
                    block = true;
                }
                if (ls.ThisWeek.ScheduleHalfHourSlots[i] == ls.ThisWeek.endHour)
                {
                    block = false;
                }
                if (block == true)
                {
                    if (ls.ThisWeek.ScheduleHalfHourSlots[i + 1] != ls.ThisWeek.endHour)
                    {
                        unassignTimes += ls.ThisWeek.ScheduleHalfHourSlots[i] + ",";
                    }
                    else
                    {
                        unassignTimes += ls.ThisWeek.ScheduleHalfHourSlots[i];
                    }
                }
            }

            FakeAPI.UnassignEmployee(unassignTimes, employeeId, ls.ThisWeek.selectedWeekday, ls.ThisWeek.EmployeeScheduledTimes, ls.ThisWeek.RequestedDates, GetStoreCode());
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateTotalHours();

            return PartialView("_LaborScheduleAssignmentView", ls);
        }

        // add either an hour or a half hour before store opening
        public ActionResult ToggleHourSlots(int slots)
        {
            LaborScheduling ls = (LaborScheduling)Session["LaborSchedulingPartial"];

            ls.ThisWeek.openHourSlots = slots;

            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.FillAllocatedHoursTable(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.FillAssignmentTable(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.GenerateBlackoutAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.GenerateTotalHours();

            return PartialView("_LaborScheduleAssignmentView", ls);
        }

        // methods for adding or subtracting hours from an employee's schedule via clicking the + and - buttons
        public ActionResult SubtractHourStart(string employeeId, string hour)
        {
            LaborScheduling ls = (LaborScheduling)Session["LaborSchedulingPartial"];

            string convertedHour = "";
            for (int i = 0; i < ls.ThisWeek.ScheduleHalfHourSlots.Length; i++)
            {
                if (ls.ThisWeek.ScheduleHalfHourSlots[i] == hour)
                {
                    convertedHour = ls.ThisWeek.SQLHours[i];
                }
            }

            string StartTime = "";
            string EndTime = "";
            for (int i = 0; i < ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows.Count; i++)
            {
                if (ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][1].ToString() == employeeId &&
                    ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][4].ToString() == convertedHour)
                {
                    StartTime = ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][4].ToString();
                    EndTime = ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][5].ToString();
                }
            }
            ls.ThisWeek.selectedEmployeeId = employeeId;
            for (int i = 0; i < ls.ThisWeek.SQLHours.Length; i++)
            {
                if (ls.ThisWeek.SQLHours[i] == StartTime)
                {
                    ls.ThisWeek.startHour = ls.ThisWeek.ScheduleHalfHourSlots[i];
                }

                if (ls.ThisWeek.SQLHours[i] == EndTime && i > 0)
                {
                    ls.ThisWeek.endHour = ls.ThisWeek.ScheduleHalfHourSlots[i];
                }
            }
            FakeAPI.UnassignEmployee(ls.ThisWeek.startHour, employeeId, ls.ThisWeek.selectedWeekday, ls.ThisWeek.EmployeeScheduledTimes, ls.ThisWeek.RequestedDates, GetStoreCode());
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateTotalHours();
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);


            return PartialView("_LaborScheduleAssignmentView", ls);
        }

        public ActionResult AddHourStart(string employeeId, string hour)
        {
            LaborScheduling ls = (LaborScheduling)Session["LaborSchedulingPartial"];

            string convertedHour = "";
            for (int i = 0; i < ls.ThisWeek.ScheduleHalfHourSlots.Length; i++)
            {
                if (ls.ThisWeek.ScheduleHalfHourSlots[i] == hour && i < ls.ThisWeek.ScheduleHalfHourSlots.Length - 1)
                {
                    convertedHour = ls.ThisWeek.SQLHours[i + 1];
                }
            }

            string StartTime = "";
            string EndTime = "";
            for (int i = 0; i < ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows.Count; i++)
            {
                if (ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][1].ToString() == employeeId &&
                    ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][4].ToString() == convertedHour)
                {
                    StartTime = ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][4].ToString();
                    EndTime = ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][5].ToString();
                }
            }
            ls.ThisWeek.selectedEmployeeId = employeeId;
            for (int i = 0; i < ls.ThisWeek.SQLHours.Length; i++)
            {
                if (ls.ThisWeek.SQLHours[i] == StartTime && i > 0)
                {
                    ls.ThisWeek.startHour = ls.ThisWeek.ScheduleHalfHourSlots[i - 1];
                }

                if (ls.ThisWeek.SQLHours[i] == EndTime && i > 0)
                {
                    ls.ThisWeek.endHour = ls.ThisWeek.ScheduleHalfHourSlots[i - 1];
                }
            }
            ls.ThisWeek.EmployeeScheduledTimes = ls.ThisWeek.UpdateEmployees(ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateTotalHours();

            return PartialView("_LaborScheduleAssignmentView", ls);
        }

        public ActionResult SubtractHourEnd(string employeeId, string hour)
        {
            LaborScheduling ls = (LaborScheduling)Session["LaborSchedulingPartial"];

            string convertedHour = "";
            for (int i = 0; i < ls.ThisWeek.ScheduleHalfHourSlots.Length; i++)
            {
                if (ls.ThisWeek.ScheduleHalfHourSlots[i] == hour && i < ls.ThisWeek.ScheduleHalfHourSlots.Length - 1)
                {
                    convertedHour = ls.ThisWeek.SQLHours[i + 1];
                }
            }

            string StartTime = "";
            string EndTime = "";
            for (int i = 0; i < ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows.Count; i++)
            {
                if (ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][1].ToString() == employeeId &&
                    ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][5].ToString() == convertedHour)
                {
                    StartTime = ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][4].ToString();
                    EndTime = ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][5].ToString();
                }
            }
            ls.ThisWeek.selectedEmployeeId = employeeId;
            for (int i = 0; i < ls.ThisWeek.SQLHours.Length; i++)
            {
                if (ls.ThisWeek.SQLHours[i] == StartTime)
                {
                    ls.ThisWeek.startHour = ls.ThisWeek.ScheduleHalfHourSlots[i];
                }

                if (ls.ThisWeek.SQLHours[i] == EndTime && i > 0)
                {
                    ls.ThisWeek.endHour = ls.ThisWeek.ScheduleHalfHourSlots[i - 1];
                }
            }

            FakeAPI.UnassignEmployee(ls.ThisWeek.endHour, employeeId, ls.ThisWeek.selectedWeekday, ls.ThisWeek.EmployeeScheduledTimes, ls.ThisWeek.RequestedDates, GetStoreCode());
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateTotalHours();
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);

            return PartialView("_LaborScheduleAssignmentView", ls);
        }

        public ActionResult AddHourEnd(string employeeId, string hour)
        {
            LaborScheduling ls = (LaborScheduling)Session["LaborSchedulingPartial"];

            string convertedHour = "";
            for (int i = 0; i < ls.ThisWeek.ScheduleHalfHourSlots.Length; i++)
            {
                if (ls.ThisWeek.ScheduleHalfHourSlots[i] == hour)
                {
                    convertedHour = ls.ThisWeek.SQLHours[i];
                }
            }

            string StartTime = "";
            string EndTime = "";
            for (int i = 0; i < ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows.Count; i++)
            {
                if (ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][1].ToString() == employeeId &&
                    ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][5].ToString() == convertedHour)
                {
                    StartTime = ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][4].ToString();
                    EndTime = ls.ThisWeek.AssignedEmployeesRequestedWeek.Rows[i][5].ToString();
                }
            }
            ls.ThisWeek.selectedEmployeeId = employeeId;
            for (int i = 0; i < ls.ThisWeek.SQLHours.Length; i++)
            {
                if (ls.ThisWeek.SQLHours[i] == StartTime)
                {
                    ls.ThisWeek.startHour = ls.ThisWeek.ScheduleHalfHourSlots[i];
                }

                if (ls.ThisWeek.SQLHours[i] == EndTime)
                {
                    ls.ThisWeek.endHour = ls.ThisWeek.ScheduleHalfHourSlots[i];
                }
            }
            ls.ThisWeek.EmployeeScheduledTimes = ls.ThisWeek.UpdateEmployees(ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateTotalHours();

            return PartialView("_LaborScheduleAssignmentView", ls);
        }

        public ActionResult AddEmployee(LaborScheduling ls, string EmployeeId /*bool AddEmployee*/)
        {
            ls = (LaborScheduling)Session["LaborSchedulingPartial"];
            //string employeeId = (string)Session["BorrowEmployeeId"];

            ls.ThisWeek.AddEmployeeToList(EmployeeId);
            ls.ThisWeek.employeeListStore = FakeAPI.GetEmployeesForStore(GetStoreCode());

            Session["UpdatedEmployeeListStore"] = ls.ThisWeek.employeeListStore;
            LaborScheduling.EmployeeListStore = ls.ThisWeek.employeeListStore;

            List<string> EmployeeIds = new List<string>();
            foreach (Employees emp in ls.ThisWeek.employeeListStore)
            {
                EmployeeIds.Add(emp.id);
            }

            ls.ThisWeek.GenerateEmployeeAvailability();
            ls.ThisWeek.EmployeeScheduledTimes = FakeAPI.GetEmployeeScheduledTimes(EmployeeIds.ToArray(), GetStoreCode(), ls.ThisWeek.RequestedDates);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateTotalHours();

            Session["LaborSchedulingPartial"] = ls;

            return PartialView("_LaborScheduleAssignmentView", ls);
        }

        // remove a borrowed employee from the store list
        public ActionResult RemoveEmployee(string EmployeeId)
        {
            LaborScheduling ls = (LaborScheduling)Session["LaborSchedulingPartial"];

            FakeAPI.RemoveBorrowedEmployee(EmployeeId, GetStoreCode());
            ls.ThisWeek.employeeListStore = FakeAPI.GetEmployeesForStore(GetStoreCode());

            Session["UpdatedEmployeeListStore"] = ls.ThisWeek.employeeListStore;
            LaborScheduling.EmployeeListStore = ls.ThisWeek.employeeListStore;

            List<string> EmployeeIds = new List<string>();
            foreach (Employees emp in ls.ThisWeek.employeeListStore)
            {
                EmployeeIds.Add(emp.id);
            }

            ls.ThisWeek.GenerateEmployeeAvailability();
            ls.ThisWeek.EmployeeScheduledTimes = FakeAPI.GetEmployeeScheduledTimes(EmployeeIds.ToArray(), GetStoreCode(), ls.ThisWeek.RequestedDates);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateTotalHours();

            Session["LaborSchedulingPartial"] = ls;

            return PartialView("_LaborScheduleAssignmentView", ls);
        }

        //public void ChooseNewStore()
        //{
        //    Session["DMStoreCode"] = null;

        //    Dashboard("", false);
        //}

        [HttpGet]
        public ActionResult DMDashboard(string SelectedStore, string ResetLocation)
        {
            //FakeAPI.dothething();
            if (ResetLocation == "true")
            {
                Session["DMStoreCode"] = null;
            }

            if (SelectedStore != null && SelectedStore != "")
            {
                Session["DMStoreCode"] = SelectedStore;
            }

            string StoreCode;

            if (Session["DMStoreCode"] != null)
            {
                StoreCode = (string)Session["DMStoreCode"];
                Session["DMCurrentUser"] = true;
            }
            else
            {
                StoreCode = GetStoreCode();
            }

            if (StoreCode == "DM" && Session["DMStoreCode"] == null)
            {
                string DistrictCode = FakeAPI.GetDistrictCode(user);
                List<string> DistrictStoreCodes = FakeAPI.GetStoresForDistrict(DistrictCode);

                Dashboard StoreSelect = new Dashboard();
                StoreSelect.DMCurrentUser = true;
                StoreSelect.DMStoreSelect = true;
                StoreSelect.DistrictStores = DistrictStoreCodes;

                return View("DashBoard", StoreSelect);
            }
            else
            {
                Dashboard Dash = new Dashboard();

                if ((bool)Session["DMCurrentUser"] == true)
                {
                    Dash.DMCurrentUser = true;
                }

                DateTime currentWeekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
                DateTime[] CurrentWeekDates = new DateTime[7];
                DateTime[] OneWeekFromNowDates = new DateTime[7];
                DateTime[] TwoWeeksFromNowDates = new DateTime[7];
                DateTime[] ThreeWeeksFromNowDates = new DateTime[7];

                for (int i = 0; i < 7; i++)
                {
                    CurrentWeekDates[i] = currentWeekMarker.AddDays(i);
                    OneWeekFromNowDates[i] = currentWeekMarker.AddDays(i + 7);
                    TwoWeeksFromNowDates[i] = currentWeekMarker.AddDays(i + 14);
                    ThreeWeeksFromNowDates[i] = currentWeekMarker.AddDays(i + 21);
                }

                Dash.CurrentWeekDates = CurrentWeekDates;
                Dash.OneWeekFromNowDates = OneWeekFromNowDates;
                Dash.TwoWeeksFromNowDates = TwoWeeksFromNowDates;
                Dash.ThreeWeeksFromNowDates = ThreeWeeksFromNowDates;

                Dash.RequestedDates = CurrentWeekDates;
                Dash.startdateCurrentWeek = Dash.CurrentWeekDates[0].ToShortDateString();
                Dash.enddateCurrentWeek = Dash.CurrentWeekDates[6].ToShortDateString();
                Dash.startdateOneWeek = Dash.OneWeekFromNowDates[0].ToShortDateString();
                Dash.enddateOneWeek = Dash.OneWeekFromNowDates[6].ToShortDateString();
                Dash.startdateTwoWeeks = Dash.TwoWeeksFromNowDates[0].ToShortDateString();
                Dash.enddateTwoWeeks = Dash.TwoWeeksFromNowDates[6].ToShortDateString();
                Dash.startdateThreeWeeks = Dash.ThreeWeeksFromNowDates[0].ToShortDateString();
                Dash.enddateThreeWeeks = Dash.ThreeWeeksFromNowDates[6].ToShortDateString();

                Dash.EmployeeListStore = FakeAPI.GetEmployeesForStore(StoreCode);
                Dash.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(Dash.RequestedDates);
                Dash.SelectedStore = StoreCode;

                return PartialView("DashBoard", Dash);
            }
        }


        [HttpGet]
        public ActionResult Dashboard(string SelectedStore, string ResetLocation)
        {
            if(ResetLocation == "true")
            {
                Session["DMStoreCode"] = null;
            }

            if(SelectedStore != null && SelectedStore != "")
            {
                Session["DMStoreCode"] = SelectedStore;
            }

            string StoreCode;

            if (Session["DMStoreCode"] != null)
            {
                StoreCode = (string)Session["DMStoreCode"];
                Session["DMCurrentUser"] = true;
            }
            else
            {
                StoreCode = GetStoreCode();
            }

            if (StoreCode == "DM" && Session["DMStoreCode"] == null)
            {
                string DistrictCode = FakeAPI.GetDistrictCode(user);
                List<string> DistrictStoreCodes = FakeAPI.GetStoresForDistrict(DistrictCode);

                Dashboard StoreSelect = new Dashboard();
                StoreSelect.DMCurrentUser = true;
                StoreSelect.DMStoreSelect = true;
                StoreSelect.DistrictStores = DistrictStoreCodes;

                return View("DashBoard", StoreSelect);
            }
            else
            {
                Dashboard Dash = new Dashboard();

                if ((bool)Session["DMCurrentUser"] == true)
                {
                    Dash.DMCurrentUser = true;
                }

                DateTime currentWeekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
                DateTime[] CurrentWeekDates = new DateTime[7];
                DateTime[] OneWeekFromNowDates = new DateTime[7];
                DateTime[] TwoWeeksFromNowDates = new DateTime[7];
                DateTime[] ThreeWeeksFromNowDates = new DateTime[7];

                for (int i = 0; i < 7; i++)
                {
                    CurrentWeekDates[i] = currentWeekMarker.AddDays(i);
                    OneWeekFromNowDates[i] = currentWeekMarker.AddDays(i + 7);
                    TwoWeeksFromNowDates[i] = currentWeekMarker.AddDays(i + 14);
                    ThreeWeeksFromNowDates[i] = currentWeekMarker.AddDays(i + 21);
                }

                Dash.CurrentWeekDates = CurrentWeekDates;
                Dash.OneWeekFromNowDates = OneWeekFromNowDates;
                Dash.TwoWeeksFromNowDates = TwoWeeksFromNowDates;
                Dash.ThreeWeeksFromNowDates = ThreeWeeksFromNowDates;

                Dash.RequestedDates = CurrentWeekDates;
                Dash.startdateCurrentWeek = Dash.CurrentWeekDates[0].ToShortDateString();
                Dash.enddateCurrentWeek = Dash.CurrentWeekDates[6].ToShortDateString();
                Dash.startdateOneWeek = Dash.OneWeekFromNowDates[0].ToShortDateString();
                Dash.enddateOneWeek = Dash.OneWeekFromNowDates[6].ToShortDateString();
                Dash.startdateTwoWeeks = Dash.TwoWeeksFromNowDates[0].ToShortDateString();
                Dash.enddateTwoWeeks = Dash.TwoWeeksFromNowDates[6].ToShortDateString();
                Dash.startdateThreeWeeks = Dash.ThreeWeeksFromNowDates[0].ToShortDateString();
                Dash.enddateThreeWeeks = Dash.ThreeWeeksFromNowDates[6].ToShortDateString();

                Dash.EmployeeListStore = FakeAPI.GetEmployeesForStore(StoreCode);
                Dash.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(Dash.RequestedDates);
                Dash.SelectedStore = StoreCode;

                return View("DashBoard", Dash);
            }
        }

        [HttpPost]
        public ActionResult Dashboard(Dashboard Dash, string RequestedDate)
        {
            Dash.startdateRequested = RequestedDate;

            DateTime currentWeekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
            DateTime[] CurrentWeekDates = new DateTime[7];
            DateTime[] OneWeekFromNowDates = new DateTime[7];
            DateTime[] TwoWeeksFromNowDates = new DateTime[7];
            DateTime[] ThreeWeeksFromNowDates = new DateTime[7];

            for (int i = 0; i < 7; i++)
            {
                CurrentWeekDates[i] = currentWeekMarker.AddDays(i);
                OneWeekFromNowDates[i] = currentWeekMarker.AddDays(i + 7);
                TwoWeeksFromNowDates[i] = currentWeekMarker.AddDays(i + 14);
                ThreeWeeksFromNowDates[i] = currentWeekMarker.AddDays(i + 21);
            }

            Dash.CurrentWeekDates = CurrentWeekDates;
            Dash.OneWeekFromNowDates = OneWeekFromNowDates;
            Dash.TwoWeeksFromNowDates = TwoWeeksFromNowDates;
            Dash.ThreeWeeksFromNowDates = ThreeWeeksFromNowDates;

            Dash.startdateCurrentWeek = Dash.CurrentWeekDates[0].ToShortDateString();
            Dash.enddateCurrentWeek = Dash.CurrentWeekDates[6].ToShortDateString();
            Dash.startdateOneWeek = Dash.OneWeekFromNowDates[0].ToShortDateString();
            Dash.enddateOneWeek = Dash.OneWeekFromNowDates[6].ToShortDateString();
            Dash.startdateTwoWeeks = Dash.TwoWeeksFromNowDates[0].ToShortDateString();
            Dash.enddateTwoWeeks = Dash.TwoWeeksFromNowDates[6].ToShortDateString();
            Dash.startdateThreeWeeks = Dash.ThreeWeeksFromNowDates[0].ToShortDateString();
            Dash.enddateThreeWeeks = Dash.ThreeWeeksFromNowDates[6].ToShortDateString();

            if (RequestedDate == Dash.startdateCurrentWeek)
            {
                Dash.RequestedDates = Dash.CurrentWeekDates;
            }
            else if (RequestedDate == Dash.startdateOneWeek)
            {
                Dash.RequestedDates = Dash.OneWeekFromNowDates;
            }
            else if (RequestedDate == Dash.startdateTwoWeeks)
            {
                Dash.RequestedDates = Dash.TwoWeeksFromNowDates;
            }
            else if (RequestedDate == Dash.startdateThreeWeeks)
            {
                Dash.RequestedDates = Dash.ThreeWeeksFromNowDates;
            }

            Dash.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(Dash.RequestedDates);
            Dash.EmployeeListStore = FakeAPI.GetEmployeesForStore(GetStoreCode());

            return PartialView("_DashboardTable", Dash);
        }

        [HttpGet]
        public ActionResult DMStoreSelect()
        {

            return View();
        }
        [HttpPost]
        public ActionResult DMStoreSelect(string StoreCode)
        {

            return View();
        }

        [HttpGet]
        public ActionResult ManagerFunctionality()
        {
            string strUser = System.Web.HttpContext.Current.User.Identity.Name;

            //AppSecurity AppSec = new AppSecurity();
            //if (!AppSec.UserHasAccess(strUser, "OutBoundTransfers"))
            //{
            //    Response.Redirect("~/SecurityError.aspx");
            //}
            //AppSec = null;

            string why = (string)Session["DMStoreCode"];

            return View();
        }

        [HttpGet]
        public ActionResult TimeOffRequest()
        {
            EmployeeModel TimeOff = new EmployeeModel();
            for (int i = 0; i < 30; i++)
            {
                TimeOff.startDates.Add(DateTime.Now.AddDays(i + 1).ToString("M/d/yyyy"));
            }
            for (int i = 1; i < 30; i++)
            {
                TimeOff.endDates.Add(DateTime.Now.AddDays(i + 1).ToString("M/d/yyyy"));
            }
            TimeOff.ListOfEmployees = FakeAPI.GetEmployeesForStore(GetStoreCode());

            return View("TimeOffRequest", TimeOff);
        }
        [HttpPost]
        public ActionResult TimeOffRequest(EmployeeModel TimeOff, string employeeId)
        {
            EmployeeTimeOffRequest TimeOffRequest = new EmployeeTimeOffRequest();
            List<Employees> ListOfEmployees = FakeAPI.GetEmployeesForStore(GetStoreCode());

            //string employeeId = (string)Session["EmployeeId"];
            string employeeName = "";

            // fill the dropdown with dates from the next two weeks
            for (int i = 0; i < 14; i++)
            {
                TimeOff.startDates.Add(DateTime.Now.AddDays(i + 1).ToString("M/d/yyyy"));
            }

            // get employee name
            foreach (Employees emp in ListOfEmployees)
            {
                if (emp.id == employeeId)
                {
                    employeeName = emp.firstName + " " + emp.lastName;
                }
            }

            if (employeeName == "")
            {
                employeeName = employeeId;
            }

            // set the time off request variables
            TimeOffRequest.Id = employeeId;
            TimeOffRequest.LocationCode = GetStoreCode();
            TimeOffRequest.Name = employeeName;
            TimeOffRequest.startDate = TimeOff.startDate;
            TimeOffRequest.endDate = TimeOff.endDate;
            TimeOffRequest.allDayCheck = TimeOff.allDayCheck;

            if (TimeOff.allDayCheck == true)
            {
                TimeOffRequest.startTime = "--";
                TimeOffRequest.endTime = "--";
            }
            else
            {
                TimeOffRequest.startTime = TimeOff.startTime;
                TimeOffRequest.endTime = TimeOff.endTime;
            }
            TimeOffRequest.created = Convert.ToString(DateTime.Now);
            TimeOffRequest.message = TimeOff.CreateMessage(TimeOffRequest.Name, TimeOffRequest.startDate, TimeOffRequest.endDate, TimeOffRequest.startTime, TimeOffRequest.endTime);

            // submit the time off request to the database
            TimeOff.submitTimeOffRequest(TimeOffRequest.created, TimeOffRequest.startDate, TimeOffRequest.endDate,
                                                  TimeOffRequest.Name, TimeOffRequest.Id, TimeOffRequest.LocationCode, TimeOffRequest.startTime,
                                                  TimeOffRequest.endTime, TimeOffRequest.message);

            //TimeOff = (EmployeeModel)Session["TimeOff"];
            //TimeOff.getMessages(TimeOffRequest.Id);
            TimeOff = new EmployeeModel();
            for (int i = 0; i < 30; i++)
            {
                TimeOff.startDates.Add(DateTime.Now.AddDays(i + 1).ToString("M/d/yyyy"));
            }
            for (int i = 1; i < 30; i++)
            {
                TimeOff.endDates.Add(DateTime.Now.AddDays(i + 1).ToString("M/d/yyyy"));
            }
            TimeOff.ListOfEmployees = FakeAPI.GetEmployeesForStore(GetStoreCode());

            return View("TimeOffRequest", TimeOff);
        }

        public ActionResult approveRequest(int messageId)
        {
            LaborScheduling ls = (LaborScheduling)Session["lsView"];

            string employeeId = (string)Session["EmployeeId"];

            FakeAPI.approveRequest(messageId, employeeId, GetStoreCode());

            return View();
        }
        public ActionResult denyRequest(int messageId)
        {
            string employeeId = (string)Session["EmployeeId"];

            FakeAPI.denyRequest(messageId, employeeId, GetStoreCode());

            return View();
        }

        public ActionResult removeEmployeeMessage(int messageId)
        {

            FakeAPI.DeleteEmployeeMessage(messageId);

            return View();
        }

        [HttpGet]
        public ActionResult LaborSchedule()
        {
            //FakeAPI.dothething();
            GetStoreCode();

            LaborScheduling LaborSchedulingViewModel = new LaborScheduling(GetStoreCode());
            Session["LaborSchedulingViewModel"] = LaborSchedulingViewModel;

            LaborScheduling ls = (LaborScheduling)Session["lsView"];

            if (ls != null)
            {
                LaborSchedulingViewModel.ThisWeek.currentStoreCode = GetStoreCode();

                if ((List<Employees>)Session["UpdatedEmployeeListStore"] != null)
                {
                    LaborSchedulingViewModel.ThisWeek.employeeListStore = (List<Employees>)Session["UpdatedEmployeeListStore"];
                }
                else
                {
                    LaborSchedulingViewModel.ThisWeek.employeeListStore = FakeAPI.GetEmployeesForStore(GetStoreCode());
                }

                // get store variables
                if (LaborSchedulingViewModel.ThisWeek.weekWeighting == null)
                {
                    LaborSchedulingViewModel.ThisWeek.weekWeighting = LaborSchedulingViewModel.ThisWeek.getDefaultWeights(LaborSchedulingViewModel.ThisWeek.NumberHistoricalWeeks);
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly = FakeAPI.GetMinEmployeesEarly(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesLater = FakeAPI.GetMinEmployeesLater(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.MaxEmployees = FakeAPI.GetMaxEmployees(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.WeekdayPowerHours = FakeAPI.GetWeekdayPowerHours(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.WeekendPowerHours = FakeAPI.GetWeekendPowerHours(GetStoreCode());

                    Session["WeekWeighting"] = LaborSchedulingViewModel.ThisWeek.weekWeighting;
                    Session["MinEmployeesEarly"] = LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly;
                    Session["MinEmployeesLater"] = LaborSchedulingViewModel.ThisWeek.MinEmployeesLater;
                    Session["MaxEmployees"] = LaborSchedulingViewModel.ThisWeek.MaxEmployees;
                    Session["WeekdayPowerHours"] = LaborSchedulingViewModel.ThisWeek.WeekdayPowerHours;
                    Session["WeekendPowerHours"] = LaborSchedulingViewModel.ThisWeek.WeekendPowerHours;
                }
                else
                {
                    LaborSchedulingViewModel.ThisWeek.weekWeighting = (int[])Session["WeekWeighting"];
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly = (int)Session["MinEmployeesEarly"];
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesLater = (int)Session["MinEmployeesLater"];
                    LaborSchedulingViewModel.ThisWeek.MaxEmployees = (int)Session["MaxEmployees"];
                    LaborSchedulingViewModel.ThisWeek.WeekdayPowerHours = (int)Session["WeekdayPowerHours"];
                    LaborSchedulingViewModel.ThisWeek.WeekendPowerHours = (int)Session["WeekendPowerHours"];
                }

                // set the dates up to three weeks from the current week
                DateTime currentWeekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
                for (int i = 0; i < 7; i++)
                {
                    LaborSchedulingViewModel.ThisWeek.OneWeekFromNowDates[i] = currentWeekMarker.AddDays(i + 7);
                    LaborSchedulingViewModel.ThisWeek.TwoWeeksFromNowDates[i] = currentWeekMarker.AddDays(i + 14);
                    LaborSchedulingViewModel.ThisWeek.ThreeWeeksFromNowDates[i] = currentWeekMarker.AddDays(i + 21);
                }
                for (int i = 0; i < 28; i++)
                {
                    LaborSchedulingViewModel.ThisWeek.NextFourWeeksDates[i] = currentWeekMarker.AddDays(i);
                }

                LaborSchedulingViewModel.ThisWeek.RequestedDates = LaborSchedulingViewModel.ThisWeek.OneWeekFromNowDates;
                LaborSchedulingViewModel.ThisWeek.startdateOneWeek = LaborSchedulingViewModel.ThisWeek.OneWeekFromNowDates[0].ToShortDateString();
                LaborSchedulingViewModel.ThisWeek.enddateOneWeek = LaborSchedulingViewModel.ThisWeek.OneWeekFromNowDates[6].ToShortDateString();
                LaborSchedulingViewModel.ThisWeek.startdateTwoWeeks = LaborSchedulingViewModel.ThisWeek.TwoWeeksFromNowDates[0].ToShortDateString();
                LaborSchedulingViewModel.ThisWeek.enddateTwoWeeks = LaborSchedulingViewModel.ThisWeek.TwoWeeksFromNowDates[6].ToShortDateString();
                LaborSchedulingViewModel.ThisWeek.startdateThreeWeeks = LaborSchedulingViewModel.ThisWeek.ThreeWeeksFromNowDates[0].ToShortDateString();
                LaborSchedulingViewModel.ThisWeek.enddateThreeWeeks = LaborSchedulingViewModel.ThisWeek.ThreeWeeksFromNowDates[6].ToShortDateString();

                ls.ManagerMessageList = LaborSchedulingViewModel.ManagerMessageList;

                Session["lsView"] = LaborSchedulingViewModel;

                return View(LaborSchedulingViewModel);
            }
            else
            {
                LaborSchedulingViewModel.ThisWeek.weekWeighting = new int[6];
                LaborSchedulingViewModel.ThisWeek.currentStoreCode = GetStoreCode();
                LaborSchedulingViewModel.ThisWeek.employeeListStore = FakeAPI.GetEmployeesForStore(GetStoreCode());

                // get the store variables
                if (Session["WeekWeighting"] == null)
                {
                    LaborSchedulingViewModel.ThisWeek.weekWeighting = LaborSchedulingViewModel.ThisWeek.getDefaultWeights(LaborSchedulingViewModel.ThisWeek.NumberHistoricalWeeks);
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly = FakeAPI.GetMinEmployeesEarly(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesLater = FakeAPI.GetMinEmployeesLater(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.MaxEmployees = FakeAPI.GetMaxEmployees(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.WeekdayPowerHours = FakeAPI.GetWeekdayPowerHours(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.WeekendPowerHours = FakeAPI.GetWeekendPowerHours(GetStoreCode());

                    Session["WeekWeighting"] = LaborSchedulingViewModel.ThisWeek.weekWeighting;
                    Session["MinEmployeesEarly"] = LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly;
                    Session["MinEmployeesLater"] = LaborSchedulingViewModel.ThisWeek.MinEmployeesLater;
                    Session["MaxEmployees"] = LaborSchedulingViewModel.ThisWeek.MaxEmployees;
                    Session["WeekdayPowerHours"] = LaborSchedulingViewModel.ThisWeek.WeekdayPowerHours;
                    Session["WeekendPowerHours"] = LaborSchedulingViewModel.ThisWeek.WeekendPowerHours;
                }
                else
                {
                    LaborSchedulingViewModel.ThisWeek.weekWeighting = (int[])Session["WeekWeighting"];
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly = (int)Session["MinEmployeesEarly"];
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesLater = (int)Session["MinEmployeesLater"];
                    LaborSchedulingViewModel.ThisWeek.MaxEmployees = (int)Session["MaxEmployees"];
                    LaborSchedulingViewModel.ThisWeek.WeekdayPowerHours = (int)Session["WeekdayPowerHours"];
                    LaborSchedulingViewModel.ThisWeek.WeekendPowerHours = (int)Session["WeekendPowerHours"];
                }

                // set the dates up to three weeks from the current week
                DateTime currentWeekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
                for (int i = 0; i < 7; i++)
                {
                    LaborSchedulingViewModel.ThisWeek.OneWeekFromNowDates[i] = currentWeekMarker.AddDays(i + 7);
                    LaborSchedulingViewModel.ThisWeek.TwoWeeksFromNowDates[i] = currentWeekMarker.AddDays(i + 14);
                    LaborSchedulingViewModel.ThisWeek.ThreeWeeksFromNowDates[i] = currentWeekMarker.AddDays(i + 21);
                }
                for (int i = 0; i < 28; i++)
                {
                    LaborSchedulingViewModel.ThisWeek.NextFourWeeksDates[i] = currentWeekMarker.AddDays(i);
                }

                LaborSchedulingViewModel.ThisWeek.RequestedDates = LaborSchedulingViewModel.ThisWeek.OneWeekFromNowDates;
                LaborSchedulingViewModel.ThisWeek.startdateOneWeek = LaborSchedulingViewModel.ThisWeek.OneWeekFromNowDates[0].ToShortDateString();
                LaborSchedulingViewModel.ThisWeek.enddateOneWeek = LaborSchedulingViewModel.ThisWeek.OneWeekFromNowDates[6].ToShortDateString();
                LaborSchedulingViewModel.ThisWeek.startdateTwoWeeks = LaborSchedulingViewModel.ThisWeek.TwoWeeksFromNowDates[0].ToShortDateString();
                LaborSchedulingViewModel.ThisWeek.enddateTwoWeeks = LaborSchedulingViewModel.ThisWeek.TwoWeeksFromNowDates[6].ToShortDateString();
                LaborSchedulingViewModel.ThisWeek.startdateThreeWeeks = LaborSchedulingViewModel.ThisWeek.ThreeWeeksFromNowDates[0].ToShortDateString();
                LaborSchedulingViewModel.ThisWeek.enddateThreeWeeks = LaborSchedulingViewModel.ThisWeek.ThreeWeeksFromNowDates[6].ToShortDateString();

                LaborSchedulingViewModel.ThisWeek.GenerateEmployeeAvailability();


                Session["lsView"] = LaborSchedulingViewModel;

                return View(LaborSchedulingViewModel);
            }
        }
        [HttpPost]
        public ActionResult LaborSchedule(LaborScheduling lsView, string RequestedDate)
        {
            //if(Session["lsView"] == null)
            //{
            //    return Dashboard();
            //}

            GetStoreCode();

            string requesteddate = lsView.ThisWeek.startdateRequested;
            double payrollHours = lsView.ThisWeek.PayrollWeeklyHours;
            int minEmpsEarly = FakeAPI.GetMinEmployeesEarly(GetStoreCode());
            int minEmpsLater = FakeAPI.GetMinEmployeesLater(GetStoreCode());
            int maxEmps = FakeAPI.GetMaxEmployees(GetStoreCode());
            int weekdayPowerHours = FakeAPI.GetWeekdayPowerHours(GetStoreCode());
            int weekendPowerHours = FakeAPI.GetWeekendPowerHours(GetStoreCode());
            //int weeksBack = FakeAPI.GetWeeksBack(GetStoreCode());
            int weeksBack = 6;

            lsView = (LaborScheduling)Session["lsView"];

            lsView.ThisWeek.startHour = "";
            lsView.ThisWeek.endHour = "";

            lsView.ThisWeek.PayrollWeeklyHours = payrollHours;
            lsView.ThisWeek.MinEmployeesEarly = minEmpsEarly;
            lsView.ThisWeek.MinEmployeesLater = minEmpsLater;
            lsView.ThisWeek.MaxEmployees = maxEmps;
            lsView.ThisWeek.WeekendPowerHours = weekdayPowerHours;
            lsView.ThisWeek.WeekdayPowerHours = weekendPowerHours;
            lsView.ThisWeek.currentStoreCode = GetStoreCode();
            int[] WeekWeighting = FakeAPI.GetWeekWeighting(6, GetStoreCode());

            if (Session["WeekdayPowerHours"] != null)
            {
                lsView.ThisWeek.WeekdayPowerHours = (int)Session["WeekdayPowerHours"];
                lsView.ThisWeek.WeekendPowerHours = (int)Session["WeekendPowerHours"];
            }

            List<DateTime> TrafficDates = new List<DateTime>();
            DateTime currentWeekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
            for (int i = 1; i <= 7 * weeksBack; i++)
            {
                TrafficDates.Add(currentWeekMarker.AddDays(-i));
            }

            lsView.ThisWeek.TrafficData = FakeAPI.GetStoreTrafficData(currentWeekMarker, TrafficDates, GetStoreCode(), WeekWeighting);

            for (int i = 0; i < lsView.ThisWeek.NextFourWeeksDates.Length; i++)
            {
                if (lsView.ThisWeek.NextFourWeeksDates[i].ToShortDateString() == requesteddate)
                {
                    for (int n = 0; n < 7; n++)
                    {
                        lsView.ThisWeek.RequestedDates[n] = lsView.ThisWeek.NextFourWeeksDates[i + n];
                    }
                    break;
                }
            }

            lsView.ThisWeek.FillDatatables();
            lsView.ThisWeek.LaborSchedule = lsView.ThisWeek.AllocatedHoursDisplay;
            lsView.ThisWeek.HourSchedule = lsView.ThisWeek.CurrentWeekHours;
            if ((string[])Session["WeekStartHours"] != null && (string[])Session["WeekEndHours"] != null)
            {
                lsView.ThisWeek.WeekStartHours = (string[])Session["WeekStartHours"];
                lsView.ThisWeek.WeekEndHours = (string[])Session["WeekEndHours"];
                lsView.ThisWeek.GetBlackoutCells(lsView.ThisWeek.AllocatedHours);
            }

            Session["AssignmentTable"] = lsView.ThisWeek.AssignmentTable;
            Session["AllocatedHoursDisplay"] = lsView.ThisWeek.AllocatedHoursDisplay;
            Session["WeekWeighting"] = lsView.ThisWeek.weekWeighting;
            lsView.ThisWeek.GetBlackoutCells(lsView.ThisWeek.AllocatedHoursDisplay);

            LaborScheduling.EmployeeListStore = FakeAPI.GetEmployeesForStore(lsView.ThisWeek.currentStoreCode);
            lsView.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(lsView.ThisWeek.RequestedDates);
            lsView.ThisWeek.GenerateTotalHours();

            Session["lsView"] = lsView;

            return PartialView("_LaborScheduleTable", lsView);
        }
        [HttpGet]
        public ActionResult _LaborScheduleAssignmentView(LaborScheduling ls, int selectedColumn)
        {
            GetStoreCode();
            DataTable AssignmentTable = (DataTable)Session["AssignmentTable"];
            ls = (LaborScheduling)Session["lsView"];

            ls.ThisWeek.employeeListStore = FakeAPI.GetEmployeesForStore(ls.ThisWeek.currentStoreCode);
            ls.ThisWeek.currentStoreCode = GetStoreCode();
            ls.ThisWeek.GenerateNumEmployeesNeeded(selectedColumn, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.selectedWeekday = selectedColumn;
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(selectedColumn);
            if (ls.ThisWeek.BlackoutTimes != null)
            {
                ls.ThisWeek.BlackoutAssignmentView = ls.ThisWeek.GenerateBlackoutAssignmentView(selectedColumn);
            }
            ls.ThisWeek.CheckSchedulingRules(selectedColumn, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);

            Session["LaborSchedulingPartial"] = ls;

            ls.ThisWeek.GenerateEmployeeAvailability();
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateTotalHours();

            Session["lsView"] = ls;

            return PartialView("_LaborScheduleAssignmentView", ls);
        }
        [HttpPost]
        public ActionResult _LaborScheduleAssignmentView(LaborScheduling ls, string employeeId, string startHour/*, string endHour*/)
        {
            //string selectedEmployeeId = ls.selectedEmployeeId;
            //string startHour = ls.ThisWeek.startHour;
            //string endHour = ls.ThisWeek.endHour;

            GetStoreCode();
            string[] unassignTimes = ls.UnassignTimes;

            ls = (LaborScheduling)Session["LaborSchedulingPartial"];

            ls.ThisWeek.employeeListStore = FakeAPI.GetEmployeesForStore(ls.ThisWeek.currentStoreCode);
            ls.ThisWeek.selectedEmployeeId = employeeId;
            ls.ThisWeek.startHour = startHour;

            foreach (Employees emp in ls.ThisWeek.employeeListStore)
            {
                if (emp.id == ls.ThisWeek.selectedEmployeeId)
                {
                    if (emp.rank < 50)
                    {
                        for (int i = 0; i < ls.ThisWeek.ScheduleHalfHourSlots.Length; i++)
                        {
                            if (ls.ThisWeek.ScheduleHalfHourSlots[i] == startHour)
                            {
                                if ((i + 8) > ls.ThisWeek.ScheduleHalfHourSlots.Length - 1)
                                {
                                    ls.ThisWeek.endHour = ls.ThisWeek.ScheduleHalfHourSlots[ls.ThisWeek.ScheduleHalfHourSlots.Length - 1];
                                }
                                else
                                {
                                    ls.ThisWeek.endHour = ls.ThisWeek.ScheduleHalfHourSlots[i + 7];
                                }
                            }
                        }
                    }
                    else if (emp.rank >= 50)
                    {
                        for (int i = 0; i < ls.ThisWeek.ScheduleHalfHourSlots.Length; i++)
                        {
                            if (ls.ThisWeek.ScheduleHalfHourSlots[i] == startHour)
                            {
                                if ((i + 16) > ls.ThisWeek.ScheduleHalfHourSlots.Length - 1)
                                {
                                    ls.ThisWeek.endHour = ls.ThisWeek.ScheduleHalfHourSlots[ls.ThisWeek.ScheduleHalfHourSlots.Length - 1];
                                }
                                else
                                {
                                    ls.ThisWeek.endHour = ls.ThisWeek.ScheduleHalfHourSlots[i + 15];
                                }
                            }
                        }
                    }
                }
            }

            int endhourStore = 0;
            int endhour = 0;
            for (int i = 0; i < ls.ThisWeek.ScheduleHalfHourSlots.Length; i++)
            {
                if (ls.ThisWeek.ScheduleHalfHourSlots[i] == ls.ThisWeek.WeekStartEndHours.Rows[1][ls.ThisWeek.selectedWeekday].ToString())
                {
                    endhourStore = i;
                }
                if (ls.ThisWeek.ScheduleHalfHourSlots[i] == ls.ThisWeek.endHour)
                {
                    endhour = i;
                }
            }
            if (endhour > endhourStore && endhourStore < ls.ThisWeek.ScheduleHalfHourSlots.Length - 1)
            {
                ls.ThisWeek.endHour = ls.ThisWeek.ScheduleHalfHourSlots[endhourStore + 1];
            }

            ls.UnassignTimes = unassignTimes;

            if (ls.ThisWeek.selectedEmployeeId != null)
            {
                LaborScheduling lsView = (LaborScheduling)Session["lsView"];
                lsView.ThisWeek.EmployeeScheduledTimes = ls.ThisWeek.UpdateEmployees(ls.ThisWeek.employeeListStore);
                ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
                ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
                Session["lsView"] = lsView;
            }

            if (ls.UnassignTimes != null && ls.UnassignTimes.Length > 0)
            {
                //FakeAPI.UnassignEmployee(ls.UnassignTimes, ls.ThisWeek.selectedEmployeeId, 2);
            }
            ls.selectedEmployeeId = ls.ThisWeek.selectedEmployeeId;
            foreach (Employees emp in ls.ThisWeek.employeeListStore)
            {
                if (emp.id == ls.selectedEmployeeId)
                {
                    ls.selectedEmployeeName = emp.firstName + " " + emp.lastName;
                }
            }
            //ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);

            return PartialView("_LaborScheduleAssignmentView", ls);
        }
        [HttpGet]
        public ActionResult _LaborScheduleEmployeeAvailability(LaborScheduling ls, int employeeId)
        {
            GetStoreCode();
            Session["SelectedEmployeeId"] = employeeId;

            ls = (LaborScheduling)Session["lsView"];

            int column = ls.ThisWeek.selectedWeekday;

            ls.ThisWeek.EmployeeAvailabilityView = ls.ThisWeek.GenerateEmployeeAvailabilityView(employeeId, column);
            ls.ThisWeek.EmployeeScheduledView = ls.ThisWeek.GenerateEmployeeScheduledView(employeeId, column);

            ls.ThisWeek.CheckScheduleConflicts(Convert.ToString(employeeId), column);

            return PartialView("_LaborScheduleEmployeeAvailability", ls);
        }
        [HttpPost]
        public ActionResult _LaborScheduleEmployeeAvailability(LaborScheduling ls, string unassignTimes)
        {
            GetStoreCode();
            //string[] unassignTimes = ls.UnassignTimes;
            string employeeId = Convert.ToString((Int32)Session["SelectedEmployeeId"]);

            ls = (LaborScheduling)Session["lsView"];

            //ls.UnassignTimes = unassignTimes;
            int selectedWeekday = ls.ThisWeek.selectedWeekday;

            FakeAPI.UnassignEmployee(unassignTimes, employeeId, ls.ThisWeek.selectedWeekday, ls.ThisWeek.EmployeeScheduledTimes, ls.ThisWeek.RequestedDates, GetStoreCode());

            //string[] employeeIdsAll = new string[LaborScheduling.EmployeeListAll.Count];
            string[] employeeIdsStore = new string[LaborScheduling.EmployeeListStore.Count];

            for (int i = 0; i < employeeIdsStore.Length; i++)
            {
                employeeIdsStore[i] = LaborScheduling.EmployeeListStore[i].id;
            }

            //ls.ThisWeek.EmployeeScheduledTimes = FakeAPI.GetEmployeeScheduledTimes(employeeIdsAll, ls.ThisWeek.NextWeekDates);
            ls.ThisWeek.GenerateNumEmployeesNeeded(selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.selectedEmployeeId = employeeId;
            foreach (Employees emp in ls.ThisWeek.employeeListStore)
            {
                if (emp.id == ls.selectedEmployeeId)
                {
                    ls.selectedEmployeeName = emp.firstName + " " + emp.lastName;
                }
            }
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);

            return PartialView("_LaborScheduleAssignmentView", ls);
        }
        [HttpGet]
        public ActionResult _LaborScheduleHourAvailability(LaborScheduling ls, int selectedHour)
        {
            GetStoreCode();
            ls = (LaborScheduling)Session["lsView"];

            int weekday = ls.ThisWeek.selectedWeekday;
            string hour = Convert.ToString(ls.ThisWeek.AssignmentView.Columns[selectedHour]);

            ls.ThisWeek.GenerateEmployeeAvailability();
            ls.ThisWeek.GetEmployeesForHour(ls.ThisWeek.employeeListStore, weekday, hour);

            return PartialView("_LaborScheduleHourAvailability", ls);
        }


        [HttpGet]
        public ActionResult EmployeeAvailability(string LocationCode)
        {
            GetStoreCode();
            AvailabilityViewModel avm = new AvailabilityViewModel(GetStoreCode());

            Session["EmpAvailabilityTable"] = avm.EmpAvailabilityTable;
            Session["AvailabilityViewModel"] = avm;

            string[] employeeIds = new string[avm.EmployeeList.Count];
            for (int i = 0; i < avm.EmployeeList.Count; i++)
            {
                employeeIds[i] = avm.EmployeeList[i].id;
            }
            DateTime currentWeekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
            DateTime[] CurrentWeekDates = new DateTime[7];
            DateTime[] NextWeekDates = new DateTime[7];
            for (int i = 0; i < 7; i++)
            {
                CurrentWeekDates[i] = currentWeekMarker.AddDays(i);
                NextWeekDates[i] = currentWeekMarker.AddDays(i + 7);
            }

            avm.EmployeeTimeOffRequests = FakeAPI.GetEmployeeTimeOff(GetStoreCode(), CurrentWeekDates);

            return View(avm);
        }
        [HttpPost]
        public ActionResult EmployeeAvailability(string UpdatedSchedule, AvailabilityViewModel model)
        {
            GetStoreCode();
            Session["UpdatedSchedule"] = model.updatedSchedule;
            string EmployeeId = (string)Session["CurrentEmployeeId"];
            string EmployeeName = (string)Session["CurrentEmployeeName"];

            model.EmpAvailabilityTable = (Dictionary<string, DataTable>)Session["EmpAvailabilityTable"];

            DateTime currentWeekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
            DateTime[] NextFourWeeksDates = new DateTime[28];
            for (int i = 0; i < 28; i++)
            {
                NextFourWeeksDates[i] = currentWeekMarker.AddDays(i);
            }

            model.UpdateSchedule(model, model.updatedSchedule, EmployeeId, NextFourWeeksDates, GetStoreCode());
            model.EmpAvailabilityTable[EmployeeId] = FakeAPI.GetEmployeeAvailability(EmployeeId);

            model = (AvailabilityViewModel)Session["AvailabilityViewModel"];

            return PartialView("_EmployeeAvailabilityTable", model.EmpAvailabilityTable[EmployeeId]);
        }
        [HttpGet]
        public ActionResult _EmployeeAvailabilityTable(AvailabilityViewModel avm, string EmployeeId, string EmployeeName)
        {
            GetStoreCode();
            avm = (AvailabilityViewModel)Session["AvailabilityViewModel"];

            if (EmployeeId == "--")
            {
                return View(avm);
            }
            else
            {
                Session["CurrentEmpAvailability"] = avm.EmpAvailabilityTable[EmployeeId];
                Session["CurrentEmployeeId"] = EmployeeId;
                foreach (Employees emp in avm.EmployeeList)
                {
                    if (emp.id == EmployeeId)
                    {
                        EmployeeName = emp.firstName;
                    }
                }
                Session["CurrentEmployeeName"] = EmployeeName;
                DateTime currentWeekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);

                DateTime[] CurrentWeekDates = new DateTime[7];
                DateTime[] NextWeekDates = new DateTime[7];
                for (int i = 0; i < 7; i++)
                {
                    CurrentWeekDates[i] = currentWeekMarker.AddDays(i);
                    NextWeekDates[i] = currentWeekMarker.AddDays(i + 7);
                }


                avm.selectedEmployeeId = EmployeeId;
                avm.selectedEmployeeName = EmployeeName;
                List<string> employeeIds = new List<string>(avm.EmpsForStore.Keys);
                employeeIds.RemoveAt(0);
                string[] empIds = employeeIds.ToArray();
                avm.EmployeeTimeOffRequests = FakeAPI.GetEmployeeTimeOff(GetStoreCode(), CurrentWeekDates);

                return PartialView("_EmployeeAvailabilityTable", avm);
                //return View(avm);
            }
        }

        [HttpGet]
        public ActionResult ActiveTimeOffRequests()
        {
            string StoreCode = GetStoreCode();

            ActiveRequests ar = new ActiveRequests();
            ar.TimeOffRequests = FakeAPI.GetFutureTimeOff(StoreCode);

            return View("ActiveTimeOffRequests", ar);
        }

        [HttpGet]
        public ActionResult ManagerConfiguration()
        {
            GetStoreCode();
            LaborScheduling ls = new LaborScheduling(GetStoreCode());
            if (Session["lsView"] != null)
            {
                ls = (LaborScheduling)Session["lsView"];
            }

            ls.ThisWeek.currentStoreCode = GetStoreCode();
            List<Employees> storeEmployees = FakeAPI.GetEmployeesForStore(GetStoreCode());
            ls.ThisWeek.WeekStartEndHours = FakeAPI.GetStoreHours(GetStoreCode());
            List<string> OpenHours = new List<string>();
            List<string> CloseHours = new List<string>();

            for (int n = 0; n < 7; n++)
            {
                for (int j = 0; j < ls.ThisWeek.ScheduleHalfHourSlots.Length; j++)
                {
                    if (ls.ThisWeek.WeekStartEndHours.Rows[0][n].ToString() == ls.ThisWeek.ScheduleHalfHourSlots[j])
                    {
                        OpenHours.Add(ls.ThisWeek.ScheduleHalfHourSlots[j]);
                    }
                    if (ls.ThisWeek.WeekStartEndHours.Rows[1][n].ToString() == ls.ThisWeek.ScheduleHalfHourSlots[j])
                    {
                        CloseHours.Add(ls.ThisWeek.ScheduleHalfHourSlots[j]);
                    }
                }
            }
            ls.ThisWeek.WeekStartHours = OpenHours.ToArray();
            ls.ThisWeek.WeekEndHours = CloseHours.ToArray();
            ls.ThisWeek.MinEmployeesEarly = FakeAPI.GetMinEmployeesEarly(GetStoreCode());
            ls.ThisWeek.MinEmployeesLater = FakeAPI.GetMinEmployeesLater(GetStoreCode());
            ls.ThisWeek.MaxEmployees = FakeAPI.GetMaxEmployees(GetStoreCode());
            ls.ThisWeek.WeekdayPowerHours = FakeAPI.GetWeekdayPowerHours(GetStoreCode());
            ls.ThisWeek.WeekendPowerHours = FakeAPI.GetWeekendPowerHours(GetStoreCode());

            // set dates that are already excluded in the database
            List<DateTime> AlreadyExcludedDates = FakeAPI.GetExcludedDates(ls.ThisWeek.currentStoreCode);
            List<DateTime> Dates = new List<DateTime>(ls.ThisWeek.ExclusionDates.Keys);
            if (AlreadyExcludedDates.Count > 0)
            {
                for (int i = 0; i < ls.ThisWeek.ExclusionDates.Keys.Count; i++)
                {
                    for (int n = 0; n < AlreadyExcludedDates.Count; n++)
                    {
                        if (Dates[i] == AlreadyExcludedDates[n])
                        {
                            ls.ThisWeek.ExclusionDates[Dates[i]] = true;
                            break;
                        }
                    }
                }
            }

            Session["lsView"] = ls;

            return View(ls);
        }
        [HttpPost]
        public ActionResult ManagerConfiguration(LaborScheduling ls)
        {
            GetStoreCode();
            string[] ExcludedDates = new string[0];
            if (ls.ThisWeek.excludedDates != null)
            {
                ExcludedDates = ls.ThisWeek.excludedDates[0].Split(',');
            }

            string[] StartHours = ls.ThisWeek.WeekStartHours;
            string[] EndHours = ls.ThisWeek.WeekEndHours;

            int MinEmployeesEarly = ls.ThisWeek.MinEmployeesEarly;
            int MinEmployeesLater = ls.ThisWeek.MinEmployeesLater;
            int MaxEmployees = ls.ThisWeek.MaxEmployees;
            int WeekdayPowerHours = ls.ThisWeek.WeekdayPowerHours;
            int WeekendPowerHours = ls.ThisWeek.WeekendPowerHours;

            ls = (LaborScheduling)Session["lsView"];
            //get the dates here and compare to the excluded dates list to determine which dates to set in the database
            List<DateTime> AlreadyExcludedDates = FakeAPI.GetExcludedDates(ls.ThisWeek.currentStoreCode);

            // if dates were updated submit them to the database
            if (ExcludedDates.Length > 0)
            {
                List<DateTime> DatesToExclude = new List<DateTime>();

                ls.ThisWeek.excludedDates = ExcludedDates;
                List<DateTime> dates = new List<DateTime>();
                foreach (DateTime date in ls.ThisWeek.ExclusionDates.Keys)
                {
                    dates.Add(date);
                }

                for (int i = 0; i < ls.ThisWeek.excludedDates.Length; i++)
                {
                    if (ls.ThisWeek.excludedDates[i] == "True")
                    {
                        DatesToExclude.Add(dates[i]);
                    }
                }

                //for (int i = 0; i < AlreadyExcludedDates.Count; i++)
                //{
                //    for (int n = DatesToExclude.Count - 1; n >= 0; n--)
                //    {
                //        if (AlreadyExcludedDates[i] == DatesToExclude[n])
                //        {
                //            DatesToExclude.RemoveAt(n);
                //            break;
                //        }
                //    }
                //}
                FakeAPI.UpdateExcludedDates(GetStoreCode(), DatesToExclude.ToArray());

                // set dates that are already excluded in the database
                List<DateTime> Dates = new List<DateTime>(ls.ThisWeek.ExclusionDates.Keys);

                for (int i = 0; i < ls.ThisWeek.ExclusionDates.Keys.Count; i++)
                {
                    if (ls.ThisWeek.excludedDates[i] == "True")
                    {
                        ls.ThisWeek.ExclusionDates[Dates[i]] = true;
                    }
                    else
                    {
                        ls.ThisWeek.ExclusionDates[Dates[i]] = false;
                    }
                }
            }



            DataTable AllocatedHours = (DataTable)Session["AllocatedHoursDisplay"];

            ls.ThisWeek.weekWeighting = FakeAPI.GetWeekWeighting(ls.ThisWeek.NumberHistoricalWeeks, GetStoreCode());

            if (StartHours != null && EndHours != null)
            {
                Session["WeekStartHours"] = StartHours;
                Session["WeekEndHours"] = EndHours;
                ls.ThisWeek.WeekStartHours = (string[])Session["WeekStartHours"];
                ls.ThisWeek.WeekEndHours = (string[])Session["WeekEndHours"];
                FakeAPI.UpdateStoreHours(GetStoreCode(), StartHours, EndHours);
            }
            if (MinEmployeesEarly != 0 || MinEmployeesLater != 0 || MaxEmployees != 0)
            {
                Session["WeekdayPowerHours"] = WeekdayPowerHours;
                Session["WeekendPowerHours"] = WeekendPowerHours;
                FakeAPI.UpdateWeekdayPowerHours(WeekdayPowerHours, GetStoreCode());
                FakeAPI.UpdateWeekendPowerHours(WeekendPowerHours, GetStoreCode());
                FakeAPI.UpdateMinEmployeesEarly(MinEmployeesEarly, GetStoreCode());
                FakeAPI.UpdateMinEmployeesLater(MinEmployeesLater, GetStoreCode());
                FakeAPI.UpdateMaxEmployees(MaxEmployees, GetStoreCode());
                Session["NumberHistoricalWeeks"] = ls.ThisWeek.NumberHistoricalWeeks;
                ls.ThisWeek.MinEmployeesEarly = MinEmployeesEarly;
                ls.ThisWeek.MinEmployeesLater = MinEmployeesLater;
                ls.ThisWeek.MaxEmployees = MaxEmployees;
                ls.ThisWeek.WeekdayPowerHours = WeekdayPowerHours;
                ls.ThisWeek.WeekendPowerHours = WeekendPowerHours;

            }


            Session["lsView"] = ls;

            return View(ls);
        }

        [HttpGet]
        public ActionResult FindEmployee(LaborScheduling ls, string EmployeeId)
        {
            GetStoreCode();
            ls = (LaborScheduling)Session["lsView"];

            DataTable FoundEmployees = ls.ThisWeek.FindEmployee(EmployeeId);
            Session["BorrowEmployeeId"] = EmployeeId;
            bool EmployeeCheck = false;

            foreach (Employees emp in ls.ThisWeek.employeeListStore)
            {
                if (emp.id == EmployeeId)
                {
                    EmployeeCheck = true;
                }
            }
            if (EmployeeCheck == true)
            {
                FoundEmployees.Rows[0][0] = "1";
            }

            return PartialView("_ManagerConfigFindEmployee", FoundEmployees);
        }

        [HttpGet]
        public ActionResult LaborScheduleCurrentWeek()
        {
            //FakeAPI.dothething();

            GetStoreCode();

            LaborScheduling LaborSchedulingViewModel = new LaborScheduling(GetStoreCode());
            Session["LaborSchedulingViewModel"] = LaborSchedulingViewModel;

            LaborScheduling ls = (LaborScheduling)Session["lsView"];

            if (ls != null)
            {
                LaborSchedulingViewModel.ThisWeek.currentStoreCode = GetStoreCode();

                if ((List<Employees>)Session["UpdatedEmployeeListStore"] != null)
                {
                    LaborSchedulingViewModel.ThisWeek.employeeListStore = (List<Employees>)Session["UpdatedEmployeeListStore"];
                }
                else
                {
                    LaborSchedulingViewModel.ThisWeek.employeeListStore = FakeAPI.GetEmployeesForStore(GetStoreCode());
                }

                // get store variables
                if (LaborSchedulingViewModel.ThisWeek.weekWeighting == null)
                {
                    LaborSchedulingViewModel.ThisWeek.weekWeighting = LaborSchedulingViewModel.ThisWeek.getDefaultWeights(LaborSchedulingViewModel.ThisWeek.NumberHistoricalWeeks);
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly = FakeAPI.GetMinEmployeesEarly(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesLater = FakeAPI.GetMinEmployeesLater(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.MaxEmployees = FakeAPI.GetMaxEmployees(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.WeekdayPowerHours = FakeAPI.GetWeekdayPowerHours(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.WeekendPowerHours = FakeAPI.GetWeekendPowerHours(GetStoreCode());

                    Session["WeekWeighting"] = LaborSchedulingViewModel.ThisWeek.weekWeighting;
                    Session["MinEmployeesEarly"] = LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly;
                    Session["MinEmployeesLater"] = LaborSchedulingViewModel.ThisWeek.MinEmployeesLater;
                    Session["MaxEmployees"] = LaborSchedulingViewModel.ThisWeek.MaxEmployees;
                    Session["WeekdayPowerHours"] = LaborSchedulingViewModel.ThisWeek.WeekdayPowerHours;
                    Session["WeekendPowerHours"] = LaborSchedulingViewModel.ThisWeek.WeekendPowerHours;
                }
                else
                {
                    LaborSchedulingViewModel.ThisWeek.weekWeighting = (int[])Session["WeekWeighting"];
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly = (int)Session["MinEmployeesEarly"];
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesLater = (int)Session["MinEmployeesLater"];
                    LaborSchedulingViewModel.ThisWeek.MaxEmployees = (int)Session["MaxEmployees"];
                    LaborSchedulingViewModel.ThisWeek.WeekdayPowerHours = (int)Session["WeekdayPowerHours"];
                    LaborSchedulingViewModel.ThisWeek.WeekendPowerHours = (int)Session["WeekendPowerHours"];
                }

                // set the dates up to three weeks from the current week
                DateTime currentWeekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
                for (int i = 0; i < 7; i++)
                {
                    LaborSchedulingViewModel.ThisWeek.CurrentWeekDates[i] = currentWeekMarker.AddDays(i);
                }
                for (int i = 0; i < 28; i++)
                {
                    LaborSchedulingViewModel.ThisWeek.NextFourWeeksDates[i] = currentWeekMarker.AddDays(i);
                }

                LaborSchedulingViewModel.ThisWeek.RequestedDates = LaborSchedulingViewModel.ThisWeek.CurrentWeekDates;

                LaborSchedulingViewModel.ManagerMessageList = LaborSchedulingViewModel.ManagerMessageList;

                Session["lsView"] = LaborSchedulingViewModel;

                return View(LaborSchedulingViewModel);
            }
            else
            {
                LaborSchedulingViewModel.ThisWeek.weekWeighting = new int[6];
                LaborSchedulingViewModel.ThisWeek.currentStoreCode = GetStoreCode();
                //List<Employees> EmployeeListStore = FakeAPI.GetEmployeesForStore(GetStoreCode());
                LaborSchedulingViewModel.ThisWeek.employeeListStore = FakeAPI.GetEmployeesForStore(GetStoreCode());

                // get the store variables
                if (Session["WeekWeighting"] == null)
                {
                    LaborSchedulingViewModel.ThisWeek.weekWeighting = LaborSchedulingViewModel.ThisWeek.getDefaultWeights(LaborSchedulingViewModel.ThisWeek.NumberHistoricalWeeks);
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly = FakeAPI.GetMinEmployeesEarly(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesLater = FakeAPI.GetMinEmployeesLater(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.MaxEmployees = FakeAPI.GetMaxEmployees(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.WeekdayPowerHours = FakeAPI.GetWeekdayPowerHours(GetStoreCode());
                    LaborSchedulingViewModel.ThisWeek.WeekendPowerHours = FakeAPI.GetWeekendPowerHours(GetStoreCode());

                    Session["WeekWeighting"] = LaborSchedulingViewModel.ThisWeek.weekWeighting;
                    Session["MinEmployeesEarly"] = LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly;
                    Session["MinEmployeesLater"] = LaborSchedulingViewModel.ThisWeek.MinEmployeesLater;
                    Session["MaxEmployees"] = LaborSchedulingViewModel.ThisWeek.MaxEmployees;
                    Session["WeekdayPowerHours"] = LaborSchedulingViewModel.ThisWeek.WeekdayPowerHours;
                    Session["WeekendPowerHours"] = LaborSchedulingViewModel.ThisWeek.WeekendPowerHours;
                }
                else
                {
                    LaborSchedulingViewModel.ThisWeek.weekWeighting = (int[])Session["WeekWeighting"];
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly = (int)Session["MinEmployeesEarly"];
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesLater = (int)Session["MinEmployeesLater"];
                    LaborSchedulingViewModel.ThisWeek.MaxEmployees = (int)Session["MaxEmployees"];
                    LaborSchedulingViewModel.ThisWeek.WeekdayPowerHours = (int)Session["WeekdayPowerHours"];
                    LaborSchedulingViewModel.ThisWeek.WeekendPowerHours = (int)Session["WeekendPowerHours"];
                }

                // set the dates up to three weeks from the current week
                DateTime currentWeekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
                for (int i = 0; i < 7; i++)
                {
                    LaborSchedulingViewModel.ThisWeek.CurrentWeekDates[i] = currentWeekMarker.AddDays(i);
                }
                for (int i = 0; i < 28; i++)
                {
                    LaborSchedulingViewModel.ThisWeek.NextFourWeeksDates[i] = currentWeekMarker.AddDays(i);
                }

                LaborSchedulingViewModel.ThisWeek.RequestedDates = LaborSchedulingViewModel.ThisWeek.CurrentWeekDates;

                LaborSchedulingViewModel.ThisWeek.GenerateEmployeeAvailability();

                Session["lsView"] = LaborSchedulingViewModel;

                return View(LaborSchedulingViewModel);
            }
        }
        [HttpPost]
        public ActionResult LaborScheduleCurrentWeek(LaborScheduling lsView)
        {
            //if(Session["lsView"] == null)
            //{
            //    return Dashboard();
            //}

            GetStoreCode();

            string requesteddate = lsView.ThisWeek.startdateRequested;
            double payrollHours = lsView.ThisWeek.PayrollWeeklyHours;
            int minEmpsEarly = FakeAPI.GetMinEmployeesEarly(GetStoreCode());
            int minEmpsLater = FakeAPI.GetMinEmployeesLater(GetStoreCode());
            int maxEmps = FakeAPI.GetMaxEmployees(GetStoreCode());
            int weekdayPowerHours = FakeAPI.GetWeekdayPowerHours(GetStoreCode());
            int weekendPowerHours = FakeAPI.GetWeekendPowerHours(GetStoreCode());

            lsView = (LaborScheduling)Session["lsView"];

            lsView.ThisWeek.PayrollWeeklyHours = payrollHours;
            lsView.ThisWeek.MinEmployeesEarly = minEmpsEarly;
            lsView.ThisWeek.MinEmployeesLater = minEmpsLater;
            lsView.ThisWeek.MaxEmployees = maxEmps;
            lsView.ThisWeek.WeekendPowerHours = weekdayPowerHours;
            lsView.ThisWeek.WeekdayPowerHours = weekendPowerHours;

            if (Session["WeekdayPowerHours"] != null)
            {
                lsView.ThisWeek.WeekdayPowerHours = (int)Session["WeekdayPowerHours"];
                lsView.ThisWeek.WeekendPowerHours = (int)Session["WeekendPowerHours"];
            }

            lsView.ThisWeek.FillDatatables();
            lsView.ThisWeek.LaborSchedule = lsView.ThisWeek.AllocatedHoursDisplay;
            lsView.ThisWeek.HourSchedule = lsView.ThisWeek.CurrentWeekHours;
            if ((string[])Session["WeekStartHours"] != null && (string[])Session["WeekEndHours"] != null)
            {
                lsView.ThisWeek.WeekStartHours = (string[])Session["WeekStartHours"];
                lsView.ThisWeek.WeekEndHours = (string[])Session["WeekEndHours"];
                lsView.ThisWeek.GetBlackoutCells(lsView.ThisWeek.AllocatedHours);
            }

            Session["AssignmentTable"] = lsView.ThisWeek.AssignmentTable;
            Session["AllocatedHoursDisplay"] = lsView.ThisWeek.AllocatedHoursDisplay;
            Session["WeekWeighting"] = lsView.ThisWeek.weekWeighting;
            lsView.ThisWeek.GetBlackoutCells(lsView.ThisWeek.AllocatedHoursDisplay);

            Session["lsView"] = lsView;

            return PartialView("_LaborScheduleTable", lsView);
        }

        //[HttpGet]
        //public ActionResult EmployeeHome(string LocationCode, string EmployeeId)
        //{
        //    EmployeeId = "5463";
        //    //maybe get the employee id and manager status here

        //    DateTime currentWeekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
        //    DateTime[] CurrentWeekDates = new DateTime[7];
        //    DateTime[] NextWeekDates = new DateTime[7];
        //    for (int i = 0; i < 7; i++)
        //    {
        //        CurrentWeekDates[i] = currentWeekMarker.AddDays(i);
        //        NextWeekDates[i] = currentWeekMarker.AddDays(i + 7);
        //    }

        //    EmployeeModel model = new EmployeeModel(LocationCode, EmployeeId, NextWeekDates);

        //    for (int i = 0; i < 14; i++)
        //    {
        //        model.startDates.Add(DateTime.Now.AddDays(i + 1).ToString("M/d/yyyy"));
        //    }

        //    Session["EmployeeId"] = EmployeeId;
        //    Session["EmployeeSchedule"] = model;

        //    return View(model);
        //}
        //[HttpPost]
        //public ActionResult EmployeeHome(EmployeeModel EmployeeSchedule)
        //{
        //    EmployeeTimeOffRequest TimeOffRequest = new EmployeeTimeOffRequest();
        //    List<Employees> ListOfEmployees = FakeAPI.GetAllEmployees();

        //    string employeeId = (string)Session["EmployeeId"];
        //    string employeeName = "";

        //    // fill the dropdown with dates from the next two weeks
        //    for (int i = 0; i < 14; i++)
        //    {
        //        EmployeeSchedule.startDates.Add(DateTime.Now.AddDays(i + 1).ToString("M/d/yyyy"));
        //    }

        //    // get employee name
        //    foreach (var emp in ListOfEmployees)
        //    {
        //        if (emp.id == employeeId)
        //        {
        //            employeeName = emp.firstName + " " + emp.lastName;
        //        }
        //    }

        //    // set the time off request variables
        //    TimeOffRequest.Id = employeeId;
        //    TimeOffRequest.Name = employeeName;
        //    TimeOffRequest.startDate = EmployeeSchedule.startDate;
        //    TimeOffRequest.endDate = EmployeeSchedule.endDate;
        //    TimeOffRequest.allDayCheck = EmployeeSchedule.allDayCheck;

        //    if (EmployeeSchedule.allDayCheck == true)
        //    {
        //        TimeOffRequest.startTime = "--";
        //        TimeOffRequest.endTime = "--";
        //    }
        //    else
        //    {
        //        TimeOffRequest.startTime = EmployeeSchedule.startTime;
        //        TimeOffRequest.endTime = EmployeeSchedule.endTime;
        //    }
        //    TimeOffRequest.created = Convert.ToString(DateTime.Now);
        //    TimeOffRequest.message = EmployeeSchedule.CreateMessage(TimeOffRequest.Name, TimeOffRequest.startDate, TimeOffRequest.endDate, TimeOffRequest.startTime, TimeOffRequest.endTime);

        //    // submit the time off request to the database
        //    EmployeeSchedule.submitTimeOffRequest(TimeOffRequest.created, TimeOffRequest.startDate, TimeOffRequest.endDate,
        //                                          TimeOffRequest.Name, TimeOffRequest.Id, TimeOffRequest.startTime, TimeOffRequest.endTime,
        //                                          TimeOffRequest.message);

        //    EmployeeSchedule = (EmployeeModel)Session["EmployeeSchedule"];
        //    EmployeeSchedule.getMessages(TimeOffRequest.Id);

        //    return View(EmployeeSchedule);
        //}

        public ActionResult Grading()
        {
            ViewBag.Message = "Grading page.";

            return View();
        }

    }
}

