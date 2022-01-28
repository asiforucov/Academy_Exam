using Exam.DAL;
using Exam.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Exam.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TeamController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;
        public TeamController(AppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            webHostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            var team = _context.Team.ToList();
            return View(team);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var speaker = await _context.Team.FindAsync(id);
            var CurrentImage = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets\\image\\team", speaker.Image);
            _context.Team.Remove(speaker);
            if (await _context.SaveChangesAsync() > 0)
            {
                if (System.IO.File.Exists(CurrentImage))
                {
                    System.IO.File.Delete(CurrentImage);
                }
            }
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Team model)
        {
            if (!ModelState.IsValid)
            {
                string uniqueFileName = UploadedFile(model);
                Team speaker = new Team
                {
                    Name = model.Name,
                    Profession = model.Profession,
                    Title = model.Title,
                    Instagram = model.Instagram,
                    Facebook = model.Facebook,
                    Twitter = model.Twitter,
                    Linkedin = model.Linkedin,
                    Image = uniqueFileName
                };

                _context.Add(speaker);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
        private string UploadedFile(Team model)
        {
            string uniqueFileName = null;

            if (model.Photo != null)
            {
                string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "assets/image/team");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Photo.CopyTo(fileStream);
                }
            }

            return uniqueFileName;
        }

        public  IActionResult Update()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Team model)
        {
            if (!ModelState.IsValid)
            {
                var team = await _context.Team.FindAsync(model.Id);
                team.Name = model.Name;
                team.Profession = model.Profession;
                team.Title = model.Title;
                team.Instagram = model.Instagram;
                team.Twitter = model.Twitter;
                team.Linkedin = model.Linkedin;
                team.Facebook = model.Facebook;

                if (model.Photo != null)
                {
                    if (model.Image != null)
                    {
                        string filePath = Path.Combine(webHostEnvironment.WebRootPath, "assets/image/team", model.Image);
                        System.IO.File.Delete(filePath);
                    }

                    team.Image = UploadedFile(model);
                }
                _context.Update(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}
