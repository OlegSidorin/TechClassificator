using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Threading;
using static TechClassificator.Main;

namespace TechClassificator
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class ClearCommand : IExternalCommand
    {
        public string User { get; set; } = Environment.UserName.ToString();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            //string originalFile = app.SharedParametersFilename;
            string fopFilePath = Main.FOPPath;
            string log = "";
            bool isExist = false;

            M1Parameter[] m1CommonParameters =
            {
                M1_PUT_K_SEMEISTVU // 10
            };

            if (doc.IsFamilyDocument)
            {
                FamilyManager familyManager = doc.FamilyManager;
                FamilyType familyType;
                familyType = familyManager.CurrentType;
                if (familyType == null)
                {
                    using (Transaction t = new Transaction(doc, "change"))
                    {
                        t.Start();
                        familyType = familyManager.NewType("Тип 1");
                        familyManager.CurrentType = familyType;
                        t.Commit();
                    }
                }

                #region clear 
                //TaskDialog.Show("Warning", "Privet");
                try
                {
                    commandData.Application.Application.SharedParametersFilename = fopFilePath;
                    using (Transaction t = new Transaction(doc, "Clear"))
                    {
                        t.Start();
                        FamilyParameterSet parametersList = familyManager.Parameters;
                        foreach (M1Parameter m1p in m1CommonParameters)
                        {
                            try
                            {
                                var p = familyManager.get_Parameter(new Guid(m1p.Guid));
                                familyManager.RemoveParameter(p);
                            }
                            catch
                            {

                            }
                        }
                        t.Commit();
                    }
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Clearing", e.ToString());
                }

                #endregion

            }
            else
            {
                TaskDialog.Show("Опа", "Это не семейство, команда работает только в семействе");
            }

            //TaskDialog.Show("Final", log);
            return Result.Succeeded;
        }
    }


}
