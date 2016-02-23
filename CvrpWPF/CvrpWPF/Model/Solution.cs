using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvrpWPF.Model
{
    class Solution
    {
        List<double> lstMinFitness = new List<double>();
        List<List<Node>> bestRutas = new List<List<Node>>();
        double fitnessBestRutas = 0;

        public List<double> LstMinFitness
        {
            get
            {
                return lstMinFitness;
            }

            set
            {
                lstMinFitness = value;
            }
        }

        internal List<List<Node>> BestRutas
        {
            get
            {
                return bestRutas;
            }

            set
            {
                bestRutas = value;
            }
        }

        public double FitnessBestRutas
        {
            get
            {
                return fitnessBestRutas;
            }

            set
            {
                fitnessBestRutas = value;
            }
        }
    }
}
