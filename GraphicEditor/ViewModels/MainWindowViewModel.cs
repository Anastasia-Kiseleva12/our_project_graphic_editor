using System.Collections.ObjectModel;

namespace GraphicEditor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ILogic _figureService;
        private ObservableCollection<IFigure> _figures;
        private IFigure _selectedFigure;

    }
}
