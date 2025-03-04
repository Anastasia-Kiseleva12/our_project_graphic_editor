using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using System.Linq;

namespace GraphicEditor.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private bool _isManualMode; //флаг для переключения режима отрисовки
        public bool IsManualMode
        {
            get => _isManualMode;
            set
            {
                Debug.WriteLine($"IsManualMode changed: {value}");
                this.RaiseAndSetIfChanged(ref _isManualMode, value);
            }
        }
        private Point? _startPoint; //временная точка для отрисовка
        private Point? _currentPoint;
        private bool _isDrawingLine;
        private bool _isDrawingCircle;

        public Point? StartPoint
        {
            get => _startPoint;
            set => this.RaiseAndSetIfChanged(ref _startPoint, value);
        }

        public Point? CurrentPoint
        {
            get => _currentPoint;
            set => this.RaiseAndSetIfChanged(ref _currentPoint, value);
        }

        public bool IsDrawingLine
        {
            get => _isDrawingLine;
            set => this.RaiseAndSetIfChanged(ref _isDrawingLine, value);
        }

        public bool IsDrawingCircle
        {
            get => _isDrawingCircle;
            set => this.RaiseAndSetIfChanged(ref _isDrawingCircle, value);
        }

        private bool _isCheckedLine;
        public bool IsCheckedLine
        {
            get => _isCheckedLine;
            set => this.RaiseAndSetIfChanged(ref _isCheckedLine, value);
        }

        private bool _isCheckedCircle;
        public bool IsCheckedCircle
        {
            get => _isCheckedCircle;
            set => this.RaiseAndSetIfChanged(ref _isCheckedCircle, value);
        }

        public ReactiveCommand<Unit, Unit> CreatePolylineCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateCircleCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveSelectedFiguresCommand { get; }
        public ReactiveCommand<IFigure, Unit> SelectFigureCommand { get; }
        public ReactiveCommand<IFigure, Unit> UnselectFigureCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadCommand { get; }

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

            RemoveSelectedFiguresCommand = ReactiveCommand.Create(RemoveSelectedFigures);

            SaveCommand = ReactiveCommand.Create(Save);
            LoadCommand = ReactiveCommand.Create(Load);

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
            if (IsDrawingLine)
            {
                if (StartPoint == null)
                {
                    StartPoint = point;
                    Debug.WriteLine($"Start point set at: {StartPoint}");
                }
                else
                {
                    var parameters = new Dictionary<string, Point>
                {
                    { "Start", StartPoint },
                    { "End", point }
                };
                    var doubleParameters = new Dictionary<string, double>();

                    var line = _figureService.Create("Line", parameters, doubleParameters);
                    _figureService.AddFigure(line);

                    Debug.WriteLine($"Line created: {StartPoint} -> {point}");

                    IsDrawingLine = false;
                    StartPoint = null;
                    CurrentPoint = null;

                    FiguresChanged?.Invoke();

                    IsCheckedLine = false;
                }
                return;
            }

            if (IsDrawingCircle)
            {
                if (StartPoint == null)
                {
                    StartPoint = point;
                    Debug.WriteLine($"Start point set at: {StartPoint}");
                }
                else
                {
                    var parameters = new Dictionary<string, Point>
                {
                    { "Center", StartPoint},
                    { "PointOnCircle", point }
                };
                    var doubleParameters = new Dictionary<string, double>();

                    var circle = _figureService.Create("Circle", parameters, doubleParameters);
                    _figureService.AddFigure(circle);

                    Debug.WriteLine($"Circle created: {StartPoint} -> {point}");

                    IsDrawingCircle = false;
                    StartPoint = null;
                    CurrentPoint = null;

                    FiguresChanged?.Invoke();

                    IsCheckedCircle = false;
                }
                return;
            }
            var eps = 80; //допустимая погрешность
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
        public void HandleCanvasMove(Point point)
        {
            if (IsDrawingLine || IsDrawingCircle)
            {
                CurrentPoint = point;
            }
        }
        private void RemoveSelectedFigures()
        {
            //удаляем все выделенные фигуры
            var selectedFigures = _figureService._selectedFigures.ToList();
            foreach (var figure in selectedFigures)
            {
                _figureService.RemoveFigure(figure);
            }

            FiguresChanged?.Invoke();
        }
        private void CreateLine()
        {
            if (IsManualMode) //создание либо дефолтной фигуры либо в ручную
            {

                Debug.WriteLine("Auto mode: Create default line.");
                var line = _figureService.CreateDefault("Line");
                _figureService.AddFigure(line);
                IsCheckedLine = false;
            }
            else
            {
                _isDrawingLine = !_isDrawingLine;
                _startPoint = null;
            }
        }

        private void CreateCircle()
        {
            if (IsManualMode)
            {
                Debug.WriteLine("Auto mode: Create default circle.");
                var circle = _figureService.CreateDefault("Circle");
                _figureService.AddFigure(circle);
                IsCheckedCircle = false;
            }
            else
            {

                _isDrawingCircle = !_isDrawingCircle;
                _startPoint = null;
            }
        }

        private void Save()
        {
            IO.SaveToFile(_figureService.Figures, "test.json");
        }

        private void Load()
        {
            IO.LoadFromFile(_figureService, "test.json");
        }
    }
}
