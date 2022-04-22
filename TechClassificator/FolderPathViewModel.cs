using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static TechClassificator.Main;

namespace TechClassificator
{
    public class FolderPathViewModel : ObservableObject
    {
        
        public FolderPathViewModel()
        {
            //FolderPath = GetLibFolder();
        }
        public string FolderPath { get; set; }
        public FolderPathView View {get; set;}

        private ICommand _saveNewPathToFile;
        public ICommand SaveNewPathToFile
        {
            get
            {
                if (_saveNewPathToFile == null)
                {
                    _saveNewPathToFile = new RelayCommand(PerformSaveNewPathToFile);
                }

                return _saveNewPathToFile;
            }
        }

        private void PerformSaveNewPathToFile(object commandParameter)
        {
            //SetLibFolder(FolderPath);
            View.Close();
        }
    }
}
