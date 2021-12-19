using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using CookComputing.XmlRpc;

namespace AddImageToWordpress
{
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct Image
    {
        public string name;
        public string type;
        public byte[] bits;  //base64;
        public bool overwrite;
        public int gallery;
        public int image_d;
    }
    public struct WordpressReturn
    {
        public string id;
        public string file;
        public string url;
        public string type;
    }

    public interface IgetCatList
    {

        [XmlRpcMethod("metaWeblog.newMediaObject")]
        WordpressReturn newImage(string blogid, string username, string password, Image theImage, bool overwrite);

    }

    class Program
    {
        static void Main(string[] args)
        {
            const int INDEX_NAME = 0;
            const int INDEX_URL = 1;
            const int INDEX_USER = 2;
            const int INDEX_PASS = 3;

            int numargs = args.Length - 1;
            string username = numargs >= INDEX_USER ? args[INDEX_USER] : "byuan";
            string password = numargs >= INDEX_PASS ? args[INDEX_PASS] : "------";

            Console.WriteLine("<IMG Reading data...");
            string stdin = allstdin();
            //
            char[] invalid = Path.GetInvalidPathChars();
            while (stdin != null && stdin.Length > 0 && invalid.Contains(stdin[0]))
                stdin = stdin.Substring(1);
            while (stdin != null && stdin.Length > 0 && invalid.Contains(stdin[stdin.Length-1]))
                stdin = stdin.Substring(0, stdin.Length - 1);
                
            Console.WriteLine("[{0}]",stdin);
            
            byte[] data = null;
            string filename = null;
            string ext = null;
            string justname = null;
            if (!string.IsNullOrWhiteSpace(stdin) && File.Exists(stdin))
            {
                filename = stdin;
                data = File.ReadAllBytes(filename);
                justname = Path.GetFileName(filename);
                ext = Path.GetExtension(filename);
            }
            else {
                if (numargs < 0)
                {
                    Console.WriteLine("Usage: PostImageToWordpress [filename:optional]");
                    return;
                }
                filename = args[INDEX_NAME];

                if (File.Exists(filename))
                {
                    data = File.ReadAllBytes(filename);
                    justname = Path.GetFileName(filename);
                    ext = Path.GetExtension(filename);
                }
                else
                {
                    Console.WriteLine("No such file {0}", filename);
                    return;
                }
            }
            Console.WriteLine("{0} bytes", data.Length);
            

            Image theImage = new Image();
            theImage.name = justname;
            theImage.bits = data; // Convert.ToBase64String(data);
            theImage.overwrite = false;
            theImage.type = (ext == "png") ? "image/png" : "image/" + ext;

            IgetCatList categories = (IgetCatList)XmlRpcProxyGen.Create(typeof(IgetCatList));
            XmlRpcClientProtocol clientProtocol = (XmlRpcClientProtocol)categories;
            clientProtocol.Url = numargs >= INDEX_URL ? args[INDEX_URL] : "https://beyondtoiletbowl.wordpress.com/xmlrpc.php";


            WordpressReturn result = default(WordpressReturn);
            try
            {
                result = categories.newImage("blogid", username, password, theImage, false);
                Console.WriteLine("Image successfully uploaded!");
                Console.WriteLine("src=\"{0}\">", result.url);
                Console.ReadLine();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }

        static string allstdin()
        {
            
            using (TextReader reader = System.Console.In)
            {
                try {
                    bool b =Console.KeyAvailable;
                    return null;
                }
                catch (InvalidOperationException ex)
                {
                    // KeyAvailable throws exception when stdin
                    return reader.ReadToEnd();
                }
                
            }
        }
    }
}
