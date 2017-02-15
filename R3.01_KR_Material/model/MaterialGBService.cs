using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Revit_Lib.Extensions;
using Autodesk.Revit.UI;
using Revit_Message;

namespace R3_01_KR_Material
{
    public class MaterialGBService
    {
        UIApplication uiApp;
        Options opt;
        Document doc;
        Error err;
        public MaterialGBService(UIApplication uiApp, Options opt, Error err)
        {
            this.err = err;
            this.uiApp = uiApp;
            this.opt = opt;
            doc = uiApp.ActiveUIDocument.Document;
        }

        public void SetParameters()
        {            
            var elements = FilterService.Filter(doc, opt.Categories);
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
                        SetParam(elem, opt.ParamKRMaterialValue);
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

        private bool HasGBMaterial(Document doc, Element elem)
        {
            // Просмотр среди материалов
            var maters = elem.GetMaterialIds(false);
            if (maters.Any(m => IsMaterialGB(((Material)doc.GetElement(m)).Name)))
            {
                return true;
            }
            // Провсмотр среди параметров, элемента и его типа
            if (HasGBMaterialInParameters(elem))
            {
                return true;
            }
            return HasGBMaterialInParameters(doc.GetElement(elem.GetTypeId()));
        }

        private bool HasGBMaterialInParameters(Element elem)
        {
            if (elem == null) return false;
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

        private bool IsMaterialGB(string name)
        {
            return Regex.IsMatch(name, opt.ParamARMaterialValue, RegexOptions.IgnoreCase);
        }

        private void SetParam(Element elem, string value)
        {
            // Записать в параметр КР_Материал = ЖБ              
            var pGB = elem.LookupParameter(opt.ParamKRMaterialName);
            if (pGB == null)
            {
                // Не может быть!
                err.AddErrorMesaage($"Элемент не содержит параметр '{opt.ParamKRMaterialName}' - '{elem.Name}'", elem);
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
                    err.AddErrorMesaage($"Ошибка установки параметра '{opt.ParamKRMaterialName}'='{opt.ParamKRMaterialValue}'. {ex.Message}", elem);
                }
            }
        }
    }    
}