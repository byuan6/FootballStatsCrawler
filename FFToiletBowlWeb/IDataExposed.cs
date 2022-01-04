using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FFToiletBowlWeb
{
    public enum TypeOfOrganization { List, RecursiveObject }
    /// <summary>
    /// The entire purpose of this interface, is to spread out the logic, 
    /// from being all encapsulated in JsonFolderHandler
    /// </summary>
    public interface IDataExposed
    {
        IJsonAble Obj { get; }
        object[] Parameters { get; set; }
    }
}