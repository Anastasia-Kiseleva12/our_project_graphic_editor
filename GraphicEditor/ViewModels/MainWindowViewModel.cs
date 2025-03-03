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
        private bool _isDrawingLine = false; // Флаг ожидания для ручной отрисовка
        private bool _isDrawingCircle = false;
        private Point? _startPoint;
        public ReactiveCommand<Unit, Unit> CreatePolylineCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateCircleCommand { get; }
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
            if(_isDrawingLine) //типа ручная отрисовка, надо вынести в отдельный метод.
            {
                if (_startPoint == null)
                {
                    _startPoint = point;
                    Debug.WriteLine($"Start point set at: {_startPoint}");
                }
                else
                {
                    var parameters = new Dictionary<string, Point>
                    {
                    { "Start", _startPoint },
                    { "End", point }
                    };
                    var doubleParameters = new Dictionary<string, double>();

                    var line = _figureService.Create("Line", parameters, doubleParameters);
                    _figureService.AddFigure(line);

                    Debug.WriteLine($"Line created: {_startPoint} -> {point}");

                    _isDrawingLine = false;
                    _startPoint = null;

                    FiguresChanged?.Invoke();
                }
                return;
            }
            if (_isDrawingCircle) // вынести в отдельный метод
            {
                if (_startPoint == null)
                {
                    _startPoint = point;
                    Debug.WriteLine($"Start point set at: {_startPoint}");
                }
                else
                {
                    var parameters = new Dictionary<string, Point>
                    {
                    { "Center", _startPoint },
                    { "PointOnCircle", point }
                    };
                    var doubleParameters = new Dictionary<string, double>();

                    var line = _figureService.Create("Circle", parameters, doubleParameters);
                    _figureService.AddFigure(line);

                    Debug.WriteLine($"Circle created: {_startPoint} -> {point}");

                    _isDrawingCircle = false;
                    _startPoint = null;

                    FiguresChanged?.Invoke();
                }
                return;
            }
            var eps = 145; //допустимая погрешность
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
            if (IsManualMode) //создание либо дефолтной фигуры либо в ручную
            {

                Debug.WriteLine("Auto mode: Create default line.");
                var line = _figureService.CreateDefault("Line");
                _figureService.AddFigure(line);
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
