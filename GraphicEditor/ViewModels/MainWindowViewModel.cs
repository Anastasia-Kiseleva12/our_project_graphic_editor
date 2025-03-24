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
using MsBox.Avalonia;
using Splat;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using Avalonia.Media;

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
        private bool _isSaved = false;

        private bool _isDragging; // Флаг перемещения
        private Point? _dragStartPoint; // Начальная точка перемещения
        private IFigure _draggedFigure; // Перемещаемая фигура

        private bool _isDrawingReflectionLine;

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
        private double _currentThickness = 1;
        public double CurrentThickness
        {
            get => _currentThickness;
            set => this.RaiseAndSetIfChanged(ref _currentThickness, value);
        }
        private double _Angle;
        public double Angle
        {
            get => _Angle;
            set => this.RaiseAndSetIfChanged(ref _Angle, value);
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
        public bool IsDrawingReflectionLine
        {
            get => _isDrawingReflectionLine;
            set => this.RaiseAndSetIfChanged(ref _isDrawingReflectionLine, value);
        }
        private bool _isPanelOpen;
        public bool IsPanelOpen
        {
            get => _isPanelOpen;
            set => this.RaiseAndSetIfChanged(ref _isPanelOpen, value);
        }
        private bool _isSelectedLine;
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
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
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

            RotateFigureCommand = ReactiveCommand.Create(RotateFigure);

            BackRotateFigureCommand = ReactiveCommand.Create(BackRotateFigure);
            ReflectionCommand = ReactiveCommand.Create(() =>
            {

                _isDrawingReflectionLine = true;
                StartPoint = null;
                CurrentPoint = null;
                Debug.WriteLine("Reflection line mode activated.");
            });
            ScaleUpCommand = ReactiveCommand.Create(ScaleUp);
            ScaleDownCommand = ReactiveCommand.Create(ScaleDown);

            CopySelectedFiguresCommand = ReactiveCommand.Create(CopySelectedFigures);
            PasteFiguresCommand = ReactiveCommand.Create(PasteFigures);
            SaveCommand = ReactiveCommand.Create(Save);
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
        private void CreateDefaultFigure(string figureType, Action<bool> setIsChecked)
        {
            Debug.WriteLine($"Auto mode: Create default {figureType}.");
            var figure = _figureService.CreateDefault(figureType);
            _figureService.AddFigure(figure);
            setIsChecked(false); // Сбрасываем флаг IsChecked
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
            if (IsDrawingRectangle)
            {
                HandleRectangleClick(point);
                return;
            }
            if (_isDrawingReflectionLine)
            {
                HandleReflectionLineClick(point);
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

        private void HandleRectangleClick(Point point)
        {
            if (StartPoint == null)
            {
                StartPoint = point;
                Debug.WriteLine($"Start point set at: {StartPoint}");
            }
            else
            {
                // Определяем 4 точки прямоугольника на основе двух кликов
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

                IsDrawingRectangle = false;
                StartPoint = null;
                CurrentPoint = null;
                IsCheckedRectangle = false;
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
                    _isDragging = true;
                    _isSelectedLine = true;
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
            if (IsDrawingLine || IsDrawingCircle || IsDrawingTriangle)
            {
                CurrentPoint = point;
            }
            else if (_isDragging && _draggedFigure != null && _dragStartPoint != null)
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
            _isDragging = false;
            _dragStartPoint = null;
            _draggedFigure = null;
        }
        private void HandleReflectionLineClick(Point point)
        {
            //_isDrawingReflectionLine = !_isDrawingReflectionLine;
            if (StartPoint == null)
            {
                // Первый клик — задаем начало линии
                StartPoint = point;
                Debug.WriteLine($"Reflection line start set at: {StartPoint}");
            }
            else if (CurrentPoint == null)
            {
                // Второй клик — задаем конец линии
                CurrentPoint = point;
                Debug.WriteLine($"Reflection line end set at: {CurrentPoint}");
                // Создаем временную линию
                ReflectionFigure();
                
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

                CreateDefaultFigure("Rectangle", value => IsCheckedRectangle = value);
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
        private void RotateFigure()
        {
            double angle = 0;
            if (SelectedFigure != null)
            {
                // Увеличиваем угол вращения на 10 градусов (можно изменить)
                angle += 10;

                // Применяем вращение к выбранной фигуре
                SelectedFigure.Rotate(angle);

                // Вызываем событие для обновления отрисовки
                FiguresChanged?.Invoke();
            }
        }

        private void BackRotateFigure()
        {
            double angle = 0;
            if (SelectedFigure != null)
            {
                // Увеличиваем угол вращения на 10 градусов (можно изменить)
                angle -= 10;

                // Применяем вращение к выбранной фигуре
                SelectedFigure.Rotate(angle);

                // Вызываем событие для обновления отрисовки
                FiguresChanged?.Invoke();
            }
        }

        private void ReflectionFigure()
        {
            if (StartPoint != null && CurrentPoint != null)
            {
                // Создаем временную линию
                var tempLine = new Line(new Point(StartPoint.X, StartPoint.Y), new Point(CurrentPoint.X, CurrentPoint.Y), 2);

                // Добавляем временную линию в коллекцию фигур для отрисовки
                _figureService.AddFigure(tempLine);

                //// Обновляем отрисовку, чтобы временная линия появилась на экране
                //FiguresChanged?.Invoke();

                // Вызываем метод отражения, передавая координаты линии
                SelectedFigure.Reflection(new Point(StartPoint.X, StartPoint.Y), new Point(CurrentPoint.X, CurrentPoint.Y));

                // Удаляем временную линию через 500 мс
                Task.Delay(500).ContinueWith(_ =>
                {
                    _figureService.RemoveFigure(tempLine);
                    FiguresChanged?.Invoke();
                }, TaskScheduler.FromCurrentSynchronizationContext());

                // Сбрасываем состояние
                _isDrawingReflectionLine = false;
                StartPoint = null;
                //CurrentPoint = null;
            }
          
        }
        private void ScaleFigure(double scaleFactor)
        {
            if (SelectedFigure != null)
            {
                // Применяем масштабирование к выбранной фигуре
                SelectedFigure.Scale(scaleFactor);

                // Вызываем событие для обновления отрисовки
                FiguresChanged?.Invoke();
            }
        }

        public void ScaleUp()
        {
            ScaleFigure(1.1); // Увеличиваем размер на 10%
        }

        public void ScaleDown()
        {
            ScaleFigure(0.9); // Уменьшаем размер на 10%
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
                clonedFigure.Move(new Point(10, 10)); // Сдвигаем фигуру для видимости при вставке
                _figureService.AddFigure(clonedFigure);
            }
            FiguresChanged?.Invoke();
        }
        private void Save()
        {
            // сохранение файла в корень проекта (временно)
            string projectRoot = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
            string filePath = Path.Combine(projectRoot, "test.json");
            IO.SaveToFile(_figureService.Figures, filePath);
            _isSaved = true;
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
                        HandleFigureSelection(new Point (-10000000, -10000000));
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
