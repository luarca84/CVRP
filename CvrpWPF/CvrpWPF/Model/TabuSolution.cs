using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvrpWPF.Model
{
    class TabuSolution
    {
        List<List<Node>> rutas;
        Node depot;
        double fitness = 0;
        int capacidad = 0;
        bool feasible = false;

        public TabuSolution(int numRutas, List<Node> lstNodes, Node depot, int capacidad)
        {
            rutas = new List<List<Node>>();
            for (int i = 0; i < numRutas; i++)
                rutas.Add(new List<Node>());

            Random r = new Random();
            foreach (Node n in lstNodes)
            {
                if (n.Id != depot.Id)
                {
                    int ruta = r.Next(0, numRutas);
                    rutas[ruta].Add(n);
                }
            }

            this.depot = depot;
            this.capacidad = capacidad;

            ReordenarNodosRutasMinimizandoCapacidad();
            ReordenarNodosRutasMinimizandoDistancia();
        }

        public TabuSolution(TabuSolution ts, Node a, int routeIndex)
        {
            rutas = new List<List<Node>>();
            foreach (List<Node> lst in ts.rutas)
            {
                rutas.Add(new List<Node>());
                foreach (Node n in lst)
                {
                    rutas[rutas.Count - 1].Add(n);
                }
            }

            depot = ts.depot;
            fitness = ts.fitness;
            capacidad = ts.capacidad;
            feasible = ts.feasible;

            int indexRouteNode = GetIdRuta(a);
            rutas[indexRouteNode].Remove(a);
            rutas[routeIndex].Add(a);

            ReordenarNodosRutasMinimizandoCapacidad();
            ReordenarNodosRutasMinimizandoDistancia();
        }

        private int GetIdRuta(Node n)
        {
            for (int i = 0; i < rutas.Count; i++)
                for (int j = 0; j < rutas[i].Count; j++)
                    if (rutas[i][j].Id == n.Id)
                        return i;
            return 0;
        }

        public void CalculateFitness()
        {
            double f = 0;
            foreach (List<Node> ruta in rutas)
            {
                f += CalcularDistanciaRuta(ruta);
            }
            Fitness = f;

            feasible = CheckTodasLasRutasSonFactibles();
        }

        private bool CheckTodasLasRutasSonFactibles()
        {
            foreach (List<Node> ruta in rutas)
            {
                int sumDemand = 0;
                foreach (Node n in ruta)
                    sumDemand += n.Demand;
                if (sumDemand > capacidad)
                    return false;
            }
            return true;
        }

        private double CalcularDistanciaRuta(List<Node> ruta)
        {
            double sum = 0;
            if (ruta.Count > 0)
            {
                sum += CalcularDistanciaEntreDosPuntos(depot, ruta[0]);
                for (int i = 0; i < ruta.Count - 1; i++)
                    sum += CalcularDistanciaEntreDosPuntos(ruta[i], ruta[i + 1]);
                sum += CalcularDistanciaEntreDosPuntos(ruta[ruta.Count - 1], depot);
            }
            return sum;
        }

        private void ReordenarNodosRutasMinimizandoCapacidad()
        {
            List<Node> lstAux = new List<Node>();
            for (int i = 0; i < rutas.Count; i++)
            {
                int sumcapacidad = CalcularSumaDemanda(rutas[i]);
                while (sumcapacidad > capacidad)
                {
                    Node n = rutas[i][0];
                    rutas[i].RemoveAt(0);
                    lstAux.Add(n);
                    sumcapacidad = CalcularSumaDemanda(rutas[i]);
                }
            }

            Random r = new Random();
            int maxNumIntentos = 1000;
            int intento = 0;
            while (lstAux.Count > 0)
            {
                Node n = lstAux[0];
                int i = r.Next(0, rutas.Count);
                int sumcapacidad = CalcularSumaDemanda(rutas[i]);
                if (sumcapacidad + n.Demand <= capacidad || intento > maxNumIntentos)
                {
                    lstAux.RemoveAt(0);
                    rutas[i].Add(n);
                }
                intento++;
            }
        }

        private int CalcularSumaDemanda(List<Node> nodes)
        {
            int sum = 0;
            foreach (Node n in nodes)
                sum += n.Demand;
            return sum;
        }

        private void ReordenarNodosRutasMinimizandoDistancia()
        {
            for (int i = 0; i < rutas.Count; i++)
            {
                rutas[i] = ReordenarNodosRuta(rutas[i]);
            }
        }

        private List<Node> ReordenarNodosRuta(List<Node> rutaOriginal)
        {
            List<Node> rutaNueva = new List<Node>();
            while (rutaNueva.Count != rutaOriginal.Count)
            {
                Node bestNode = null;
                double costbestNode = double.MaxValue;
                for (int i = 0; i < rutaOriginal.Count; i++)
                {
                    Node node = rutaOriginal[i];
                    if (!rutaNueva.Contains(node))
                    {
                        List<Node> lstAux = new List<Node>();
                        lstAux.AddRange(rutaNueva);
                        lstAux.Add(node);
                        double cost = CalcularDistanciaRuta(lstAux);
                        if (cost < costbestNode)
                        {
                            costbestNode = cost;
                            bestNode = node;
                        }
                    }
                }

                rutaNueva.Add(bestNode);
            }
            return rutaNueva;
        }

        private double CalcularDistanciaEntreDosPuntos(Node a, Node b)
        {
            double distancia = Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
            return distancia;
        }


        internal List<List<Node>> Rutas
        {
            get
            {
                return rutas;
            }

            set
            {
                rutas = value;
            }
        }

        public double Fitness
        {
            get
            {
                return fitness;
            }

            set
            {
                fitness = value;
            }
        }

        public bool Feasible
        {
            get
            {
                return feasible;
            }

            set
            {
                feasible = value;
            }
        }
    }
}
