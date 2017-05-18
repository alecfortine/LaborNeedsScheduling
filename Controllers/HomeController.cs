using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LaborNeedsScheduling.Models;
using LSAData;

namespace LaborNeedsScheduling.Controllers
{
    public class HomeController : Controller
    {

        [HttpGet]
        public ActionResult LaborSchedule()
        {

            LaborScheduling lsViewModel = new LaborScheduling();
            Session["lsViewModel"] = lsViewModel;

            return View(lsViewModel);
        }

        [HttpPost]
        public ActionResult LaborSchedule(LaborScheduling lsViewModel)
        {

            LaborScheduling oldModel = (LaborScheduling)Session["lsViewModel"];

            lsViewModel.ThisWeek.ExclusionDates = oldModel.ThisWeek.ExclusionDates;
            lsViewModel.ThisWeek.WeeksAgo = oldModel.ThisWeek.WeeksAgo;

            lsViewModel.ThisWeek.FillDatatables();

            lsViewModel.ThisWeek.LaborSchedule = lsViewModel.ThisWeek.AllocatedHours;
            lsViewModel.ThisWeek.HourSchedule = lsViewModel.ThisWeek.CurrentWeekHours;


            return View(lsViewModel);
        }

        [HttpGet]
        public ActionResult EmployeeHome(string LocationCode, string EmployeeID)
        {
            EmployeeModel model = new EmployeeModel(LocationCode, "001");

            return View(model);
        }
        [HttpPost]
        public ActionResult EmployeeHome(LaborScheduling EmployeeSchedule)
        {
            ViewBag.Message = "Employee page.";

            return View(EmployeeSchedule);
        }




        [HttpGet]
        public ActionResult EmployeeAvailability(string LocationCode)
        {
            AvailabilityViewModel avm = new AvailabilityViewModel(LocationCode);

            Session["AvailabilityViewModel"] = avm;

            return View(avm);
        }
        [HttpPost]
        public ActionResult EmployeeAvailability(AvailabilityViewModel model)
        {
            ViewBag.Message = "Availability page.";

            return View(model);
        }

        [HttpGet]
        public ActionResult EmployeeAvailabilityTable(string EmployeeID)
        {
            AvailabilityViewModel avm = (AvailabilityViewModel)Session["AvailabilityViewModel"];

            return PartialView("AvailabilityTable", avm.EmpAvail[EmployeeID]);
        }




        public ActionResult ManagerConfiguration(LaborScheduling lsViewModel)
        {
            ViewBag.Message = "Configuration page.";

            LaborScheduling oldModel = (LaborScheduling)Session["lsViewModel"];

            lsViewModel.ThisWeek.FillDatatables();

            lsViewModel.ThisWeek.LaborSchedule = lsViewModel.ThisWeek.TimeSelectionTable;

            return View(lsViewModel);
        }

        public ActionResult Grading()
        {
            ViewBag.Message = "Grading page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact page.";

            return View();
        }
    }
}

