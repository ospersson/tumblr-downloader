using System.ComponentModel;

namespace TDownGUI
{
    public interface ILogger
    {
        string LogText { get; set; }
        event PropertyChangedEventHandler PropertyChanged;
    }
}
