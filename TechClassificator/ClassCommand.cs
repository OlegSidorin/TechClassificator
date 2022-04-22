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
    class ClassCommand : IExternalCommand
    {
        public string User { get; set; } = Environment.UserName.ToString();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            //string path = Assembly.GetExecutingAssembly().Location;
            string assemblyTablePath = ClassificatorPath;
            string str = "";


            var resourceReference = ExternalResourceReference.CreateLocalResource(doc, ExternalResourceTypes.BuiltInExternalResourceTypes.AssemblyCodeTable,
                ModelPathUtils.ConvertUserVisiblePathToModelPath(assemblyTablePath), PathType.Absolute) as ExternalResourceReference;

            using (Transaction t = new Transaction(doc, "Apply Classifikator"))
            {
                t.Start();
                AssemblyCodeTable.GetAssemblyCodeTable(doc).LoadFrom(resourceReference, new KeyBasedTreeEntriesLoadResults());
                t.Commit();
            }

            AssemblyCodeTable act = AssemblyCodeTable.GetAssemblyCodeTable(doc);
            var kbte = act.GetKeyBasedTreeEntries();

            foreach (var item in kbte)
            {
                str += item.Key + ", ";
            }

            //TaskDialog.Show("Final", "Файл классификатора загружен" + "\n" + assemblyTablePath + "\n - - - - - -" + str);

            return Result.Succeeded;
        }
    }
}
