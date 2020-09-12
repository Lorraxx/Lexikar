using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lexikar.DAL;
using Lexikar.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Lexikar.Controllers._4EUPLUS
{
    [Route("4EUPLUS-LEX")]
    public class UPLOAD_4EUPLUSController : UploadController
    {
        public UPLOAD_4EUPLUSController(DictionaryContext context, IOptions<LoginDetailModel> app) : base (context, app)
        {
            CloneID = 1; // See TranslationController constructor comments.
        }
    }
}
