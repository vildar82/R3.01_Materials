using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3_01_KR_Material
{
    /// <summary>
    /// Свойства параметра
    /// </summary>
    public class DefinitionInfo
    {
        public DefinitionInfo(Definition def, List<BuiltInCategory> categories)
        {
            Definition = def;
            Categories = categories;               
        }

        public Definition Definition { get; set; }
        public bool IsBinding { get; set; }
        public bool IsInstanceBinding { get; set; }
        public List<BuiltInCategory> Categories { get; set; }                
    }
}
