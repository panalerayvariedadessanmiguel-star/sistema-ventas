using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using SistemaVentas.Business.Services;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Presentation
{
    public partial class FrmProductos : Form
    {
        private readonly ProductoService _productoService;
        private readonly CategoriaService _categoriaService;
        private readonly ConfiguracionService _configService;

        private decimal _margenFijos;
        private decimal _margenVariables;
        private decimal _margenUtilidad;
        private Label lblInfoMargen;
        private Button btnConfigMargenes;

        public FrmProductos()
        {
            _productoService = new ProductoService();
            _categoriaService = new CategoriaService();
            _configService = new ConfiguracionService();
            InitializeComponent();
            CargarMargenes();
            CargarProductos();
            CargarCategorias();
            LimpiarFormulario();
        }

        private void CargarMargenes()
        {
            _margenFijos = _configService.LeerDecimal("MARGEN_FIJOS", 25);
            _margenVariables = _configService.LeerDecimal("MARGEN_VARIABLES", 5);
            _margenUtilidad = _configService.LeerDecimal("MARGEN_UTILIDAD", 15);
            ActualizarLabelMargen();
        }

        private void ActualizarLabelMargen()
        {
            decimal total = _margenFijos + _margenVariables + _margenUtilidad;
            lblInfoMargen.Text = $"Margen: C.Fijos {_margenFijos:0}% + C.Var {_margenVariables:0}% + Utilidad {_margenUtilidad:0}% = {total:0}%";
        }

        private decimal CalcularPrecioVenta(decimal precioCompra)
        {
            decimal margenTotal = _margenFijos + _margenVariables + _margenUtilidad;
            if (margenTotal <= 0 || margenTotal >= 100 || precioCompra <= 0)
                return 0;
            return Math.Round(precioCompra / (1 - margenTotal / 100m), 2);
        }

        private decimal? ParsePrecio(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;
            var normalizado = texto.Trim().Replace(".", "").Replace(",", ".");
            if (decimal.TryParse(normalizado, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal r))
                return r;
            return null;
        }

        private void FormatearPrecio(TextBox txt)
        {
            var val = ParsePrecio(txt.Text);
            if (val.HasValue)
                txt.Text = val.Value.ToString("N2");
        }

        private void txtPrecioCompra_Leave(object sender, EventArgs e)
        {
            FormatearPrecio(txtPrecioCompra);
            var val = ParsePrecio(txtPrecioCompra.Text);
            if (val.HasValue && val.Value > 0)
            {
                decimal precioVenta = CalcularPrecioVenta(val.Value);
                if (precioVenta > 0)
                    txtPrecioVenta.Text = precioVenta.ToString("N2");
            }
        }

        private void txtPrecioVenta_Leave(object sender, EventArgs e)
        {
            FormatearPrecio(txtPrecioVenta);
        }

        private void btnConfigMargenes_Click(object sender, EventArgs e)
        {
            var frm = new FrmConfiguracionMargenes();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                _margenFijos = frm.MargenFijos;
                _margenVariables = frm.MargenVariables;
                _margenUtilidad = frm.MargenUtilidad;
                ActualizarLabelMargen();

                if (decimal.TryParse(txtPrecioCompra.Text, out decimal precioCompra) && precioCompra > 0)
                {
                    decimal precioVenta = CalcularPrecioVenta(precioCompra);
                    if (precioVenta > 0)
                        txtPrecioVenta.Text = precioVenta.ToString("0.00");
                }
            }
        }

        private void CargarProductos()
        {
            dgvProductos.Rows.Clear();
            var productos = _productoService.GetAll();
            foreach (var p in productos)
            {
                string codigos = p.CodigoBarras;
                if (p.CodigosBarras != null && p.CodigosBarras.Count > 0)
                {
                    codigos += " (+" + p.CodigosBarras.Count + ")";
                }
                dgvProductos.Rows.Add(p.Id, codigos, p.Nombre, p.NombreCategoria, p.Descripcion, p.PrecioCompra.ToString("C2"), p.PrecioVenta.ToString("C2"), p.Stock, p.StockMinimo, p.FechaCreacion.ToShortDateString(), p.Activo ? "Si" : "No");
            }
        }

        private void CargarCategorias()
        {
            cmbCategoria.Items.Clear();
            cmbCategoria.Items.Add(new { Id = 0, Nombre = "Seleccionar..." });
            var categorias = _categoriaService.GetAll();
            foreach (var c in categorias)
            {
                cmbCategoria.Items.Add(new { Id = c.Id, Nombre = c.Nombre });
            }
            cmbCategoria.DisplayMember = "Nombre";
            cmbCategoria.ValueMember = "Id";
            cmbCategoria.SelectedIndex = 0;
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MessageBox.Show("Ingrese el nombre del producto", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var pc = ParsePrecio(txtPrecioCompra.Text);
                var pv = ParsePrecio(txtPrecioVenta.Text);
                if (!pc.HasValue || !pv.HasValue)
                {
                    MessageBox.Show("Ingrese precios validos", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                decimal precioCompra = pc.Value;
                decimal precioVenta = pv.Value;

                int stock, stockMinimo;
                if (!int.TryParse(txtStock.Text, out stock) || !int.TryParse(txtStockMinimo.Text, out stockMinimo))
                {
                    MessageBox.Show("Ingrese valores de stock validos", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var categoriaItem = cmbCategoria.SelectedItem as dynamic;
                int categoriaId = categoriaItem != null ? categoriaItem.Id : 0;

                if (categoriaId == 0)
                {
                    MessageBox.Show("Seleccione una categoria", "Adventencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var producto = new Producto
                {
                    Id = string.IsNullOrEmpty(txtId.Text) ? 0 : int.Parse(txtId.Text),
                    CodigoBarras = txtCodigoBarras.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Descripcion = txtDescripcion.Text.Trim(),
                    CategoriaId = categoriaId,
                    PrecioCompra = precioCompra,
                    PrecioVenta = precioVenta,
                    Stock = stock,
                    StockMinimo = stockMinimo,
                    Activo = chkActivo.Checked,
                    FechaCreacion = string.IsNullOrEmpty(txtId.Text) ? DateTime.Now : DateTime.Parse(txtFechaCreacion.Text)
                };

                foreach (var item in lstCodigos.Items)
                {
                    producto.CodigosBarras.Add(new ProductoCodigoBarras
                    {
                        CodigoBarras = item.ToString()
                    });
                }

                if (producto.Id == 0)
                {
                    _productoService.Create(producto);
                    MessageBox.Show("Producto creado exitosamente", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _productoService.Update(producto);
                    MessageBox.Show("Producto actualizado exitosamente", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                CargarProductos();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text)) return;

            if (MessageBox.Show("Esta seguro de eliminar este producto?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _productoService.Delete(int.Parse(txtId.Text));
                MessageBox.Show("Producto eliminado", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarProductos();
                LimpiarFormulario();
            }
        }

        private void dgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dgvProductos.Rows[e.RowIndex];
                int id = (int)row.Cells["colId"].Value;
                var producto = _productoService.GetById(id);

                txtId.Text = id.ToString();
                txtNombre.Text = producto.Nombre;
                txtDescripcion.Text = producto.Descripcion;
                txtPrecioCompra.Text = producto.PrecioCompra.ToString("0.00");
                txtPrecioVenta.Text = producto.PrecioVenta.ToString("0.00");
                txtStock.Text = producto.Stock.ToString();
                txtStockMinimo.Text = producto.StockMinimo.ToString();
                txtFechaCreacion.Text = producto.FechaCreacion.ToShortDateString();
                chkActivo.Checked = producto.Activo;
                txtCodigoBarras.Text = producto.CodigoBarras;

                lstCodigos.Items.Clear();
                if (producto.CodigosBarras != null)
                {
                    foreach (var b in producto.CodigosBarras)
                    {
                        lstCodigos.Items.Add(b.CodigoBarras);
                    }
                }
            }
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            string termino = txtBuscar.Text.Trim();
            if (string.IsNullOrEmpty(termino))
            {
                CargarProductos();
            }
            else
            {
                dgvProductos.Rows.Clear();
                var productos = _productoService.Search(termino);
                foreach (var p in productos)
                {
                    string codigos = p.CodigoBarras;
                    if (p.CodigosBarras != null && p.CodigosBarras.Count > 0)
                    {
                        codigos += " (+" + p.CodigosBarras.Count + ")";
                    }
                    dgvProductos.Rows.Add(p.Id, codigos, p.Nombre, p.NombreCategoria, p.Descripcion, p.PrecioCompra.ToString("C2"), p.PrecioVenta.ToString("C2"), p.Stock, p.StockMinimo, p.FechaCreacion.ToShortDateString(), p.Activo ? "Si" : "No");
                }
            }
        }

        private void btnAgregarCodigo_Click(object sender, EventArgs e)
        {
            string codigo = txtNuevoCodigo.Text.Trim();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                MessageBox.Show("Ingrese un codigo de barras", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (lstCodigos.Items.Contains(codigo))
            {
                MessageBox.Show("El codigo ya esta en la lista", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            lstCodigos.Items.Add(codigo);
            txtNuevoCodigo.Clear();
            txtNuevoCodigo.Focus();
        }

        private void btnQuitarCodigo_Click(object sender, EventArgs e)
        {
            if (lstCodigos.SelectedItem != null)
            {
                lstCodigos.Items.Remove(lstCodigos.SelectedItem);
            }
        }

        private void LimpiarFormulario()
        {
            txtId.Clear();
            txtCodigoBarras.Clear();
            txtNombre.Clear();
            txtDescripcion.Clear();
            txtPrecioCompra.Clear();
            txtPrecioVenta.Clear();
            txtStock.Text = "0";
            txtStockMinimo.Text = "5";
            txtFechaCreacion.Clear();
            chkActivo.Checked = true;
            cmbCategoria.SelectedIndex = 0;
            lstCodigos.Items.Clear();
            txtNuevoCodigo.Clear();
        }
    }

    partial class FrmProductos
    {
        private System.ComponentModel.IContainer components = null;
        private DataGridView dgvProductos;
        private TextBox txtId;
        private TextBox txtCodigoBarras;
        private TextBox txtNombre;
        private TextBox txtDescripcion;
        private TextBox txtPrecioCompra;
        private TextBox txtPrecioVenta;
        private TextBox txtStock;
        private TextBox txtStockMinimo;
        private ComboBox cmbCategoria;
        private TextBox txtBuscar;
        private TextBox txtFechaCreacion;
        private CheckBox chkActivo;
        private Button btnNuevo;
        private Button btnGuardar;
        private Button btnEliminar;
        private ListBox lstCodigos;
        private TextBox txtNuevoCodigo;
        private Button btnAgregarCodigo;
        private Button btnQuitarCodigo;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Text = "Gestion de Productos";
            this.Size = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterParent;

            // Panel Formulario
            var panelForm = new Panel();
            panelForm.Dock = DockStyle.Top;
            panelForm.Height = 310;
            panelForm.BackColor = Color.FromArgb(236, 240, 241);
            panelForm.Padding = new Padding(10);

            var y = 15;
            var col1 = 15;

            var lblId = new Label() { Text = "ID:", Location = new Point(col1, y), Size = new Size(50, 20) };
            this.txtId = new TextBox() { Location = new Point(col1 + 50, y), Size = new Size(60, 20), ReadOnly = true, BackColor = Color.LightGray };

            var lblCodigo = new Label() { Text = "Codigo Barras:", Location = new Point(300, y), Size = new Size(100, 20) };
            this.txtCodigoBarras = new TextBox() { Location = new Point(400, y), Size = new Size(150, 20) };

            y += 30;
            var lblNombre = new Label() { Text = "Nombre:", Location = new Point(col1, y), Size = new Size(60, 20) };
            this.txtNombre = new TextBox() { Location = new Point(col1 + 60, y), Size = new Size(350, 20) };

            y += 30;
            var lblDescripcion = new Label() { Text = "Descripcion:", Location = new Point(col1, y), Size = new Size(80, 20) };
            this.txtDescripcion = new TextBox() { Location = new Point(col1 + 80, y), Size = new Size(400, 20) };

            y += 30;
            var lblPC = new Label() { Text = "Precio Compra:", Location = new Point(col1, y), Size = new Size(100, 20) };
            this.txtPrecioCompra = new TextBox() { Location = new Point(col1 + 100, y), Size = new Size(100, 20) };
            this.txtPrecioCompra.Leave += new EventHandler(this.txtPrecioCompra_Leave);

            var lblPV = new Label() { Text = "Precio Venta:", Location = new Point(col1 + 220, y), Size = new Size(90, 20) };
            this.txtPrecioVenta = new TextBox() { Location = new Point(col1 + 310, y), Size = new Size(100, 20) };
            this.txtPrecioVenta.Leave += new EventHandler(this.txtPrecioVenta_Leave);

            var lblCategoria = new Label() { Text = "Categoria:", Location = new Point(580, y), Size = new Size(80, 20) };
            this.cmbCategoria = new ComboBox() { Location = new Point(660, y), Size = new Size(180, 20), DropDownStyle = ComboBoxStyle.DropDownList };

            y += 30;
            lblInfoMargen = new Label() { Location = new Point(col1, y), Size = new Size(500, 20), ForeColor = Color.FromArgb(52, 73, 94) };

            btnConfigMargenes = new Button() { Text = "Configurar Margenes", Location = new Point(580, y - 2), Size = new Size(160, 25), BackColor = Color.FromArgb(243, 156, 18), ForeColor = Color.White };
            btnConfigMargenes.Click += new EventHandler(this.btnConfigMargenes_Click);

            y += 30;
            var lblStock = new Label() { Text = "Stock:", Location = new Point(col1, y), Size = new Size(50, 20) };
            this.txtStock = new TextBox() { Location = new Point(col1 + 50, y), Size = new Size(60, 20), Text = "0" };

            var lblSM = new Label() { Text = "Stock Min:", Location = new Point(col1 + 130, y), Size = new Size(70, 20) };
            this.txtStockMinimo = new TextBox() { Location = new Point(col1 + 200, y), Size = new Size(60, 20), Text = "5" };

            y += 30;
            var lblFecha = new Label() { Text = "Fecha Creacion:", Location = new Point(col1, y), Size = new Size(100, 20) };
            this.txtFechaCreacion = new TextBox() { Location = new Point(col1 + 110, y), Size = new Size(150, 20), ReadOnly = true, BackColor = Color.LightGray };

            var lblActivo = new Label() { Text = "Activo:", Location = new Point(col1 + 300, y), Size = new Size(60, 20) };
            this.chkActivo = new CheckBox() { Location = new Point(col1 + 360, y), Size = new Size(80, 20), Checked = true };

            // Codigos de barras adicionales
            y += 30;
            var groupCodigos = new GroupBox() { Text = "Codigos de barras adicionales", Location = new Point(col1, y), Size = new Size(490, 130) };

            var lblNuevoCodigo = new Label() { Text = "Nuevo codigo:", Location = new Point(10, 25), Size = new Size(90, 20) };
            this.txtNuevoCodigo = new TextBox() { Location = new Point(100, 23), Size = new Size(180, 20) };

            this.btnAgregarCodigo = new Button() { Text = "Agregar", Location = new Point(290, 22), Size = new Size(80, 23), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White };
            this.btnAgregarCodigo.Click += new EventHandler(this.btnAgregarCodigo_Click);

            this.lstCodigos = new ListBox() { Location = new Point(10, 52), Size = new Size(360, 68) };

            this.btnQuitarCodigo = new Button() { Text = "Quitar", Location = new Point(380, 52), Size = new Size(80, 23), BackColor = Color.FromArgb(192, 57, 43), ForeColor = Color.White };
            this.btnQuitarCodigo.Click += new EventHandler(this.btnQuitarCodigo_Click);

            groupCodigos.Controls.Add(lblNuevoCodigo);
            groupCodigos.Controls.Add(this.txtNuevoCodigo);
            groupCodigos.Controls.Add(this.btnAgregarCodigo);
            groupCodigos.Controls.Add(this.lstCodigos);
            groupCodigos.Controls.Add(this.btnQuitarCodigo);

            // Botones
            this.btnNuevo = new Button() { Text = "Nuevo", Location = new Point(800, 20), Size = new Size(100, 30), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
            this.btnGuardar = new Button() { Text = "Guardar", Location = new Point(910, 20), Size = new Size(100, 30), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White };
            this.btnEliminar = new Button() { Text = "Eliminar", Location = new Point(800, 60), Size = new Size(100, 30), BackColor = Color.FromArgb(192, 57, 43), ForeColor = Color.White };

            this.btnNuevo.Click += new EventHandler(this.btnNuevo_Click);
            this.btnGuardar.Click += new EventHandler(this.btnGuardar_Click);
            this.btnEliminar.Click += new EventHandler(this.btnEliminar_Click);

            panelForm.Controls.Add(lblId);
            panelForm.Controls.Add(this.txtId);
            panelForm.Controls.Add(lblCodigo);
            panelForm.Controls.Add(this.txtCodigoBarras);
            panelForm.Controls.Add(lblCategoria);
            panelForm.Controls.Add(this.cmbCategoria);
            panelForm.Controls.Add(lblNombre);
            panelForm.Controls.Add(this.txtNombre);
            panelForm.Controls.Add(lblDescripcion);
            panelForm.Controls.Add(this.txtDescripcion);
            panelForm.Controls.Add(lblPC);
            panelForm.Controls.Add(this.txtPrecioCompra);
            panelForm.Controls.Add(lblPV);
            panelForm.Controls.Add(this.txtPrecioVenta);
            panelForm.Controls.Add(lblInfoMargen);
            panelForm.Controls.Add(btnConfigMargenes);
            panelForm.Controls.Add(lblStock);
            panelForm.Controls.Add(this.txtStock);
            panelForm.Controls.Add(lblSM);
            panelForm.Controls.Add(this.txtStockMinimo);
            panelForm.Controls.Add(lblFecha);
            panelForm.Controls.Add(this.txtFechaCreacion);
            panelForm.Controls.Add(lblActivo);
            panelForm.Controls.Add(this.chkActivo);
            panelForm.Controls.Add(groupCodigos);
            panelForm.Controls.Add(this.btnNuevo);
            panelForm.Controls.Add(this.btnGuardar);
            panelForm.Controls.Add(this.btnEliminar);

            // Panel Grid
            var panelGrid = new Panel();
            panelGrid.Dock = DockStyle.Fill;
            panelGrid.Padding = new Padding(10);

            var lblBuscar = new Label() { Text = "Buscar:", Location = new Point(15, 15), Size = new Size(60, 25) };
            this.txtBuscar = new TextBox() { Location = new Point(80, 15), Size = new Size(300, 25) };
            this.txtBuscar.TextChanged += new EventHandler(this.txtBuscar_TextChanged);

            this.dgvProductos = new DataGridView();
            this.dgvProductos.Location = new Point(10, 50);
            this.dgvProductos.Size = new Size(1050, 300);
            this.dgvProductos.AllowUserToAddRows = false;
            this.dgvProductos.ReadOnly = true;
            this.dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvProductos.BackgroundColor = Color.White;
            this.dgvProductos.Columns.Add("colId", "ID");
            this.dgvProductos.Columns.Add("colCodigo", "Codigo");
            this.dgvProductos.Columns.Add("colNombre", "Nombre");
            this.dgvProductos.Columns.Add("colCategoria", "Categoria");
            this.dgvProductos.Columns.Add("colDescripcion", "Descripcion");
            this.dgvProductos.Columns.Add("colPrecioCompra", "P. Compra");
            this.dgvProductos.Columns.Add("colPrecioVenta", "P. Venta");
            this.dgvProductos.Columns.Add("colStock", "Stock");
            this.dgvProductos.Columns.Add("colStockMinimo", "Stock Min");
            this.dgvProductos.Columns.Add("colFecha", "Fecha Creacion");
            this.dgvProductos.Columns.Add("colActivo", "Activo");
            this.dgvProductos.Columns["colId"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgvProductos.Columns["colCodigo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvProductos.Columns["colNombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dgvProductos.Columns["colCategoria"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvProductos.Columns["colDescripcion"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvProductos.Columns["colPrecioCompra"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgvProductos.Columns["colPrecioVenta"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgvProductos.Columns["colStock"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgvProductos.Columns["colStockMinimo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgvProductos.Columns["colFecha"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvProductos.Columns["colActivo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgvProductos.CellClick += new DataGridViewCellEventHandler(this.dgvProductos_CellClick);

            panelGrid.Controls.Add(lblBuscar);
            panelGrid.Controls.Add(this.txtBuscar);
            panelGrid.Controls.Add(this.dgvProductos);

            this.Controls.Add(panelGrid);
            this.Controls.Add(panelForm);
        }
    }
}
