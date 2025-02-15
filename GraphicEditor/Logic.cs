using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicEditor
{
    internal class Logic
    {
    }

    public class FigureService : ILogic
    {
        public IEnumerable<IFigure> Figures => throw new NotImplementedException();

        public IEnumerable<string> FigureNamesToCreate => throw new NotImplementedException();

        public void AddFigure(IFigure figure)
        {
            throw new NotImplementedException();
        }

        public IFigure Create(string name, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public IFigure Find(Point p, double eps)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<(string, Type)> GetParameters(string figure)
        {
            throw new NotImplementedException();
        }

        public void Load(string FilePath, string FileFormat)
        {
            throw new NotImplementedException();
        }

        public void RemoveFigure(IFigure figure)
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
