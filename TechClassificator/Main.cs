using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TechClassificator
{
    class Main : IExternalApplication
    {
        //public static string Folder { get; set; }
        public static string TabName { get; set; } = "M1-IOS";
        public static string PanelSortName { get; set; } = "Классификатор семейств ТХ";
        public static string Comment { get; set; } = " ";
        public static string FOPPath { get; set; } = @"\\ukkalita.local\iptg\Строительно-девелоперский дивизион\М1 Проект\Проекты\10. Отдел информационного моделирования\01. REVIT\00. ФОП\ФОП2019.txt";
        public static string ClassificatorPath { get; set; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\res\\Классификатор ТХ.txt";
        public static string User { get; set; } = Environment.UserName.ToString();
        public static M1Parameter M1_PUT_K_SEMEISTVU { get; set; }
        public static void SetM1Parameters()
        {
            
            M1_PUT_K_SEMEISTVU = new M1Parameter()
            {
                Name = "М1_Путь к семейству",
                Guid = "b8764317-5cd5-487a-ad9c-d2622c551bb2",
                GroupNumber = "23",
                GroupName = "21 Классификатор семейств",
                IsInstance = false
            };
           
        }

        public Result OnStartup(UIControlledApplication application)
        {
            List<RibbonPanel> panelList = new List<RibbonPanel>();

            try { panelList = application.GetRibbonPanels(TabName); } catch { }

            if (panelList.Count == 0) application.CreateRibbonTab(TabName);

            panelList = application.GetRibbonPanels(TabName);

            RibbonPanel panelSort = null;

            bool isPanelSortExists = false;

            foreach (var panel in panelList)
            {
                if (panel.Name.Equals(PanelSortName))
                {
                    isPanelSortExists = true;
                    panelSort = panel;
                }
            }

            if (!isPanelSortExists) panelSort = application.CreateRibbonPanel(TabName, PanelSortName);

            string path = Assembly.GetExecutingAssembly().Location;
            //Folder = GetLibFolder();

            var ClassBtnData = new PushButtonData("ClassBtnData", "Назначить\nклассификатор", path, "TechClassificator.ClassCommand")
            {
                ToolTipImage = new BitmapImage(new Uri(Path.GetDirectoryName(path) + "\\res\\classificator-th-32.png", UriKind.Absolute)),
                ToolTip = "Подгружает файл классификатора для назначения кода классификатора на семейство"
            };
            var ClassBtn = panelSort.AddItem(ClassBtnData) as PushButton;
            ClassBtn.LargeImage = new BitmapImage(new Uri(Path.GetDirectoryName(path) + "\\res\\classificator-th-32.png", UriKind.Absolute));

            var AddParamsBtnData = new PushButtonData("AddParamsBtnData", "Добавить\nпараметры", path, "TechClassificator.BindingCommand")
            {
                ToolTipImage = new BitmapImage(new Uri(Path.GetDirectoryName(path) + "\\res\\parameters-plus-32.png", UriKind.Absolute)),
                ToolTip = "Добавляет общие параметры в семейство, которые позволят его каталогизировать"
            };
            //var AddParamsBtn = panelSort.AddItem(AddParamsBtnData) as PushButton;
            AddParamsBtnData.LargeImage = new BitmapImage(new Uri(Path.GetDirectoryName(path) + "\\res\\parameters-plus-32.png", UriKind.Absolute));

            var ClearBtnData = new PushButtonData("ClearBtnData", "Очистить\nсемейство", path, "TechClassificator.ClearCommand")
            {
                ToolTipImage = new BitmapImage(new Uri(Path.GetDirectoryName(path) + "\\res\\duster-32.png", UriKind.Absolute)),
                ToolTip = "Удаляются общие параметры из семейства из группы Свойства модели"
            };
            //var ClearBtn = panelSort.AddItem(ClearBtnData) as PushButton;
            ClearBtnData.LargeImage = new BitmapImage(new Uri(Path.GetDirectoryName(path) + "\\res\\duster-32.png", UriKind.Absolute));
            /*
            var FolderBtnData = new PushButtonData("FolderBtnData", "Расположение\nбиблиотеки", path, "TechClassificator.FolderCommand")
            {
                ToolTipImage = new BitmapImage(new Uri(Path.GetDirectoryName(path) + "\\res\\folder-32.png", UriKind.Absolute)),
                ToolTip = "Установить расположение библиотеки семейств"
            };
            //var FolderBtn = panelSort.AddItem(FolderBtnData) as PushButton;
            FolderBtnData.LargeImage = new BitmapImage(new Uri(Path.GetDirectoryName(path) + "\\res\\folder-32.png", UriKind.Absolute));
            */
            SplitButtonData sBtnData = new SplitButtonData("splitButton", "Split");
            SplitButton sBtn = panelSort.AddItem(sBtnData) as SplitButton;
            sBtn.AddPushButton(AddParamsBtnData);
            sBtn.AddPushButton(ClearBtnData);
            //sBtn.AddPushButton(FolderBtnData);

            var SaveBtnData = new PushButtonData("SaveBtnData", "Сохранить\nв библиотеку", path, "TechClassificator.SaveCommand")
            {
                ToolTipImage = new BitmapImage(new Uri(Path.GetDirectoryName(path) + "\\res\\save-32.png", UriKind.Absolute)),
                ToolTip = "Сохраняет семейство в библиотеку"
            };
            var SaveBtn = panelSort.AddItem(SaveBtnData) as PushButton;
            SaveBtn.LargeImage = new BitmapImage(new Uri(Path.GetDirectoryName(path) + "\\res\\save-32.png", UriKind.Absolute));

            SetM1Parameters();

            return Result.Succeeded;
        }
        //public static string GetLibFolder()
        //{
        //    string path = Assembly.GetExecutingAssembly().Location;
        //    string folder = "";
        //    string familyFolderPath = path.Replace($@"\{Assembly.GetExecutingAssembly().GetName().Name}.dll", "") + @"\res\folderTHO.txt";
        //    //System.Windows.MessageBox.Show($@"\{Assembly.GetExecutingAssembly().GetName().Name}.dll");
        //    //Open the file to read from.
        //    using (StreamReader sr = File.OpenText(familyFolderPath))
        //    {
        //        string s;
        //        while ((s = sr.ReadLine()) != null)
        //        {
        //            folder += s;
        //        }
        //    }
        //    return folder;
        //}
        //public static void SetLibFolder(string text)
        //{
        //    string path = Assembly.GetExecutingAssembly().Location;
        //    string folder = "";
        //    string familyFolderPath = path.Replace($@"\{Assembly.GetExecutingAssembly().GetName().Name}.dll", "") + @"\res\folderTHO.txt";

        //    if (File.Exists(familyFolderPath))
        //    {
        //        // Create a file to write to.
        //        using (StreamWriter sw = File.CreateText(familyFolderPath))
        //        {
        //            sw.Write(text);
        //        }
        //    }
        //}

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }


    }
}
