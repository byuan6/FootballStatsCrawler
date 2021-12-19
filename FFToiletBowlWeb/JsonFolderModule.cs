using System;
using System.Web;
using System.IO;

namespace FFToiletBowlWeb
{
    public class JsonFolderModule : IHttpModule
    {
        /// <summary>
        /// You will need to configure this module in the web.config file of your
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpModule Members

        public void Dispose()
        {
            //clean-up code here.
        }

        public void Init(HttpApplication context)
        {
            // Below is an example of how you can handle LogRequest event and provide 
            // custom logging implementation for it
            //context.LogRequest += new EventHandler(OnLogRequest);
            context.BeginRequest += new EventHandler(begin_request);
            context.EndRequest += new EventHandler(end_request);
        }

        #endregion

        /*
        public void OnLogRequest(Object source, EventArgs e)
        {
            //custom logging logic can go here
        }
        */

        public void begin_request(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var cxt = app.Context;
            var req = cxt.Request;

            string upperPath = req.Path.ToUpper();
            if (upperPath.StartsWith("/JSON") && upperPath.EndsWith(".JSON"))
            {
                //https://docs.microsoft.com/en-us/dotnet/api/system.web.httpcontext.rewritepath?view=netframework-4.8
                if (!File.Exists(req.PhysicalPath))
                {
                    if (upperPath.Contains("?"))
                        cxt.RewritePath(req.Path + "&build=on");
                    else
                        cxt.RewritePath(req.Path + "?&build=on");
                }
            }
        }
        public void end_request(object sender, EventArgs e)
        {

        }   
    }
}
