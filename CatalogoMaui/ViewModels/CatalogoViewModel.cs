using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogoMaui.Models;

namespace CatalogoMaui.ViewModels
{
    public class CatalogoViewModel
    {
        // Cada objeto de esta lista se convertirá en una tarjeta.
        public List<Producto> Productos { get; } = new()
        {
        new Producto
        {
        Nombre = "Mochila urbana",
        Descripcion = "Con espacio para laptop y cuadernos.",
        Precio = 499.00m,
        Detalle = "Mochila de exelente calidad y para uso diario exelente para la escuela",
        Foto = "mochila.png"
        },
        new Producto
        {
        Nombre = "Libreta A5",
        Descripcion = "80 hojas rayadas con pasta dura.",
        Precio = 89.50m,
        Detalle = "libreta con 100 hojas para el trabajo o escuela",
        Foto = "cuaderno.png"
        },
        new Producto
        {
        Nombre = "Termo de acero",
        Descripcion = "Botella reutilizable de 500 ml.",
        Precio = 219.00m,
        Detalle = "Termo que mantiene las bebidas en frio o caliente",
        Foto = "termo.png"
        },
        new Producto
        { 
            Nombre = "Tenis de Basket",
            Descripcion ="Tenis para baloncesto",
            Precio = 1200.00m,
            Detalle = "Tenis con nueva tecnologia para la duela y mejor estabilidad",
            Foto = "teni.png"
        }


        };
    }
}
