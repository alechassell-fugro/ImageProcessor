using System.ComponentModel;

namespace ImageProcessor
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public string Title { get; set; } = "Image Processor";

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}