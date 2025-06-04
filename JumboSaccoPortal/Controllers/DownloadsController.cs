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
        private string _rootDocumentFolderPath;

        public ActionResult Index() // Shows the list of folders
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            _rootDocumentFolderPath = ConfigurationManager.AppSettings["SaccoDocuments"];

            if (!Directory.Exists(_rootDocumentFolderPath))
            {
                Directory.CreateDirectory(_rootDocumentFolderPath);
            }

            var viewModel = new FolderListViewModel
            {
                Subfolders = Directory.GetDirectories(_rootDocumentFolderPath)
                                      .Select(d => new DirectoryInfo(d).Name)
                                      .ToList()
            };

            return View(viewModel);
        }

        public ActionResult Documents(string folderName)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            _rootDocumentFolderPath = ConfigurationManager.AppSettings["SaccoDocuments"];
            string currentFolderPath = Path.Combine(_rootDocumentFolderPath, folderName);

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

            return View("Documents", viewModel); // Use a specific view for document listing
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