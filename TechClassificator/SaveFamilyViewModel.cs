using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static TechClassificator.Main;
using ZetaLongPaths;
using System;

namespace TechClassificator
{
    public class SaveFamilyViewModel : ObservableObject
    {
        public SaveFamilyViewModel()
        {

        }
        public Autodesk.Revit.DB.Document FamilyDoc { get; set; }

        private string _folderPath;
        public string FolderPath
        {
            get { return _folderPath; }
            set
            {
                if (_folderPath != value)
                {
                    _folderPath = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _familyName;
        public string FamilyName
        {
            get { return _familyName; }
            set
            {
                if (_familyName != value)
                {
                    _familyName = value;
                    OnPropertyChanged();
                }
            }
        }


        public SaveFamilyView View { get; set; }

        private ICommand _saveFamily;
        public ICommand SaveFamily
        {
            get
            {
                if (_saveFamily == null)
                {
                    _saveFamily = new RelayCommand(PerformSaveFamily);
                }

                return _saveFamily;
            }
        }

        private void PerformSaveFamily(object commandParameter)
        {
            //MessageBox.Show(FamilyName);
            try
            {
                FamilyDoc.SaveAs(FolderPath + "\\" + FamilyName + ".rfa");

                var docPath = FamilyDoc.PathName.ToString();

                var dir = docPath.Replace(FamilyDoc.Title + ".rfa", "");

                ZlpFileInfo[] st = ZlpIOHelper.GetFiles(dir, "*.00??.rfa");
                //TaskDialog.Show("Warn", dir);
                foreach (ZlpFileInfo fi in st)
                {
                    if (ZlpIOHelper.FileExists(fi.FullName))
                    {
                        try
                        {
                            ZlpIOHelper.DeleteFile(fi.FullName);
                        }
                        catch (System.IO.IOException e)
                        {
                            MessageBox.Show(e.ToString());

                        }
                    }
                }

                st = ZlpIOHelper.GetFiles(dir, "*.AS$");
                //TaskDialog.Show("Warn", dir);
                foreach (var fi in st)
                {
                    if (ZlpIOHelper.FileExists(fi.FullName))
                    {
                        try
                        {
                            ZlpIOHelper.DeleteFile(fi.FullName);
                        }
                        catch (System.IO.IOException e)
                        {
                            MessageBox.Show(e.ToString());

                        }
                    }
                }
                View.Close();

                MessageBox.Show($"Семейство успешно сохранено в:\n{FamilyDoc.PathName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }
    }
}
