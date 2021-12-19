using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{

    public class Img : NilTag, IIndexableParseElement
    {
        public Img(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "IMG"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Img(parentContext, value); }

        public string Src
        {
            get
            {
                var src = this.Attrib.FirstOrDefault(s => s.Name.ToLower() == "src");
                if (src != null)
                    return src.Value;
                else
                    return null;
            }
            set
            {
                var src = this.Attrib.FirstOrDefault(s => s.Name.ToLower() == "src");
                if (src != null)
                    this.Attrib.Remove(src);

                this.Attrib.Add(new TagAttribute("src",value));
            }
        }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
    public class Area : InlineTag, IIndexableParseElement
    {
        public Area(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "AREA"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Area(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
    public class Map : InlineTag, IIndexableParseElement
    {
        public Map(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "MAP"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Map(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
}
