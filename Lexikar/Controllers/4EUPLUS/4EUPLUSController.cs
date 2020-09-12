using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lexikar.DAL;
using Microsoft.AspNetCore.Mvc;

namespace Lexikar.Controllers
{

    [Route("4EUPLUS-LEX")]
    public class _4EUPLUSController : TranslationController
    {
        public _4EUPLUSController(DictionaryContext context) : base(context)
        {
            CloneID = 1; // See TranslationController constructor comments.
        }
    }
}
