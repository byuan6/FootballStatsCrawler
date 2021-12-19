using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CookComputing.XmlRpc;

namespace PostNewToWordpress
{
    public struct blogInfo
    {

        public string title;
        public string description;

    }

    public interface IgetCatList
    {

        [CookComputing.XmlRpc.XmlRpcMethod("metaWeblog.newPost")]

        string NewPage(int blogId, string strUserName,

            string strPassword, blogInfo content, int publish);

    }

    class Program
    {
        static void Main(string[] args)
        {
            const int INDEX_TITLE = 0;
            const int INDEX_URL = 1;
            const int INDEX_USER = 2;
            const int INDEX_PASS = 3;

            int numargs = args.Length-1;
            if (numargs < 0)
            {
                Console.WriteLine("Usage: PostNewToWordpress [title] [url:optional] [user:optional] [pass:optional]");
                return;
            }

            blogInfo newBlogPost = default(blogInfo);
            newBlogPost.title = numargs >= INDEX_TITLE ? args[INDEX_TITLE] : DateTime.Now.ToLongDateString();
            newBlogPost.description = "<h1>blog</h1>";

            IgetCatList categories = (IgetCatList)XmlRpcProxyGen.Create(typeof(IgetCatList));
            XmlRpcClientProtocol clientProtocol = (XmlRpcClientProtocol)categories;
            clientProtocol.Url = numargs >= INDEX_URL ? args[INDEX_URL] : "https://beyondtoiletbowl.wordpress.com/xmlrpc.php";

            string username = numargs >= INDEX_USER ? args[INDEX_USER] : "byuan";
            string password = numargs >= INDEX_PASS ? args[INDEX_PASS] : "-----";

            Console.WriteLine("Reading data...");
            string s;
            /*
            StringBuilder sb = new StringBuilder();
            int lines = 0;
            while ((s = Console.ReadLine()) != null)
            {
                Console.CursorLeft = 0;
                Console.Write(lines++);
                sb.AppendLine(s);
            }*/
            s = System.Console.In.ReadToEnd();

            Console.WriteLine(s);
            newBlogPost.description = s;// sb.ToString();


            string result = null;
            try
            {

                result = categories.NewPage(1, username, password, newBlogPost, 1);
                Console.WriteLine("Posted to Blog successfullly! Post ID : " + result);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                
            }
        }
    }
}
