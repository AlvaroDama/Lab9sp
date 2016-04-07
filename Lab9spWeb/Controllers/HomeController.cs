using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lab9spWeb.Models;

namespace Lab9spWeb.Controllers
{
    public class HomeController : Controller
    {
        [SharePointContextFilter]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TotalPedidos()
        {
            User spUser = null;

            var spContext = SharePointContextProvider.Current.GetSharePointContext(HttpContext);

            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    Web web = clientContext.Web;
                    clientContext.Load(web);
                    clientContext.ExecuteQuery();

                    ListCollection lists = web.Lists;
                    clientContext.Load<ListCollection>(lists);
                    clientContext.ExecuteQuery();

                    var pedidos = lists.GetByTitle("Pedidos");
                    var productos = lists.GetByTitle("Productos");
                    clientContext.Load(pedidos);
                    clientContext.Load(productos);
                    clientContext.ExecuteQuery();
                    CamlQuery pedidosQuery = new CamlQuery();

                    ListItemCollection pedidosItems = pedidos.GetItems(pedidosQuery);
                    clientContext.Load(pedidosItems);
                    clientContext.ExecuteQuery();

                    var total = 0.0;
                    var clientes = new Dictionary<string, double>();

                    foreach (var item in pedidosItems)
                    {
                        FieldLookupValue lookup = item["Producto"] as FieldLookupValue;

                        int lookId = lookup.LookupId;
                        var uds = item["Unidades"];
                        var pi = productos.GetItemById(lookId);

                        clientContext.Load(pi);
                        clientContext.ExecuteQuery();

                        var precio = pi["Precio"];
                        var venta = (double)precio * (double)uds;
                        total += venta;

                        if (clientes.ContainsKey(item["Title"].ToString()))
                            clientes[item["Title"].ToString()] += venta;
                        else
                            clientes.Add(item["Title"].ToString(), venta);
                    }

                    var mc = total / clientes.Keys.Count;

                    var model = new Totales() { Numero = pedidosItems.Count, MediaCliente = mc, Total = total };

                    return View(model);
                }
            }
            return View();
        }

        public ActionResult ListaPedidos()
        {
            List<Pedidos> model = new List<Pedidos>();

            var spContext = SharePointContextProvider.Current.GetSharePointContext(HttpContext);

            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    Web web = clientContext.Web;
                    clientContext.Load(web);
                    clientContext.ExecuteQuery();

                    ListCollection lists = web.Lists;
                    clientContext.Load<ListCollection>(lists);
                    clientContext.ExecuteQuery();

                    var pedidos = lists.GetByTitle("Pedidos");
                    var productos = lists.GetByTitle("Productos");
                    clientContext.Load(pedidos);
                    clientContext.Load(productos);
                    clientContext.ExecuteQuery();
                    CamlQuery pedidosQuery = new CamlQuery();

                    ListItemCollection pedidosItems = pedidos.GetItems(pedidosQuery);
                    clientContext.Load(pedidosItems);
                    clientContext.ExecuteQuery();

                    var total = 0.0;
                    var clientes = new Dictionary<string, double>();

                    foreach (var item in pedidosItems)
                    {
                        FieldLookupValue lookup = item["Producto"] as FieldLookupValue;

                        int lookId = lookup.LookupId;
                        int uds;
                        int.TryParse(item["Unidades"].ToString(), out uds);
                        var pi = productos.GetItemById(lookId);

                        clientContext.Load(pi);
                        clientContext.ExecuteQuery();

                        var precio = pi["Precio"];
                        var venta = (double) precio*(double) uds;
                        //falta código

                        model.Add(new Pedidos()
                        {
                            Cliente = item["Title"].ToString(),
                            Pedido = pi["Title"].ToString(),
                            Unidades = uds,
                            Total = venta
                        });
                    }
                }
            }
            return View(model);
        }

        public ActionResult Create()
        {
            var prodList = new List<Productos>();

            var spContext = SharePointContextProvider.Current.GetSharePointContext(HttpContext);

            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    Web web = clientContext.Web;
                    clientContext.Load(web);
                    clientContext.ExecuteQuery();

                    ListCollection lists = web.Lists;
                    clientContext.Load<ListCollection>(lists);
                    clientContext.ExecuteQuery();

                    var productos = lists.GetByTitle("Productos");
                    clientContext.Load(productos);
                    clientContext.ExecuteQuery();

                    CamlQuery query = new CamlQuery();
                    ListItemCollection prodItems = productos.GetItems(query);
                    clientContext.Load(prodItems);
                    clientContext.ExecuteQuery();

                    foreach (var item in prodItems)
                    {
                        int id;
                        int.TryParse(item["ID"].ToString(), out id);

                        prodList.Add(new Productos()
                        {
                            Id = id,
                            Nombre = item["Title"].ToString()
                        });
                    }
                }
            }
            ViewBag.idProducto = new SelectList(prodList, "Id", "Nombre");
            return View(new Pedidos());
        }

        [HttpPost]
        public ActionResult Create(Pedidos model)
        {
            var spContext = SharePointContextProvider.Current.GetSharePointContext(HttpContext);

            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    Web web = clientContext.Web;
                    clientContext.Load(web);
                    clientContext.ExecuteQuery();

                    ListCollection lists = web.Lists;
                    clientContext.Load<ListCollection>(lists);
                    clientContext.ExecuteQuery();

                    var pedidos = lists.GetByTitle("Pedidos");
                    clientContext.Load(pedidos);

                    ListItemCreationInformation lci = new ListItemCreationInformation();
                    ListItem pedListItem = pedidos.AddItem(lci);
                    pedListItem["Title"] = model.Cliente;
                    pedListItem["Unidades"] = model.Unidades;
                    pedListItem["Fecha"] = DateTime.Now;
                    var lv = new FieldLookupValue() {LookupId = model.IdProducto};
                    pedListItem["Producto"] = lv;

                    pedListItem.Update();
                    clientContext.ExecuteQuery();
                }
            }
            return RedirectToAction("Index",
                new {SPHostUrl = SharePointContext.GetSPHostUrl(HttpContext.Request).AbsoluteUri});
        }
    }
}
