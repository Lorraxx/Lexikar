using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lexikar.DAL;
using Lexikar.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Configuration;
using Microsoft.Extensions.Options;

namespace Lexikar.Controllers
{
    [Route("legilex-fr")]
    public class UploadController : Controller
    {
        private readonly IOptions<LoginDetailModel> _LoginDetail;
        private readonly DictionaryContext _context;
        protected int CloneID;
        
        public UploadController(DictionaryContext context, IOptions<LoginDetailModel> loginDetail)
        {
            _context = context;
            _LoginDetail = loginDetail;
            CloneID = 0;
        }
        
        [Route("Login")]
        public async Task<IActionResult> Login()
        {
            return View("Login", this._context.CorpusFiles.Where(x => x.CloneID == this.CloneID));
        }
        
        [Route("Login")]
        [HttpPost, ActionName("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginConfirmed(string password)
        {
            if (Authenticate(password))
            {
                ViewBag.Message = "Login successful";
                ViewBag.Secret = password;  // Never heep the password stored anywhere like this
                return View("LoginConfirmed");
            }
            else
            {
                ViewBag.Error = "Login Failed";
                return View("LoginConfirmed");  // Why return LoginConfirmed on fail? Return LoginFailed instead.
            }
        }
        
        [Route("Upload")]
        public async Task<IActionResult> Index()
        {
            return View("Index", this._context.CorpusFiles.Where(x=> x.CloneID == this.CloneID));
        }

        // GET: Movies/Delete/5
        [Route("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var file = await _context.CorpusFiles
                .FirstOrDefaultAsync(m => m.ID == id);
            if (file == null)
            {
                return NotFound();
            }

            return View(file);
        }

        // POST: Movies/Delete/5
        [Route("Delete")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string pass) // Do not send passwords like this! Use tokens!
        {
            if (!Authenticate(pass))
            {
                ViewBag.Error = "Wrong login!";
                return View("Login");
            }
            var file = await _context.CorpusFiles.FindAsync(id);
            _context.CorpusFiles.Remove(file);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void ParseFile(IFormFile file, string[] allowedFileExtensions, Action<string> fileContentAction, int MaxSize = 3 /*3 MB*/ )
        {
            if (file == null)
            {
                ViewBag.Error = "Please Upload Your file";
            }
            else if (file.Length > 0)
            {
                int MaxContentLength = 1024 * 1024 * MaxSize;

                if (!allowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
                {
                    ViewBag.Error = "Please file of type: " + string.Join(", ", allowedFileExtensions);
                }

                else if (file.Length > MaxContentLength)
                {
                    ViewBag.Error = "Your file is too large, maximum allowed size is: " + MaxSize + " MB";
                }
                else
                {
                    ViewBag.Message = "File uploaded successfully";
                    string text =
                        System.Text.Encoding.UTF8.GetString(
                            new BinaryReader(
                                file.OpenReadStream()
                                ).ReadBytes((int)file.Length)
                            );

                    fileContentAction(text);
                }
            }
        }

        [Route("FileUploadLex")]
        // Do not send passwords like this! Use tokens.
        public ActionResult FileUploadTranslations([FromForm] IFormFile lex, [FromForm] string pass) 
        {
            if (!Authenticate(pass))
            {
                ViewBag.Error = "Wrong login!";
                return View("Login"); // use redirect
            }
            if (ModelState.IsValid)
            {
                // Consider using a library for work with CSV files
                Action<string> LoadTranslationsToDatabase = delegate (string text) 
                {
                    string[] result = text.Split('\n');


                    // Use repositories
                    //MSSQL
                    _context.Database.ExecuteSqlCommand("TRUNCATE TABLE [dbo].[Translations]");

                    Char separatorChar = ';';
                    
                    // What does this regex even do? Comment, use descriptive variable names.
                    Regex regex = new Regex(separatorChar + "(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");    

                    foreach (var item in result.Skip(1))
                    {
                        string[] row = regex.Split(item);
                        if (row.Length != 9)
                            continue;
                        for (int i = 0; i < row.Length; i++)
                        {
                            row[i] = Regex.Replace(row[i], @"\t|\n|\r", "");
                        }
                        _context.Translations.Add(
                            new Translation
                            {
                                Nom = row[0] == "x",
                                Verb = row[1] == "x",
                                Ost = row[2] == "x",
                                FR = row[3].Trim('"'),
                                CS = row[4].Trim('"'),
                                Zdroj = row[5].Trim('"'),
                                Systematika = row[6].Trim('"'), 
                                Nezamnenovat = row[7].Trim('"'),
                                Poznamka = row[8].Trim('"'),
                                CloneID = this.CloneID
                            });
                    }

                    _context.SaveChanges();
                };
                this.ParseFile(lex, new string[] { ".csv" }, LoadTranslationsToDatabase, MaxSize: 3);
            }
            return View("Index", this._context.CorpusFiles.Where(x => x.CloneID == this.CloneID));
        }


        // Move data manipulation logic to repositories, controllers are for diplaying views.

        private Action<string> LoadDescText(string type)
        {
            // Why write extra delegate?
            Action<string> LoadDescTextAction = delegate (string text)
            {
                string result = text;
                var val = _context.DescTexts.SingleOrDefault(dt => dt.what == type && dt.CloneID == this.CloneID);
                if (val != null)
                    _context.DescTexts.Remove(val);

                _context.DescTexts.Add(
                    new DescText
                    {
                        text = result,
                        what = type,
                        CloneID = this.CloneID
                    }
                    );

                _context.SaveChanges();
            };
            return LoadDescTextAction;
        }
        [Route("FileUploadAbout")]
        public ActionResult FileUploadAbout([FromForm] IFormFile About, [FromForm] string Pass)
        {
            if (!Authenticate(Pass))
            {
                ViewBag.Error = "Wrong login!";
                return View("Login");
            }
            if (ModelState.IsValid)
            {
                // Same allowed suffixes, use variable instead.
                this.ParseFile(About, new string[] { ".txt" }, LoadDescText("about"), MaxSize: 3); 
            }
            return View("Index", this._context.CorpusFiles.Where(x => x.CloneID == this.CloneID));
        }
        [Route("FileUploadHelp")]
        public ActionResult FileUploadHelp([FromForm] IFormFile Help, [FromForm] string Pass)
        {
            if (!Authenticate(Pass))
            {
                ViewBag.Error = "Wrong login!";
                return View("Login");
            }
            if (ModelState.IsValid)
            {
                this.ParseFile(Help, new string[] { ".txt" }, LoadDescText("help"), MaxSize: 3); 
            }
            return View("Index", this._context.CorpusFiles.Where(x => x.CloneID == this.CloneID));
        }
        [Route("FileUploadCorpus")]
        public ActionResult FileUploadCorpus([FromForm] List<IFormFile> Corpus, [FromForm] string Pass)
        {
            if (!Authenticate(Pass))
            {
                ViewBag.Error = "Wrong login!";
                return View("Login");
            }
            if (ModelState.IsValid)
            {
                var ID = this._context.CorpusFiles.Count() > 0 ? this._context.CorpusFiles.Max(c => c.ID) + 1 : 1;
                List<string> conflictingNames = new List<string>();
                foreach (var item in Corpus)
                {
                    if ( this._context.CorpusFiles.Count(c => c.Name == item.FileName && c.CloneID == this.CloneID) > 0 )
                    {
                        conflictingNames.Add(item.FileName);
                        continue;
                    }
                    var corpusFile = new CorpusFile
                    {
                        ID = ID,
                        Name = item.FileName,
                        CloneID = this.CloneID
                    };
                    this._context.CorpusFiles.Add(corpusFile);

                    Action<string> CorpusAction = delegate (string text)
                    {
                        string[] rows = text.Split('\n');

                        for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
                        {
                            rows[rowIndex] = Regex.Replace(rows[rowIndex], @"\t|\n|\r", "");
                            if(string.IsNullOrWhiteSpace(rows[rowIndex]))
                            {
                                continue;
                            }
                            _context.CorpusSources.Add(new CorpusSource()
                            {
                                RowNumber = rowIndex,
                                TextRow = rows[rowIndex],
                                FileSourceID = ID,
                                CorpusFile = corpusFile,
                                CloneID = this.CloneID
                            });
                        }
                        _context.SaveChanges();
                    };

                    ID++;
                    this.ParseFile(item, new string[] { ".txt" }, CorpusAction, 30);
                }
                if (conflictingNames.Count() > 0)
                    ViewBag.CorpusErrorConflictingNames = conflictingNames;
            }
            return View("Index", this._context.CorpusFiles.Where(x => x.CloneID == this.CloneID));
        }

        private bool Authenticate(string password) // Do not send password like this! Use tonkes!
        {
            return password == _LoginDetail.Value.Password;
        }
    }
}
