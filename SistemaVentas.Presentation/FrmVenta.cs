using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SistemaVentas.Business.Services;
using SistemaVentas.Data.Models;
using SistemaVentas.Data.Repositories;
using System.Text;
using System.Globalization;

namespace SistemaVentas.Presentation
{
    public partial class FrmVenta : Form
    {
        private readonly ProductoService _productoService;
        private readonly VentaService _ventaService;
        private readonly int _cajaId;
        private readonly string _usuario;
        private readonly List<DetalleVenta> _detalles = new List<DetalleVenta>();
        private DataGridView dgvCarrito;
        private DataGridView dgvCatalogo;
        private TextBox txtBuscar;
        private Label lblTotal;
        private Label lblSubtotal;
        private Label lblImpuesto;
        private Label lblNumeroVenta;
        private ComboBox cmbMetodoPago;
        private TextBox txtMontoPagado;
        private Label lblCambio;
        private TextBox txtNombreCliente;
        private ComboBox cmbTipoDocumento;
        private TextBox txtDocumentoCliente;
        private Button btnCobrar;
        private Button btnCancelar;
        private Button btnBuscar;
        private Button btnAgregar;
        private Button btnMontoExacto;
        private decimal _totalActual = 0;
        private List<Producto> _todosProductos = new List<Producto>();
        private List<Producto> _productosMostrados = new List<Producto>();

        public FrmVenta(int cajaId, string usuario)
        {
            _cajaId = cajaId;
            _usuario = usuario;
            _productoService = new ProductoService();
            _ventaService = new VentaService();
            InitializeComponent();
            CargarNumeroVenta();
            CargarMetodosPago();
            CargarCatalogoProductos();
            this.Shown += FrmVenta_Shown;
        }

        private async void FrmVenta_Shown(object sender, EventArgs e)
        {
            txtBuscar.Focus();
            try
            {
                var syncService = new ProductSyncService();
                await syncService.ForceFullPull();
                CargarCatalogoProductos();
            }
            catch { }
        }

        private void CargarNumeroVenta()
        {
            lblNumeroVenta.Text = _ventaService.GenerarNumeroVenta();
        }

        private void CargarMetodosPago()
        {
            cmbMetodoPago.Items.Add("Efectivo");
            cmbMetodoPago.Items.Add("Tarjeta");
            cmbMetodoPago.Items.Add("Davivienda Bre-B QR");
            cmbMetodoPago.Items.Add("Transferencia");
            cmbMetodoPago.SelectedIndex = 0;
        }

        private void CargarCatalogoProductos()
        {
            _todosProductos = _productoService.GetAll();
            MostrarProductosEnCatalogo(_todosProductos);
        }

        private void CargarProductos()
        {
            var productos = _productoService.GetAll();
            MostrarProductosEnCatalogo(productos);
        }

        private void MostrarProductosEnCatalogo(List<Producto> productos)
        {
            _productosMostrados = productos;
            dgvCatalogo.Rows.Clear();
            foreach (var p in productos)
            {
                dgvCatalogo.Rows.Add(p.Nombre, p.Stock, p.PrecioVenta.ToString("C0"));
            }
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            string termino = txtBuscar.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(termino))
            {
                MostrarProductosEnCatalogo(_todosProductos);
            }
            else
            {
                var filtrados = _todosProductos.FindAll(p =>
                    p.Nombre.ToLower().Contains(termino) ||
                    (p.CodigoBarras != null && p.CodigoBarras.ToLower().Contains(termino)));

                if (filtrados.Count == 0)
                {
                    var producto = _productoService.GetByCodigoBarras(termino);
                    if (producto != null)
                        filtrados.Add(producto);
                }

                MostrarProductosEnCatalogo(filtrados);
            }
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (dgvCatalogo.Rows.Count > 0)
                {
                    dgvCatalogo.Focus();
                    if (dgvCatalogo.SelectedRows.Count == 0)
                        dgvCatalogo.Rows[0].Selected = true;
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (_productosMostrados.Count == 1)
                {
                    dgvCatalogo.Rows[0].Selected = true;
                    AgregarProductoSeleccionado();
                }
                else if (dgvCatalogo.SelectedRows.Count > 0)
                {
                    AgregarProductoSeleccionado();
                }
                else if (dgvCatalogo.Rows.Count > 0)
                {
                    dgvCatalogo.Rows[0].Selected = true;
                    AgregarProductoSeleccionado();
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void dgvCatalogo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (dgvCatalogo.SelectedRows.Count > 0)
                    AgregarProductoSeleccionado();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape || (e.KeyCode == Keys.Up && dgvCatalogo.SelectedRows.Count > 0 && dgvCatalogo.SelectedRows[0].Index == 0))
            {
                txtBuscar.Focus();
                txtBuscar.Select(txtBuscar.Text.Length, 0);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            txtBuscar.Focus();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            AgregarProductoSeleccionado();
        }

        private void AgregarProductoSeleccionado()
        {
            if (dgvCatalogo.SelectedRows.Count == 0) return;

            int rowIndex = dgvCatalogo.SelectedRows[0].Index;
            if (rowIndex < 0 || rowIndex >= _productosMostrados.Count) return;

            Producto producto = _productosMostrados[rowIndex];

            if (producto.Stock <= 0)
            {
                MessageBox.Show("No hay stock disponible", "Sin stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if product has variants
            ProductoVariante? varianteSel = null;
            var varianteRepo = new ProductoVarianteRepository();
            var variantes = varianteRepo.GetByProducto(producto.Id);
            bool tieneVariantes = variantes.Count > 0;

            if (tieneVariantes)
            {
                using (var dlgVar = new FrmSeleccionVariante(producto))
                {
                    if (dlgVar.ShowDialog() != DialogResult.OK) return;
                    varianteSel = dlgVar.VarianteSeleccionado;
                }

                // If user chose a variant but it has no stock
                if (varianteSel != null && varianteSel.Stock.HasValue && varianteSel.Stock <= 0)
                {
                    MessageBox.Show("No hay stock disponible para esta variante", "Sin stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            using (var dlg = new FrmCantidad(producto, varianteSel))
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                int cantidad = dlg.Cantidad;

                int stockDisponible = varianteSel?.Stock ?? producto.Stock;
                if (cantidad > stockDisponible)
                {
                    MessageBox.Show("Stock insuficiente. Disponible: " + stockDisponible, "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var detalleExistente = _detalles.Find(d =>
                    d.ProductoId == producto.Id && d.ProductoVarianteId == varianteSel?.Id);

                string nombreVariante = "";
                if (varianteSel != null)
                {
                    if (varianteSel.Nombre == "Única" && varianteSel.ColorHex == "#9E9E9E")
                        nombreVariante = varianteSel.Talla != null ? $"Talla {varianteSel.Talla}" : "Única";
                    else
                        nombreVariante = $"{varianteSel.Nombre}{(varianteSel.Talla != null ? $" Talla {varianteSel.Talla}" : "")}";
                }

                if (detalleExistente != null)
                {
                    int nuevaCantidad = detalleExistente.Cantidad + cantidad;
                    if (nuevaCantidad > stockDisponible)
                    {
                        MessageBox.Show("Stock insuficiente. Disponible: " + stockDisponible, "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    detalleExistente.Cantidad = nuevaCantidad;
                    detalleExistente.SubTotal = producto.PrecioVenta * nuevaCantidad;
                    detalleExistente.Total = producto.PrecioVenta * nuevaCantidad;
                }
                else
                {
                    _detalles.Add(new DetalleVenta
                    {
                        ProductoId = producto.Id,
                        ProductoVarianteId = varianteSel?.Id,
                        NombreProducto = producto.Nombre,
                        NombreVariante = string.IsNullOrEmpty(nombreVariante) ? null : nombreVariante,
                        Cantidad = cantidad,
                        PrecioUnitario = producto.PrecioVenta,
                        CostoUnitario = producto.PrecioCompra,
                        SubTotal = producto.PrecioVenta * cantidad,
                        Impuesto = 0,
                        Total = producto.PrecioVenta * cantidad
                    });
                }
            }

            ActualizarCarrito();
            ActualizarTotales();
            txtBuscar.Focus();
        }

        private void dgvCatalogo_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dgvCatalogo.Rows[e.RowIndex].Selected = true;
                AgregarProductoSeleccionado();
            }
        }

        private void ActualizarCarrito()
        {
            dgvCarrito.Rows.Clear();
            foreach (var detalle in _detalles)
            {
                string productoDisplay = detalle.NombreProducto;
                if (!string.IsNullOrEmpty(detalle.NombreVariante))
                    productoDisplay += $" ({detalle.NombreVariante})";

                dgvCarrito.Rows.Add(
                    productoDisplay,
                    detalle.Cantidad,
                    detalle.PrecioUnitario.ToString("C0"),
                    detalle.SubTotal.ToString("C0"));
            }
        }

        private void ActualizarTotales()
        {
            decimal subtotal = 0;
            foreach (var d in _detalles)
            {
                subtotal += d.SubTotal;
            }

            decimal impuesto = 0;
            _totalActual = subtotal + impuesto;

            lblSubtotal.Text = "Subtotal: " + subtotal.ToString("C0");
            lblImpuesto.Text = "Impuesto: " + impuesto.ToString("C0");
            lblTotal.Text = "TOTAL: " + _totalActual.ToString("C0");

            CalcularCambio();
        }

        private void CalcularCambio()
        {
            if (_totalActual == 0)
            {
                lblCambio.Text = "Cambio: $0";
                lblCambio.ForeColor = Color.FromArgb(39, 174, 96);
                return;
            }

            string textoMonto = txtMontoPagado.Text.Replace("$", "").Replace(".", "").Trim();
            if (decimal.TryParse(textoMonto, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal pagado))
            {
                decimal cambio = pagado - _totalActual;
                lblCambio.Text = "Cambio: " + cambio.ToString("C0");
                lblCambio.ForeColor = cambio >= 0 ? Color.FromArgb(39, 174, 96) : Color.FromArgb(192, 57, 43);
            }
            else
            {
                lblCambio.Text = "Cambio: ---";
                lblCambio.ForeColor = Color.Gray;
            }
        }

        private bool _formateandoMonto = false;

        private void txtMontoPagado_TextChanged(object sender, EventArgs e)
        {
            if (_formateandoMonto) return;

            _formateandoMonto = true;

            string texto = txtMontoPagado.Text.Replace("$", "").Replace(".", "").Trim();
            if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal monto))
            {
                int cursorPos = txtMontoPagado.SelectionStart;
                int longitudAntes = txtMontoPagado.Text.Length;
                txtMontoPagado.Text = monto.ToString("C0");
                int longitudDespues = txtMontoPagado.Text.Length;
                int nuevaPos = cursorPos + (longitudDespues - longitudAntes);
                txtMontoPagado.SelectionStart = Math.Max(0, nuevaPos);
            }

            _formateandoMonto = false;

            CalcularCambio();
        }

        private void btnMontoExacto_Click(object sender, EventArgs e)
        {
            txtMontoPagado.Text = _totalActual.ToString("C0");
        }

        private void btnCobrar_Click(object sender, EventArgs e)
        {
            try
            {
                if (_detalles.Count == 0)
                {
                    MessageBox.Show("Agregue al menos un producto", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal total = 0;
                foreach (var d in _detalles) total += d.SubTotal;

                decimal montoPagado;
                if (cmbMetodoPago.Text == "Efectivo")
                {
                    string textoMonto = txtMontoPagado.Text.Replace("$", "").Replace(".", "").Trim();
                    if (!decimal.TryParse(textoMonto, NumberStyles.Number, CultureInfo.CurrentCulture, out montoPagado) || montoPagado < total)
                    {
                        MessageBox.Show("El monto pagado debe ser mayor o igual al total", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else
                {
                    montoPagado = total;
                }

                var venta = new Venta
                {
                    NumeroVenta = lblNumeroVenta.Text,
                    CajaId = _cajaId,
                    SubTotal = total,
                    Impuesto = 0,
                    Total = total,
                    MetodoPago = cmbMetodoPago.Text,
                    MontoPagado = montoPagado,
                    Cambio = montoPagado - total,
                    Usuario = _usuario,
                    NombreCliente = string.IsNullOrWhiteSpace(txtNombreCliente.Text) ? "Consumidor Final" : txtNombreCliente.Text.Trim(),
                    TipoDocumentoCliente = string.IsNullOrWhiteSpace(cmbTipoDocumento.Text) ? "" : cmbTipoDocumento.Text.Trim(),
                    DocumentoCliente = string.IsNullOrWhiteSpace(txtDocumentoCliente.Text) ? "" : txtDocumentoCliente.Text.Trim()
                };

                // Guardar detalles antes de que se limpien
                var detallesParaImprimir = new List<DetalleVenta>(_detalles);

                var resultado = _ventaService.RegistrarVenta(venta, _detalles);

                string puerto = null;
                try
                {
                    puerto = EpsonPrinterHelper.DetectarImpresoraEpson();
                }
                catch
                {
                }

                bool imprimirRecibo = false;
                if (!string.IsNullOrEmpty(puerto))
                {
                    imprimirRecibo = MessageBox.Show("¿Desea imprimir el recibo?", "Imprimir Recibo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                }

                if (imprimirRecibo)
                {
                    try
                    {
                        var configService = new ConfiguracionService();
                        string nit = configService.LeerConfig("NIT", "123456789-0");
                        string direccion = configService.LeerConfig("DIRECCION", "Calle 123 #45-67");
                        string telefono = configService.LeerConfig("TELEFONO", "601 123 4567");
                        string nombreEmpresa = configService.LeerConfig("NOMBRE_EMPRESA", "Sistema de Ventas SAS");

                        string nombreCliente = string.IsNullOrWhiteSpace(txtNombreCliente.Text) ? "Consumidor Final" : txtNombreCliente.Text.Trim();
                        string documentoCliente = "";
                        if (!string.IsNullOrWhiteSpace(cmbTipoDocumento.Text) || !string.IsNullOrWhiteSpace(txtDocumentoCliente.Text))
                        {
                            documentoCliente = $"{cmbTipoDocumento.Text} {txtDocumentoCliente.Text}".Trim();
                        }

                        EpsonPrinterHelper.ImprimirFacturaEpson(
                            puerto, nombreEmpresa, nit, direccion, telefono,
                            venta, detallesParaImprimir, nombreCliente, documentoCliente);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al imprimir: {ex.Message}", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                try
                {
                    if (!string.IsNullOrEmpty(puerto))
                        EpsonPrinterHelper.AbrirCaja(puerto);
                }
                catch
                {
                }

                MessageBox.Show("Venta registrada exitosamente!\nNumero: " + venta.NumeroVenta + "\nTotal: " + total.ToString("C0") + "\nCambio: " + venta.Cambio.ToString("C0"),
                    "Venta Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);

                CargarCatalogoProductos();

                _detalles.Clear();
                ActualizarCarrito();
                ActualizarTotales();
                txtMontoPagado.Clear();
                lblCambio.Text = "Cambio: $0";
                CargarNumeroVenta();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (_detalles.Count > 0 && MessageBox.Show("Cancelar la venta actual?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _detalles.Clear();
                ActualizarCarrito();
                ActualizarTotales();
                txtMontoPagado.Clear();
                lblCambio.Text = "Cambio: $0";
                CargarNumeroVenta();
            }
        }

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.StartPosition = FormStartPosition.CenterParent;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 245, 245);

            var panelCatalogo = new Panel();
            panelCatalogo.Dock = DockStyle.Fill;
            panelCatalogo.Padding = new Padding(10);

            var lblCatalogo = new Label();
            lblCatalogo.Text = "Catalogo de Productos";
            lblCatalogo.Font = new System.Drawing.Font("Segoe UI", 14, FontStyle.Bold);
            lblCatalogo.ForeColor = Color.FromArgb(44, 62, 80);
            lblCatalogo.Location = new System.Drawing.Point(10, 10);
            lblCatalogo.Size = new Size(250, 30);
            lblCatalogo.AutoSize = false;

            this.txtBuscar = new TextBox();
            this.txtBuscar.Location = new System.Drawing.Point(10, 50);
            this.txtBuscar.Size = new Size(500, 30);
            this.txtBuscar.Font = new System.Drawing.Font("Segoe UI", 12);
            this.txtBuscar.PlaceholderText = "Buscar producto...";
            this.txtBuscar.TextChanged += new EventHandler(this.txtBuscar_TextChanged);
            this.txtBuscar.KeyDown += new KeyEventHandler(this.txtBuscar_KeyDown);

            this.btnBuscar = new Button();
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.Location = new System.Drawing.Point(520, 50);
            this.btnBuscar.Size = new Size(100, 30);
            this.btnBuscar.Font = new System.Drawing.Font("Segoe UI", 10);
            this.btnBuscar.Click += new EventHandler(this.btnBuscar_Click);

            this.dgvCatalogo = new DataGridView();
            this.dgvCatalogo.Location = new System.Drawing.Point(10, 90);
            this.dgvCatalogo.Size = new Size(610, 400);
            this.dgvCatalogo.AllowUserToAddRows = false;
            this.dgvCatalogo.ReadOnly = true;
            this.dgvCatalogo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvCatalogo.BackgroundColor = System.Drawing.Color.White;
            this.dgvCatalogo.Columns.Add("Nombre", "Producto");
            this.dgvCatalogo.Columns.Add("Stock", "Stock");
            this.dgvCatalogo.Columns.Add("Precio", "Precio");
            this.dgvCatalogo.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dgvCatalogo.Columns["Stock"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgvCatalogo.Columns["Precio"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgvCatalogo.CellDoubleClick += new DataGridViewCellEventHandler(this.dgvCatalogo_CellDoubleClick);
            this.dgvCatalogo.KeyDown += new KeyEventHandler(this.dgvCatalogo_KeyDown);

            this.btnAgregar = new Button();
            this.btnAgregar.Text = "Agregar al Carrito";
            this.btnAgregar.BackColor = Color.FromArgb(52, 152, 219);
            this.btnAgregar.ForeColor = Color.White;
            this.btnAgregar.Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold);
            this.btnAgregar.Location = new System.Drawing.Point(10, 500);
            this.btnAgregar.Size = new Size(200, 35);
            this.btnAgregar.Click += new EventHandler(this.btnAgregar_Click);

            var panelDerecho = new Panel();
            panelDerecho.Dock = DockStyle.Right;
            panelDerecho.Width = 380;
            panelDerecho.BackColor = Color.FromArgb(236, 240, 241);
            panelDerecho.Padding = new Padding(15);

            var lblCarrito = new Label();
            lblCarrito.Text = "Carrito de Venta";
            lblCarrito.Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold);
            lblCarrito.ForeColor = Color.FromArgb(44, 62, 80);
            lblCarrito.Location = new System.Drawing.Point(15, 10);
            lblCarrito.Size = new Size(350, 25);

            // Campo para nombre del cliente
            var lblCliente = new Label();
            lblCliente.Text = "Cliente (opcional):";
            lblCliente.Font = new System.Drawing.Font("Segoe UI", 9);
            lblCliente.Location = new System.Drawing.Point(15, 40);
            lblCliente.Size = new Size(350, 20);

            this.txtNombreCliente = new TextBox();
            this.txtNombreCliente.Location = new System.Drawing.Point(15, 60);
            this.txtNombreCliente.Size = new Size(350, 25);
            this.txtNombreCliente.Font = new System.Drawing.Font("Segoe UI", 10);
            this.txtNombreCliente.PlaceholderText = "Ingrese nombre del cliente";

            var lblDocumento = new Label();
            lblDocumento.Text = "Cédula/NIT (opcional):";
            lblDocumento.Font = new System.Drawing.Font("Segoe UI", 9);
            lblDocumento.Location = new System.Drawing.Point(15, 90);
            lblDocumento.Size = new Size(350, 20);

var lblTipoDoc = new Label();
            lblTipoDoc.Text = "Tipo:";
            lblTipoDoc.Font = new System.Drawing.Font("Segoe UI", 9);
            lblTipoDoc.Location = new System.Drawing.Point(15, 110);
            lblTipoDoc.Size = new Size(50, 20);

            this.cmbTipoDocumento = new ComboBox();
            this.cmbTipoDocumento.Location = new System.Drawing.Point(55, 108);
            this.cmbTipoDocumento.Size = new Size(100, 25);
            this.cmbTipoDocumento.Font = new System.Drawing.Font("Segoe UI", 9);
            this.cmbTipoDocumento.DropDownStyle = ComboBoxStyle.DropDown;
            this.cmbTipoDocumento.DropDownWidth = 100;
            this.cmbTipoDocumento.Items.Add("CC");
            this.cmbTipoDocumento.Items.Add("NIT");
            this.cmbTipoDocumento.Items.Add("CE");
            this.cmbTipoDocumento.Items.Add("PP");
            this.cmbTipoDocumento.Items.Add("TI");
            this.cmbTipoDocumento.Items.Add("RC");
            this.cmbTipoDocumento.SelectedIndex = 0;

            var lblNumDoc = new Label();
            lblNumDoc.Text = "Número:";
            lblNumDoc.Font = new System.Drawing.Font("Segoe UI", 9);
            lblNumDoc.Location = new System.Drawing.Point(165, 110);
            lblNumDoc.Size = new Size(60, 20);

            this.txtDocumentoCliente = new TextBox();
            this.txtDocumentoCliente.Location = new System.Drawing.Point(225, 108);
            this.txtDocumentoCliente.Size = new Size(140, 25);
            this.txtDocumentoCliente.Font = new System.Drawing.Font("Segoe UI", 10);
            this.txtDocumentoCliente.PlaceholderText = "Número";

            this.dgvCarrito = new DataGridView();
            this.dgvCarrito.Location = new System.Drawing.Point(15, 175);
            this.dgvCarrito.Size = new Size(350, 170);
            this.dgvCarrito.AllowUserToAddRows = false;
            this.dgvCarrito.ReadOnly = true;
            this.dgvCarrito.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvCarrito.BackgroundColor = System.Drawing.Color.White;
            this.dgvCarrito.Columns.Add("Producto", "Producto");
            this.dgvCarrito.Columns.Add("Cant", "Cant");
            this.dgvCarrito.Columns.Add("Precio", "Precio");
            this.dgvCarrito.Columns.Add("SubTotal", "Subtotal");
            this.dgvCarrito.Columns["Producto"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dgvCarrito.Columns["Cant"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgvCarrito.Columns["Precio"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgvCarrito.Columns["SubTotal"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;

            this.lblNumeroVenta = new Label();
            this.lblNumeroVenta.Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold);
            this.lblNumeroVenta.ForeColor = Color.FromArgb(44, 62, 80);
            this.lblNumeroVenta.Location = new System.Drawing.Point(15, 325);
            this.lblNumeroVenta.Size = new Size(350, 25);

            this.lblSubtotal = new Label();
            this.lblSubtotal.Font = new System.Drawing.Font("Segoe UI", 11);
            this.lblSubtotal.Location = new System.Drawing.Point(15, 355);
            this.lblSubtotal.Size = new Size(350, 25);

            this.lblImpuesto = new Label();
            this.lblImpuesto.Font = new System.Drawing.Font("Segoe UI", 11);
            this.lblImpuesto.Location = new System.Drawing.Point(15, 380);
            this.lblImpuesto.Size = new Size(350, 25);

            this.lblTotal = new Label();
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 18, FontStyle.Bold);
            this.lblTotal.ForeColor = Color.FromArgb(44, 62, 80);
            this.lblTotal.Location = new System.Drawing.Point(15, 410);
            this.lblTotal.Size = new Size(350, 40);
            this.lblTotal.Text = "TOTAL: $0";

            var lblMetodoPago = new Label();
            lblMetodoPago.Text = "Metodo de Pago:";
            lblMetodoPago.Font = new System.Drawing.Font("Segoe UI", 10);
            lblMetodoPago.Location = new System.Drawing.Point(15, 460);
            lblMetodoPago.Size = new Size(150, 25);

            this.cmbMetodoPago = new ComboBox();
            this.cmbMetodoPago.Location = new System.Drawing.Point(15, 485);
            this.cmbMetodoPago.Size = new Size(350, 30);
            this.cmbMetodoPago.DropDownStyle = ComboBoxStyle.DropDownList;

            var lblMontoPagado = new Label();
            lblMontoPagado.Text = "Monto Pagado:";
            lblMontoPagado.Font = new System.Drawing.Font("Segoe UI", 10);
            lblMontoPagado.Location = new System.Drawing.Point(15, 525);
            lblMontoPagado.Size = new Size(150, 25);

            this.txtMontoPagado = new TextBox();
            this.txtMontoPagado.Location = new System.Drawing.Point(15, 550);
            this.txtMontoPagado.Size = new Size(200, 30);
            this.txtMontoPagado.Font = new System.Drawing.Font("Segoe UI", 14, FontStyle.Bold);
            this.txtMontoPagado.TextChanged += new EventHandler(this.txtMontoPagado_TextChanged);

            this.btnMontoExacto = new Button();
            this.btnMontoExacto.Text = "Monto Exacto";
            this.btnMontoExacto.BackColor = Color.FromArgb(52, 152, 219);
            this.btnMontoExacto.ForeColor = Color.White;
            this.btnMontoExacto.Font = new System.Drawing.Font("Segoe UI", 10);
            this.btnMontoExacto.Location = new System.Drawing.Point(225, 550);
            this.btnMontoExacto.Size = new Size(140, 30);
            this.btnMontoExacto.Click += new EventHandler(this.btnMontoExacto_Click);

            this.lblCambio = new Label();
            this.lblCambio.Font = new System.Drawing.Font("Segoe UI", 14, FontStyle.Bold);
            this.lblCambio.ForeColor = Color.FromArgb(39, 174, 96);
            this.lblCambio.Location = new System.Drawing.Point(15, 590);
            this.lblCambio.Size = new Size(350, 30);
            this.lblCambio.Text = "Cambio: $0";

            this.btnCobrar = new Button();
            this.btnCobrar.Text = "COBRAR";
            this.btnCobrar.BackColor = Color.FromArgb(39, 174, 96);
            this.btnCobrar.ForeColor = Color.White;
            this.btnCobrar.Font = new System.Drawing.Font("Segoe UI", 14, FontStyle.Bold);
            this.btnCobrar.Location = new System.Drawing.Point(15, 635);
            this.btnCobrar.Size = new Size(350, 50);
            this.btnCobrar.Click += new EventHandler(this.btnCobrar_Click);

            this.btnCancelar = new Button();
            this.btnCancelar.Text = "Cancelar Venta";
            this.btnCancelar.BackColor = Color.FromArgb(192, 57, 43);
            this.btnCancelar.ForeColor = Color.White;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold);
            this.btnCancelar.Location = new System.Drawing.Point(15, 695);
            this.btnCancelar.Size = new Size(350, 45);
            this.btnCancelar.Click += new EventHandler(this.btnCancelar_Click);

            panelCatalogo.Controls.Add(lblCatalogo);
            panelCatalogo.Controls.Add(this.txtBuscar);
            panelCatalogo.Controls.Add(this.btnBuscar);
            panelCatalogo.Controls.Add(this.dgvCatalogo);
            panelCatalogo.Controls.Add(this.btnAgregar);

            panelDerecho.Controls.Add(lblCarrito);
            panelDerecho.Controls.Add(lblCliente);
            panelDerecho.Controls.Add(this.txtNombreCliente);
            panelDerecho.Controls.Add(lblDocumento);
            panelDerecho.Controls.Add(lblTipoDoc);
            panelDerecho.Controls.Add(this.cmbTipoDocumento);
            panelDerecho.Controls.Add(lblNumDoc);
            panelDerecho.Controls.Add(this.txtDocumentoCliente);
            panelDerecho.Controls.Add(this.dgvCarrito);
            panelDerecho.Controls.Add(this.lblNumeroVenta);
            panelDerecho.Controls.Add(this.lblSubtotal);
            panelDerecho.Controls.Add(this.lblImpuesto);
            panelDerecho.Controls.Add(this.lblTotal);
            panelDerecho.Controls.Add(lblMetodoPago);
            panelDerecho.Controls.Add(this.cmbMetodoPago);
            panelDerecho.Controls.Add(lblMontoPagado);
            panelDerecho.Controls.Add(this.txtMontoPagado);
            panelDerecho.Controls.Add(this.btnMontoExacto);
            panelDerecho.Controls.Add(this.lblCambio);
            panelDerecho.Controls.Add(this.btnCobrar);
            panelDerecho.Controls.Add(this.btnCancelar);

            this.Controls.Add(panelDerecho);
            this.Controls.Add(panelCatalogo);
        }
    }
}
