using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace R3_01_KR_Material
{
    public class MaterialGBService
    {       
        public static void SetParameters()
        {
            var doc = Command.UiApp.ActiveUIDocument.Document;
            var elements = FilterService.Filter();
            using (var t = new Transaction(doc, "Установка параметра ЖБ"))
            {
                t.Start();
                foreach (var elem in elements)
                {
                    if (elem is ElementType)
                    {
                        Debug.WriteLine($"Пропущен элемент - {elem}");
                        continue;
                    }

                    if (HasGBMaterial(doc, elem))
                    {
                        SetParam(elem, Command.Options.ParamKRMaterialValue);
                        Debug.WriteLine($"SetMaterialGB - {elem}");
                    }
                    else
                    {
                        SetParam(elem, null);
                    }
                }
                t.Commit();
            }
        }

        private static bool HasGBMaterial(Document doc, Element elem)
        {
            var maters = elem.GetMaterialIds(false);
            if (!maters.Any())
            {
                // Нет материалов в элементе - искать в параметрах типа
                return HasGBMaterialInParameters(doc.GetElement(elem.GetTypeId()));                                                
            }
            return maters.Any(m => IsMaterialGB(((Material)doc.GetElement(m)).Name));
        }

        private static bool HasGBMaterialInParameters(Element elem)
        {
            foreach (Parameter item in elem.Parameters)
            {
                if (item.Definition.ParameterType == ParameterType.Material &&
                    item.HasValue &&
                    IsMaterialGB(item.AsValueString()))
                {
                    return true;
                }
            }
            return false;            
        }

        private static bool IsMaterialGB(string name)
        {
            return Regex.IsMatch(name, Command.Options.ParamARMaterialValue, RegexOptions.IgnoreCase);
        }

        private static void SetParam(Element elem, string value)
        {
            // Записать в параметр КР_Материал = ЖБ
            // Что если у элемента несколько параметров "КР_Материал"
            var pGB = elem.LookupParameter(Command.Options.ParamKRMaterialName);
            if (pGB == null)
            {
                // Не может быть!
                Command.Error.AddErrorMesaage($"Элемент не содержит параметр '{Command.Options.ParamKRMaterialName}' - '{elem.Name}'", elem);
            }
            else if ((pGB.HasValue && value == null) ||
                pGB.AsString() != value)
            {                
                try
                {
                    pGB.Set(value);
                }
                catch (Exception ex)
                {
                    Command.Error.AddErrorMesaage($"Ошибка установки параметра '{Command.Options.ParamKRMaterialName}'='{Command.Options.ParamKRMaterialValue}' в элемент {elem.Name}. {ex.Message}", elem);
                }
            }
        }
    }    
}