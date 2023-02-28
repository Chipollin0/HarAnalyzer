using HarAnalyzer.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace HarAnalyzer.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private HarLog harLog;
        private HarEntry selectedHarEntry;
        private string fileName;

        #region Constructors

        public MainViewModel()
        {
            harLog = new HarLog();
            selectedHarEntry = new HarEntry();
            fileName = string.Empty;
            Error = string.Empty;
        }

        #endregion

        #region Properties

        public HarLog HarLog
        {
            get { return harLog; }
            set
            {
                if (harLog == value)
                    return;
                harLog = value;
                OnPropertyChanged(nameof(HarLog));
            }
        }

        public HarEntry SelectedHarEntry
        {
            get { return selectedHarEntry; }
            set
            {
                if (selectedHarEntry == value)
                    return;
                selectedHarEntry = value;
                OnPropertyChanged(nameof(selectedHarEntry));
            }
        }

        public string FileName
        {
            get { return fileName; }
            set
            {
                if (fileName == value)
                    return;
                fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        #endregion

        #region Actions

        public void LoadHarFile()
        {
            var text = File.ReadAllText(fileName);
            var harLog = HarParser.Parse(text);
            HarLog = harLog.Result;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(property));
        }

        #endregion

        #region IDataErrorInfo

        public string Error { get; }
        public string this[string propertyName]
        {
            get
            {
                //if (propertyName == nameof(LoggerUser))
                //    return ValidateUser();

                return null;
            }
        }

        #endregion
    }
}
