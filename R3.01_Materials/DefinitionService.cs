using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;

namespace R3_01_Materials
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
            // Поиск параметра            
            var paramsDefInfoFind = IterateParameters(doc, d=>string.Equals(d.Key.Name, Command.Options.ParamKRMaterialName,
                    StringComparison.OrdinalIgnoreCase));

            var defFromSharedFile = GetDefinitionFromSharedParameterFile();
            
            if (paramsDefInfoFind == null || !paramsDefInfoFind.Any())
            {
                // Если параметра нет, то создание из файла общих парамеров
                CreateDefinition(uiApp, defFromSharedFile);                
            }
            else if (paramsDefInfoFind.Skip(1).Any())
            {
                // Несколько параметров КР_Материал - удаление и загрузка из файла общих параметров
                foreach (var item in paramsDefInfoFind)
                {
                    DeleteParam(uiApp,item);
                }
                CreateDefinition(uiApp, defFromSharedFile);
            }
            else
            {
                // Проверка параметра
                var paramDefFind = paramsDefInfoFind.First();
                var defInfoFromSharedFile = new DefinitionInfo(defFromSharedFile, Command.Options.Categories);
                if (!paramDefFind.IsEquals(defInfoFromSharedFile))
                {
                    DeleteParam(uiApp, paramDefFind);
                    CreateDefinition(uiApp, defFromSharedFile);
                }
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
                    var d = it.Key;
                    Debug.WriteLine($"{d.Name}, {d}, {d.ParameterType}");
                    if (predicate(it))
                    {
                        var categories = (it.Current as ElementBinding).Categories.OfType<Category>().
                                        Select(s => (BuiltInCategory)s.Id.IntegerValue).ToList();
                        yield return new DefinitionInfo(it.Key, categories);
                    }
                }
            }
        }

        /// <summary>
        /// Создание параметра из файла общих параметров
        /// </summary>        
        private static void CreateDefinition(UIApplication uiApp, Definition defKRMaterial)
        {
            using (var t = new Transaction(uiApp.ActiveUIDocument.Document, "Создание параметра"))
            {
                t.Start();
                // Привязка категорий
                var categories = uiApp.Application.Create.NewCategorySet();
                foreach (var catId in Command.Options.Categories)
                {
                    var cat = uiApp.ActiveUIDocument.Document.Settings.Categories.get_Item(catId);
                    categories.Insert(cat);
                }

                var binding = uiApp.Application.Create.NewInstanceBinding(categories);
                if (!uiApp.ActiveUIDocument.Document.ParameterBindings.Insert(defKRMaterial, binding))
                {
                    throw new Exception($"Не удалось создать параметр '{defKRMaterial.Name}' из файла общих параметров.");
                }
                t.Commit();
            }
        }

        private static Definition GetDefinitionFromSharedParameterFile()
        {
            var oldFile = Command.UiApp.Application.SharedParametersFilename;
            try
            {
                Command.UiApp.Application.SharedParametersFilename = Command.Options.SharedParameterFile;
                // Определение параметра из файла общих параметров
                return Command.UiApp.Application.OpenSharedParameterFile().Groups.First(g => g.Name == "KR").Definitions.
                    First(d => string.Equals(d.Name, Command.Options.ParamKRMaterialName, StringComparison.OrdinalIgnoreCase));                
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
            var res = uiApp.ActiveUIDocument.Document.ParameterBindings.Erase(item.Definition);
        }        
    }
}
