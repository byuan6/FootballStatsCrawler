using System;
using System.Web;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions;

namespace FFToiletBowlWeb
{
    public class JsonFolderHandler : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        static Dictionary<string, Type> __datasource = new Dictionary<string, Type>() { 
            {"/Json/PlayerIndex.json",typeof(PlayerIndex) },
            {"/Json/InjuryModelData.json",typeof(InjuryModelData) },
            {@"^/Json/InjuryModelData/.+\.json$",typeof(InjuryModelData) },
            {@"^/Json/ViewSchedule/.+json$",typeof(viewschedule) },
        };
        public void ProcessRequest(HttpContext context)
        {
            //write your handler implementation here.
            var filename = context.Request.PhysicalPath;
            var virtualpath = context.Request.Path; //.ToUpper();
            if(File.Exists(filename)) 
            {
                context.Response.WriteFile(filename);
            }
            else if(__datasource.ContainsKey(virtualpath))
            {
                using (var relevantpage = (IDisposable)Activator.CreateInstance(__datasource[virtualpath]))
                {
                    var datasource = (IDataExposed)relevantpage;
                    var lst = datasource.Obj;
                    string json = string.Join(string.Empty, lst.ToJsonParts());
                    ThreadPool.QueueUserWorkItem(queueSaveFile, new Tuple<string, string>(filename, json));

                    context.Response.Write(json);
                }
            }
            else if (__datasource.Any(s=>Regex.IsMatch(virtualpath,s.Key)))
            {
                var relevantpagetype = __datasource.Single(s=>Regex.IsMatch(virtualpath,s.Key)).Value;
                using (var relevantpage = (IDisposable)Activator.CreateInstance(relevantpagetype))
                {
                    var datasource = (IDataExposed)relevantpage;
                    datasource.Parameters = virtualpath.Substring(1, virtualpath.LastIndexOf(".json")-1).Split('/');
                    var lst = datasource.Obj;
                    string json = string.Join(string.Empty, lst.ToJsonParts());
                    ThreadPool.QueueUserWorkItem(queueSaveFile, new Tuple<string, string>(filename, json));

                    context.Response.Write(json);
                }
            }
            else
                context.Response.StatusCode = 404;
        }
        void queueSaveFile(object state) 
        {
            var pack = (Tuple<string,string>)state;
            var filename = pack.Item1;
            var tmp = filename + Environment.TickCount;
            var json = pack.Item2;
            File.WriteAllText(tmp, json);
            if(File.Exists(filename))
                File.Delete(filename);
            File.Move(tmp, filename);
        }
        #endregion
    }
}
