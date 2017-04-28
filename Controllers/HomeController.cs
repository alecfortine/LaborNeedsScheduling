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
        public ActionResult ManagerTable()
        {

            LaborScheduling lsViewModel = new LaborScheduling();
            Session["lsViewModel"] = lsViewModel;

            return View(lsViewModel);
        }

        [HttpPost]
        public ActionResult ManagerTable(LaborScheduling lsViewModel)
        {

            LaborScheduling oldModel = (LaborScheduling)Session["lsViewModel"];

            lsViewModel.ThisWeek.ExclusionDates = oldModel.ThisWeek.ExclusionDates;
            lsViewModel.ThisWeek.WeeksAgo = oldModel.ThisWeek.WeeksAgo;

            lsViewModel.ThisWeek.FillDatatables();

            lsViewModel.ThisWeek.LaborSchedule = lsViewModel.ThisWeek.AllocatedHours;

            return View(lsViewModel);
        }


        public ActionResult Employee()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}

