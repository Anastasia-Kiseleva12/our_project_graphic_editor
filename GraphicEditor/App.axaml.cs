using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GraphicEditor.ViewModels;
using GraphicEditor.Views;

namespace GraphicEditor
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var viewModel = new MainWindowViewModel();
                var mainWindow = new MainWindow(viewModel)
                {
                    DataContext = viewModel,
                };

                desktop.MainWindow = mainWindow;

                Debug.WriteLine($"DataContext set to MainWindowViewModel: {viewModel.GetHashCode()}");
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
