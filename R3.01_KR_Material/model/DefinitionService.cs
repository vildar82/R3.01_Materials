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
using Revit_Message;

namespace R3_01_KR_Material
{
    /// <summary>
    /// Проверка опредедления параметра в документе
    /// </summary>
    public class DefinitionService
    {
        UIApplication uiApp;
        Options opt;
        Document doc;
        Error err;

        public DefinitionService(UIApplication uiApp, Options opt, Error err)
        {
            this.err = err;
            this.uiApp = uiApp;
            this.opt = opt;
            doc = uiApp.ActiveUIDocument.Document;
        }

        /// <summary>
        /// Проверка параметра "КР_Материал"
        /// </summary>        
        public void CheckDefinition() 
        {                        
            using (var t = new Transaction(doc, "Проверка параметра"))
            {
                t.Start();
                // Поиск параметра            
                var defefInfoFinds = IterateParameters(doc, d => string.Equals(d.Key.Name, opt.ParamKRMaterialName,
                    StringComparison.OrdinalIgnoreCase)).ToList();

                // параметра из файла общих параметров
                var defFromSharedFile = GetDefinitionFromSharedParameterFile();

                // Если нет такого параметра, или проверены найденные параметры и определен правильный
                CheckFindsDefs(defefInfoFinds, defFromSharedFile);                
                                
                t.Commit();
            }
        }

        /// <summary>
        /// Проверка найденных параметров КР_Материал
        /// </summary>
        /// <param name="defefInfoFinds">Найденные параметры в проекте</param>
        /// <param name="defFromSharedFile">Параметр из файла общих параметров</param>        
        private void CheckFindsDefs(IEnumerable<DefinitionInfo> defefInfoFinds, ExternalDefinition defFromSharedFile)
        {
            if (defefInfoFinds == null || !defefInfoFinds.Any())
            {
                // Нет параметра в проекте - создание
                CreateDefinition(defFromSharedFile);
                return;
            }

            bool isFindGuid = false;
            foreach (var defFind in defefInfoFinds)
            {
                var guid = DefineGUID(opt.ParamKRMaterialName);
                if (guid == defFromSharedFile.GUID)
                {
                    isFindGuid = true;
                    // Проверка настроек параметра
                    if (!CheckCategories(defFind))
                    {
                        // Исправление набора категорий
                        DeleteParam(defFind);
                        CreateDefinition(defFromSharedFile);
                    }
                    else
                    {
                        // Проверить примениение параметра в группах
                        var internalDef = (InternalDefinition)defFind.Definition;
                        if (!internalDef.VariesAcrossGroups)
                        {
                            internalDef.SetAllowVaryBetweenGroups(doc, true);
                        }
                    }
                }
                else
                {
                    // Предупреждение                    
                    throw new Exception($"Неверный параметр '{opt.ParamKRMaterialName}' (GUID отличается от файла общих параметров.");
                }
            }
            if (!isFindGuid)
            {
                CreateDefinition(defFromSharedFile);
            }
        }

        public IEnumerable<DefinitionInfo> IterateParameters(Document doc, Predicate<DefinitionBindingMapIterator> predicate)
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
                        yield return new DefinitionInfo(it.Key, it.Current as ElementBinding);
                    }
                }
            }
        }

        private void DeleteParam(DefinitionInfo item)
        {
            doc.ParameterBindings.Remove(item.Definition);
        }

        /// <summary>
        /// Создание параметра из файла общих параметров
        /// </summary>        
        private void CreateDefinition(Definition defKRMaterial)
        {            
            // Привязка категорий
            var categories = uiApp.Application.Create.NewCategorySet();
            foreach (var catId in opt.Categories)
            {                
                categories.Insert(doc.Settings.Categories.get_Item(catId));
            }
            var binding = uiApp.Application.Create.NewInstanceBinding(categories);
            if (!doc.ParameterBindings.Insert(defKRMaterial, binding, BuiltInParameterGroup.PG_TEXT))
            {
                throw new Exception($"Не удалось создать параметр '{defKRMaterial.Name}' из файла общих параметров.");
            }
            var resDef = IterateParameters(doc, (p) => p.Key.Name == defKRMaterial.Name).First().Definition as InternalDefinition;
            resDef.SetAllowVaryBetweenGroups(doc, true);
        }

        private ExternalDefinition GetDefinitionFromSharedParameterFile()
        {
            var oldFile = uiApp.Application.SharedParametersFilename;
            try
            {
                uiApp.Application.SharedParametersFilename = opt.SharedParameterFile;
                // Определение параметра из файла общих параметров
                return uiApp.Application.OpenSharedParameterFile().Groups.First(g => g.Name == opt.ParamKRMaterialGroup).
                    Definitions.First(d => string.Equals(d.Name, opt.ParamKRMaterialName,
                        StringComparison.OrdinalIgnoreCase)) as ExternalDefinition;
            }
            catch (Exception ex)
            {
                throw new Exception($"Не найден параметр '{opt.ParamKRMaterialName}' в общем файле параметров. {ex.Message}");
            }
            finally
            {
                uiApp.Application.SharedParametersFilename = oldFile;
            }
        }        

        private Guid DefineGUID(string name)
        {
            // Найти этот параметр у любого элемента в моделе
            var param = FilterService.Filter(doc, opt.Categories).Where(e => !(e is ElementType)).First().LookupParameter(name);
            if (param == null)
            {
                throw new Exception($"Не найден параметр '{name}'");
            }
            return param.GUID;
        }

        /// <summary>
        /// Сравнение набора категорий в параметре текущего проекта с необходимым набором категорий
        /// </summary>        
        private bool CheckCategories(DefinitionInfo paramDefFind)
        {
            return opt.Categories.EqualsList(paramDefFind.Categories);
        }        
    }
}
