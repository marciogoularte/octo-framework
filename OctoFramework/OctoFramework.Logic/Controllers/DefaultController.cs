﻿using System.Web.Configuration;
using System.Web.Mvc;
using DevTrends.MvcDonutCaching;
using OctoFramework.Logic.Cache;
using Elmah;
using umbraco;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace OctoFramework.Logic.Controllers
{
    public class DefaultController : SurfaceController, IRenderMvcController
    {
        //Constructors
        public DefaultController()
        {
            Context = UmbracoContext.Current;
        }

        public DefaultController(UmbracoContext context) : base(context)
        {
            Context = context;
        }

        public DefaultController(UmbracoContext context, UmbracoHelper helper) : base(context, helper)
        {
            Context = context;
            _umbraco = helper;
        }

        protected UmbracoContext Context { get; }

        private readonly UmbracoHelper _umbraco;

        public override UmbracoHelper Umbraco => _umbraco ?? base.Umbraco;

        /// <summary>
        /// If the route hijacking doesn't find a controller this default controller will be used.
        /// That way a each page will always go through a controller and we can always have a MasterModel for the masterpage.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [UmbracoOutputCache]
        public virtual ActionResult Index(RenderModel model)
        {
            return CurrentTemplate(model);
            //return View(ControllerContext.RouteData.Values["action"].ToString(), model.Content);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            //Log the exception.
            LogHelper.Error<RenderMvcController>("Exception On Render", filterContext.Exception);
            ErrorSignal.FromCurrentContext().Raise(filterContext.Exception);
            //Email out
            var message = $"<p>Url: {filterContext.HttpContext.Request.Url}  <br/><br/>" + $"Exception occured: {filterContext.Exception}</p>";
            var user = WebConfigurationManager.AppSettings["ErrorEmailAddress"];
            var defaultEmail = WebConfigurationManager.AppSettings["DefaultSenderEmailAddress"] ?? "ing@octoframework.io";
            library.SendMail(defaultEmail, user, "Error Occurred", message, true);

            //Check if its been handled
            if (filterContext.ExceptionHandled)
            {
                return;
            }

            //Clear the cache if an error occurs.
            var cacheManager = new OutputCacheManager();

            cacheManager.RemoveItems();

            //Show the view error.
            filterContext.Result = View("Error");
            filterContext.ExceptionHandled = true;
        }

        protected ActionResult CurrentTemplate<T>(T model)
        {
            var template = ControllerContext.RouteData.Values["action"].ToString();
            if (!EnsurePhsyicalViewExists(template))
            {
                return HttpNotFound();
            }
            return View(template, model);
        }

        protected bool EnsurePhsyicalViewExists(string template)
        {
            var result = ViewEngines.Engines.FindView(ControllerContext, template, null);
            if (result.View == null)
            {
                LogHelper.Warn<DefaultController>("No physical template file was found for template " + template);
                return false;
            }
            return true;
        }
    }
}