using System.ComponentModel;
using System.Windows.Input;

namespace ImageProcessor
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public string Title { get; set; } = "Image Processor";

        public ICommand ChooseImageCommand => new Command(ChooseImage);

        private void ChooseImage()
        {
            // Implementation for choosing an image goes here
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}