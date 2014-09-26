﻿using System;
using System.Linq;
using System.Data.Entity;
using DataLayer.DataClasses;
using DataLayer.Startup;
using GenericServices.Services.Concrete;
using NUnit.Framework;
using ServiceLayer.CourseServices;
using Tests.Helpers;

namespace Tests.UnitTests.Group03ServiceLayer
{
    class Test04CourseDtos
    {

        private ClaimsIdentityHelper _userSetup;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _userSetup = new ClaimsIdentityHelper();
        }

        [SetUp]
        public void SetUp()
        {
            using (var db = new SampleWebAppDb())
            {
                DataLayerInitialise.InitialiseThis(false, true);
                DataLayerInitialise.ResetCourses(db);
            }
        }

        [Test]
        public void Check01ListCoursesOk()
        {
            _userSetup.SetUser("ada");
            using (var db = new SecureSampleWebAppDb())
            {
                //SETUP
                var service = new ListService(db);

                //ATTEMPT
                var courses = service.GetAll<CourseListDto>().ToList();

                //VERIFY
                courses.Count.ShouldEqual(2);
                courses[0].StartDate.ShouldEqual(new DateTime(1839, 04, 14));
            }
        }

        [Test]
        public void Check02CourseDetailOk()
        {
            _userSetup.SetUser("ada");
            using (var db = new SecureSampleWebAppDb())
            {
                //SETUP
                var lastCourseId = db.Courses.OrderByDescending( x => x.CourseId).First().CourseId;
                var service = new DetailService(db);

                //ATTEMPT
                var status = service.GetDetail<CourseDetailDto>(lastCourseId);

                //VERIFY
                status.IsValid.ShouldEqual(true, status.Errors);
                status.Result.ShouldNotEqualNull();
                status.Result.AttendeesNames.ShouldEqual("Andrew Crosse, Sir David Brewster, Charles Wheatstone, Charles Dickens, Michael Faraday, John Hobhouse");
            }
        }

        [Test]
        public void Check05CourseUpdateOk()
        {
            _userSetup.SetUser("ada");
            using (var db = new SecureSampleWebAppDb())
            {
                //SETUP
                var lastCourseId = db.Courses.OrderByDescending(x => x.CourseId).First().CourseId;
                var setupService = new UpdateSetupService(db);
                var service = new UpdateService(db);

                //ATTEMPT
                var setupStatus = setupService.GetOriginal<CourseDetailDto>(lastCourseId);
                setupStatus.IsValid.ShouldEqual(true, setupStatus.Errors);
                setupStatus.Result.Name = "Unit Test";
                setupStatus.Result.StartDate = new DateTime(2000,1,1);
                var status = service.Update(setupStatus.Result);

                //VERIFY
                status.IsValid.ShouldEqual(true, status.Errors);
                var course = db.Courses.Include(x => x.Attendees).AsNoTracking().ToList().Last();
                course.Name.ShouldEqual( "Unit Test");
                course.StartDate.ShouldEqual(new DateTime(2000, 1, 1));
                //not changed
                course.MainPresenter.ShouldEqual("Ada Lovelace");
                string.Join( ", ", course.Attendees.Select( x => x.FullName))
                    .ShouldEqual("Andrew Crosse, Sir David Brewster, Charles Wheatstone, Charles Dickens, Michael Faraday, John Hobhouse");
            }
        }

        [Test]
        public void Check06CourseCreateOk()
        {
            _userSetup.SetUser("ada");
            using (var db = new SecureSampleWebAppDb())
            {
                //SETUP
                var setupService = new CreateSetupService(db);
                var service = new CreateService(db);

                //ATTEMPT
                var dto = setupService.GetDto<CourseDetailDto>();
                dto.Name = "Unit Test";
                dto.MainPresenter = "A person";
                dto.Description = "a description";
                dto.StartDate = new DateTime(2000, 1, 1);
                var status = service.Create(dto);

                //VERIFY
                status.IsValid.ShouldEqual(true, status.Errors);
                var course =
                    db.Courses.Include(x => x.Attendees).AsNoTracking().OrderByDescending(x => x.CourseId).First();
                course.Name.ShouldEqual("Unit Test");
                course.MainPresenter.ShouldEqual("A person");
                course.StartDate.ShouldEqual(new DateTime(2000, 1, 1));
                course.Attendees.Count.ShouldEqual(0);
            }
        }

    }
}
