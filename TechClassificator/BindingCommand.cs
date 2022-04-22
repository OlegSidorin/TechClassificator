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
using Autodesk.Revit.Attributes;
using System.Threading;
using static TechClassificator.Main;

namespace TechClassificator
{
    [Transaction(TransactionMode.Manual), Regeneration(RegenerationOption.Manual)]
    class BindingCommand : IExternalCommand
    {
        public string User { get; set; } = Environment.UserName.ToString();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            //string originalFile = app.SharedParametersFilename;

            string assemblyTablePath = ClassificatorPath;
            string log = "";
            bool isExist = false;

            var resourceReference = ExternalResourceReference.CreateLocalResource(doc, ExternalResourceTypes.BuiltInExternalResourceTypes.AssemblyCodeTable,
                ModelPathUtils.ConvertUserVisiblePathToModelPath(assemblyTablePath), PathType.Absolute) as ExternalResourceReference;
            using (Transaction t = new Transaction(doc, "Get Classificator"))
            {
                t.Start();
                AssemblyCodeTable.GetAssemblyCodeTable(doc).LoadFrom(resourceReference, new KeyBasedTreeEntriesLoadResults());
                t.Commit();
            }
            M1Parameter[] m1Parameters =
            {
                M1_PUT_K_SEMEISTVU, // 10
            };

            if (doc.IsFamilyDocument)
            {

                FamilyManager familyManager = doc.FamilyManager;
                FamilyType familyType = familyManager.CurrentType;
                FamilyParameterSet parametersList = familyManager.Parameters;

                #region check and delete wrong parameter
                foreach (M1Parameter m1p in m1Parameters)
                {
                    var p = familyManager.get_Parameter(new Guid(m1p.Guid));
                    if (p != null)
                    {
                        if (!(p.Definition.Name == m1p.Name))
                        {
                            using (Transaction tr = new Transaction(doc, $"delete parameter {m1p.Name}"))
                            {
                                tr.Start();
                                string str1 = p.Definition.Name;
                                familyManager.RemoveParameter(p);
                                TaskDialog.Show("Deleting parameters", $"Был удален общий параметр {m1p.Name}:" + str1);
                                tr.Commit();
                            }
                        }
                    }
                }
                #endregion

                #region check is family type is Annotation
                bool isAnnotation = false;
                try
                {

                    var annotationFamily = new FilteredElementCollector(doc).OfClass(typeof(Family)).Cast<Family>().ToList().FirstOrDefault();
                    if (annotationFamily.FamilyCategory.CategoryType.ToString() == "Annotation")
                    {
                        isAnnotation = true;
                    }
                    else
                    {
                        isAnnotation = false;
                    }

                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Check on Annotation", log + "\n" + ex.ToString());
                }
                #endregion

                #region check if no types in family
                if (familyType == null)
                {
                    using (Transaction t = new Transaction(doc, "create new type in family"))
                    {
                        t.Start();
                        familyType = familyManager.NewType(doc.Title);
                        familyManager.CurrentType = familyType;
                        t.Commit();
                    }
                }
                #endregion

                #region add parameters in family
                try
                {
                    commandData.Application.Application.SharedParametersFilename = FOPPath;
                    using (Transaction t = new Transaction(doc, "Add paramters"))
                    {
                        t.Start();
                        DefinitionFile sharedParametersFile = commandData.Application.Application.OpenSharedParameterFile();

                        Definition sharedParameterDefinition;
                        ExternalDefinition externalDefinition;
                        //parametersList = familyManager.Parameters;

                        string current = "";

                        isExist = false;

                        foreach (M1Parameter m1p in m1Parameters)
                        {
                            foreach (FamilyParameter fp in parametersList)
                            {
                                if (m1p.Name == fp.Definition.Name)
                                    isExist = true;
                            }
                            if (!isExist)
                            {
                                sharedParameterDefinition = sharedParametersFile.Groups.get_Item(m1p.GroupName).Definitions.get_Item(m1p.Name);
                                externalDefinition = sharedParameterDefinition as ExternalDefinition;
                                familyManager.AddParameter(externalDefinition, BuiltInParameterGroup.PG_ADSK_MODEL_PROPERTIES, m1p.IsInstance);
                                log += "\nДобавлен параметр <" + m1p.Name + ">";
                            }
                            isExist = false;
                        }
                        t.Commit();
                    }
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Adding parameters", log + "\n" + e.ToString());
                }
                #endregion

                #region set values to parameters in all familytypes
                FamilyTypeSet types = familyManager.Types;
                Guid famGuid = Guid.NewGuid();
                string pKeyValue = "";

                using (Transaction trans = new Transaction(doc, "Get Klassificator Cod"))
                {
                    trans.Start();
                    if (!isAnnotation)
                    {
                        var pKey = familyManager.get_Parameter("Код по классификатору");
                        pKeyValue = familyType.AsString(pKey);
                    }
                    trans.Commit();
                }

                //string libFolderPath = Main.GetLibFolder();

                foreach (FamilyType type in types)
                {
                    try
                    {
                        using (Transaction t = new Transaction(doc, "Set parameters to current type"))
                        {
                            t.Start();

                            familyManager.CurrentType = type;

                            #region set parameters with Classificator Cod
                            if (!isAnnotation)
                            {
                                try
                                {
                                    var p1 = familyManager.get_Parameter("Код по классификатору");
                                    familyManager.Set(p1, pKeyValue);
                                }
                                catch
                                {
                                    TaskDialog.Show("Ahtung", "Не назначен Код классификатора. \nДля того, что бы назначить " +
                                        "Код классификатора перейдите на вкладку Создание и в Типоразмерах в семействе выберите " +
                                        "соответстующий Код классификатора.");
                                }

                                var p = familyManager.get_Parameter(M1_PUT_K_SEMEISTVU.Name);
                                string pathToFam = TableEntry.GetPathToFamily(pKeyValue);
                                familyManager.Set(p, pathToFam);

                            }
                            #endregion

                            t.Commit();
                        }
                    }
                    catch (Exception e)
                    {
                        TaskDialog.Show("Set Parameters in Current Type", e.ToString());
                    }
                }

                #endregion
            }
            else
            {
                TaskDialog.Show("Блин, вот так ..", "Это не семейство, команда работает только в семействе");
            }

            return Result.Succeeded;
        }
    }
}
