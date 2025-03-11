using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using System.Linq;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Platform.Storage;
using Avalonia.Controls.ApplicationLifetimes;
using Splat;

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
        private bool _isDrawingTriangle;
        private bool _isDrawingRectangle;

        public Point? StartPoint
        {
            get => _startPoint;
            set => this.RaiseAndSetIfChanged(ref _startPoint, value);
        }
        private Point? _secondPoint;
        public Point? SecondPoint
        {
            get => _secondPoint;
            set => this.RaiseAndSetIfChanged(ref _secondPoint, value);
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

        public bool IsDrawingTriangle
        {
            get => _isDrawingTriangle;
            set => this.RaiseAndSetIfChanged(ref _isDrawingTriangle, value);
        }

        public bool IsDrawingRectangle
        {
            get => _isDrawingRectangle;
            set => this.RaiseAndSetIfChanged(ref _isDrawingRectangle, value);
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

        private bool _isCheckedTriangle;
        public bool IsCheckedTriangle
        {
            get => _isCheckedTriangle;
            set => this.RaiseAndSetIfChanged(ref _isCheckedTriangle, value);
        }

        private bool _isCheckedRectangle;
        public bool IsCheckedRectangle
        {
            get => _isCheckedRectangle;
            set => this.RaiseAndSetIfChanged(ref _isCheckedRectangle, value);
        }

        public ReactiveCommand<Unit, Unit> CreatePolylineCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateCircleCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateTriangleCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateRectangleCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveSelectedFiguresCommand { get; }
        public ReactiveCommand<IFigure, Unit> SelectFigureCommand { get; }
        public ReactiveCommand<IFigure, Unit> UnselectFigureCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveAsCommand { get; }
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

            CreateTriangleCommand = ReactiveCommand.Create(CreateTriangle);

            CreateRectangleCommand = ReactiveCommand.Create(CreateRectangle);

            RemoveSelectedFiguresCommand = ReactiveCommand.Create(RemoveSelectedFigures);

            SaveCommand = ReactiveCommand.Create(Save);
            SaveAsCommand = ReactiveCommand.CreateFromTask(SaveAs);
            LoadCommand = ReactiveCommand.CreateFromTask(Load);

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
        private void CreateDefaultFigure(string figureType, Action<bool> setIsChecked)
        {
            Debug.WriteLine($"Auto mode: Create default {figureType}.");
            var figure = _figureService.CreateDefault(figureType);
            _figureService.AddFigure(figure);
            setIsChecked(false); // Сбрасываем флаг IsChecked
        }
        private void CreateFigure(string figureType, Dictionary<string, Point> parameters, Dictionary<string, double> doubleParameters)
        {
            var figure = _figureService.Create(figureType, parameters, doubleParameters);
            _figureService.AddFigure(figure);
            FiguresChanged?.Invoke();
        }
        public void SelectFigure(IFigure figure)
        {
            if (figure == null)
                return;

            // Снимаем выделение с предыдущей фигуры (если есть)
            if (_selectedFigure != null)
            {
                _selectedFigure.IsSelected = false;
                _selectedFigure = null;
            }

            // Выделяем новую фигуру
            _selectedFigure = figure;
            _selectedFigure.IsSelected = true;

            // Уведомляем об изменении
            this.RaisePropertyChanged(nameof(SelectedFigure));
            FiguresChanged?.Invoke(); // Обновляем отрисовку
        }
        public void HandleCanvasClick(Point point)
        {
            if (IsDrawingLine)
            {
                HandleLineClick(point);
                return;
            }

            if (IsDrawingCircle)
            {
                HandleCircleClick(point);
                return;
            }

            if (IsDrawingTriangle)
            {
                HandleTriangleClick(point);
                return;
            }
            HandleFigureSelection(point);
        }
        private void HandleLineClick(Point point)
        {
            if (StartPoint == null)
            {
                StartPoint = point;
                Debug.WriteLine($"Start point set at: {StartPoint}");
            }
            else
            {
                var pointParameters = new Dictionary<string, Point>
                {
                    { "Start", StartPoint },
                    { "End", point }
                };
                var doubleParameters = new Dictionary<string, double>
                {
                    { "StrokeThickness", 2}
                };

                CreateFigure("Line", pointParameters, doubleParameters);

                IsDrawingLine = false;
                StartPoint = null;
                CurrentPoint = null;
                IsCheckedLine = false;
            }
        }

        private void HandleCircleClick(Point point)
        {
            if (StartPoint == null)
            {
                StartPoint = point;
                Debug.WriteLine($"Start point set at: {StartPoint}");
            }
            else
            {
                var pointParameters = new Dictionary<string, Point>
                {
                    { "Center", StartPoint },
                    { "PointOnCircle", point }
                };
                var doubleParameters = new Dictionary<string, double>
                {
                    { "StrokeThickness", 2}
                };
                CreateFigure("Circle", pointParameters, doubleParameters);

                IsDrawingCircle = false;
                StartPoint = null;
                CurrentPoint = null;
                IsCheckedCircle = false;
            }
        }

        private void HandleTriangleClick(Point point)
        {
            if (StartPoint == null)
            {
                StartPoint = point;
                Debug.WriteLine($"Start point set at: {StartPoint}");
            }
            else if (SecondPoint == null)
            {
                SecondPoint = point;
                Debug.WriteLine($"Second point set at: {SecondPoint}");
            }
            else
            {
                var pointParameters = new Dictionary<string, Point>
                {
                    { "P1", StartPoint },
                    { "P2", SecondPoint },
                    { "P3", point }
                };
                var doubleParameters = new Dictionary<string, double>
                {
                    { "StrokeThickness", 2}
                };
                CreateFigure("Triangle", pointParameters, doubleParameters);

                IsDrawingTriangle = false;
                StartPoint = null;
                SecondPoint = null;
                CurrentPoint = null;
                IsCheckedTriangle = false;
            }
        }

        private void HandleFigureSelection(Point point)
        {
            var eps = 80; // допустимая погрешность
            var figure = _figureService.Find(new Point (point.X, point.Y), eps);
            //Предыдущее решение, пока не удаляю, может понадобится
            //if (figure != null)
            //{
            //    Debug.WriteLine($"Figure found: {figure.Name}");
            //    if (_selectedFigure == figure)
            //    {
            //       UnselectFigureCommand.Execute(figure).Subscribe();
            //    }
            //    else
            //    {
            //        SelectFigureCommand.Execute(figure).Subscribe();
            //    }
            //}
            //else
            //{
            //    UnselectFigureCommand.Execute(null).Subscribe();
            //}

            //FiguresChanged?.Invoke();
            if (figure != null)
            {
                Debug.WriteLine($"Figure found: {figure.Name}");
                if (_selectedFigure == figure)
                {
                    // Снимаем выделение с текущей фигуры
                    _selectedFigure.IsSelected = false;
                    _selectedFigure = null;
                }
                else
                {
                    // Снимаем выделение с предыдущей фигуры (если есть)
                    if (_selectedFigure != null)
                    {
                        _selectedFigure.IsSelected = false;
                    }

                    // Выделяем новую фигуру
                    _selectedFigure = figure;
                    _selectedFigure.IsSelected = true;
                }
            }
            else
            {
                // Снимаем выделение с текущей фигуры (если есть)
                if (_selectedFigure != null)
                {
                    _selectedFigure.IsSelected = false;
                    _selectedFigure = null;
                }
            }

            FiguresChanged?.Invoke();
        }
        public void HandleCanvasMove(Point point)
        {
            if (IsDrawingLine || IsDrawingCircle || IsDrawingTriangle)
            {
                CurrentPoint = point;
            }
        }      
        private void CreateLine()
        {
            if (IsManualMode)
            {
                CreateDefaultFigure("Line", value => IsCheckedLine = value);
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
                CreateDefaultFigure("Circle", value => IsCheckedCircle = value);
            }
            else
            {
                _isDrawingCircle = !_isDrawingCircle;
                _startPoint = null;
            }
        }

        private void CreateTriangle()
        {
            if (IsManualMode)
            {
                CreateDefaultFigure("Triangle", value => IsCheckedTriangle = value);
            }
            else
            {
                _isDrawingTriangle = !_isDrawingTriangle;
                _startPoint = null;
            }
        }

        private void CreateRectangle()
        {
            if (IsManualMode)
            {
                // Пока не реализовано создание прямоугольника по умолчанию
                Debug.WriteLine("Auto mode: Create default rectangle.");
                // CreateDefaultFigure("Rectangle", value => IsCheckedRectangle = value);
            }
            else
            {
                _isDrawingRectangle = !_isDrawingRectangle;
                _startPoint = null;
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

        public IFigure GetFigureAtPoint(Point point)
        {
            foreach (var figure in _figureService.Figures)
            {
                if (figure.IsIn(point, 5))
                {
                    return figure;
                }
            }
            return null;
        }

        private void Save()
        {
            // сохранение файла в корень проекта (временно)
            string projectRoot = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
            string filePath = Path.Combine(projectRoot, "test.json");
            IO.SaveToFile(_figureService.Figures, filePath);
        }

        private async Task SaveAs()
        { 
            if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null)
                return;
            // получаем главное окно
            var storageProvider = desktop.MainWindow.StorageProvider;
            if (storageProvider == null)
                return;

            var filePickerOptions = new FilePickerSaveOptions
            {
                Title = "Сохранить как",
                SuggestedFileName = "picture", //дефолтное имя файла
                ShowOverwritePrompt = true, // предупреждение о перезаписи файла
                FileTypeChoices = new[] // забиваем доступные форматы в выпадающий список
                {
                    new FilePickerFileType("Файл") { Patterns = new[] { "*.json" } }
                }
            };
           //открываем проводник
            var res = await storageProvider.SaveFilePickerAsync(filePickerOptions);

            if (res != null)
            {
                var filePath = res.Path.LocalPath; // получаем путь
                IO.SaveToFile(_figureService.Figures, filePath);
            }

        }

        private async Task Load()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null)
                return;
            // получаем главное окно
            var storageProvider = desktop.MainWindow.StorageProvider;
            if (storageProvider == null)
                return;

            var filePickerOptions = new FilePickerOpenOptions
            {
                Title = "Открыть файл",
                AllowMultiple = false, // только один файл
                FileTypeFilter = new[] // забиваем доступные форматы в выпадающий список
                {
                        new FilePickerFileType("Файл") { Patterns = new[] { "*.json" } },
                    }
            };
            //открываем проводник
            var res = await storageProvider.OpenFilePickerAsync(filePickerOptions);

            if (res.Count > 0)
            {
                var filePath = res[0].Path.LocalPath; // получаем путь
                IO.LoadFromFile(_figureService, filePath);
            }

        }
    }
}
