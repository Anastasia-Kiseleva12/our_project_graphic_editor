using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;

namespace GraphicEditor.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> CreatePolylineCommand { get; }

        public FigureService _figureService;

        // Событие, которое будет вызываться при изменении коллекции
        public event Action FiguresChanged;

        public MainWindowViewModel()
        {
            _figureService = new FigureService();

            // Подписываемся на изменения коллекции фигур из FigureService
            _figureService._figures
                .Connect()
                .Subscribe(_ =>
                {
                    Debug.WriteLine("Figures collection changed!");
                    if (FiguresChanged != null)
                    {
                        Debug.WriteLine("FiguresChanged is not null, invoking.");
                        FiguresChanged.Invoke();
                    }
                    else
                    {
                        Debug.WriteLine("FiguresChanged is null!");
                    }
                });

            CreatePolylineCommand = ReactiveCommand.Create(CreateLine);
        }

        private void CreateLine()
        {
            var pointParameters = new Dictionary<string, Point>
            {
                { "First", new Point { X = 50, Y = 50 } },
                { "Second", new Point { X = 150, Y = 150 } }
            };

            var doubleParameters = new Dictionary<string, double>();

            // Создаем линию и добавляем её в коллекцию
            var line = _figureService.Create("Line", pointParameters, doubleParameters);
            _figureService.AddFigure(line); // Добавляем фигуру в SourceCache
        }
    }
}
