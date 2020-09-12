using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lexikar.DAL;
using Lexikar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lexikar.Controllers
{
    public class SearchResults // Move to separate file
    {
        public List<Translation> FR { get; set; } = new List<Translation>(); // Other languages are used as well
        public List<Translation> CS { get; set; } = new List<Translation>();
        public List<CorpusString> Corpus { get; set; } = new List<CorpusString>();
        public class CorpusString
        {
            public int rownumber;
            public string left = "";
            public string right = "";
            public string middle = "";
            public string left_middle = "";
            public string right_middle = "";
            public string source = "";
        }
    }
    [Route("legilex-fr")]
    public class TranslationController : Controller
    {
        protected readonly DictionaryContext _context;

        protected int CloneID;
        public TranslationController(DictionaryContext context)
        {
            _context = context;
            CloneID = 0;    // Add another level of abstraction,
                            // specify CloneID only for concrete classes
                            // instead of overwriting it in child constructors.
        }


        // GET: Translation
        [Route("")]
        [Route("Index")]
        [Route("Index/{id?}/{searchString?}/{sortchoice?}/{orderby?}")]
        public async Task<ActionResult> Index(string searchString, string sortChoice, string orderBy, int? id = 0)
        {
            ViewBag.ID = id;
            SearchResults results = new SearchResults();

            ListFilter(searchString, results);
            CorpusRoutine(searchString, results);
            SortRoutine(sortChoice, orderBy, results);
            
            // Use paging / split into different methods and call with Ajax instead of returning whole database
            return View(model: results);

            // local functions;

            // These functions should be in a repository
            // Use returned value instead of relying on the metod to edit content of the imput parameter results
            void ListFilter(string searchString, SearchResults results)
            {
                ViewBag.SearchString = searchString == null ? "" : searchString;

                // Misterious uncomented regex. What does it do? Use decriptive names
                const string pattern = "\\([^)]*?\\)";

                if (String.IsNullOrEmpty(searchString))
                {
                    // Change to query database and not localy
                    results.FR = _context.Translations.Where(x => x.CloneID == this.CloneID).ToList();                    
                    results.FR = results.FR.OrderBy(t =>
                    {
                        
                        return Regex.Replace(t.FR, pattern, "");
                    }).ToList();

                    // Change to query database and not localy
                    results.CS = _context.Translations.Where(x => x.CloneID == this.CloneID).ToList();
                    results.CS = results.CS.OrderBy(t => Regex.Replace(t.CS, pattern, "")).ToList(); 
                }
                else
                {
                    // Change to query database and not localy
                    results.FR = _context.Translations.Where(x => x.CloneID == this.CloneID).ToList();
                    //results.FR = results.FR.Where(t => Regex.Replace(t.FR, "\\([^)]*?\\)", "").Contains(searchString)).OrderBy(t => Regex.Replace(t.FR, "\\([^)]*?\\)", "")).ToList();

                    // Returns list of results in FR column regardless of case.
                    results.FR = results.FR
                        .Where
                        (
                            t =>
                            CultureInfo.InvariantCulture.CompareInfo
                            .IndexOf
                            (
                                Regex.Replace(t.FR, pattern, ""),    
                                searchString,
                                CompareOptions.IgnoreCase
                            ) >= 0
                        )
                        .OrderBy
                        (
                            t => 
                            Regex.Replace(t.FR, pattern, "") 
                        )
                        .ToList();

                    // Change to query database and not localy
                    results.CS = _context.Translations.Where(x => x.CloneID == this.CloneID).ToList();
                    results.CS = results.CS
                        .Where
                        (
                            t =>
                            CultureInfo.InvariantCulture.CompareInfo
                            .IndexOf(
                                Regex.Replace(t.CS, pattern, ""), 
                                searchString,
                                CompareOptions.IgnoreCase) >= 0
                        )
                        .OrderBy
                        (
                            t =>
                            Regex.Replace(t.CS, pattern, "") 
                        ) 
                        .ToList();

                    // Clean up commented code

                    //results.FR = _context.Translations.Where(t => t.FR.Contains(searchString)).OrderBy(t => t.FR).ToList();
                    //results.CS = _context.Translations.Where(t => t.CS.Contains(searchString)).OrderBy(t => t.CS).ToList();
                }
                foreach (var item in results.FR)
                {
                    item.FR = item.FR.Replace("(", "<mark class=\"lexitem\">(").Replace(")", ")</mark>");
                }
                foreach (var item in results.CS)
                {
                    item.CS = item.CS.Replace("(", "<mark class=\"lexitem\">(").Replace(")", ")</mark>");
                }
            }
            void CorpusRoutine(string searchString, SearchResults results)
            {
                if (String.IsNullOrEmpty(searchString))
                    return;
                char[] spacers = { ' ' /*space*/, '.', ',', '!', '?', ':', '(', ')', ' ' /*nbsp*/ };

                // Change to query database and not localy
                var list = _context.CorpusSources
                    .Include(c => c.CorpusFile)
                    .Where(c => c.CloneID == this.CloneID)
                    .ToList()
                    .Where(c => c.TextRow.Contains(searchString, StringComparison.OrdinalIgnoreCase));

                foreach (var item in list)
                {
                    int index = item.TextRow.IndexOf(ViewBag.SearchString, StringComparison.OrdinalIgnoreCase);
                    int start = item.TextRow.LastIndexOfAny(spacers, index);
                    // Full names, not abbreviations
                    int startOfLeftMiddleWord = -1;
                    int endOfRightMiddleWord = -1; 
                    if (start <= 0)
                        start = 0;   // v případě žádného předcházejího spaceru je slovo na začátku
                    else
                    {
                        startOfLeftMiddleWord = item.TextRow.LastIndexOfAny(spacers, start - 1);
                        if (startOfLeftMiddleWord < 0)
                            startOfLeftMiddleWord = 0;
                    }
                    int end = item.TextRow.Length;
                    int count = item.TextRow.IndexOfAny(spacers, index + searchString.Length, end - (index + searchString.Length)) - start;
                    if (count < 0)
                    {
                        count = end - start; // v případě žádného následujícího spaceru je slovo na konci
                        endOfRightMiddleWord = start + count;
                    }
                    else
                    {
                        endOfRightMiddleWord = item.TextRow.IndexOfAny(spacers, start + count + 1, end - (start + count + 1));
                        if (endOfRightMiddleWord < 0)
                            //ermw = start + count;
                            endOfRightMiddleWord = end;
                    }
                    int rownumber = item.RowNumber;
                    string left = startOfLeftMiddleWord > -1 
                        ? item.TextRow.Substring(0, startOfLeftMiddleWord + 1) 
                        : item.TextRow.Substring(0, start);
                    left = left.Trim(' '/*nbsp*/, ' '/*space*/);
                    string left_middle = startOfLeftMiddleWord > -1 ? item.TextRow.Substring(startOfLeftMiddleWord, start - startOfLeftMiddleWord) : "";
                    string middle = item.TextRow.Substring(start, count);
                    string right_middle = item.TextRow.Substring(start + count, endOfRightMiddleWord - start - count);
                    string right = item.TextRow.Substring(endOfRightMiddleWord, end - endOfRightMiddleWord);

                    if (middle[0] == '(')
                    {
                        middle = middle.Trim('(');
                        left_middle = left_middle + "(";
                    }
                    if (left_middle.Length == 1 && left.Length >= 1 && left[left.Length - 1] == left_middle[0])
                    {
                        left_middle = "";
                    }

                    results.Corpus.Add(new SearchResults.CorpusString()
                    {
                        rownumber = rownumber,
                        left = left,
                        left_middle = left_middle,
                        middle = middle,
                        right_middle = right_middle,
                        right = right,
                        source = item.CorpusFile.Name.Substring(0, item.CorpusFile.Name.IndexOf('.'))
                    });
                }
            }

            void SortRoutine(string sortChoice, string orderBy, SearchResults results)
            {
                // Styling should be separated in View pages.

                const string secondaryHiglightColor = "blue;";
                // Where is primary highlight color?

                // Results should be already trimmed
                switch (sortChoice)
                {
                    default:
                    case "none":
                        ViewBag.SortChoiceNone = "checked";
                        //results.Corpus.Sort(Comparer<SearchResults.CorpusString>.Create((x, y) => { return String.Compare(x.middle, y.middle); }));
                        results.Corpus = results.Corpus.OrderBy(k => k.middle.Trim()).ToList();
                        ViewBag.OrderChoiceMain = "checked";
                        break;
                    case "left":
                        ViewBag.SortChoiceLeft = "checked";
                        ViewBag.LeftColor = "color:" + secondaryHiglightColor;
                        ViewBag.LeftFW = "font - weight: bold;";
                        switch (orderBy) 
                        {
                            //case "main":
                            //    ViewBag.OrderChoiceMain = "checked";
                            //    results.Corpus = results.Corpus.OrderBy(k => k.middle).ThenBy(k => k.left_middle).ToList();
                            //    break;
                            default:
                            case "secondary":
                                ViewBag.OrderChoiceSecondary = "checked";
                                results.Corpus = results.Corpus.OrderBy(k => k.left_middle.Trim()).ThenBy(k => k.middle.Trim()).ToList();
                                break;
                        }
                        break;
                    case "right":
                        ViewBag.SortChoiceRight = "checked";
                        ViewBag.RightColor = "color:" + secondaryHiglightColor;
                        ViewBag.RightFW = "font - weight: bold;";
                        switch (orderBy)
                        {
                            //case "main":
                            //    ViewBag.OrderChoiceMain = "checked";
                            //    results.Corpus = results.Corpus.OrderBy(k => k.middle).ThenBy(k => k.right_middle).ToList();
                            //    break;
                            default:
                            case "secondary":
                                ViewBag.OrderChoiceSecondary = "checked";
                                results.Corpus = results.Corpus.OrderBy(k => k.right_middle.Trim()).ThenBy(k => k.middle.Trim()).ToList();
                                break;
                        }
                        break;
                }
            }
        }

        [Route("About")]
        public ActionResult About()
        {
            DescText DT = _context.DescTexts.Where(x => x.what == "about" && x.CloneID == this.CloneID).FirstOrDefault();
            return View(DT);
        }
        [Route("Help")]
        public ActionResult Help()
        {
            DescText DT = _context.DescTexts.Where(x => x.what == "help" && x.CloneID == this.CloneID).FirstOrDefault();
            return View(DT);
        }

        #region Helpfunctions
        [Route("Corpus")]
        public CorpusSource[] GetCorpusData()
        {
            return _context.CorpusSources.ToArray();
        }
        [Route("Trim")]
        public string Trim()
        {
            foreach (var item in _context.Translations)
            {
                item.FR = item.FR.Trim('"');
                item.CS = item.CS.Trim('"');
            }
            _context.SaveChanges();
            return "trim";
        }
        [Route("Test")]
        public string TestConnection()
        {
            try
            {
                if (_context.Database.CanConnect())
                    return "Connection to database is [   UP   ]";
                else
                    return "Connection to database is [  DOWN  ]";
            }
            catch (Exception ex)
            {
                return "[  FAIL  ] - couldn't connect.\n Error:\n\n" + ex.Message;
            }
        }
        #endregion
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
