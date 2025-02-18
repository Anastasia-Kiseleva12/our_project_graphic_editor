using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;

namespace GraphicEditor
{
    public class FigureService : ILogic
    {
        private readonly List<IFigure> _figures = new(); // Все фигуры
        private readonly HashSet<IFigure> _selectedFigures = new(); // Выбранные фигуры

        public IEnumerable<IFigure> Figures => _figures;

        public IEnumerable<string> FigureNamesToCreate => FigureFabric.AvailableFigures; //список всех доступных имен фигур

        public void AddFigure(IFigure figure)
        {
            if (figure == null) throw new ArgumentNullException(nameof(figure));
            _figures.Add(figure);
        }

        public void RemoveFigure(IFigure figure)
        {
            if (figure == null) return;
            _figures.Remove(figure);
            _selectedFigures.Remove(figure);
        }

        public IFigure Create(string name, IDictionary<string, object> parameters)
        {
            if (!FigureFabric.AvailableFigures.Contains(name))
            {
                throw new ArgumentException($"Фигура с именем {name} не найдена.", nameof(name));
            }

            var figure = FigureFabric.CreateFigure(name);

            if (figure is Circle circle)
            {
                var center = (Point)parameters["Center"];
                var pointOnCircle = (Point)parameters["PointOnCircle"];
                figure = new Circle(center, pointOnCircle);
            }
            //пример создания остальных фигур
            //else if (figure is Rectangle rectangle)
            //{
            //    var topLeft = (Point)parameters["TopLeft"];
            //    var bottomRight = (Point)parameters["BottomRight"];
            //    figure = new Rectangle(topLeft, bottomRight);
            //}
            //else if (figure is Triangle triangle)
            //{
            //    var a = (Point)parameters["A"];
            //    var b = (Point)parameters["B"];
            //    var c = (Point)parameters["C"];
            //    figure = new Triangle(a, b, c);
            //}
            //else if (figure is Line line)
            //{
            //    var start = (Point)parameters["Start"];
            //    var end = (Point)parameters["End"];
            //    figure = new Line(start, end);
            //}

            return figure;
        }

        public IFigure Find(Point p, double eps)
        {
            return _figures.FirstOrDefault(f => f.IsIn(p, eps));
        }


        public IEnumerable<(string, Type)> GetParameters(string figure)
        {
            throw new NotImplementedException();
        }

        public void Load(string FilePath, string FileFormat)
        {
            throw new NotImplementedException();
        }


        public void Save(string FilePath, string FileFormat)
        {
            throw new NotImplementedException();
        }

        public void Select(IFigure f)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFigure> Selected()
        {
            throw new NotImplementedException();
        }

        public void UnSelect(IFigure f)
        {
            throw new NotImplementedException();
        }
    }
}
