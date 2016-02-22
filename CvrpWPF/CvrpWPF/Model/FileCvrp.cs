using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvrpWPF.Model
{
    class FileCvrp
    {
        string name;
        string comment;
        string type;
        int dimension;
        string edgeWeightType;
        int capacity;
        List<Node> lstNodes;
        int idNodeDepot;
        int numRutas;

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public string Comment
        {
            get
            {
                return comment;
            }

            set
            {
                comment = value;
            }
        }

        public string Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        public int Dimension
        {
            get
            {
                return dimension;
            }

            set
            {
                dimension = value;
            }
        }

        public string EdgeWeightType
        {
            get
            {
                return edgeWeightType;
            }

            set
            {
                edgeWeightType = value;
            }
        }

        public int Capacity
        {
            get
            {
                return capacity;
            }

            set
            {
                capacity = value;
            }
        }

        internal List<Node> LstNodes
        {
            get
            {
                return lstNodes;
            }

            set
            {
                lstNodes = value;
            }
        }

        public int IdNodeDepot
        {
            get
            {
                return idNodeDepot;
            }

            set
            {
                idNodeDepot = value;
            }
        }

        public int NumRutas
        {
            get
            {
                return numRutas;
            }

            set
            {
                numRutas = value;
            }
        }
    }
}
