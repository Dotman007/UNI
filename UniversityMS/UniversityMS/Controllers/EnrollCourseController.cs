using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using UniversityMS.Models;
using UniversityMS.Context;
using System.Text;
using System.IO;
using System.Diagnostics;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio.TwiML;
//using Twilio.AspNet.Mvc;

namespace UniversityMS.Controllers
{
    public class EnrollCourseController : Controller
    {
        private UniversityDbContext db = new UniversityDbContext();

        // GET: /EnrollCourse/
        public ActionResult Index()
        {
            var enrollcourses = db.EnrollCourses.Include(e => e.Course);
            return View(enrollcourses.ToList());
        }



        ////public ActionResult SendSMS()
        ////{
        ////    var accountSid = "AC906cf3e15a58e843bd25e9298e88e043";
        ////    var authToken = "18504a6d2779d2a1971ebe0cb669b11e";
        ////    TwilioClient.Init(accountSid, authToken);
        ////    var to = new PhoneNumber();
        ////    var from = new PhoneNumber("+13343924931");
        ////    var message = MessageResource.Create(to: to, from: from, body: "");
        ////    return message;
        ////}
        // GET: /EnrollCourse/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EnrollCourse enrollcourse = db.EnrollCourses.Find(id);
            if (enrollcourse == null)
            {
                return HttpNotFound();
            }
            return View(enrollcourse);
        }

        // GET: /EnrollCourse/Create
        public ActionResult Create()
        {
            ViewBag.StudentList = db.Students.ToList(); //Lists The Student Matric No
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "CourseCode");
            return View();
        }


        public JsonResult GetStudentById(string studentRegNo)
        {
            var students = db.Students.Where(m => m.StudentRegNo == studentRegNo).ToList();
            return Json(students, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCoursesbyDeptId(int deptId)
        {
            var courses = db.Courses.Where(m => m.Department.Id == deptId).ToList();
            return Json(courses, JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsAlreadyEnrolled(string regNo,int courseId)
        {
            var enrollCourses = db.EnrollCourses.Where(m => m.RegistrationNo == regNo && m.CourseId == courseId);

            if (enrollCourses.Count() == 0)
            {
                return Json(false);
            }
            return Json(true);
        }

        public JsonResult EnrollStudentToCourse(EnrollCourse enrollCourse,Student student)
        {

            var enrollCourses = db.EnrollCourses.Where(m => m.RegistrationNo == enrollCourse.RegistrationNo && m.CourseId == enrollCourse.CourseId).ToList();
            
            if (enrollCourses.Count() == 1)
            {
                var id = enrollCourses[0].Id;
                var date = enrollCourses[0].EnrollDate;
                enrollCourse.Id = id;
                enrollCourse.EnrollDate = date;
                db.EnrollCourses.AddOrUpdate(enrollCourse);
               
            }
            else
            {
                db.EnrollCourses.Add(enrollCourse); 
            }
         
             db.SaveChanges();
            return Json(true);
        }

        // POST: /EnrollCourse/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Id,RegistrationNo,CourseId,EnrollDate,CourseGrade,Email,ContactNo")] EnrollCourse enrollcourse,Student student)
        {
            if (ModelState.IsValid)
            {
                if (enrollcourse.CourseGrade != null)
                {
                    WebMail.SmtpServer = "smtp-mail.outlook.com";
                    WebMail.SmtpPort = 587;
                    WebMail.SmtpUseDefaultCredentials = true;
                    WebMail.EnableSsl = true;
                    WebMail.UserName = "dontreply009@outlook.com";
                    WebMail.Password = "Iyaniwura";
                    WebMail.From = "dontreply009@outlook.com";
                    WebMail.Send(to: student.Email, subject: "Course Result",
                        body: student.Name + "," + " " + student.StudentRegNo + "You Have the following Result " + "<br />"
                        + enrollcourse.Course.CourseCode + " " + enrollcourse.CourseGrade, isBodyHtml: true);
                }
                if (enrollcourse.CourseGrade == null)
                {
                    WebMail.SmtpServer = "smtp-mail.outlook.com";
                    WebMail.SmtpPort = 587;
                    WebMail.SmtpUseDefaultCredentials = true;
                    WebMail.EnableSsl = true;
                    WebMail.UserName = "dontreply009@outlook.com";
                    WebMail.Password = "Iyaniwura";
                    WebMail.From = "dontreply009@outlook.com";
                    WebMail.Send(to: student.Email, subject: "Course Registration",
                        body: student.Name + "," + " " + student.StudentRegNo + "You Registered " + "<br />"
                        + enrollcourse.Course.CourseCode, isBodyHtml: true);
                }
                db.EnrollCourses.Add(enrollcourse);
                db.SaveChanges();
               
                  
        }
          ViewBag.CourseId = new SelectList(db.Courses, "Id", "CourseCode", enrollcourse.CourseId);
            return View(enrollcourse);
        }


        public ActionResult SaveResult(Student student)
        {
            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseCode");
            ViewBag.StudentList = db.Students.ToList();//List Students Registration Number
            ViewBag.GradeList = db.Grades.ToList();//List the Varieties of Grade
            return View();
            
        }

        public JsonResult GetCoursesbyRegNo(string regNo)
        {
            var courses = db.EnrollCourses.Where(m => m.RegistrationNo == regNo).ToList();
            return Json(courses, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewResult()
        {
            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseCode");//List the Courses
            ViewBag.StudentList = db.Students.ToList();//List the Student Registration No
            return View();
        }

        // GET: /EnrollCourse/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EnrollCourse enrollcourse = db.EnrollCourses.Find(id);
            if (enrollcourse == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "CourseCode", enrollcourse.CourseId);
            return View(enrollcourse);
        }

        // POST: /EnrollCourse/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Id,RegistrationNo,CourseId,EnrollDate,CourseGrade")] EnrollCourse enrollcourse)
        {
            if (ModelState.IsValid)
            {
                db.Entry(enrollcourse).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "CourseCode", enrollcourse.CourseId);
            return View(enrollcourse);
        }

        // GET: /EnrollCourse/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EnrollCourse enrollcourse = db.EnrollCourses.Find(id);
            if (enrollcourse == null)
            {
                return HttpNotFound();
            }
            return View(enrollcourse);
        }

        // POST: /EnrollCourse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EnrollCourse enrollcourse = db.EnrollCourses.Find(id);
            db.EnrollCourses.Remove(enrollcourse);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
