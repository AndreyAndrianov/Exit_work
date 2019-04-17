using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainigDataSetGeneration
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataSetgenerator = new DatasetGenerator();
            dataSetgenerator.GenerateComplexDataset(5, 500, 5, withPhone: true);
        }
    }
}
