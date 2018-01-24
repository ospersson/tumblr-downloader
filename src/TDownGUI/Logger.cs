using System;
using System.ComponentModel;
using System.Text;

namespace TDownGUI
{
    public class Logger : ILogger, INotifyPropertyChanged
    {
        private static StringBuilder _logText = new StringBuilder();

        public string LogText
        {
            get
            {
                return _logText.ToString();
            }
            set
            {
                _logText.AppendFormat(value + Environment.NewLine);
                OnPropertyChanged("LogText");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
