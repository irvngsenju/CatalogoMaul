using System.Globalization;
using CatalogoMaui.Models;
using CatalogoMaui.Services;

namespace CatalogoMaui.Views;

public partial class ProductoDetallePage : ContentPage, IQueryAttributable
{
    private int id;
    private string fotoOriginal = "";
    private string? rutaTemporalFoto;

    public ProductoDetallePage()
    {
        InitializeComponent();
        Title = "Agregar producto";
        Foto.ItemsSource = new List<string> { "mochila.png", "cuaderno.png", "termo.png","teni.png" };
        Foto.SelectedIndex = 0;
    }

    // Con datos es edición. Sin datos conservamos el formulario nuevo.
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("producto", out var dato) || dato is not Producto p)
            return;

        id = p.Id;
        fotoOriginal = p.Foto;
        rutaTemporalFoto = null;

        Title = "Editar producto";
        Nombre.Text = p.Nombre;
        Descripcion.Text = p.Descripcion;
        Detalle.Text = p.Detalle;
        Precio.Text = p.Precio.ToString("0.00", CultureInfo.CurrentCulture);

        if (Foto.ItemsSource is IList<string> opciones && opciones.Contains(p.Foto))
        {
            Foto.SelectedItem = p.Foto;
            VistaFoto.Source = FotosProducto.Resolver(p.Foto);
        }
        else
        {
            Foto.SelectedItem = null;
            VistaFoto.Source = FotosProducto.Resolver(p.Foto);
        }

        Eliminar.IsVisible = true;
    }

    private void AlCambiarFoto(object? sender, EventArgs e)
    {
        if (Foto.SelectedItem is string archivo)
        {
            FotosProducto.EliminarTemporal(rutaTemporalFoto);
            rutaTemporalFoto = null;
            VistaFoto.Source = FotosProducto.Resolver(archivo);
        }
    }

    private async void AlElegirFoto(object? sender, EventArgs e)
    {
        if (!ElegirFoto.IsEnabled) return;

        ElegirFoto.IsEnabled = false;
        try
        {
            var nuevaTemporal = await FotosProducto.ElegirAsync();
            if (nuevaTemporal is null)
                return;

            FotosProducto.EliminarTemporal(rutaTemporalFoto);
            rutaTemporalFoto = nuevaTemporal;
            Foto.SelectedItem = null;
            VistaFoto.Source = rutaTemporalFoto;
        }
        catch (Exception ex)
        {
            await DisplayAlert("No se pudo seleccionar la foto", ex.Message, "Aceptar");
        }
        finally
        {
            ElegirFoto.IsEnabled = true;
        }
    }

    private async void AlGuardar(object? sender, EventArgs e)
    {
        if (!Guardar.IsEnabled) return;

        var textoPrecio = Precio.Text?.Trim().Replace(',', '.');
        if (string.IsNullOrWhiteSpace(Nombre.Text) ||
            string.IsNullOrWhiteSpace(Descripcion.Text) ||
            string.IsNullOrWhiteSpace(Detalle.Text) ||
            !decimal.TryParse(textoPrecio, NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var importe) ||
            importe < 0 || importe > 99999999.99m ||
            decimal.Round(importe, 2) != importe)
        {
            await DisplayAlert("Revisa los datos",
                "Completa los campos y escribe un precio válido de hasta dos decimales.", "Aceptar");
            return;
        }

        string? fotoFinal = null;
        if (!string.IsNullOrWhiteSpace(rutaTemporalFoto))
        {
            fotoFinal = rutaTemporalFoto;
        }
        else if (Foto.SelectedItem is string seleccionada && !string.IsNullOrWhiteSpace(seleccionada))
        {
            fotoFinal = seleccionada;
        }
        else if (!string.IsNullOrWhiteSpace(fotoOriginal))
        {
            fotoFinal = fotoOriginal;
        }

        if (string.IsNullOrWhiteSpace(fotoFinal))
        {
            await DisplayAlert("Revisa los datos",
                "Selecciona una imagen del catálogo o elige una foto de la galería.", "Aceptar");
            return;
        }

        string? nombreFotoGuardada = null;
        string? fotoAnteriorAEliminar = null;

        var producto = new Producto
        {
            Id = id,
            Nombre = Nombre.Text.Trim(),
            Descripcion = Descripcion.Text.Trim(),
            Detalle = Detalle.Text.Trim(),
            Precio = importe,
            Foto = fotoFinal
        };

        await EjecutarAsync(async () =>
        {
            if (!string.IsNullOrWhiteSpace(rutaTemporalFoto))
            {
                nombreFotoGuardada = await FotosProducto.GuardarAsync(rutaTemporalFoto);
                producto.Foto = nombreFotoGuardada;
                if (!string.Equals(fotoOriginal, nombreFotoGuardada, StringComparison.OrdinalIgnoreCase))
                    fotoAnteriorAEliminar = fotoOriginal;
            }
            else if (!string.Equals(fotoOriginal, fotoFinal, StringComparison.OrdinalIgnoreCase))
            {
                fotoAnteriorAEliminar = fotoOriginal;
            }

            try
            {
                // Id cero: todavía no hay fila. Otro Id: actualizamos esa fila.
                if (id == 0)
                    await ProductosSqlite.Instancia.Insertar(producto);
                else if (!await ProductosSqlite.Instancia.Actualizar(producto))
                    throw new InvalidOperationException("El producto ya no existe.");

                if (!string.IsNullOrWhiteSpace(fotoAnteriorAEliminar))
                    FotosProducto.EliminarSiEsPropia(fotoAnteriorAEliminar);

                FotosProducto.EliminarTemporal(rutaTemporalFoto);
                rutaTemporalFoto = null;
                fotoOriginal = producto.Foto;
                return true;
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(nombreFotoGuardada))
                    FotosProducto.EliminarSiEsPropia(nombreFotoGuardada);

                throw;
            }
        });
    }

    private async void AlEliminar(object? sender, EventArgs e)
    {
        if (!Eliminar.IsEnabled || id == 0) return;

        await EjecutarAsync(async () =>
        {
            if (!await DisplayAlert("Eliminar producto", "¿Deseas eliminar este producto?",
                "Eliminar", "Cancelar"))
                return false;

            if (!await ProductosSqlite.Instancia.Eliminar(id))
                throw new InvalidOperationException("El producto ya no existe.");

            FotosProducto.EliminarSiEsPropia(fotoOriginal);
            FotosProducto.EliminarTemporal(rutaTemporalFoto);
            rutaTemporalFoto = null;
            return true;
        });
    }

    private async Task EjecutarAsync(Func<Task<bool>> accion)
    {
        Guardar.IsEnabled = false;
        Eliminar.IsEnabled = false;
        ElegirFoto.IsEnabled = false;

        try
        {
            var ok = await accion();
            if (ok)
                await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("No se completó la operación", ex.Message, "Aceptar");
        }
        finally
        {
            Guardar.IsEnabled = true;
            Eliminar.IsEnabled = id != 0;
            ElegirFoto.IsEnabled = true;
        }
    }
}