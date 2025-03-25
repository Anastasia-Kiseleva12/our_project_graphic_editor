using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;

namespace GraphicEditor.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public enum DrawingMode
        {
            None,
            Line,
            Circle,
            Triangle,
            Rectangle,
            ReflectionLine,
            Dragging
        }

        private DrawingMode _currentDrawingMode = DrawingMode.None;
        public DrawingMode CurrentDrawingMode
        {
            get => _currentDrawingMode;
            set => this.RaiseAndSetIfChanged(ref _currentDrawingMode, value);
        }

        private bool _isManualMode;
        public bool IsManualMode
        {
            get => _isManualMode;
            set => this.RaiseAndSetIfChanged(ref _isManualMode, value);
        }

        private Point? _startPoint;
        private Point? _secondPoint;
        private Point? _currentPoint;
        private bool _isSaved = false;
        private Point? _dragStartPoint;
        private IFigure _draggedFigure;

        private bool _isCheckedLine;
        public bool IsCheckedLine
        {
            get => _isCheckedLine;
            set => this.RaiseAndSetIfChanged(ref _isCheckedLine, value);
        }

        private bool _isCheckedRectangle;
        public bool IsCheckedRectangle
        {
            get => _isCheckedRectangle;
            set => this.RaiseAndSetIfChanged(ref _isCheckedRectangle, value);
        }

        private bool _isCheckedTriangle;
        public bool IsCheckedTriangle
        {
            get => _isCheckedTriangle;
            set => this.RaiseAndSetIfChanged(ref _isCheckedTriangle, value);
        }

        private bool _isCheckedCircle;
        public bool IsCheckedCircle
        {
            get => _isCheckedCircle;
            set => this.RaiseAndSetIfChanged(ref _isCheckedCircle, value);
        }
        public Point? StartPoint
        {
            get => _startPoint;
            set => this.RaiseAndSetIfChanged(ref _startPoint, value);
        }

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

        private double _currentThickness = 1;
        public double CurrentThickness
        {
            get => _currentThickness;
            set => this.RaiseAndSetIfChanged(ref _currentThickness, value);
        }

        private double _angle;
        public double Angle
        {
            get => _angle;
            set => this.RaiseAndSetIfChanged(ref _angle, value);
        }

        private bool _isPanelOpen;
        public bool IsPanelOpen
        {
            get => _isPanelOpen;
            set => this.RaiseAndSetIfChanged(ref _isPanelOpen, value);
        }

        public ReactiveCommand<Unit, Unit> CreatePolylineCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateCircleCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateTriangleCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateRectangleCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveSelectedFiguresCommand { get; }
        public ReactiveCommand<IFigure, Unit> SelectFigureCommand { get; }
        public ReactiveCommand<IFigure, Unit> UnselectFigureCommand { get; }
        public ReactiveCommand<Unit, Unit> RotateFigureCommand { get; }
        public ReactiveCommand<Unit, Unit> BackRotateFigureCommand { get; }
        public ReactiveCommand<Unit, Unit> ReflectionCommand { get; }
        public ReactiveCommand<Unit, Unit> ScaleUpCommand { get; }
        public ReactiveCommand<Unit, Unit> ScaleDownCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveAsCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }
        public ReactiveCommand<Unit, Unit> CopySelectedFiguresCommand { get; }
        public ReactiveCommand<Unit, Unit> PasteFiguresCommand { get; }

        private List<IFigure> _copiedFigures = new List<IFigure>();
        private IFigure _selectedFigure;
        public IFigure SelectedFigure
        {
            get => _selectedFigure;
            set
            {
                Debug.WriteLine($"SelectedFigure changed from {_selectedFigure?.Name} to {value?.Name}");
                this.RaiseAndSetIfChanged(ref _selectedFigure, value);
                IsPanelOpen = value != null;
                _isSaved = false;
            }
        }

        public FigureService _figureService;
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

            RotateFigureCommand = ReactiveCommand.Create(RotateFigure);

            BackRotateFigureCommand = ReactiveCommand.Create(BackRotateFigure);
            ReflectionCommand = ReactiveCommand.Create(() =>
            {

                CurrentDrawingMode = DrawingMode.ReflectionLine;
                StartPoint = null;
                CurrentPoint = null;
                Debug.WriteLine("Reflection line mode activated.");
            });
            ScaleUpCommand = ReactiveCommand.Create(ScaleUp);
            ScaleDownCommand = ReactiveCommand.Create(ScaleDown);

            CopySelectedFiguresCommand = ReactiveCommand.Create(CopySelectedFigures);
            PasteFiguresCommand = ReactiveCommand.Create(PasteFigures);
            SaveAsCommand = ReactiveCommand.CreateFromTask(SaveAs);
            LoadCommand = ReactiveCommand.CreateFromTask(Load);
            ExitCommand = ReactiveCommand.CreateFromTask(Exit);

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
        private void CreateDefaultFigure(string figureType)
        {
            Debug.WriteLine($"Auto mode: Create default {figureType}.");
            var figure = _figureService.CreateDefault(figureType);
            _figureService.AddFigure(figure);
            _isSaved = false;
        }
        private void CreateFigure(string figureType, Dictionary<string, Point> parameters, Dictionary<string, double> doubleParameters)
        {
            var figure = _figureService.Create(figureType, parameters, doubleParameters);
            _figureService.AddFigure(figure);
            FiguresChanged?.Invoke();
            _isSaved = false;
        }
        public void HandleCanvasClick(Point point)
        {
            if (StartPoint != null && CalculateDistance(StartPoint, point) < 20)
                return;

            if (SecondPoint != null && CalculateDistance(SecondPoint, point) < 20)
                return;

            switch (CurrentDrawingMode)
            {
                case DrawingMode.Line:
                    HandleLineClick(point);
                    break;
                case DrawingMode.Circle:
                    HandleCircleClick(point);
                    break;
                case DrawingMode.Triangle:
                    HandleTriangleClick(point);
                    break;
                case DrawingMode.Rectangle:
                    HandleRectangleClick(point);
                    break;
                case DrawingMode.ReflectionLine:
                    HandleReflectionLineClick(point);
                    break;
                default:
                    HandleFigureSelection(point);
                    break;
            }

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

                ResetDrawingState();
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

                ResetDrawingState();
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
                if (CalculateDistance(point, StartPoint) < 10 ||
                    CalculateDistance(point, SecondPoint) < 10)
                {
                    Debug.WriteLine("Point is too close to existing points");
                    return;
                }

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

                ResetDrawingState();
            }
        }

        private void HandleRectangleClick(Point point)
        {
            if (StartPoint == null)
            {
                StartPoint = point;
                Debug.WriteLine($"Start point set at: {StartPoint}");
            }
            else
            {
                // Проверяем, что точки не на одной линии
                if (Math.Abs(StartPoint.X - point.X) < 5 || Math.Abs(StartPoint.Y - point.Y) < 5)
                {
                    // Показываем сообщение об ошибке
                    Debug.WriteLine("Cannot create rectangle - points are collinear");
                    return;
                }
                var P1 = StartPoint;
                var P2 = new Point(point.X, StartPoint.Y);
                var P3 = point;
                var P4 = new Point(StartPoint.X, point.Y);

                var pointParameters = new Dictionary<string, Point>
                {
                    { "P1", P1 },
                    { "P2", P2 },
                    { "P3", P3 },
                    { "P4", P4 }
                };

                var doubleParameters = new Dictionary<string, double>
                {
                    { "StrokeThickness", 2}
                };

                CreateFigure("Rectangle", pointParameters, doubleParameters);

                ResetDrawingState();
            }
        }

        private void HandleFigureSelection(Point point)
        {
            var eps = 80; // допустимая погрешность
            var figure = _figureService.Find(new Point (point.X, point.Y), eps);
            //Предыдущее решение, пока не удаляю, может понадобится
            if (figure != null)
            {
                Debug.WriteLine($"Figure found: {figure.Name}");
                if (_selectedFigure == figure)
                {
                    // Если фигура уже выделена, начинаем перемещение
                    CurrentDrawingMode = DrawingMode.Dragging;
                    _dragStartPoint = point;
                    _draggedFigure = figure;
                }
                else
                {
                    // Если фигура не выделена, выделяем её
                    SelectFigureCommand.Execute(figure).Subscribe();
                    CurrentThickness = figure.StrokeThickness;
                }
            }
            else
            {
                // Если кликнули на пустую область, снимаем выделение
                UnselectFigureCommand.Execute(null).Subscribe();
                CurrentThickness = 1;
            }

            FiguresChanged?.Invoke();
        }
        public void HandleCanvasMove(Point point)
        {
            if (CurrentDrawingMode == DrawingMode.ReflectionLine && StartPoint != null)
            {
                CurrentPoint = point;
                FiguresChanged?.Invoke();
            }
            else if (CurrentDrawingMode is DrawingMode.Line or DrawingMode.Circle or DrawingMode.Triangle or DrawingMode.Rectangle)
            {
                CurrentPoint = point;
            }
            else if (CurrentDrawingMode == DrawingMode.Dragging && _draggedFigure != null && _dragStartPoint != null)
            {
                var deltaX = point.X - _dragStartPoint.X;
                var deltaY = point.Y - _dragStartPoint.Y;
                var vector = new Point(deltaX, deltaY);

                _draggedFigure.Move(vector);
                _dragStartPoint = point;
                FiguresChanged?.Invoke();
            }
        }
        public void HandleCanvasRelease()
        {
            if (CurrentDrawingMode == DrawingMode.Dragging)
            {
                CurrentDrawingMode = DrawingMode.None;
                _dragStartPoint = null;
                _draggedFigure = null;
            }
        }
        private async void HandleReflectionLineClick(Point point)
        {
            if (StartPoint == null)
            {
                StartPoint = point;
                Debug.WriteLine($"Start reflection line start point: {StartPoint}");
            }
            else
            {
                CurrentPoint = point;

                ReflectionFigure();

                await Task.Delay(100);

                ResetDrawingState();
                CurrentDrawingMode = DrawingMode.None;
            }
        }

        private void CreateLine()
        {
            ResetDrawingState();
            if (IsManualMode)
            {
                CreateDefaultFigure("Line");
            }
            else
            {
                CurrentDrawingMode = DrawingMode.Line;
                IsCheckedLine = true;
                StartPoint = null;
            }
        }

        private void CreateCircle()
        {
            ResetDrawingState();
            if (IsManualMode)
            {
                CreateDefaultFigure("Circle");
            }
            else
            {
                CurrentDrawingMode = DrawingMode.Circle;
                IsCheckedCircle = true;
                StartPoint = null;
            }
        }

        private void CreateTriangle()
        {
            ResetDrawingState();
            if (IsManualMode)
            {
                CreateDefaultFigure("Triangle");
            }
            else
            {
                CurrentDrawingMode = DrawingMode.Triangle;
                IsCheckedTriangle = true;
                StartPoint = null;
            }
        }

        private void CreateRectangle()
        {
            ResetDrawingState();
            if (IsManualMode)
            {
                CreateDefaultFigure("Rectangle");
            }
            else
            {
                CurrentDrawingMode = DrawingMode.Rectangle;
                IsCheckedRectangle = true;
                StartPoint = null;
            }
        }

        private void RemoveSelectedFigures()
        {
            var selectedFigures = _figureService._selectedFigures.ToList();
            foreach (var figure in selectedFigures)
            {
                _figureService.RemoveFigure(figure);
            }
            FiguresChanged?.Invoke();
        }
        private void RotateFigure()
        {
            double angle = 0;
            if (SelectedFigure != null)
            {
                angle += 10;
                SelectedFigure.Rotate(angle);
                FiguresChanged?.Invoke();
            }
        }

        private void BackRotateFigure()
        {
            double angle = 0;
            if (SelectedFigure != null)
            {
                angle -= 10;
                SelectedFigure.Rotate(angle);
                FiguresChanged?.Invoke();
            }
        }

        private void ReflectionFigure()
        {
            if (SelectedFigure == null || StartPoint == null || CurrentPoint == null)
                return;
            SelectedFigure.Reflection(StartPoint, CurrentPoint);
            FiguresChanged?.Invoke();

        }
        private void ScaleFigure(double scaleFactor)
        {
            if (SelectedFigure != null)
            {
                SelectedFigure.Scale(scaleFactor);
                FiguresChanged?.Invoke();
            }
        }

        public void ScaleUp()
        {
            ScaleFigure(1.1);
        }

        public void ScaleDown()
        {
            ScaleFigure(0.9);
        }


        private double CalculateDistance(Point p1, Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        private void CopySelectedFigures()
        {
            _copiedFigures.Clear();
            foreach (var figure in _figureService._selectedFigures)
            {
                _copiedFigures.Add(figure.Clone());
            }
        }

        private void PasteFigures()
        {
            foreach (var figure in _copiedFigures)
            {
                var clonedFigure = figure.Clone();
                clonedFigure.Move(new Point(10, 10));
                _figureService.AddFigure(clonedFigure);
            }
            FiguresChanged?.Invoke();
        }
        private void ResetDrawingState()
        {
            CurrentDrawingMode = DrawingMode.None;
            StartPoint = null;
            SecondPoint = null;
            CurrentPoint = null;

            IsCheckedRectangle = false;
            IsCheckedLine = false;
            IsCheckedCircle = false;
            IsCheckedTriangle = false;
        }
       private async Task SaveAs()
        {
            UnselectFigureCommand.Execute(null).Subscribe();
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
                    new FilePickerFileType("Файл JSON") { Patterns = new[] { "*.json" } },
                    new FilePickerFileType("Файл SVG") { Patterns = new[] { "*.svg" } },
                    new FilePickerFileType("Файл PNG") { Patterns = new[] { "*.png" }}
                }
            };

            //открываем проводник
            var res = await storageProvider.SaveFilePickerAsync(filePickerOptions);

            if (res != null)
            {
                var filePath = res.Path.LocalPath; // получаем путь
                var extension = Path.GetExtension(filePath).ToLower(); // определяем расширение файла

                switch (extension)
                {
                    case ".json":
                        IO.SaveToFile(_figureService.Figures, filePath);
                        break;
                    case ".svg":
                        IO.SaveToSvg(_figureService.Figures, filePath);
                        break;
                    case ".png":
                        HandleCanvasClick(new Point(-1000000, -1000000));
                        IO.SaveToPng(filePath);
                        break;
                    default:
                        break;
                }
                _isSaved = true;
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

        public async Task Exit()
        {
            switch (_isSaved)
            {
                case false:
                    var box = MessageBoxManager.GetMessageBoxCustom(
                    new MessageBoxCustomParams
                    {
                        ContentTitle = "Выход из приложения",
                        ContentMessage = "Все несохранённые данные будут потеряны. Вы уверены, что хотите выйти?",
                        ButtonDefinitions = new[]
                        {
                            new ButtonDefinition { Name = "Да"},
                            new ButtonDefinition { Name = "Нет"},
                            new ButtonDefinition { Name = "Сохранить файл"}
                        },
                        Icon = MsBox.Avalonia.Enums.Icon.Question,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        CanResize = false,
                        MaxWidth = 500,
                        MaxHeight = 800,
                        SizeToContent = SizeToContent.WidthAndHeight,
                        ShowInCenter = true,
                        Topmost = false,
                    });

                    var result = await box.ShowAsync();
                    if (result == "Да")
                    {
                        Environment.Exit(0);
                    }
                    else if (result == "Сохранить файл")
                    {
                        await SaveAs();
                        Environment.Exit(0);
                    }
                    break;
                case true:
                    Environment.Exit(0);
                    break;
            }
            _isSaved = false;
        }
    }
}
