using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3_01_Materials
{
    /// <summary>
    /// Фильтр элементов
    /// </summary>
    public static class FilterService
    {
        public static IEnumerable<Element> Filter(Document doc, List<BuiltInCategory> categories)
        {
            var catFilters = categories.Select(s=>(ElementFilter)new ElementCategoryFilter(s)).ToList();                
            var orFilter = new LogicalOrFilter(catFilters);
            return new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).WherePasses(orFilter);            
        }
    }
}
