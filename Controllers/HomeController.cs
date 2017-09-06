using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LaborNeedsScheduling.Models;
using LSAData;
using System.Data;
using System.Diagnostics;

namespace LaborNeedsScheduling.Controllers
{
    public class HomeController : Controller
    {

        [HttpGet]
        public ActionResult LaborSchedule()
        {
            //FakeAPI.dothething();

            string StoreCode = "0001"; // change to be set by the login
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
            foreach(Employees emp in storeEmployees)
            {
                if(emp.id == EmployeeId)
                {
                    if(emp.rank == 50 || emp.rank == 60)
                    {
                        isManager = true;
                    }
                }
            }

            Session["StoreCode"] = StoreCode;
            Session["EmployeeId"] = EmployeeId;
            Session["EmployeeStatus"] = isManager;

            LaborScheduling LaborSchedulingViewModel = new LaborScheduling();
            Session["LaborSchedulingViewModel"] = LaborSchedulingViewModel;

            LaborScheduling ls = (LaborScheduling)Session["lsView"];
            string[] ExcludedDates = (string[])Session["UpdatedSchedule"];

            if (ls != null && Session["ManagerConfig"] != null)
            {
                if (Session["UpdatedExclusions"] == null)
                {
                    LaborSchedulingViewModel.ThisWeek.excludedDates = (string[])Session["UpdatedSchedule"];
                }
                if (ls.ThisWeek.MinEmployeesDefault == 0)
                {
                    ls.ThisWeek.MinEmployees = ls.ThisWeek.MinEmployeesDefault;
                    Session["MinEmployeesDefault"] = ls.ThisWeek.MinEmployeesDefault;
                }
                else
                {
                    ls.ThisWeek.MinEmployees = (int)Session["MinEmployees"];
                }

                if (ls.ThisWeek.MaxEmployeesDefault == 0)
                {
                    ls.ThisWeek.MaxEmployees = ls.ThisWeek.MaxEmployeesDefault;
                    Session["MaxEmployeesDefault"] = ls.ThisWeek.MaxEmployeesDefault;
                }
                else
                {
                    ls.ThisWeek.MaxEmployees = (int)Session["MaxEmployees"];
                }

                ls.ThisWeek.excludedDates = ExcludedDates;
                ls.ThisWeek.WeekStartHours = (string[])Session["WeekStartHours"];
                ls.ThisWeek.WeekStartHours = (string[])Session["WeekEndHours"];
                ls.ThisWeek.MinEmployeesDefault = (int)Session["MinEmployeesDefault"];
                ls.ThisWeek.MaxEmployeesDefault = (int)Session["MaxEmployeesDefault"];
                ls.ThisWeek.WeekdayPowerHours = (int)Session["WeekdayPowerHours"];
                ls.ThisWeek.WeekendPowerHours = (int)Session["WeekendPowerHours"];
                //ls.ThisWeek.NumberHistoricalWeeks = (int)Session["NumberHistoricalWeeks"];
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

                if (Session["UpdatedExclusions"] == null)
                {
                    LaborSchedulingViewModel.ThisWeek.excludedDates = (string[])Session["UpdatedExclusions"];
                }
                if ((int)Session["MinEmployees"] == 0)
                {
                    ls.ThisWeek.MinEmployees = ls.ThisWeek.MinEmployeesDefault;
                }
                else
                {
                    ls.ThisWeek.MinEmployees = (int)Session["MinEmployees"];
                }

                if ((int)Session["MaxEmployees"] == 0)
                {
                    ls.ThisWeek.MaxEmployees = ls.ThisWeek.MaxEmployeesDefault;
                }
                else
                {
                    ls.ThisWeek.MaxEmployees = (int)Session["MaxEmployees"];
                }

                ls.ThisWeek.excludedDates = ExcludedDates;

                //ls = (LaborScheduling)Session["lsView"];
                Session["lsView"] = ls;

                return View(ls);
            }
            else
            {
                LaborSchedulingViewModel.ThisWeek.weekWeighting = new double[6];
                LaborSchedulingViewModel.ThisWeek.currentStoreCode = StoreCode;
                LaborSchedulingViewModel.ThisWeek.employeeStatus = isManager;
                LaborSchedulingViewModel.ThisWeek.employeeListAll = FakeAPI.GetAllEmployees();
                LaborSchedulingViewModel.ThisWeek.employeeListStore = storeEmployees;

                if (Session["UpdatedExclusions"] == null)
                {
                    LaborSchedulingViewModel.ThisWeek.excludedDates = (string[])Session["UpdatedExclusions"];
                }

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

                if (LaborSchedulingViewModel.ThisWeek.MinEmployees == 0 && LaborSchedulingViewModel.ThisWeek.MinEmployeesDefault == 0)
                {
                    LaborSchedulingViewModel.ThisWeek.MinEmployeesDefault = 3;
                    LaborSchedulingViewModel.ThisWeek.MinEmployees = LaborSchedulingViewModel.ThisWeek.MinEmployeesDefault;
                    Session["MinEmployees"] = LaborSchedulingViewModel.ThisWeek.MinEmployees;

                }
                else
                {
                    Session["MinEmployees"] = LaborSchedulingViewModel.ThisWeek.MinEmployees;
                }

                if (LaborSchedulingViewModel.ThisWeek.MaxEmployees == 0 && LaborSchedulingViewModel.ThisWeek.MaxEmployeesDefault == 0)
                {
                    LaborSchedulingViewModel.ThisWeek.MaxEmployeesDefault = 20;
                    LaborSchedulingViewModel.ThisWeek.MaxEmployees = LaborSchedulingViewModel.ThisWeek.MaxEmployeesDefault;
                    Session["MaxEmployees"] = LaborSchedulingViewModel.ThisWeek.MaxEmployees;
                }
                else
                {
                    Session["MaxEmployees"] = LaborSchedulingViewModel.ThisWeek.MaxEmployees;
                }

                LaborSchedulingViewModel.ThisWeek.CreateEmpAvailabilityText();

                Session["lsView"] = LaborSchedulingViewModel;

                return View(LaborSchedulingViewModel);
            }
        }
        [HttpPost]
        public ActionResult LaborSchedule(LaborScheduling lsView)
        {
            string starthour = lsView.ThisWeek.WeekStartHour;
            string endhour = lsView.ThisWeek.WeekEndHour;
            string[] excludedDates = lsView.ExcludedDates;

            lsView = (LaborScheduling)Session["lsView"];

            lsView.ThisWeek.WeekStartHour = starthour;
            lsView.ThisWeek.WeekEndHour = endhour;
            lsView.ThisWeek.excludedDates = excludedDates;
            lsView.ExcludedDates = excludedDates;
            double[] oldWeighting = (double[])Session["WeekWeighting"];
            lsView.ThisWeek.currentStoreCode = (string)Session["StoreCode"];
            lsView.ThisWeek.employeeStatus = (bool)Session["EmployeeStatus"];

            if (lsView.ThisWeek.weekWeighting.Length < 6)
            {
                double[] weights = new double[6];
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
                if (lsView.ExcludeDates(lsView.ExcludedDates)[0] == "" || lsView.ExcludeDates(lsView.ExcludedDates)[0] == "System.String[]")
                {
                    lsView.ExcludedDates = new string[lsView.ThisWeek.ExclusionDates.Count];
                    for (int i = 0; i < lsView.ThisWeek.ExclusionDates.Count; i++)
                    {
                        lsView.ExcludedDates[i] = "False";
                    }

                    lsView.ThisWeek.excludedDates = lsView.ExcludedDates;
                }
                else if (Session["UpdatedExclusions"] == null)
                {
                    Session["UpdatedSchedule"] = lsView.ExcludeDates(lsView.ExcludedDates);
                    lsView.ThisWeek.excludedDates = lsView.ExcludeDates(lsView.ExcludedDates);
                    Session["UpdatedExclusions"] = lsView.ThisWeek.excludedDates;
                }
                else
                {
                    lsView.ThisWeek.excludedDates = lsView.ExcludeDates(lsView.ExcludedDates);
                    Session["UpdatedExclusions"] = lsView.ThisWeek.excludedDates;
                }
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

            if (Session["MinEmployeesDefault"] != null && Session["MaxEmployeesDefault"] != null)
            {
                lsView.ThisWeek.MinEmployeesDefault = (int)Session["MinEmployeesDefault"];
                lsView.ThisWeek.MaxEmployeesDefault = (int)Session["MaxEmployeesDefault"];
            }
            else
            {
                lsView.ThisWeek.MinEmployeesDefault = 3;
                lsView.ThisWeek.MaxEmployeesDefault = 20;
                Session["MinEmployeesDefault"] = lsView.ThisWeek.MinEmployeesDefault;
                Session["MaxEmployeesDefault"] = lsView.ThisWeek.MaxEmployeesDefault;
                lsView.ThisWeek.WeekdayPowerHours = 3;
                lsView.ThisWeek.WeekendPowerHours = 4;
                Session["WeekdayPowerHours"] = lsView.ThisWeek.WeekdayPowerHours;
                Session["WeekendPowerHours"] = lsView.ThisWeek.WeekendPowerHours;
            }

            //Session["MinEmployees"] = lsView.ThisWeek.MinEmployees;
            //Session["MaxEmployees"] = lsView.ThisWeek.MaxEmployees;
            Session["AssignmentTable"] = lsView.ThisWeek.AssignmentTable;
            Session["AllocatedHoursDisplay"] = lsView.ThisWeek.AllocatedHoursDisplay;
            Session["WeekWeighting"] = lsView.ThisWeek.weekWeighting;

            if (lsView.ThisWeek.MinEmployeesDefault != 0 && lsView.ThisWeek.MaxEmployeesDefault != 0)
            {
                if (lsView.ThisWeek.MinEmployees > lsView.ThisWeek.MinEmployeesDefault)
                {
                    lsView.ThisWeek.MinEmployees = lsView.ThisWeek.MinEmployeesDefault;
                }
                if (lsView.ThisWeek.MaxEmployees < lsView.ThisWeek.MaxEmployeesDefault)
                {
                    lsView.ThisWeek.MaxEmployees = lsView.ThisWeek.MaxEmployeesDefault;
                }
            }

            Session["lsView"] = lsView;

            //return View(lsView);
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

            Session["LaborSchedulingPartial"] = ls;

            ls.ThisWeek.CreateEmpAvailabilityText();

            return PartialView("_LaborScheduleAssignmentView", ls);
        }
        [HttpPost]
        public ActionResult _LaborScheduleAssignmentView(LaborScheduling ls, string employeeId, string startHour, string endHour)
        {
            //string selectedEmployeeId = ls.selectedEmployeeId;
            //string startHour = ls.ThisWeek.startHour;
            //string endHour = ls.ThisWeek.endHour;

            string[] unassignTimes = ls.UnassignTimes;

            ls = (LaborScheduling)Session["LaborSchedulingPartial"];

            ls.ThisWeek.selectedEmployeeId = employeeId;
            ls.ThisWeek.startHour = startHour;
            ls.ThisWeek.endHour = endHour;
            ls.UnassignTimes = unassignTimes;

            if (ls.ThisWeek.selectedEmployeeId != null)
            {
                LaborScheduling lsView = (LaborScheduling)Session["lsView"];
                lsView.ThisWeek.EmployeeScheduledTimes = ls.ThisWeek.UpdateEmployees(ls.ThisWeek.employeeListStore);
                ls.ThisWeek.CheckSchedulingRules(ls.ThisWeek.selectedWeekday, ls.ThisWeek.employeeListStore);
                Session["lsView"] = lsView;
            }

            if (ls.UnassignTimes != null && ls.UnassignTimes.Length > 0)
            {
                //FakeAPI.UnassignEmployee(ls.UnassignTimes, ls.ThisWeek.selectedEmployeeId, 2);
            }
            ls.selectedEmployeeId = ls.ThisWeek.selectedEmployeeId;

            ////return PartialView("LaborSchedule", ls);
            return PartialView("_LaborScheduleAssignmentView", ls);
            //return PartialView("testpartial");
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

            FakeAPI.UnassignEmployee(unassignTimes, employeeId, ls.ThisWeek.selectedWeekday, ls.ThisWeek.EmployeeScheduledTimes, ls.ThisWeek.CurrentWeekDates);

            string[] employeeIdsAll = new string[LaborScheduling.EmployeeListAll.Count];
            string[] employeeIdsStore = new string[LaborScheduling.EmployeeListStore.Count];

            for (int i = 0; i < employeeIdsStore.Length; i++)
            {
                employeeIdsStore[i] = LaborScheduling.EmployeeListStore[i].id;
            }

            ls.ThisWeek.EmployeeScheduledTimes = FakeAPI.GetEmployeeScheduledTimes(employeeIdsAll, ls.ThisWeek.CurrentWeekDates);
            ls.ThisWeek.GenerateNumEmployeesNeeded(selectedWeekday, ls.ThisWeek.employeeListStore);
            ls.selectedEmployeeId = ls.ThisWeek.selectedEmployeeId;
            //return View("LaborSchedule", ls);
            return PartialView("_LaborScheduleAssignmentView", ls);
        }
        [HttpGet]
        public ActionResult LaborScheduleHourAvailability(LaborScheduling ls, int selectedHour)
        {
            ls = (LaborScheduling)Session["lsView"];

            int weekday = ls.ThisWeek.selectedWeekday;
            string hour = Convert.ToString(ls.ThisWeek.AssignmentView.Columns[selectedHour]);

            ls.ThisWeek.GetEmployeesForHour(ls.ThisWeek.employeeListStore, weekday, hour);

            return PartialView("LaborScheduleHourAvailability", ls);
        }


        [HttpGet]
        public ActionResult EmployeeAvailability(string LocationCode)
        {
            //maybe get the employee id and manager status here
            
            AvailabilityViewModel avm = new AvailabilityViewModel(LocationCode);

            Session["EmpAvailabilityTable"] = avm.EmpAvailabilityTable;
            Session["AvailabilityViewModel"] = avm;

            avm.EmployeeStatus = (bool)Session["EmployeeStatus"];

            return View(avm);
        }
        [HttpPost]
        public ActionResult EmployeeAvailability(string EmployeeId, string UpdatedSchedule, AvailabilityViewModel model)
        {
            Session["UpdatedSchedule"] = model.updatedSchedule;
            EmployeeId = (string)Session["CurrentEmployeeId"];
            model.EmpAvailabilityTable = (Dictionary<string, DataTable>)Session["EmpAvailabilityTable"];

            model.UpdateSchedule(model, model.updatedSchedule, EmployeeId);
            model.EmpAvailabilityTable[EmployeeId] = FakeAPI.GetEmployeeAvailability(EmployeeId);

            model = (AvailabilityViewModel)Session["AvailabilityViewModel"];

            return PartialView("EmployeeAvailabilityTable", model.EmpAvailabilityTable[EmployeeId]);
        }
        [HttpGet]
        public ActionResult EmployeeAvailabilityTable(string EmployeeId)
        {
            AvailabilityViewModel avm = (AvailabilityViewModel)Session["AvailabilityViewModel"];

            if (EmployeeId == "--")
            {
                return View(avm);
            }
            else
            {
                Session["CurrentEmpAvailability"] = avm.EmpAvailabilityTable[EmployeeId];
                Session["CurrentEmployeeId"] = EmployeeId;

                return PartialView("EmployeeAvailabilityTable", avm.EmpAvailabilityTable[EmployeeId]);
            }
        }


        [HttpGet]
        public ActionResult EmployeeHome(string LocationCode, string EmployeeId)
        {
            EmployeeId = "3213";
            //maybe get the employee id and manager status here

            EmployeeModel model = new EmployeeModel(LocationCode, EmployeeId);

            for (int i = 0; i < 14; i++)
            {
                model.startDates.Add(DateTime.Now.AddDays(i + 1).ToString("M/d/yyyy"));
            }

            Session["EmployeeId"] = EmployeeId;
            Session["EmployeeSchedule"] = model;

            return View(model);
        }
        [HttpPost]
        public ActionResult EmployeeHome(EmployeeModel EmployeeSchedule)
        {
            EmployeeTimeOffRequest TimeOffRequest = new EmployeeTimeOffRequest();
            List<Employees> ListOfEmployees = FakeAPI.GetAllEmployees();

            string employeeId = (string)Session["EmployeeId"];
            string employeeName = "";

            // fill the dropdown with dates from the next two weeks
            for (int i = 0; i < 14; i++)
            {
                EmployeeSchedule.startDates.Add(DateTime.Now.AddDays(i + 1).ToString("M/d/yyyy"));
            }

            // get employee name
            foreach (var emp in ListOfEmployees)
            {
                if (emp.id == employeeId)
                {
                    employeeName = emp.firstName;
                }
            }

            // set the time off request variables
            TimeOffRequest.Id = employeeId;
            TimeOffRequest.Name = employeeName;
            TimeOffRequest.startDate = EmployeeSchedule.startDate;
            TimeOffRequest.endDate = EmployeeSchedule.endDate;
            TimeOffRequest.startTime = EmployeeSchedule.startTime;
            TimeOffRequest.endTime = EmployeeSchedule.endTime;
            TimeOffRequest.allDayCheck = EmployeeSchedule.allDayCheck;
            TimeOffRequest.created = Convert.ToString(DateTime.Now);
            TimeOffRequest.message = EmployeeSchedule.CreateMessage(TimeOffRequest.Name, TimeOffRequest.startDate, TimeOffRequest.endDate, TimeOffRequest.startTime, TimeOffRequest.endTime, TimeOffRequest.allDayCheck);

            // submit the time off request to the database
            EmployeeSchedule.submitTimeOffRequest(TimeOffRequest.created, TimeOffRequest.startDate, TimeOffRequest.endDate,
                TimeOffRequest.Name, TimeOffRequest.Id, TimeOffRequest.startTime, TimeOffRequest.endTime, TimeOffRequest.message);
            // get the employee's messages from the database
            EmployeeSchedule.getMessages(TimeOffRequest.Id);

            EmployeeSchedule = (EmployeeModel)Session["EmployeeSchedule"];

            return View(EmployeeSchedule);
        }


        [HttpGet]
        public ActionResult ManagerConfiguration()
        {
            string StoreCode = "0001"; // change to be set by the login
            string EmployeeId = "9999"; // change to be set by the login

            bool scheduleExists = false;
            LaborScheduling ManagerConfig = (LaborScheduling)Session["ManagerConfig"];
            LaborScheduling ls = new LaborScheduling();
            bool EmployeeStatus = false;
            List<Employees> storeEmployees = FakeAPI.GetEmployeesForStore(StoreCode);
            foreach (Employees emp in storeEmployees)
            {
                if (emp.id == EmployeeId)
                {
                    if (emp.rank == 50 || emp.rank == 60)
                    {
                        EmployeeStatus = true;
                    }
                }
            }
            Session["EmployeeStatus"] = EmployeeStatus;

            ls = (LaborScheduling)Session["lsView"];
            if(ls.ThisWeek.AllocatedHours.Rows.Count > 0)
            {
                scheduleExists = true;
            }
            if (EmployeeStatus == false)
            {
                LaborScheduling employee = new LaborScheduling();
                employee.ThisWeek.employeeStatus = EmployeeStatus;
                return View(employee);
            }
            else
            {
                if (ManagerConfig != null)
                {
                    StoreCode = (string)Session["StoreCode"];
                    EmployeeStatus = (bool)Session["EmployeeStatus"];

                    ls.ThisWeek.WeekStartHours = ManagerConfig.ThisWeek.WeekStartHours;
                    ls.ThisWeek.WeekEndHours = ManagerConfig.ThisWeek.WeekEndHours;
                    ls.ThisWeek.MinEmployeesDefault = ManagerConfig.ThisWeek.MinEmployeesDefault;
                    ls.ThisWeek.MaxEmployeesDefault = ManagerConfig.ThisWeek.MaxEmployeesDefault;
                    ls.ThisWeek.WeekdayPowerHours = ManagerConfig.ThisWeek.WeekdayPowerHours;
                    ls.ThisWeek.WeekendPowerHours = ManagerConfig.ThisWeek.WeekendPowerHours;
                    ls.ThisWeek.NumberHistoricalWeeks = ManagerConfig.ThisWeek.NumberHistoricalWeeks;
                    ls.ThisWeek.employeeListStore = FakeAPI.GetEmployeesForStore(StoreCode);
                    ls.ThisWeek.employeeStatus = EmployeeStatus;

                    Session["lsView"] = ls;

                    return View(ls);
                }
                else if (scheduleExists == true)
                {
                    StoreCode = (string)Session["StoreCode"];
                    EmployeeStatus = (bool)Session["EmployeeStatus"];

                    if (ls.ThisWeek.MinEmployeesDefault == 0)
                    {
                        ls.ThisWeek.MinEmployeesDefault = 3;
                    }
                    if (ls.ThisWeek.MaxEmployeesDefault == 0)
                    {
                        ls.ThisWeek.MaxEmployeesDefault = 20;
                    }
                    if (ls.ThisWeek.WeekdayPowerHours == 0)
                    {
                        ls.ThisWeek.WeekdayPowerHours = 3;
                    }
                    if (ls.ThisWeek.WeekendPowerHours == 0)
                    {
                        ls.ThisWeek.WeekendPowerHours = 4;
                    }
                    //ls.ThisWeek.employeeListStore = FakeAPI.GetEmployeesForStore(StoreCode);
                    ls.ThisWeek.employeeStatus = EmployeeStatus;

                    return View(ls);
                }
                else
                {
                    return View(ls);
                }
            }
        }
        [HttpPost]
        public ActionResult ManagerConfiguration(LaborScheduling ls)
        {
            DataTable AllocatedHours = (DataTable)Session["AllocatedHoursDisplay"];

            ls.ThisWeek.weekWeighting = (double[])Session["WeekWeighting"];

            if (ls.ThisWeek.WeekStartHours != null)
            {
                ls.ThisWeek.GetBlackoutCells(AllocatedHours);
                Session["BlackoutCells"] = ls.ThisWeek.BlackoutTimes;
                Session["WeekStartHours"] = ls.ThisWeek.WeekStartHours;
                Session["WeekEndHours"] = ls.ThisWeek.WeekEndHours;
            }

            if (ls.ThisWeek.MinEmployeesDefault != 0 && ls.ThisWeek.MaxEmployeesDefault != 0)
            {
                Session["MinEmployeesDefault"] = ls.ThisWeek.MinEmployeesDefault;
                Session["MaxEmployeesDefault"] = ls.ThisWeek.MaxEmployeesDefault;
                Session["WeekdayPowerHours"] = ls.ThisWeek.WeekdayPowerHours;
                Session["WeekendPowerHours"] = ls.ThisWeek.WeekendPowerHours;
                Session["NumberHistoricalWeeks"] = ls.ThisWeek.NumberHistoricalWeeks;
            }

            ls = (LaborScheduling)Session["lsView"];
            ls.ThisWeek.WeekStartHours = (string[])Session["WeekStartHours"];
            ls.ThisWeek.WeekEndHours = (string[])Session["WeekEndHours"];
            ls.ThisWeek.weekWeighting = (double[])Session["WeekWeighting"];
            ls.ThisWeek.MinEmployeesDefault = (int)Session["MinEmployeesDefault"];
            ls.ThisWeek.MaxEmployeesDefault = (int)Session["MaxEmployeesDefault"];
            ls.ThisWeek.WeekdayPowerHours = (int)Session["WeekdayPowerHours"];
            ls.ThisWeek.WeekendPowerHours = (int)Session["WeekendPowerHours"];
            ls.ThisWeek.BlackoutTimes = (DataTable)Session["BlackoutCells"];
            Session["ManagerConfig"] = ls;
            //session for ls ?

            return View(ls);
        }
        [HttpGet]
        public ActionResult _ManagerConfigFindEmployee(LaborScheduling ls, string EmployeeInfo)
        {
            ls = (LaborScheduling)Session["lsView"];

            DataTable foundEmployee = ls.ThisWeek.FindEmployee(EmployeeInfo);

            if (foundEmployee.Rows.Count > 0)
            {
                Session["FoundEmployee"] = foundEmployee.Rows[0][1].ToString();

                return PartialView(foundEmployee);
            }
            else
            {
                return View(ls);
            }
        }
        [HttpPost]
        public ActionResult _ManagerConfigFindEmployee(LaborScheduling ls, bool AddEmployee)
        {
            ls = (LaborScheduling)Session["lsView"];

            string employeeId = (string)Session["FoundEmployee"];
            ls.ThisWeek.AddEmployeeToList(employeeId);
            Session["UpdatedEmployeeListStore"] = ls.ThisWeek.employeeListStore;

            return View(ls);
        }





        public ActionResult Grading()
        {
            ViewBag.Message = "Grading page.";

            return View();
        }

    }
}

