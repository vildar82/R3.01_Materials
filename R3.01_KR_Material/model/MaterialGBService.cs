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
                        continue;
                    if (elem.GetMaterialIds(false).Any(m => Regex.IsMatch(
                        ((Material)doc.GetElement(m)).Name,
                        Command.Options.ParamARMaterialValue, RegexOptions.IgnoreCase)))
                    {
                        SetMaterialGB(elem);
                        Debug.WriteLine($"SetMaterialGB - {elem}");                        
                    }
                }
                t.Commit();
            }
        }

        private static void SetMaterialGB(Element elem)
        {
            // Записать в параметр КР_Материал = ЖБ
            // Что если у элемента несколько параметров "КР_Материал"
            var pGB = elem.LookupParameter(Command.Options.ParamKRMaterialName);
            if (pGB == null)
            {
                // Не может быть
                Command.Error.AddErrorMesaage($"Элемент не содержит параметр '{Command.Options.ParamKRMaterialName}' - '{elem.Name}'", elem);
            }
            else if (pGB.AsString() != Command.Options.ParamKRMaterialValue)
            {
                try
                {
                    pGB.Set(Command.Options.ParamKRMaterialValue);
                }
                catch (Exception ex)
                {
                    Command.Error.AddErrorMesaage($"Ошибка установки параметра '{Command.Options.ParamKRMaterialName}'='{Command.Options.ParamKRMaterialValue}' в элемент {elem.Name}. {ex.Message}", elem);
                }
            }
        }
    }    
}