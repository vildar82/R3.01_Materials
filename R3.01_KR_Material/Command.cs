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
        public static UIApplication UiApp { get; private set; }
        public static Options Options { get; private set; }
        public static Error Error { get; private set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet errElements)
        {
            try
            {
                UiApp = commandData.Application;
                Options = new Options();
                Error = new Error();
                var doc = commandData.Application.ActiveUIDocument.Document;

                // Проверка определения параметра в проекте                      
                DefinitionService.CheckDefinition();

                // Установка параметра ЖБ для элементов
                MaterialGBService.SetParameters();

                if (Error.IsError)
                {
                    Error.Show(commandData.Application);
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
            finally
            {
                UiApp = null;
                Options = null;
                Error = null;
            }        
        }        
    }
}
