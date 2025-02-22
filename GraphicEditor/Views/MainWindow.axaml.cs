using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using DynamicData;
using GraphicEditor.ViewModels;

namespace GraphicEditor.Views
{
    public partial class MainWindow : Window
    {
        public MainWindowViewModel _viewModel;

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            Debug.WriteLine("MainWindow constructor called!");
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Устанавливаем ViewModel
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.FiguresChanged += () =>
            {
                Debug.WriteLine("FiguresChanged event triggered!");
                Dispatcher.UIThread.Post(() => Draw());
            };
        }

        private void Draw()
        {

            // Очищаем Canvas перед отрисовкой новых фигур
            DrawingCanvas.Children.Clear();

            // Получаем фигуры из FigureService
            var figures = _viewModel._figureService.Figures;

            // Проходим по всем фигурам и рисуем их
            foreach (var figure in figures)
            {
                if (figure is Line line)
                {
                    Debug.WriteLine($"Drawing line from ({line.Start.X}, {line.Start.Y}) to ({line.End.X}, {line.End.Y})");

                    // Создаем LineGeometry
                    var lineGeometry = new LineGeometry
                    {
                        StartPoint = new Avalonia.Point(line.Start.X, line.Start.Y),
                        EndPoint = new Avalonia.Point(line.End.X, line.End.Y)
                    };

                    // Создаем Path
                    var lineShape = new Path
                    {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Data = lineGeometry
                    };

                    // Добавляем Path на Canvas
                    DrawingCanvas.Children.Add(lineShape);
                    DrawingCanvas.InvalidateVisual();
                }
            }
        }
    }
}
