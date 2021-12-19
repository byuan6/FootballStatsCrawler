using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HtmlTable
{

    public class DataPoint
    {
        public bool IsValid { get; set; }
        public string Value { get; set; }
        public enum DataType { none, tostring, tonumber, todate }
        public DataType ValueType { get; set; }

        public object Convert()
        {
            return this.convert(this.Value, this.ValueType);
        }

        object convert(string value, DataType datatype)
        {
            double dbltmp;
            DateTime datetmp;
            //there's no way to know if the data has decimal points until it's been fully parsed, otherwise specific numeric type has to be indicated in command line (we can autodetect instead)
            switch (datatype)
            {
                case DataType.tostring:
                    return value;
                    break;
                case DataType.tonumber:
                    if (double.TryParse(value, out dbltmp))
                        return dbltmp;
                    else
                    {
                        this.IsValid = false;
                        return null;
                    }
                    break;
                case DataType.todate:
                    if (DateTime.TryParse(value, out datetmp))
                        return datetmp;
                    else
                    {
                        this.IsValid = false;
                        return null;
                    }
                    break;
            }
            return null;
        }
    }

}
