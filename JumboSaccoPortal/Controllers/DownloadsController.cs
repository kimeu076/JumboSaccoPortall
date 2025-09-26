using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JumboSaccoPortal.Controllers
{
    public class DownloadsController : Controller
    {
        private string _saccoDocumentFolderPath;
        private string _loanDocumentFolderPath;

        public ActionResult Index(string type = "sacco") // Default = Sacco documents
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (type.ToLower() == "sacco")
            {
                // --- Sacco documents (flat files) ---
                _saccoDocumentFolderPath = ConfigurationManager.AppSettings["SaccoDocuments"];

                if (!Directory.Exists(_saccoDocumentFolderPath))
                {
                    Directory.CreateDirectory(_saccoDocumentFolderPath);
                }

                var viewModel = new DocumentListViewModel
                {
                    FolderName = "Sacco Documents",
                    Documents = Directory.GetFiles(_saccoDocumentFolderPath)
                                         .Select(f => new DocumentViewModel
                                         {
                                             FileName = Path.GetFileName(f),
                                             FilePath = f
                                         })
                                         .ToList()
                };

                return View("Documents", viewModel); // Reuse the Documents view
            }
            else
            {
                // --- Loan documents (subfolder-based) ---
                _loanDocumentFolderPath = ConfigurationManager.AppSettings["LoanDocumentsPath"];

                if (!Directory.Exists(_loanDocumentFolderPath))
                {
                    Directory.CreateDirectory(_loanDocumentFolderPath);
                }

                var viewModel = new FolderListViewModel
                {
                    Subfolders = Directory.GetDirectories(_loanDocumentFolderPath)
                                          .Select(d => new DirectoryInfo(d).Name)
                                          .ToList()
                };

                return View("Index", viewModel); // Folder listing
            }
        }

        public ActionResult Documents(string folderName)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            _loanDocumentFolderPath = ConfigurationManager.AppSettings["LoanDocumentsPath"];
            string currentFolderPath = Path.Combine(_loanDocumentFolderPath, folderName);

            if (!Directory.Exists(currentFolderPath))
            {
                return Content($"Error: Folder not found at '{currentFolderPath}'");
            }

            var viewModel = new DocumentListViewModel
            {
                FolderName = folderName,
                Documents = Directory.GetFiles(currentFolderPath)
                                     .Select(f => new DocumentViewModel
                                     {
                                         FileName = Path.GetFileName(f),
                                         FilePath = f
                                     })
                                     .ToList()
            };

            return View("Documents", viewModel);
        }

        public ActionResult Download(string filePath)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                return HttpNotFound();
            }

            try
            {
                string fileName = Path.GetFileName(filePath);
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                string contentType = MimeMapping.GetMimeMapping(fileName);

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return Content($"An error occurred: {ex.Message}");
            }
        }
    }

    public class DocumentViewModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }

    public class FolderListViewModel
    {
        public List<string> Subfolders { get; set; }
    }

    public class DocumentListViewModel
    {
        public string FolderName { get; set; }
        public List<DocumentViewModel> Documents { get; set; }
    }
}
