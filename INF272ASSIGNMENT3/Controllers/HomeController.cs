using INF272ASSIGNMENT3.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using OfficeOpenXml;
using Rotativa;
using Newtonsoft.Json;

namespace INF272ASSIGNMENT3.Controllers
{
    public class HomeController : Controller
    {
        private LibraryEntities db = new LibraryEntities();

       
        public async Task<ActionResult> Home(int? page)
        {
            int pageSize = 5; 
            int pageNumber = (page ?? 1);

            var students = await db.students.OrderBy(s => s.studentId).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var books = await db.books.OrderBy(b => b.bookId).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Students = students;
            ViewBag.Books = books;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;

            return View();
        }

        
        public ActionResult CreateStudent()
        {
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateStudent([Bind(Include = "studentId,name,surname")] students student)
        {
            if (ModelState.IsValid)
            {
                db.students.Add(student);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(student);
        }

        public ActionResult CreateBook()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateBook([Bind(Include = "bookId,title,author")] books book)
        {
            if (ModelState.IsValid)
            {
                db.books.Add(book);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(book);
        }





        public async Task<ActionResult> Maintain(int? page)
        {
            int pageSize = 5; 
            int pageNumber = (page ?? 1);

            var types = await db.types.OrderBy(t => t.typeId).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var authors = await db.authors.OrderBy(a => a.authorId).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var borrows = await db.borrows.Include(b => b.books).Include(b => b.students).OrderBy(b => b.borrowId).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Types = types;
            ViewBag.Authors = authors;
            ViewBag.Borrows = borrows;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;

            return View();
        }



        public ActionResult Reports()
        {
            var loans = db.borrows
                .Include(b => b.books)
                .Include(b => b.students)
                .Where(b => b.broughtDate == null)
                .ToList();

            var chartData = DataService.GetData(loans);

            ViewBag.ChartLabels = chartData.Select(dp => dp.Label).ToArray();
            ViewBag.ChartData = chartData.Select(dp => dp.Count).ToArray();

            return View(loans);
        }




        public async Task<ActionResult> Reporting()
        {
            var loans = await GenerateCurrentLoansReport();
            return View(loans);
        }

        private async Task<List<borrows>> GenerateCurrentLoansReport()
        {
            var currentLoans = await db.borrows
                .Include(b => b.books)
                .Include(b => b.students)
                .Where(b => b.broughtDate == null)
                .ToListAsync();

            List<borrows> loans = new List<borrows>();

            foreach (var loan in currentLoans)
            {
                loans.Add(new borrows
                {
                    bookId = loan.bookId,
                    books = loan.books,
                    studentId = loan.studentId,
                    students = loan.students
                });
            }

            return loans;
        }


        public async Task<ActionResult> SaveReport(string filename, string fileType)
        {
            var loans = await GenerateCurrentLoansReport();

            switch (fileType.ToLower())
            {
                case "excel":
                    // Save as Excel
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Report");
                        // Populate the worksheet with data...
                        System.IO.File.WriteAllBytes(filename, package.GetAsByteArray());
                    }
                    break;

                case "pdf":
                    // Save as PDF
                    var report = new ViewAsPdf("Reports", loans);
                    var binary = report.BuildFile(ControllerContext);
                    System.IO.File.WriteAllBytes(filename, binary);
                    break;
            }

            return RedirectToAction("Reports");
        }


        public ActionResult Archive()
        {
            string reportDirectory = @"C:\Reports";

            List<Report> reports;
            if (Directory.Exists(reportDirectory))
            {
                reports = Directory.EnumerateFiles(reportDirectory)
                    .Select(filename => new Report
                    {
                        Filename = filename,
                        DateCreated = System.IO.File.GetCreationTime(filename)
                    })
                    .ToList();
            }
            else
            {
                reports = new List<Report>();
            }

            ViewBag.Reports = reports;

            return View();
        }

    }
}











