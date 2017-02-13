using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3_01_KR_Material
{
    /// <summary>
    /// Фильтр элементов
    /// </summary>
    public static class FilterService
    {
        public static IEnumerable<Element> Filter()
        {             
            var catFilters = Command.Options.Categories.Select(s=>(ElementFilter)new ElementCategoryFilter(s)).ToList();            
            var orFilter = new LogicalOrFilter(catFilters);
            return new FilteredElementCollector(Command.UiApp.ActiveUIDocument.Document).                
                WherePasses(orFilter);            
        }        
    }
}
