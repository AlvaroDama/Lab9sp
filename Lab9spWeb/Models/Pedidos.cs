using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lab9spWeb.Models
{
    public class Pedidos
    {
        public string Cliente { get; set; }
        public string Pedido { get; set; }
        public int Unidades { get; set; }
        public double Total { get; set; }
        public int IdProducto { get; set; }
    }
}