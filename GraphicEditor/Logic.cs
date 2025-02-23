using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;

using DynamicData;

namespace GraphicEditor
{
    public class FigureService 
    {
        public readonly SourceCache<IFigure,string> _figures = new(fig=>fig.Id); // Все фигуры
        private readonly HashSet<IFigure> _selectedFigures = new(); // Выбранные фигуры

        public IEnumerable<IFigure> Figures => _figures.Items;

        public IEnumerable<string> FigureNamesToCreate => FigureFabric.AvailableFigures; //список всех доступных имен фигур

        public void AddFigure(IFigure figure)
        {
            if (figure == null) throw new ArgumentNullException(nameof(figure));
            _figures.AddOrUpdate(figure);
        }

        public void RemoveFigure(IFigure figure)
        {
            if (figure == null) return;
            _figures.Remove(figure);
            _selectedFigures.Remove(figure);
        }

        public IFigure Create(string name, IDictionary<string, Point> parameters, IDictionary<string, double> doubleparameters)
        {
            if (!FigureFabric.AvailableFigures.Contains(name))
            {
                throw new ArgumentException($"Фигура с именем {name} не найдена.", nameof(name));
            }

            var figure = FigureFabric.CreateFigure(name);
            figure.SetParameters(doubleparameters, parameters);        

            return figure;
        }

        public IFigure? Find(Point p, double eps)
        {
            return Figures.FirstOrDefault(f => f.IsIn(p, eps));
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
            if (f == null) throw new ArgumentNullException(nameof(f));
            _selectedFigures.Add(f);
        }

        public IEnumerable<IFigure> Selected()
        {
            throw new NotImplementedException();
        }

        public void UnSelect(IFigure f)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));
            _selectedFigures.Remove(f);
        }
    }
}
