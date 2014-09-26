﻿using System.Linq;
using System.Web.Mvc;
using DataLayer.DataClasses;
using DataLayer.DataClasses.Concrete;
using DataLayer.Startup;
using GenericSecurity;
using GenericServices;
using GenericServices.Core;
using SampleWebApp.Infrastructure;
using SampleWebApp.Models;
using ServiceLayer.AttendeeServices;

namespace SampleWebApp.Controllers
{
    public class AttendeesController : Controller
    {
        // GET: Atendees
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListNames(IListService service)
        {
            var status = service.GetAll<Attendee>().Select(x => x.FullName).RealiseManyWithErrorChecking();

            if (status.IsValid)
                return View(status.Result);

            TempData["errorMessage"] = new MvcHtmlString(status.ErrorsAsHtml());
            return RedirectToAction("Index");
        }

        public ActionResult ListPaid(IListService service)
        {
            var status = service.GetAll<Attendee>().OrderBy(x => x.BookedOn.StartDate).ThenBy(x => x.HasPaid)
                .Select(x => new AttendeeAllListModel()
                {
                    AttendeeId = x.AttendeeId,
                    FullName = x.FullName,
                    EmailAddress = x.EmailAddress,
                    HasPaid = x.HasPaid,
                    CourseName = x.BookedOn.Name
                }).RealiseManyWithErrorChecking();

            if (status.IsValid)
                return View(status.Result);

            TempData["errorMessage"] = new MvcHtmlString(status.ErrorsAsHtml());
            return RedirectToAction("Index");
        }


        public ActionResult DetailsNotPaid(int id, IDetailService service)
        {
            var status = service.GetDetail<AttendeeNotPaidDto>(id);
            if (status.IsValid)
                return View(status.Result);

            TempData["errorMessage"] = new MvcHtmlString(status.ErrorsAsHtml());
            return RedirectToAction("ListPaid");
        }

        public ActionResult DetailsAll(int id, IDetailService service)
        {
            var status = service.GetDetail<AttendeeDetailAllDto>(id);
            if (status.IsValid)
                return View(status.Result);

            TempData["errorMessage"] = new MvcHtmlString(status.ErrorsAsHtml());
            return RedirectToAction("ListPaid");
        }


        public ActionResult EditNotPaid(int id, IDetailService service)
        {
            var status = service.GetDetail<AttendeeNotPaidDto>(id);
            if (status.IsValid)
                return View(status.Result);

            TempData["errorMessage"] = new MvcHtmlString(status.ErrorsAsHtml());
            return RedirectToAction("ListPaid");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditNotPaid(AttendeeNotPaidDto dto, IUpdateService service)
        {
            if (!ModelState.IsValid)
                //model errors so return immediately
                return View(dto);

            var response = service.Update(dto);
            if (response.IsValid)
            {
                TempData["message"] = response.SuccessMessage;
                return RedirectToAction("ListPaid");
            }

            //else errors, so copy the errors over to the ModelState and return to view
            response.CopyErrorsToModelState(ModelState, dto);
            return View(dto);
        }

        public ActionResult EditAll(int id, IDetailService service)
        {
            var status = service.GetDetail<AttendeeDetailAllDto>(id);
            if (status.IsValid)
                return View(status.Result);

            TempData["errorMessage"] = new MvcHtmlString(status.ErrorsAsHtml());
            return RedirectToAction("ListPaid");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAll(AttendeeDetailAllDto dto, IUpdateService service)
        {
            if (!ModelState.IsValid)
                //model errors so return immediately
                return View(dto);

            var response = service.Update(dto);
            if (response.IsValid)
            {
                TempData["message"] = response.SuccessMessage;
                return RedirectToAction("ListPaid");
            }

            //else errors, so copy the errors over to the ModelState and return to view
            response.CopyErrorsToModelState(ModelState, dto);
            return View(dto);
        }


        public ActionResult Delete(int id, IDeleteService service)
        {

            var response = service.Delete<Attendee>(id);
            if (response.IsValid)
                TempData["message"] = response.SuccessMessage;
            else
                //else errors, so send back an error message
                TempData["errorMessage"] = new MvcHtmlString(response.ErrorsAsHtml());

            return RedirectToAction("ListPaid");
        }


        //--------------------------------------------

        public ActionResult Reset(SampleWebAppDb db)
        {
            DataLayerInitialise.ResetCourses(db);
            TempData["message"] = "Successfully reset the courses data";
            return RedirectToAction("Index");
        }


    }
}