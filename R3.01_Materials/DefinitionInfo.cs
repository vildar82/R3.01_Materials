using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3_01_Materials
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
                //(binding as ElementBinding).Categories.OfType<Category>().
                //Select(s => (BuiltInCategory)s.Id.IntegerValue).ToList();
        }

        public Definition Definition { get; set; }
        public bool IsBinding { get; set; }
        public bool IsInstanceBinding { get; set; }
        public List<BuiltInCategory> Categories { get; set; }

        public bool IsEquals (DefinitionInfo defOther)
        {
            var res = Definition.Name.Equals(defOther.Definition.Name, StringComparison.OrdinalIgnoreCase) &&
                Definition.ParameterGroup == defOther.Definition.ParameterGroup &&
                Definition.ParameterType == defOther.Definition.ParameterType &&
                Definition.UnitType == defOther.Definition.UnitType;
            if (!res) return false;

            if (Definition is InternalDefinition)
            {
                var idDefThis = (InternalDefinition)Definition;
                var idDefOther = (ExternalDefinition)defOther.Definition;
                if (idDefThis.Visible != idDefOther.Visible) return false;
            }
            return true;
        }
    }
}
