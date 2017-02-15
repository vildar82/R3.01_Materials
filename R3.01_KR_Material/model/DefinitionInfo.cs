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
        public DefinitionInfo(Definition def, ElementBinding binding)
        {
            Definition = def;
            Binding = binding;
            Categories = binding.Categories.Cast<Category>().Select(s=>(BuiltInCategory)s.Id.IntegerValue).ToList();               
        }

        public ElementBinding Binding { get; set; }
        public Definition Definition { get; set; }        
        public List<BuiltInCategory> Categories { get; set; }        
    }
}
