﻿using Microsoft.AspNet.Identity;
using OpenDiscussionPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenDiscussionPlatform.Controllers
{
    public class SubjectsController : Controller
    {
        private Models.ApplicationDbContext db = new Models.ApplicationDbContext();


        // GET: Show
        public ActionResult Show(int id)
        {
            Subject subject = db.Subjects.Find(id);
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            SetAccessRights();
            return View(subject);
        }

        // POST: Show
        [Authorize(Roles = "User, Moderator, Admin")]
        [HttpPost]
        public ActionResult Show(Reply reply)
        {
            reply.Date = DateTime.Now;
            //reply user = getuser
            try
            {
                if (ModelState.IsValid)
                {
                    db.Replies.Add(reply);
                    db.SaveChanges();
                    TempData["message"] = "Raspunsul a fost adaugat!";
                    return Redirect("/Subjects/Show/" + reply.SubjectID);
                }
                else
                {
                    Subject s = db.Subjects.Find(reply.SubjectID);
                    SetAccessRights();
                    return View(s);
                }
            }
            catch (Exception)
            {
                Subject s = db.Subjects.Find(reply.SubjectID);
                SetAccessRights();
                return View(s);
            }
        }


        // GET: New
        [Authorize(Roles = "User, Moderator, Admin")]
        public ActionResult New(int id)
        {
            Subject subject = new Subject();
            subject.CategoryID = id;
            subject.UserID = User.Identity.GetUserId();
            var category = db.Categories.Find(id);
            ViewBag.CategoryName = category.Name;
            return View(subject);
        }

        // POST: New
        [Authorize(Roles = "User, Moderator, Admin")]
        [HttpPost]
        public ActionResult New(Subject subject)
        {
            subject.Date = DateTime.Now;
            try
            {
                if (ModelState.IsValid)
                {
                    db.Subjects.Add(subject);
                    db.SaveChanges();
                    TempData["message"] = "Subiectul a fost adaugat!";
                    return Redirect("/Categories/Show/" + subject.CategoryID);
                }
                else
                {
                    var category = db.Categories.Find(subject.CategoryID);
                    ViewBag.CategoryName = category.Name;
                    return View(subject);
                }
            }
            catch (Exception)
            {
                var category = db.Categories.Find(subject.CategoryID);
                ViewBag.CategoryName = category.Name;
                return View(subject);
            }
        }


        // GET: Edit
        [Authorize(Roles = "User, Moderator, Admin")]
        public ActionResult Edit(int id)
        {
            Subject subject = db.Subjects.Find(id);
            var category = db.Categories.Find(subject.CategoryID);
            ViewBag.CategoryName = category.Name;

            if (subject.UserID == User.Identity.GetUserId())
            {
                return View(subject);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati acest subiect!";
                return Redirect("/Subjects/Show/" + subject.SubjectID);
            }

        }

        //PUT: Edit
        [Authorize(Roles = "User, Moderator, Admin")]
        [HttpPut]
        public ActionResult Edit(int id, Subject requestSubject)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Subject subject = db.Subjects.Find(id);
                    if (subject.UserID == User.Identity.GetUserId())
                    {
                        if (TryUpdateModel(subject))
                        {
                            subject.Title = requestSubject.Title;
                            subject.Content = requestSubject.Content;
                            db.SaveChanges();
                            TempData["message"] = "Subiectul a fost modificat!";
                        }
                        return Redirect("/Categories/Show/" + subject.CategoryID);
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa editati acest subiect!";
                        return Redirect("/Subjects/Show/" + subject.SubjectID);
                    }
                }
                var category = db.Categories.Find(requestSubject.CategoryID);
                ViewBag.CategoryName = category.Name;
                return View(requestSubject);
            }
            catch (Exception)
            {
                var category = db.Categories.Find(requestSubject.CategoryID);
                ViewBag.CategoryName = category.Name;
                return View(requestSubject);
            }
        }


        // GET: EditCategory
        [Authorize(Roles = "Moderator, Admin")]
        public ActionResult EditCategory(int id)
        {
            Subject subject = db.Subjects.Find(id);
            subject.Categs = GetAllCategories();
            return View(subject);

        }

        //PUT: EditCategory
        [Authorize(Roles = "Moderator, Admin")]
        [HttpPut]
        public ActionResult EditCategory(int id, Subject requestSubject)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Subject subject = db.Subjects.Find(id);
                    if (TryUpdateModel(subject))
                    {
                        subject.CategoryID = requestSubject.CategoryID;
                        db.SaveChanges();
                        TempData["message"] = "Subiectul a fost mutat!";
                    }
                    return Redirect("/Categories/Show/" + subject.CategoryID);
                }
                requestSubject.Categs = GetAllCategories();
                return View(requestSubject);
            }
            catch (Exception)
            {
                requestSubject.Categs = GetAllCategories();
                return View(requestSubject);
            }
        }


        //DELETE: Delete
        [Authorize(Roles = "User, Moderator, Admin")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Subject subject = db.Subjects.Find(id);
            if (subject.UserID == User.Identity.GetUserId() || User.IsInRole("Moderator") || User.IsInRole("Admin"))
            {
                db.Subjects.Remove(subject);
                db.SaveChanges();
                TempData["message"] = "Subiectul a fost sters!";
                return Redirect("/Categories/Show/" + subject.CategoryID);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti acest subiect!";
                return Redirect("/Subjects/Show/" + subject.SubjectID);
            }
        }


        #region Helpers
        [NonAction]
        private IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();
            var categories = db.Categories;

            foreach (var category in categories)
            {
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryID.ToString(),
                    Text = category.Name.ToString()
                });
            }
            return selectList;
        }

        private void SetAccessRights()
        {
            ViewBag.isUser = User.IsInRole("User");
            ViewBag.isModerator = User.IsInRole("Moderator");
            ViewBag.isAdmin = User.IsInRole("Admin");
            ViewBag.currentUser = User.Identity.GetUserId();
        }
        #endregion
    }
}
