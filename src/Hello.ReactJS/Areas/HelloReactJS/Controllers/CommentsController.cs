using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using Biggy.Core;
using Biggy.Data.Json;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
using Pingo.Filters.Attributes;

namespace Hello.ReactJS.Areas.HelloReactJS.Controllers
{
/*=============================================
    {
        "id": 1388534400000,
        "author": "Pete Hunt",
        "text": "Hey there!"
    }
===============================================*/

    public class CommentRecord
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public string Text { get; set; }
    }

    public class OnDemand<T> where T : class, new()
    {
        public bool IsNull => _t == null;
        private T _t;

        public T N => _t ?? (_t = new T());
    }

    public class JSonStoreWrapper<T> where T : class, new()
    {
        public string DatabaseDirectory { get; set; }
        private JsonStore<T> _jsonStore;
        private BiggyList<T> _tBiggyList;

        public JsonStore<T> JsonStore => _jsonStore ?? (_jsonStore = new JsonStore<T>(DatabaseDirectory,DatabaseName,TableName));
        public BiggyList<T> BiggyList => _tBiggyList ?? (_tBiggyList = new BiggyList<T>(JsonStore));
        public string DatabaseName { get; set; }
        public string TableName { get; set; }
    }

    [Scope(new[] { "api" })]
    [Area("api")]
    [Route("[area]/HelloReactJS/[controller]")]
    public class CommentsController : Controller
    {
        private static OnDemand<JSonStoreWrapper<CommentRecord>> JSonStoreWrapper = new OnDemand<JSonStoreWrapper<CommentRecord>>();

        private static void InitializeDatabase(IApplicationEnvironment appEnvironment)
        {
            if (JSonStoreWrapper.IsNull)
            {
                var wrapper = JSonStoreWrapper.N;
                wrapper.DatabaseDirectory = Path.Combine(appEnvironment.ApplicationBasePath, "App_Data");
                wrapper.DatabaseName = "ReactJS";
                wrapper.TableName = "Comments";
            }
        }

        private readonly ClaimsPrincipal _caller;
        private IApplicationEnvironment _appEnvironment;

        public CommentsController(IApplicationEnvironment appEnvironment,ClaimsPrincipal caller)
        {
            _caller = caller;
            _appEnvironment = appEnvironment;
            InitializeDatabase(_appEnvironment);
        }
        [HttpPost]
        public ActionResult Create([FromForm, Required]string author, string text)
        {
            if(string.IsNullOrEmpty(author))
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(text))
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);

            var rec = new CommentRecord
            {
                Id = Guid.NewGuid().ToString(),
                Author = author,
                Text = text
            };
            JSonStoreWrapper.N.BiggyList.Add(rec);
            return new HttpOkResult();
        }

        [HttpGet]
        public ActionResult Get()
        {
            try
            {
                var rec = new CommentRecord
                {
                    Id = Guid.NewGuid().ToString(),Author = "Herb",Text = "So, whats going on!"
                };
                var result = JSonStoreWrapper.N.BiggyList.ToArray();

                return new JsonResult(result);      
            }
            catch (Exception e)
            {
                throw;
            }
            

        }
    }
}
