﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3_01_Materials
{
    public class Options
    {
        public string ParamKRMaterialName { get; set; } = "KR_Материал";
        public string ParamKRMaterialValue { get; set; } = "ЖБ";
        public string ParamARMaterialValue { get; set; } = "ЖЕЛЕЗОБЕТОН";        
        public string SharedParameterFile { get; set; } = @"\\dsk2.picompany.ru\project\CAD_Settings\Revit_server\04. Shared Parameters\SP-BS-PIC(2).txt"; 
        public List<BuiltInCategory> Categories { get; set; } = new List<BuiltInCategory> {
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_StructuralFraming,
            BuiltInCategory.OST_Columns,
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_Roofs,
            BuiltInCategory.OST_Stairs,
            BuiltInCategory.OST_GenericModel,
            BuiltInCategory.OST_Ramps,
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_StructuralFoundation
        };
    }
}