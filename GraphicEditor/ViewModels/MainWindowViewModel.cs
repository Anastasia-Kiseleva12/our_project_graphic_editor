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
        public ReactiveCommand<Unit, Unit> CreateCircleCommand { get; }
        public ReactiveCommand<IFigure, Unit> SelectFigureCommand { get; }
        public ReactiveCommand<IFigure, Unit> UnselectFigureCommand { get; }

        private IFigure _selectedFigure;
        public IFigure SelectedFigure
        {

            get => _selectedFigure;
            set
            {
                Debug.WriteLine($"SelectedFigure changed from {_selectedFigure?.Name} to {value?.Name}");
                this.RaiseAndSetIfChanged(ref _selectedFigure, value); }
        }

        public FigureService _figureService;

        //событие, которое будет вызываться при изменении коллекции
        public event Action FiguresChanged;

        public MainWindowViewModel()
        {
            _figureService = new FigureService();

            //подписываемся на изменения коллекции фигур из FigureService
            _figureService._figures
                .Connect()
                .Subscribe(_ =>
                {
                    if (FiguresChanged != null)
                    {
                        FiguresChanged.Invoke();
                    }
                    else
                    {
                        Debug.WriteLine("FiguresChanged is null!");
                    }
                });

            CreatePolylineCommand = ReactiveCommand.Create(CreateLine);

            CreateCircleCommand = ReactiveCommand.Create(CreateCircle);

            SelectFigureCommand = ReactiveCommand.Create<IFigure>(figure =>
            {
                _figureService.Select(figure);
                SelectedFigure = figure; 
            });

            UnselectFigureCommand = ReactiveCommand.Create<IFigure>(figure =>
            {
                _figureService.UnSelect(figure);

                if (figure == null || SelectedFigure == figure)
                {
                    SelectedFigure = null;
                }
            });


        }
        public void HandleCanvasClick(Point point)
        {
            var eps = 45; //допустимая погрешность
            var figure = _figureService.Find(new Point { X = point.X, Y = point.Y }, eps);

            if (figure != null)
            {
                Debug.WriteLine($"Figure found: {figure.Name}");
                if (_selectedFigure == figure)
                {
                    //если кликнули на уже выделенную фигуру снимаем выделение
                    UnselectFigureCommand.Execute(figure).Subscribe();
                }
                else
                {
                    //если кликнули на другую фигуру выделяем её
                    SelectFigureCommand.Execute(figure).Subscribe();
                }
            }
            else
            {
                //если кликнули на пустую область снимаем выделение со всех фигур
                UnselectFigureCommand.Execute(null).Subscribe();
            }

            FiguresChanged?.Invoke();
        }
        private void CreateLine()
        {
            var pointParameters = new Dictionary<string, Point>
            {
                { "First", new Point { X = 50, Y = 50 } },
                { "Second", new Point { X = 150, Y = 150 } }
            };

            var doubleParameters = new Dictionary<string, double>();

            var line = _figureService.Create("Line", pointParameters, doubleParameters);
            _figureService.AddFigure(line);
        }
        private void CreateCircle()
        {
            var pointParameters = new Dictionary<string, Point>
            {
                { "Center",  new Point { X = 250, Y = 250 } },
                { "PointOnCircle", new Point { X = 200, Y = 200 } }
            };

            var doubleParameters = new Dictionary<string, double>();

            var circle = _figureService.Create("Circle", pointParameters, doubleParameters);
            _figureService.AddFigure(circle); // Добавляем фигуру в SourceCache
        }
    }
}
