using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace R3_01_Materials
{
    public class MaterialGBService
    {
        /// <summary>
        /// Установка параметра КР_Материал = ЖБ, для элементов с параметром типа материал = Железобетон
        /// </summary>
        /// <param name="elements"></param>
        public static void SetParameters(Document doc, IEnumerable<Element> elements)
        {
            // Параметры с типом Материала - сгруппированные по категориям
            var dictMaterialDefsByCategory = DefinitionService.IterateParameters(doc,
                d => d.Key.ParameterType == ParameterType.Material).
                SelectMany(s => s.Categories.
                Select(r => new { cat = r, defName = s.Definition.Name })).GroupBy(g => g.cat).
                ToDictionary(k => k.Key, v => v.Select(s => s.defName).ToList());

            using (var t = new Transaction(doc, "Установка парметра материала для ЖБ элементов"))
            {
                t.Start();

                // группировка элементов по категориям                
                foreach (var groupCategory in elements.GroupBy(g => g.Category))
                {
                    // Имена параметров с типом материал для этой категории
                    List<string> defNames;
                    dictMaterialDefsByCategory.TryGetValue((BuiltInCategory)groupCategory.Key.Id.IntegerValue, out defNames);
                    if (defNames == null)
                        continue;

                    foreach (var elem in groupCategory)
                    {
                        // Параметры с типом метериала и этой категорией
                        foreach (var defName in defNames)
                        {
                            var paramMater = elem.LookupParameter(defName);
                            //  Если значение параметра = Железобетон
                            if (paramMater != null &&
                                string.Equals(paramMater.AsString(), Command.Options.ParamARMaterialValue, StringComparison.OrdinalIgnoreCase))
                            {
                                // Записать в параметр КР_Материал = ЖБ
                                // Что если у элемента несколько параметров "КР_Материал"
                                var pGB = elem.LookupParameter(Command.Options.ParamKRMaterialName);
                                if (pGB != null)
                                {
                                    pGB.Set(Command.Options.ParamKRMaterialValue);
                                }
                                else
                                {
                                    // Не может быть
                                    Command.Error.AddErrorMesaage($"Элемент не содержит параметр {Command.Options.ParamKRMaterialName}", elem);
                                }
                            }
                        }
                    }
                }
                t.Commit();
            }
        }        
    }    
}