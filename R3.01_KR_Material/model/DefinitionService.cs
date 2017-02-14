using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Revit_Lib.Extensions;

namespace R3_01_KR_Material
{
    /// <summary>
    /// Проверка опредедления параметра в документе
    /// </summary>
    public static class DefinitionService
    {
        /// <summary>
        /// Проверка параметра "КР_Материал"
        /// </summary>        
        public static void CheckDefinition() 
        {
            var uiApp = Command.UiApp;
            var doc = uiApp.ActiveUIDocument.Document;
            using (var t = new Transaction(doc, "Проверка параметра"))
            {
                t.Start();
                // Поиск параметра            
                var defefInfoFinds = IterateParameters(doc, d => string.Equals(d.Key.Name, Command.Options.ParamKRMaterialName,
                    StringComparison.OrdinalIgnoreCase));

                // параметра из файла общих параметров
                var defFromSharedFile = GetDefinitionFromSharedParameterFile();

                if (defefInfoFinds == null || !defefInfoFinds.Any())
                {
                    // Нет параметра в проекте - создание
                    CreateDefinition(defFromSharedFile);
                }
                else if (defefInfoFinds.Skip(1).Any())
                {
                    // Несколько параметров КР_Материал - удаление и создание
                    foreach (var item in defefInfoFinds)
                    {
                        DeleteParam(uiApp, item);
                    }
                    CreateDefinition(defFromSharedFile);
                }
                else
                {
                    // Найден только один параметр КР_Материал в проекте
                    var paramDefFind = defefInfoFinds.First();
                    // Определение GUIDa параметра
                    var guid = DefineGUID(Command.Options.ParamKRMaterialName);
                    // Если гуиды совпадают и группы, то ок, если нет, то замена параметра из файла общих параметров
                    if (guid != defFromSharedFile.GUID ||
                        !CheckCategories(paramDefFind))
                    {
                        DeleteParam(uiApp, paramDefFind);
                        CreateDefinition(defFromSharedFile);
                    }
                    else
                    {
                        // Проверить примениение параметра в группах
                        var internalDef = (InternalDefinition)paramDefFind.Definition;
                        if (!internalDef.VariesAcrossGroups)
                        {
                            internalDef.SetAllowVaryBetweenGroups(doc, true);
                        }
                    }
                }
                t.Commit();
            }
        }        

        public static IEnumerable<DefinitionInfo> IterateParameters(Document doc, Predicate<DefinitionBindingMapIterator> predicate)
        {
            var bindings = doc.ParameterBindings;
            int n = bindings.Size;
            if (0 < n)
            {
                DefinitionBindingMapIterator it = bindings.ForwardIterator();
                while (it.MoveNext())
                {                    
                    Debug.WriteLine($"{it.Key.Name}, {it.Key}, {it.Key.ParameterType}");
                    if (predicate(it))
                    {
                        var categories = (it.Current as ElementBinding)?.Categories.OfType<Category>().
                                        Select(s => (BuiltInCategory)s.Id.IntegerValue).ToList();
                        yield return new DefinitionInfo(it.Key, categories);
                    }
                }
            }
        }

        /// <summary>
        /// Создание параметра из файла общих параметров
        /// </summary>        
        private static void CreateDefinition(Definition defKRMaterial)
        {
            var doc = Command.UiApp.ActiveUIDocument.Document;
            // Привязка категорий
            var categories = Command.UiApp.Application.Create.NewCategorySet();
            foreach (var catId in Command.Options.Categories)
            {
                var cat = doc.Settings.Categories.get_Item(catId);
                categories.Insert(cat);
            }
            var binding = Command.UiApp.Application.Create.NewInstanceBinding(categories);
            if (!doc.ParameterBindings.Insert(defKRMaterial, binding, BuiltInParameterGroup.PG_TEXT))
            {
                throw new Exception($"Не удалось создать параметр '{defKRMaterial.Name}' из файла общих параметров.");
            }

            var resDef = IterateParameters(doc, (p) => p.Key.Name == defKRMaterial.Name).First().Definition as InternalDefinition;
            resDef.SetAllowVaryBetweenGroups(doc, true);
        }

        private static ExternalDefinition GetDefinitionFromSharedParameterFile()
        {
            var oldFile = Command.UiApp.Application.SharedParametersFilename;
            try
            {
                Command.UiApp.Application.SharedParametersFilename = Command.Options.SharedParameterFile;
                // Определение параметра из файла общих параметров
                return Command.UiApp.Application.OpenSharedParameterFile().Groups.First(g => g.Name == Command.Options.ParamKRMaterialGroup).
                    Definitions.First(d => string.Equals(d.Name, Command.Options.ParamKRMaterialName,
                        StringComparison.OrdinalIgnoreCase)) as ExternalDefinition;
            }
            catch (Exception ex)
            {
                throw new Exception($"Не найден параметр '{Command.Options.ParamKRMaterialName}' в общем файле параметров. {ex.Message}");
            }
            finally
            {
                Command.UiApp.Application.SharedParametersFilename = oldFile;
            }
        }

        private static void DeleteParam(UIApplication uiApp, DefinitionInfo item)
        {
            var res = uiApp.ActiveUIDocument.Document.ParameterBindings.Remove(item.Definition);
        }        

        private static Guid DefineGUID(string name)
        {
            // Найти этот параметр у любого элемента в моделе
            var param = FilterService.Filter().Where(e => !(e is ElementType)).First().LookupParameter(name);
            if (param == null)
            {
                throw new Exception($"Не найден параметр '{name}'");
            }
            return param.GUID;
        }

        /// <summary>
        /// Сравнение набора категорий в параметре текущего проекта с необходимым набором категорий
        /// </summary>        
        private static bool CheckCategories(DefinitionInfo paramDefFind)
        {
            return Command.Options.Categories.EqualsList(paramDefFind.Categories);
        }        
    }
}
