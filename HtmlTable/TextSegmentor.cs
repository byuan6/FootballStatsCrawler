using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HtmlTable
{


    public class TextSegmentator //if you ever implement database as source, create interface with ValueX, ValueY,ValueZ as the common fields.
    {
        public TextSegmentator()
        {
            this._master = this;
            this.Delimiter = '\t';
        }
        public TextSegmentator(TextSegmentator master)
            : base()
        {
            this._master = master;
        }
        TextSegmentator _master;

        public char Delimiter { get; set; }
        string _raw;
        public string Raw
        {
            get
            {
                return this._raw;
            }
            set
            {
                this._raw = value;
                this.Record = this._raw.Split(this.Delimiter);
            }
        }
        public string[] Record { get; set; }
        public int Source { get; set; }
        public string Value()
        {
            if (this._master.Record.Length<=this.Source)
                return null;
            else
                return this._master.Record[this.Source];
        }

        public TextSegmentator NewInstance()
        {
            TextSegmentator copy = new TextSegmentator(this)
            {
                Source = this.Source,
            };
            return copy;
        }
    }

}
