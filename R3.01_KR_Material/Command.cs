using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Revit_Message;

namespace R3_01_KR_Material
{
    /// <summary>
    /// Проверка параметра "КР_Материал" у семейств и запись этого параметра в элементы если у них материал Железобетон
    /// </summary>
    [Transaction( TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet errElements)
        {
            try
            {
                var uiApp = commandData.Application;
                var options = new Options();
                var errors = new Error();
                var doc = commandData.Application.ActiveUIDocument.Document;

                // Проверка определения параметра в проекте                      
                var checkDef = new DefinitionService(uiApp, options, errors);
                checkDef.CheckDefinition();

                // Установка параметра ЖБ для элементов
                var materGBserv = new MaterialGBService(uiApp, options, errors);
                materGBserv.SetParameters();

                if (errors.IsError)
                {
                    errors.Show(commandData.Application);
                }
                else
                {
                    TaskDialog.Show(options.ParamKRMaterialName, $"Значение параметра '{options.ParamKRMaterialName}' обновлено");
                }                
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }            
        }        
    }
}
