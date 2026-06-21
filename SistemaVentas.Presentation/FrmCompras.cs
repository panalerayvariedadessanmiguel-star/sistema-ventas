using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SistemaVentas.Business.Services;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Presentation
{
    public partial class FrmCompras : Form
    {
        private readonly ProductoService _productoService;
        private readonly CompraService _compraService;
        private readonly string _usuario;
        private readonly List<DetalleCompra> _detalles = new List<DetalleCompra>();
        private DataGridView dgvCarrito;
        private DataGridView dgvCatalogo;
        private TextBox txtBuscar;
        private Label lblTotal;
        private Label lblNumeroCompra;
        private TextBox txtProveedor;
        private Button btnGuardar;
        private Button btnCancelar;
        private decimal _totalActual = 0;
        private List<Producto> _todosProductos = new List<Producto>();
        private List<Producto> _productosMostrados = new List<Producto>();

        public FrmCompras(string usuario)
        {
            _usuario = usuario;
            _productoService = new ProductoService();
            _compraService = new CompraService();
            InitializeComponent();
            CargarNumeroCompra();
            CargarCatalogoProductos();
            Shown += (s, e) => txtBuscar.Focus();
        }

        private void CargarNumeroCompra()
        {
            lblNumeroCompra.Text = _compraService.GenerarNumeroCompra();
        }

        private void CargarCatalogoProductos()
        {
            _todosProductos = _productoService.GetAll();
            MostrarProductosEnCatalogo(_todosProductos);
        }

        private void MostrarProductosEnCatalogo(List<Producto> productos)
        {
            _productosMostrados = productos;
            dgvCatalogo.Rows.Clear();
            foreach (var p in productos)
            {
                dgvCatalogo.Rows.Add(p.Nombre, p.Stock, p.PrecioCompra.ToString("N0"), p.PrecioVenta.ToString("N0"));
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
                MostrarProductosEnCatalogo(filtrados);
            }
        }

        private void dgvCatalogo_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dgvCatalogo.Rows[e.RowIndex].Selected = true;
                AgregarProductoSeleccionado();
            }
        }

        private void AgregarProductoSeleccionado()
        {
            if (dgvCatalogo.SelectedRows.Count == 0) return;
            int rowIndex = dgvCatalogo.SelectedRows[0].Index;
            if (rowIndex < 0 || rowIndex >= _productosMostrados.Count) return;

            var producto = _productosMostrados[rowIndex];

            using (var dlg = new FrmCantidadCompra(producto))
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;

                var detalleExistente = _detalles.Find(d => d.ProductoId == producto.Id);
                if (detalleExistente != null)
                {
                    detalleExistente.Cantidad += dlg.Cantidad;
                    detalleExistente.PrecioUnitario = dlg.PrecioUnitario;
                    detalleExistente.SubTotal = detalleExistente.Cantidad * detalleExistente.PrecioUnitario;
                }
                else
                {
                    _detalles.Add(new DetalleCompra
                    {
                        ProductoId = producto.Id,
                        NombreProducto = producto.Nombre,
                        Cantidad = dlg.Cantidad,
                        PrecioUnitario = dlg.PrecioUnitario,
                        SubTotal = dlg.Cantidad * dlg.PrecioUnitario
                    });
                }
            }

            ActualizarCarrito();
            ActualizarTotales();
        }

        private void ActualizarCarrito()
        {
            dgvCarrito.Rows.Clear();
            foreach (var d in _detalles)
            {
                dgvCarrito.Rows.Add(d.NombreProducto, d.Cantidad, d.PrecioUnitario.ToString("N0"), d.SubTotal.ToString("N0"));
            }
        }

        private void ActualizarTotales()
        {
            _totalActual = 0;
            foreach (var d in _detalles)
                _totalActual += d.SubTotal;
            lblTotal.Text = "TOTAL: " + _totalActual.ToString("N0");
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (_detalles.Count == 0)
                {
                    MessageBox.Show("Agregue al menos un producto", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtProveedor.Text))
                {
                    MessageBox.Show("Ingrese el nombre del proveedor", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtProveedor.Focus();
                    return;
                }

                var compra = new Compra
                {
                    NumeroCompra = lblNumeroCompra.Text,
                    Proveedor = txtProveedor.Text.Trim(),
                    Usuario = _usuario
                };

                _compraService.RegistrarCompra(compra, _detalles);

                MessageBox.Show("Compra registrada exitosamente!\nNumero: " + compra.NumeroCompra + "\nTotal: " + _totalActual.ToString("N0"),
                    "Compra Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _detalles.Clear();
                ActualizarCarrito();
                ActualizarTotales();
                txtProveedor.Clear();
                CargarNumeroCompra();
                CargarCatalogoProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (_detalles.Count > 0 &&
                MessageBox.Show("Cancelar la compra actual?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _detalles.Clear();
                ActualizarCarrito();
                ActualizarTotales();
                txtProveedor.Clear();
                CargarNumeroCompra();
            }
        }

        private void InitializeComponent()
        {
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(245, 245, 245);

            var panelCatalogo = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var lblCatalogo = new Label
            {
                Text = "Catalogo de Productos",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(10, 10),
                Size = new Size(250, 30)
            };

            txtBuscar = new TextBox
            {
                Location = new Point(10, 50),
                Size = new Size(500, 30),
                Font = new Font("Segoe UI", 12),
                PlaceholderText = "Buscar producto..."
            };
            txtBuscar.TextChanged += txtBuscar_TextChanged;

            dgvCatalogo = new DataGridView
            {
                Location = new Point(10, 90),
                Size = new Size(610, 400),
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };
            dgvCatalogo.Columns.Add("Nombre", "Producto");
            dgvCatalogo.Columns.Add("Stock", "Stock");
            dgvCatalogo.Columns.Add("PrecioCompra", "Costo");
            dgvCatalogo.Columns.Add("PrecioVenta", "Venta");
            dgvCatalogo.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvCatalogo.CellDoubleClick += dgvCatalogo_CellDoubleClick;

            panelCatalogo.Controls.Add(lblCatalogo);
            panelCatalogo.Controls.Add(txtBuscar);
            panelCatalogo.Controls.Add(dgvCatalogo);

            var panelDerecho = new Panel
            {
                Dock = DockStyle.Right,
                Width = 380,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(15)
            };

            var lblCarrito = new Label
            {
                Text = "Detalle de Compra",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(15, 10),
                Size = new Size(350, 25)
            };

            var lblProv = new Label
            {
                Text = "Proveedor:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(15, 45),
                Size = new Size(80, 25)
            };

            txtProveedor = new TextBox
            {
                Location = new Point(95, 43),
                Size = new Size(270, 25),
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "Nombre del proveedor"
            };

            lblNumeroCompra = new Label
            {
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(15, 78),
                Size = new Size(350, 25)
            };

            dgvCarrito = new DataGridView
            {
                Location = new Point(15, 115),
                Size = new Size(350, 250),
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };
            dgvCarrito.Columns.Add("Producto", "Producto");
            dgvCarrito.Columns.Add("Cant", "Cant");
            dgvCarrito.Columns.Add("Precio", "Precio");
            dgvCarrito.Columns.Add("SubTotal", "Subtotal");
            dgvCarrito.Columns["Producto"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            lblTotal = new Label
            {
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(15, 380),
                Size = new Size(350, 40),
                Text = "TOTAL: $0"
            };

            btnGuardar = new Button
            {
                Text = "GUARDAR COMPRA",
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(15, 440),
                Size = new Size(350, 50)
            };
            btnGuardar.Click += btnGuardar_Click;

            btnCancelar = new Button
            {
                Text = "Cancelar",
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(15, 500),
                Size = new Size(350, 45)
            };
            btnCancelar.Click += btnCancelar_Click;

            panelDerecho.Controls.Add(lblCarrito);
            panelDerecho.Controls.Add(lblProv);
            panelDerecho.Controls.Add(txtProveedor);
            panelDerecho.Controls.Add(lblNumeroCompra);
            panelDerecho.Controls.Add(dgvCarrito);
            panelDerecho.Controls.Add(lblTotal);
            panelDerecho.Controls.Add(btnGuardar);
            panelDerecho.Controls.Add(btnCancelar);

            Controls.Add(panelDerecho);
            Controls.Add(panelCatalogo);
        }
    }
}
