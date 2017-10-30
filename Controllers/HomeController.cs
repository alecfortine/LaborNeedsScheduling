using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LaborNeedsScheduling.Models;
using LSAData;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Configuration;

namespace LaborNeedsScheduling.Controllers
{
    public class HomeController : Controller
    {
        public string StoreCode = "1008";

        public ActionResult ImportSchedule()
        {
            LaborScheduling ls = (LaborScheduling)Session["LaborSchedulingPartial"];

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
            Dictionary<string, Dictionary<string, string[]>> EmployeeSchedulesLastWeek = FakeAPI.GetEmployeeScheduledTimes(employeeIds.ToArray(), StoreCode, RequestedDates);

            ls.ThisWeek.EmployeeScheduledTimes = EmployeeSchedulesLastWeek;

            FakeAPI.ImportLastWeekSchedule(StoreCode, employeeIds.ToArray(), RequestedDates, PreviousWeekDates);

            //ls.ThisWeek.EmployeeScheduledTimes = ls.ThisWeek.UpdateEmployees(ls.ThisWeek.employeeListStore);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateTotalHours();

            return PartialView("_LaborScheduleAssignmentView", ls);
        }

        public ActionResult ClearSchedule()
        {
            LaborScheduling ls = (LaborScheduling)Session["LaborSchedulingPartial"];

            List<string> employeeIds = new List<string>();
            foreach (Employees emp in ls.ThisWeek.employeeListStore)
            {
                employeeIds.Add(emp.id);
            }

            FakeAPI.ClearRequestedWeekSchedule(StoreCode, employeeIds.ToArray(), ls.ThisWeek.RequestedDates);

            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateTotalHours();

            return PartialView("_LaborScheduleAssignmentView", ls);
        }

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
                    if (emp.rank < 50)
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
                    else if (emp.rank >= 50)
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

            ls.ThisWeek.EmployeeScheduledTimes = ls.ThisWeek.UpdateEmployees(ls.ThisWeek.employeeListStore);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.GenerateTotalHours();

            return PartialView("_LaborScheduleAssignmentView", ls);
        }

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

            FakeAPI.UnassignEmployee(unassignTimes, employeeId, ls.ThisWeek.selectedWeekday, ls.ThisWeek.EmployeeScheduledTimes, ls.ThisWeek.RequestedDates, StoreCode);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.GenerateTotalHours();

            return PartialView("_LaborScheduleAssignmentView", ls);
        }

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
            FakeAPI.UnassignEmployee(ls.ThisWeek.startHour, employeeId, ls.ThisWeek.selectedWeekday, ls.ThisWeek.EmployeeScheduledTimes, ls.ThisWeek.RequestedDates, StoreCode);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateTotalHours();

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

            FakeAPI.UnassignEmployee(ls.ThisWeek.endHour, employeeId, ls.ThisWeek.selectedWeekday, ls.ThisWeek.EmployeeScheduledTimes, ls.ThisWeek.RequestedDates, StoreCode);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateTotalHours();

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

        public ActionResult AddEmployee(LaborScheduling ls, bool AddEmployee)
        {
            ls = (LaborScheduling)Session["LaborSchedulingPartial"];
            string employeeId = (string)Session["BorrowEmployeeId"];

            ls.ThisWeek.AddEmployeeToList(employeeId);
            ls.ThisWeek.employeeListStore = FakeAPI.GetEmployeesForStore(StoreCode);

            Session["UpdatedEmployeeListStore"] = ls.ThisWeek.employeeListStore;
            LaborScheduling.EmployeeListStore = ls.ThisWeek.employeeListStore;

            List<string> EmployeeIds = new List<string>();
            foreach (Employees emp in ls.ThisWeek.employeeListStore)
            {
                EmployeeIds.Add(emp.id);
            }

            ls.ThisWeek.GenerateEmployeeAvailability();
            ls.ThisWeek.EmployeeScheduledTimes = FakeAPI.GetEmployeeScheduledTimes(EmployeeIds.ToArray(), StoreCode, ls.ThisWeek.RequestedDates);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateTotalHours();

            Session["LaborSchedulingPartial"] = ls;

            return PartialView("_LaborScheduleAssignmentView", ls);
        }
        public ActionResult RemoveEmployee(string EmployeeId)
        {
            LaborScheduling ls = (LaborScheduling)Session["LaborSchedulingPartial"];

            FakeAPI.RemoveBorrowedEmployee(EmployeeId, StoreCode);
            ls.ThisWeek.employeeListStore = FakeAPI.GetEmployeesForStore(StoreCode);

            Session["UpdatedEmployeeListStore"] = ls.ThisWeek.employeeListStore;
            LaborScheduling.EmployeeListStore = ls.ThisWeek.employeeListStore;

            List<string> EmployeeIds = new List<string>();
            foreach (Employees emp in ls.ThisWeek.employeeListStore)
            {
                EmployeeIds.Add(emp.id);
            }

            ls.ThisWeek.GenerateEmployeeAvailability();
            ls.ThisWeek.EmployeeScheduledTimes = FakeAPI.GetEmployeeScheduledTimes(EmployeeIds.ToArray(), StoreCode, ls.ThisWeek.RequestedDates);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.GenerateNumEmployeesNeeded(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.ThisWeek.AssignmentView = ls.ThisWeek.GenerateAssignmentView(ls.ThisWeek.selectedWeekday);
            ls.ThisWeek.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(ls.ThisWeek.RequestedDates);
            ls.ThisWeek.GenerateTotalHours();

            Session["LaborSchedulingPartial"] = ls;

            return PartialView("_LaborScheduleAssignmentView", ls);
        }


        [HttpGet]
        public ActionResult Dashboard()
        {
            Dashboard Dash = new Dashboard();

            string storecode = "1008";

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


            Dash.EmployeeListStore = FakeAPI.GetEmployeesForStore(storecode);
            Dash.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(Dash.RequestedDates);

            Session["Dashboard"] = Dash;

            return View(Dash);
        }
        [HttpPost]
        public ActionResult Dashboard(Dashboard Dash, string RequestedDate)
        {
            Dash = (Dashboard)Session["DashBoard"];
            Dash.startdateRequested = RequestedDate;

            if (Dash.startdateRequested == Dash.startdateCurrentWeek)
            {
                Dash.RequestedDates = Dash.CurrentWeekDates;
            }
            else if (Dash.startdateRequested == Dash.startdateOneWeek)
            {
                Dash.RequestedDates = Dash.OneWeekFromNowDates;
            }
            else if (Dash.startdateRequested == Dash.startdateTwoWeeks)
            {
                Dash.RequestedDates = Dash.TwoWeeksFromNowDates;
            }
            else if (Dash.startdateRequested == Dash.startdateThreeWeeks)
            {
                Dash.RequestedDates = Dash.ThreeWeeksFromNowDates;
            }
            else
            {
                Dash.RequestedDates = Dash.OneWeekFromNowDates;
            }

            Dash.AssignedEmployeesRequestedWeek = FakeAPI.CreateConsolidatedSchedule(Dash.RequestedDates);

            return PartialView("_DashboardTable", Dash);
        }

        [HttpGet]
        public ActionResult _DashboardCurrentWeekSchedule()
        {
            Dashboard Dash = (Dashboard)Session["Dashboard"];

            return PartialView("_DashboardCurrentWeekSchedule", Dash);
        }
        [HttpGet]
        public ActionResult _DashboardNextWeekSchedule()
        {
            Dashboard Dash = (Dashboard)Session["Dashboard"];

            return PartialView("_DashboardNextWeekSchedule", Dash);
        }

        [HttpGet]
        public ActionResult ManagerFunctionality()
        {

            return View();
        }

        [HttpGet]
        public ActionResult TimeOffRequest()
        {
            string storecode = "1008";

            EmployeeModel TimeOff = new EmployeeModel();
            for (int i = 0; i < 30; i++)
            {
                TimeOff.startDates.Add(DateTime.Now.AddDays(i + 1).ToString("M/d/yyyy"));
            }
            for (int i = 1; i < 30; i++)
            {
                TimeOff.endDates.Add(DateTime.Now.AddDays(i + 1).ToString("M/d/yyyy"));
            }
            TimeOff.ListOfEmployees = FakeAPI.GetEmployeesForStore(storecode);

            return View("TimeOffRequest", TimeOff);
        }
        [HttpPost]
        public ActionResult TimeOffRequest(EmployeeModel TimeOff, string employeeId)
        {
            string storecode = "1008";

            EmployeeTimeOffRequest TimeOffRequest = new EmployeeTimeOffRequest();
            List<Employees> ListOfEmployees = FakeAPI.GetEmployeesForStore(storecode);

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
            TimeOffRequest.LocationCode = StoreCode;
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

            return View("TimeOffRequest", TimeOff);
        }

        public ActionResult approveRequest(int messageId)
        {
            LaborScheduling ls = (LaborScheduling)Session["lsView"];

            string storecode = "1008";
            string employeeId = (string)Session["EmployeeId"];

            FakeAPI.approveRequest(messageId, employeeId, storecode);

            return View();
        }
        public ActionResult denyRequest(int messageId)
        {
            string storecode = "1008";
            string employeeId = (string)Session["EmployeeId"];

            FakeAPI.denyRequest(messageId, employeeId, storecode);

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

            string StoreCode = "1008"; // change to be set by the login
            string EmployeeId = "9999"; // change to be set by the login
            bool isManager = true;
            List<Employees> storeEmployees = new List<Employees>();
            if ((List<Employees>)Session["UpdatedEmployeeListStore"] != null)
            {
                storeEmployees = (List<Employees>)Session["UpdatedEmployeeListStore"];
            }
            else
            {
                storeEmployees = FakeAPI.GetEmployeesForStore(StoreCode);
            }
            foreach (Employees emp in storeEmployees)
            {
                if (emp.id == EmployeeId)
                {
                    if (emp.rank == 50 || emp.rank == 60)
                    {
                        isManager = true;
                    }
                }
            }

            Session["StoreCode"] = StoreCode;
            Session["EmployeeId"] = EmployeeId;
            Session["EmployeeStatus"] = isManager;

            LaborScheduling LaborSchedulingViewModel = new LaborScheduling(StoreCode);
            Session["LaborSchedulingViewModel"] = LaborSchedulingViewModel;

            LaborScheduling ls = (LaborScheduling)Session["lsView"];

            if (ls != null)
            {
                ls.ThisWeek.currentStoreCode = StoreCode;
                ls.ThisWeek.employeeStatus = isManager;
                ls.ThisWeek.employeeListStore = storeEmployees;

                // get store variables
                if (LaborSchedulingViewModel.ThisWeek.weekWeighting == null)
                {
                    LaborSchedulingViewModel.ThisWeek.weekWeighting = LaborSchedulingViewModel.ThisWeek.getDefaultWeights(LaborSchedulingViewModel.ThisWeek.NumberHistoricalWeeks);
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly = FakeAPI.GetMinEmployeesEarly(StoreCode);
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesLater = FakeAPI.GetMinEmployeesLater(StoreCode);
                    LaborSchedulingViewModel.ThisWeek.MaxEmployees = FakeAPI.GetMaxEmployees(StoreCode);
                    LaborSchedulingViewModel.ThisWeek.WeekdayPowerHours = FakeAPI.GetWeekdayPowerHours(StoreCode);
                    LaborSchedulingViewModel.ThisWeek.WeekendPowerHours = FakeAPI.GetWeekendPowerHours(StoreCode);

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

                //if ((int)Session["MinEmployeesEarly"] == 0)
                //{
                //    ls.ThisWeek.MinEmployeesEarly = FakeAPI.GetMinEmployeesEarly(StoreCode);
                //    ls.ThisWeek.MinEmployeesLater = FakeAPI.GetMinEmployeesLater(StoreCode);
                //}
                //else
                //{
                //    ls.ThisWeek.MinEmployeesEarly = (int)Session["MinEmployeesEarly"];
                //}

                //if ((int)Session["MaxEmployees"] == 0)
                //{
                //    ls.ThisWeek.MaxEmployees = FakeAPI.GetMaxEmployees(StoreCode);
                //}
                //else
                //{
                //    ls.ThisWeek.MaxEmployees = (int)Session["MaxEmployees"];
                //}

                //ls.ThisWeek.excludedDates = ExcludedDates;
                //ls.ThisWeek.WeekdayPowerHours = FakeAPI.GetWeekdayPowerHours(StoreCode);
                //ls.ThisWeek.WeekendPowerHours = FakeAPI.GetWeekendPowerHours(StoreCode);

                ls.ManagerMessageList = LaborSchedulingViewModel.ManagerMessageList;

                Session["lsView"] = ls;

                return View(ls);
            }
            else
            {
                LaborSchedulingViewModel.ThisWeek.weekWeighting = new int[6];
                LaborSchedulingViewModel.ThisWeek.currentStoreCode = StoreCode;
                LaborSchedulingViewModel.ThisWeek.employeeStatus = isManager;
                LaborSchedulingViewModel.ThisWeek.employeeListStore = storeEmployees;

                // get the store variables
                if (Session["WeekWeighting"] == null)
                {
                    LaborSchedulingViewModel.ThisWeek.weekWeighting = LaborSchedulingViewModel.ThisWeek.getDefaultWeights(LaborSchedulingViewModel.ThisWeek.NumberHistoricalWeeks);
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly = FakeAPI.GetMinEmployeesEarly(StoreCode);
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesLater = FakeAPI.GetMinEmployeesLater(StoreCode);
                    LaborSchedulingViewModel.ThisWeek.MaxEmployees = FakeAPI.GetMaxEmployees(StoreCode);
                    LaborSchedulingViewModel.ThisWeek.WeekdayPowerHours = FakeAPI.GetWeekdayPowerHours(StoreCode);
                    LaborSchedulingViewModel.ThisWeek.WeekendPowerHours = FakeAPI.GetWeekendPowerHours(StoreCode);

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
            string requesteddate = lsView.ThisWeek.startdateRequested;

            double payrollHours = lsView.ThisWeek.PayrollWeeklyHours;
            int minEmpsEarly = FakeAPI.GetMinEmployeesEarly(StoreCode);
            int minEmpsLater = FakeAPI.GetMinEmployeesLater(StoreCode);
            int maxEmps = FakeAPI.GetMaxEmployees(StoreCode);

            lsView = (LaborScheduling)Session["lsView"];

            //if (requesteddate == lsView.ThisWeek.startdateOneWeek)
            //{
            //    lsView.ThisWeek.RequestedDates = lsView.ThisWeek.OneWeekFromNowDates;
            //}
            //else if (requesteddate == lsView.ThisWeek.startdateTwoWeeks)
            //{
            //    lsView.ThisWeek.RequestedDates = lsView.ThisWeek.TwoWeeksFromNowDates;
            //}
            //else if (requesteddate == lsView.ThisWeek.startdateThreeWeeks)
            //{
            //    lsView.ThisWeek.RequestedDates = lsView.ThisWeek.ThreeWeeksFromNowDates;
            //}
            //else
            //{
            //    lsView.ThisWeek.RequestedDates = lsView.ThisWeek.OneWeekFromNowDates;
            //}

            lsView.ThisWeek.PayrollWeeklyHours = payrollHours;
            //lsView.ThisWeek.MinEmployeesEarly = minEmpsEarly;
            //lsView.ThisWeek.MinEmployeesLater = minEmpsLater;
            //lsView.ThisWeek.MaxEmployees = maxEmps;

            //int[] oldWeighting = (int[])Session["WeekWeighting"];
            //lsView.ThisWeek.currentStoreCode = (string)Session["StoreCode"];
            //lsView.ThisWeek.employeeStatus = (bool)Session["EmployeeStatus"];

            //if (lsView.ThisWeek.weekWeighting.Length < 6)
            //{
            //    int[] weights = new int[6];
            //    for (int i = 0; i < 6; i++)
            //    {
            //        if (i < lsView.ThisWeek.weekWeighting.Length)
            //        {
            //            weights[i] = (lsView.ThisWeek.weekWeighting[i]);
            //        }
            //        else
            //        {
            //            weights[i] = (oldWeighting[i]);
            //        }
            //    }
            //    lsView.ThisWeek.weekWeighting = weights;
            //}

            //if (lsView.ExcludeDates(lsView.ExcludedDates)[0] == "" || lsView.ExcludeDates(lsView.ExcludedDates)[0] == "System.String[]")
            //{
            //    lsView.ExcludedDates = new string[lsView.ThisWeek.ExclusionDates.Count];
            //    for (int i = 0; i < lsView.ThisWeek.ExclusionDates.Count; i++)
            //    {
            //        lsView.ExcludedDates[i] = "False";
            //    }

            //    lsView.ThisWeek.excludedDates = lsView.ExcludedDates;
            //}
            //else if (Session["UpdatedExclusions"] == null)
            //{
            //    Session["UpdatedSchedule"] = lsView.ExcludeDates(lsView.ExcludedDates);
            //    lsView.ThisWeek.excludedDates = lsView.ExcludeDates(lsView.ExcludedDates);
            //    Session["UpdatedExclusions"] = lsView.ThisWeek.excludedDates;
            //}
            //else
            //{
            //    lsView.ThisWeek.excludedDates = lsView.ExcludeDates(lsView.ExcludedDates);
            //    Session["UpdatedExclusions"] = lsView.ThisWeek.excludedDates;
            //}

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
        [HttpGet]
        public ActionResult _LaborScheduleAssignmentView(LaborScheduling ls, int selectedColumn)
        {
            DataTable AssignmentTable = (DataTable)Session["AssignmentTable"];
            ls = (LaborScheduling)Session["lsView"];

            ls.ThisWeek.currentStoreCode = (string)Session["StoreCode"];
            ls.ThisWeek.employeeStatus = (bool)Session["EmployeeStatus"];
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

            //found it
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
            //string[] unassignTimes = ls.UnassignTimes;
            string employeeId = Convert.ToString((Int32)Session["SelectedEmployeeId"]);

            ls = (LaborScheduling)Session["lsView"];

            //ls.UnassignTimes = unassignTimes;
            int selectedWeekday = ls.ThisWeek.selectedWeekday;

            FakeAPI.UnassignEmployee(unassignTimes, employeeId, ls.ThisWeek.selectedWeekday, ls.ThisWeek.EmployeeScheduledTimes, ls.ThisWeek.RequestedDates, StoreCode);

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
            //maybe get the employee id and manager status here

            AvailabilityViewModel avm = new AvailabilityViewModel(StoreCode);

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

            avm.EmployeeTimeOffRequests = FakeAPI.GetEmployeeTimeOff(StoreCode, CurrentWeekDates);

            return View(avm);
        }
        [HttpPost]
        public ActionResult EmployeeAvailability(string UpdatedSchedule, AvailabilityViewModel model)
        {
            Session["UpdatedSchedule"] = model.updatedSchedule;
            string EmployeeId = (string)Session["CurrentEmployeeId"];
            string EmployeeName = (string)Session["CurrentEmployeeName"];

            model.EmpAvailabilityTable = (Dictionary<string, DataTable>)Session["EmpAvailabilityTable"];

            model.UpdateSchedule(model, model.updatedSchedule, EmployeeId);
            model.EmpAvailabilityTable[EmployeeId] = FakeAPI.GetEmployeeAvailability(EmployeeId);

            model = (AvailabilityViewModel)Session["AvailabilityViewModel"];

            return PartialView("_EmployeeAvailabilityTable", model.EmpAvailabilityTable[EmployeeId]);
        }
        [HttpGet]
        public ActionResult _EmployeeAvailabilityTable(AvailabilityViewModel avm, string EmployeeId, string EmployeeName)
        {
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
                avm.EmployeeTimeOffRequests = FakeAPI.GetEmployeeTimeOff(StoreCode, CurrentWeekDates);

                return PartialView("_EmployeeAvailabilityTable", avm);
                //return View(avm);
            }
        }

        [HttpGet]
        public ActionResult ManagerConfiguration()
        {
            string StoreCode = "1008"; // change to be set by the login
            string EmployeeId = "008142"; // change to be set by the login

            LaborScheduling ls = (LaborScheduling)Session["lsView"]; ;
            List<Employees> storeEmployees = FakeAPI.GetEmployeesForStore(StoreCode);

            //foreach (Employees emp in storeEmployees)
            //{
            //    if (emp.id == EmployeeId)
            //    {
            //        if (emp.rank == 50 || emp.rank == 60)
            //        {
            //            EmployeeStatus = true;
            //        }
            //    }
            //}
            //Session["EmployeeStatus"] = EmployeeStatus;

            //ls = (LaborScheduling)Session["lsView"];

            ls.ThisWeek.WeekStartEndHours = FakeAPI.GetStoreHours(StoreCode);
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

            ls.ThisWeek.MinEmployeesEarly = FakeAPI.GetMinEmployeesEarly(StoreCode);
            ls.ThisWeek.MinEmployeesLater = FakeAPI.GetMinEmployeesLater(StoreCode);
            ls.ThisWeek.MaxEmployees = FakeAPI.GetMaxEmployees(StoreCode);

            ls.ThisWeek.WeekdayPowerHours = FakeAPI.GetWeekdayPowerHours(StoreCode);
            ls.ThisWeek.WeekendPowerHours = FakeAPI.GetWeekendPowerHours(StoreCode);

            Session["lsView"] = ls;

            return View(ls);
        }
        [HttpPost]
        public ActionResult ManagerConfiguration(LaborScheduling ls)
        {
            string[] ExcludedDates = new string[0];
            if (ls.ThisWeek.excludedDates != null)
            {
                ExcludedDates = ls.ThisWeek.excludedDates[0].Split(',');
            }

            string[] StartHours = ls.ThisWeek.WeekStartHours;
            string[] EndHours = ls.ThisWeek.WeekEndHours;

            ls = (LaborScheduling)Session["lsView"];
            //get the dates here and compare to the excluded dates list to determine which dates to set in the database
            List<DateTime> AlreadyExcludedDates = FakeAPI.GetExcludedDates(StoreCode);

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

                for (int i = 0; i < AlreadyExcludedDates.Count; i++)
                {
                    for (int n = DatesToExclude.Count - 1; n >= 0; n--)
                    {
                        if (AlreadyExcludedDates[i] == DatesToExclude[n])
                        {
                            DatesToExclude.RemoveAt(n);
                            break;
                        }
                    }
                }

                FakeAPI.UpdateExcludedDates(StoreCode, DatesToExclude.ToArray());


            }

            DataTable AllocatedHours = (DataTable)Session["AllocatedHoursDisplay"];

            ls.ThisWeek.weekWeighting = FakeAPI.GetWeekWeighting(ls.ThisWeek.NumberHistoricalWeeks, StoreCode);

            if (ls.ThisWeek.WeekStartHours != null)
            {
                Session["WeekStartHours"] = StartHours;
                Session["WeekEndHours"] = EndHours;
                ls.ThisWeek.WeekStartHours = (string[])Session["WeekStartHours"];
                ls.ThisWeek.WeekEndHours = (string[])Session["WeekEndHours"];
                FakeAPI.UpdateStoreHours("1008", StartHours, EndHours);
            }
            if (ls.ThisWeek.WeekdayPowerHours != 0)
            {
                Session["WeekdayPowerHours"] = ls.ThisWeek.WeekdayPowerHours;
                Session["WeekendPowerHours"] = ls.ThisWeek.WeekendPowerHours;
                FakeAPI.UpdateWeekdayPowerHours(ls.ThisWeek.WeekdayPowerHours, StoreCode);
                FakeAPI.UpdateWeekendPowerHours(ls.ThisWeek.WeekendPowerHours, StoreCode);
                FakeAPI.UpdateMinEmployeesEarly(ls.ThisWeek.MinEmployeesEarly, StoreCode);
                FakeAPI.UpdateMinEmployeesLater(ls.ThisWeek.MinEmployeesLater, StoreCode);
                FakeAPI.UpdateMaxEmployees(ls.ThisWeek.MaxEmployees, StoreCode);
                Session["NumberHistoricalWeeks"] = ls.ThisWeek.NumberHistoricalWeeks;
            }


            Session["lsView"] = ls;

            return View(ls);
        }

        [HttpGet]
        public ActionResult FindEmployee(LaborScheduling ls, string EmployeeId)
        {
            ls = (LaborScheduling)Session["lsView"];

            DataTable FoundEmployee = ls.ThisWeek.FindEmployee(EmployeeId);
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
                FoundEmployee.Rows[0][0] = "1";
            }

            return PartialView("_ManagerConfigFindEmployee", FoundEmployee);
        }

        [HttpGet]
        public ActionResult LaborScheduleCurrentWeek()
        {
            //FakeAPI.dothething();

            string StoreCode = "1008"; // change to be set by the login
            string EmployeeId = "9999"; // change to be set by the login
            bool isManager = false;
            List<Employees> storeEmployees = new List<Employees>();
            if ((List<Employees>)Session["UpdatedEmployeeListStore"] != null)
            {
                storeEmployees = (List<Employees>)Session["UpdatedEmployeeListStore"];
            }
            else
            {
                storeEmployees = FakeAPI.GetEmployeesForStore(StoreCode);
            }
            foreach (Employees emp in storeEmployees)
            {
                if (emp.id == EmployeeId)
                {
                    if (emp.rank == 50 || emp.rank == 60)
                    {
                        isManager = true;
                    }
                }
            }

            Session["StoreCode"] = StoreCode;
            Session["EmployeeId"] = EmployeeId;
            Session["EmployeeStatus"] = isManager;

            LaborScheduling LaborSchedulingViewModel = new LaborScheduling(StoreCode);
            Session["LaborSchedulingViewModel"] = LaborSchedulingViewModel;

            LaborScheduling ls = (LaborScheduling)Session["lsView"];
            string[] ExcludedDates = (string[])Session["UpdatedSchedule"];

            if (ls != null && Session["ManagerConfig"] != null)
            {
                ls.ThisWeek.WeekdayPowerHours = FakeAPI.GetWeekdayPowerHours(StoreCode);
                ls.ThisWeek.WeekendPowerHours = FakeAPI.GetWeekendPowerHours(StoreCode);
                ls.ThisWeek.WeekStartHours = (string[])Session["WeekStartHours"];
                ls.ThisWeek.WeekEndHours = (string[])Session["WeekEndHours"];
                ls.ThisWeek.BlackoutTimes = (DataTable)Session["BlackoutCells"];

                if (ls.ThisWeek.weekWeighting[0] == 0)
                {
                    for (int i = 0; i < ls.ThisWeek.NumberHistoricalWeeks; i++)
                    {
                        ls.ThisWeek.weekWeighting[i] = ls.ThisWeek.getDefaultWeights(ls.ThisWeek.NumberHistoricalWeeks)[i];
                    }
                }

                Session["WeekWeighting"] = ls.ThisWeek.weekWeighting;

                ls = (LaborScheduling)Session["lsView"];

                //ls.ThisWeek.GetBlackoutCells();
                ls.ThisWeek.GeneratePowerHourCells();

                return View(ls);
            }
            else if (ls != null)
            {
                ls.ThisWeek.currentStoreCode = StoreCode;
                ls.ThisWeek.employeeStatus = isManager;
                ls.ThisWeek.employeeListStore = storeEmployees;

                //if (Session["UpdatedExclusions"] == null)
                //{
                //    LaborSchedulingViewModel.ThisWeek.excludedDates = (string[])Session["UpdatedExclusions"];
                //}
                if ((int)Session["MinEmployeesEarly"] == 0)
                {
                    ls.ThisWeek.MinEmployeesEarly = FakeAPI.GetMinEmployeesEarly(StoreCode);
                    ls.ThisWeek.MinEmployeesLater = FakeAPI.GetMinEmployeesLater(StoreCode);
                }
                else
                {
                    ls.ThisWeek.MinEmployeesEarly = (int)Session["MinEmployeesEarly"];
                    ls.ThisWeek.MinEmployeesLater = (int)Session["MinEmployeesLater"];
                }

                if ((int)Session["MaxEmployees"] == 0)
                {
                    ls.ThisWeek.MaxEmployees = FakeAPI.GetMaxEmployees(StoreCode);
                }
                else
                {
                    ls.ThisWeek.MaxEmployees = (int)Session["MaxEmployees"];
                }

                //ls.ThisWeek.excludedDates = ExcludedDates;
                ls.ManagerMessageList = LaborSchedulingViewModel.ManagerMessageList;
                //ls = (LaborScheduling)Session["lsView"];
                Session["lsView"] = ls;

                return View(ls);
            }
            else
            {
                LaborSchedulingViewModel.ThisWeek.weekWeighting = new int[6];
                LaborSchedulingViewModel.ThisWeek.currentStoreCode = StoreCode;
                LaborSchedulingViewModel.ThisWeek.employeeStatus = isManager;
                //LaborSchedulingViewModel.ThisWeek.employeeListAll = FakeAPI.GetAllEmployees();
                LaborSchedulingViewModel.ThisWeek.employeeListStore = storeEmployees;

                //if (Session["UpdatedExclusions"] == null)
                //{
                //    LaborSchedulingViewModel.ThisWeek.excludedDates = (string[])Session["UpdatedExclusions"];
                //}

                if (LaborSchedulingViewModel.ThisWeek.NumberHistoricalWeeks == 0)
                {
                    LaborSchedulingViewModel.ThisWeek.NumberHistoricalWeeks = LaborSchedulingViewModel.ThisWeek.DefaultHistoricalWeeks;
                }

                if (LaborSchedulingViewModel.ThisWeek.weekWeighting[0] == 0)
                {
                    for (int i = 0; i < LaborSchedulingViewModel.ThisWeek.NumberHistoricalWeeks; i++)
                    {
                        LaborSchedulingViewModel.ThisWeek.weekWeighting[i] = LaborSchedulingViewModel.ThisWeek.getDefaultWeights(LaborSchedulingViewModel.ThisWeek.NumberHistoricalWeeks)[i];
                    }
                    Session["WeekWeighting"] = LaborSchedulingViewModel.ThisWeek.weekWeighting;
                }

                if (LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly == 0/* && LaborSchedulingViewModel.ThisWeek.MinEmployeesDefault == 0*/)
                {
                    //LaborSchedulingViewModel.ThisWeek.MinEmployeesDefault = 3;
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly = FakeAPI.GetMinEmployeesEarly(StoreCode);
                    Session["MinEmployeesEarly"] = LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly;
                    Session["MinEmployeesLater"] = LaborSchedulingViewModel.ThisWeek.MinEmployeesLater;
                }
                else
                {
                    Session["MinEmployeesEarly"] = LaborSchedulingViewModel.ThisWeek.MinEmployeesEarly;
                    Session["MinEmployeesLater"] = LaborSchedulingViewModel.ThisWeek.MinEmployeesLater;
                }

                if (LaborSchedulingViewModel.ThisWeek.MaxEmployees == 0/* && LaborSchedulingViewModel.ThisWeek.MaxEmployeesDefault == 0*/)
                {
                    //LaborSchedulingViewModel.ThisWeek.MaxEmployeesDefault = 20;
                    LaborSchedulingViewModel.ThisWeek.MaxEmployees = FakeAPI.GetMaxEmployees(StoreCode);
                    Session["MaxEmployees"] = LaborSchedulingViewModel.ThisWeek.MaxEmployees;
                }
                else
                {
                    Session["MaxEmployees"] = LaborSchedulingViewModel.ThisWeek.MaxEmployees;
                }

                LaborSchedulingViewModel.ThisWeek.GenerateEmployeeAvailability();


                DateTime currentWeekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
                DateTime[] CurrentWeekDates = new DateTime[7];


                for (int i = 0; i < 7; i++)
                {
                    CurrentWeekDates[i] = currentWeekMarker.AddDays(i);
                }

                LaborSchedulingViewModel.ThisWeek.CurrentWeekDates = CurrentWeekDates;


                LaborSchedulingViewModel.ThisWeek.RequestedDates = CurrentWeekDates;
                LaborSchedulingViewModel.ThisWeek.startdateCurrentWeek = LaborSchedulingViewModel.ThisWeek.CurrentWeekDates[0].ToShortDateString();
                LaborSchedulingViewModel.ThisWeek.enddateCurrentWeek = LaborSchedulingViewModel.ThisWeek.CurrentWeekDates[6].ToShortDateString();

                Session["lsView"] = LaborSchedulingViewModel;

                return View(LaborSchedulingViewModel);
            }
        }
        [HttpPost]
        public ActionResult LaborScheduleCurrentWeek(LaborScheduling lsView)
        {
            string requesteddate = lsView.ThisWeek.startdateRequested;
            //string starthour = lsView.ThisWeek.WeekStartHour;
            //string endhour = lsView.ThisWeek.WeekEndHour;
            string[] excludedDates = lsView.ExcludedDates;
            double payrollHours = lsView.ThisWeek.PayrollWeeklyHours;
            //int minEmps = lsView.ThisWeek.MinEmployees;
            //int maxEmps = lsView.ThisWeek.MaxEmployees;
            int minEmpsEarly = FakeAPI.GetMinEmployeesEarly(StoreCode);
            int minEmpsLater = FakeAPI.GetMinEmployeesLater(StoreCode);
            int maxEmps = FakeAPI.GetMaxEmployees(StoreCode);
            //bool[] PowerHourCells = lsView.ThisWeek.PowerHourCells;

            lsView = (LaborScheduling)Session["lsView"];

            lsView.ThisWeek.RequestedDates = lsView.ThisWeek.CurrentWeekDates;


            //lsView.ThisWeek.WeekStartHour = starthour;
            //lsView.ThisWeek.WeekEndHour = endhour;
            //lsView.ThisWeek.excludedDates = excludedDates;
            lsView.ThisWeek.PayrollWeeklyHours = payrollHours;
            lsView.ThisWeek.MinEmployeesEarly = minEmpsEarly;
            lsView.ThisWeek.MinEmployeesLater = minEmpsLater;
            lsView.ThisWeek.MaxEmployees = maxEmps;

            //lsView.ThisWeek.PowerHourCells = PowerHourCells;
            lsView.ExcludedDates = excludedDates;
            int[] oldWeighting = (int[])Session["WeekWeighting"];
            lsView.ThisWeek.currentStoreCode = (string)Session["StoreCode"];
            lsView.ThisWeek.employeeStatus = (bool)Session["EmployeeStatus"];

            if (lsView.ThisWeek.weekWeighting.Length < 6)
            {
                int[] weights = new int[6];
                for (int i = 0; i < 6; i++)
                {
                    if (i < lsView.ThisWeek.weekWeighting.Length)
                    {
                        weights[i] = (lsView.ThisWeek.weekWeighting[i]);
                    }
                    else
                    {
                        weights[i] = (oldWeighting[i]);
                    }
                }
                lsView.ThisWeek.weekWeighting = weights;
            }

            //if (lsView.ExcludedDates != null)
            //{
            //if (lsView.ExcludeDates(lsView.ExcludedDates)[0] == "" || lsView.ExcludeDates(lsView.ExcludedDates)[0] == "System.String[]")
            //{
            //    lsView.ExcludedDates = new string[lsView.ThisWeek.ExclusionDates.Count];
            //    for (int i = 0; i < lsView.ThisWeek.ExclusionDates.Count; i++)
            //    {
            //        lsView.ExcludedDates[i] = "False";
            //    }

            //    lsView.ThisWeek.excludedDates = lsView.ExcludedDates;
            //}
            //else if (Session["UpdatedExclusions"] == null)
            //{
            //    Session["UpdatedSchedule"] = lsView.ExcludeDates(lsView.ExcludedDates);
            //    lsView.ThisWeek.excludedDates = lsView.ExcludeDates(lsView.ExcludedDates);
            //    Session["UpdatedExclusions"] = lsView.ThisWeek.excludedDates;
            //}
            //else
            //{
            //    lsView.ThisWeek.excludedDates = lsView.ExcludeDates(lsView.ExcludedDates);
            //    Session["UpdatedExclusions"] = lsView.ThisWeek.excludedDates;
            //}
            //}

            if (Session["WeekdayPowerHours"] != null)
            {
                lsView.ThisWeek.WeekdayPowerHours = (int)Session["WeekdayPowerHours"];
                lsView.ThisWeek.WeekendPowerHours = (int)Session["WeekendPowerHours"];
            }

            //DateTime currentWeekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
            //DateTime[] NextWeekDates = new DateTime[7];
            //for (int i = 0; i < 7; i++)
            //{
            //    NextWeekDates[i] = currentWeekMarker.AddDays(i + 7);
            //}
            //lsView.ThisWeek.RequestedDates = NextWeekDates;
            lsView.ThisWeek.FillDatatables();
            lsView.ThisWeek.LaborSchedule = lsView.ThisWeek.AllocatedHoursDisplay;
            lsView.ThisWeek.HourSchedule = lsView.ThisWeek.CurrentWeekHours;
            if ((string[])Session["WeekStartHours"] != null && (string[])Session["WeekEndHours"] != null)
            {
                lsView.ThisWeek.WeekStartHours = (string[])Session["WeekStartHours"];
                lsView.ThisWeek.WeekEndHours = (string[])Session["WeekEndHours"];
                lsView.ThisWeek.GetBlackoutCells(lsView.ThisWeek.AllocatedHours);
            }

            //if (Session["MinEmployeesDefault"] != null && Session["MaxEmployeesDefault"] != null)
            //{
            //    lsView.ThisWeek.MinEmployeesDefault = (int)Session["MinEmployeesDefault"];
            //    lsView.ThisWeek.MaxEmployeesDefault = (int)Session["MaxEmployeesDefault"];
            //}
            //else
            //{
            //    lsView.ThisWeek.MinEmployeesDefault = 3;
            //    lsView.ThisWeek.MaxEmployeesDefault = 20;
            //    Session["MinEmployeesDefault"] = lsView.ThisWeek.MinEmployeesDefault;
            //    Session["MaxEmployeesDefault"] = lsView.ThisWeek.MaxEmployeesDefault;
            //    lsView.ThisWeek.WeekdayPowerHours = 3;
            //    lsView.ThisWeek.WeekendPowerHours = 4;
            //    Session["WeekdayPowerHours"] = lsView.ThisWeek.WeekdayPowerHours;
            //    Session["WeekendPowerHours"] = lsView.ThisWeek.WeekendPowerHours;
            //}

            //Session["MinEmployeesEarly"] = lsView.ThisWeek.MinEmployees;
            //Session["MaxEmployees"] = lsView.ThisWeek.MaxEmployees;
            Session["AssignmentTable"] = lsView.ThisWeek.AssignmentTable;
            Session["AllocatedHoursDisplay"] = lsView.ThisWeek.AllocatedHoursDisplay;
            Session["WeekWeighting"] = lsView.ThisWeek.weekWeighting;
            lsView.ThisWeek.GetBlackoutCells(lsView.ThisWeek.AllocatedHoursDisplay);

            //if (lsView.ThisWeek.MinEmployeesDefault != 0 && lsView.ThisWeek.MaxEmployeesDefault != 0)
            //{
            //    if (lsView.ThisWeek.MinEmployees > lsView.ThisWeek.MinEmployeesDefault)
            //    {
            //        lsView.ThisWeek.MinEmployees = lsView.ThisWeek.MinEmployeesDefault;
            //    }
            //    if (lsView.ThisWeek.MaxEmployees < lsView.ThisWeek.MaxEmployeesDefault)
            //    {
            //        lsView.ThisWeek.MaxEmployees = lsView.ThisWeek.MaxEmployeesDefault;
            //    }
            //}

            Session["lsView"] = lsView;

            //return View(lsView);
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

