using System;
using System.Drawing;
using System.Windows.Forms;
using SistemaVentas.Business.Services;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Presentation
{
    public partial class FrmInventario : Form
    {
        private readonly ProductoService _productoService;
        private readonly string _usuario;

        public FrmInventario(string usuario)
        {
            _usuario = usuario;
            _productoService = new ProductoService();
            InitializeComponent();
            CargarProductos();
        }

        private void CargarProductos()
        {
            dgvInventario.Rows.Clear();
            var productos = _productoService.GetAll();
            foreach (var p in productos)
            {
                dgvInventario.Rows.Add(p.Id, p.Nombre, p.CodigoBarras, p.Stock, p.StockMinimo, p.Stock <= p.StockMinimo ? "BAJO" : "OK");
            }
        }

        private void btnEntrada_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtProductoId.Text))
            {
                MessageBox.Show("Seleccione un producto", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int cantidad;
            if (!int.TryParse(txtCantidad.Text, out cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingrese una cantidad valida", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnEntrada.Enabled = false;
            try
            {
                _productoService.RegistrarEntrada(
                    int.Parse(txtProductoId.Text),
                    cantidad,
                    txtMotivo.Text.Trim(),
                    _usuario);

                MessageBox.Show("Entrada de stock registrada", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarProductos();
                txtCantidad.Clear();
                txtMotivo.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnEntrada.Enabled = true;
            }
        }

        private void btnSalida_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtProductoId.Text))
            {
                MessageBox.Show("Seleccione un producto", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int cantidad;
            if (!int.TryParse(txtCantidad.Text, out cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingrese una cantidad valida", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSalida.Enabled = false;
            try
            {
                bool resultado = _productoService.RegistrarSalida(
                    int.Parse(txtProductoId.Text),
                    cantidad,
                    txtMotivo.Text.Trim(),
                    _usuario);

                if (resultado)
                {
                    MessageBox.Show("Salida de stock registrada", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarProductos();
                    txtCantidad.Clear();
                    txtMotivo.Clear();
                }
                else
                {
                    MessageBox.Show("Stock insuficiente", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSalida.Enabled = true;
            }
        }

        private void dgvInventario_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dgvInventario.Rows[e.RowIndex];
                txtProductoId.Text = row.Cells["colId"].Value.ToString();
                txtProductoNombre.Text = row.Cells["colNombre"].Value.ToString();
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
                dgvInventario.Rows.Clear();
                var productos = _productoService.Search(termino);
                foreach (var p in productos)
                {
                    dgvInventario.Rows.Add(p.Id, p.Nombre, p.CodigoBarras, p.Stock, p.StockMinimo, p.Stock <= p.StockMinimo ? "BAJO" : "OK");
                }
            }
        }
    }

    partial class FrmInventario
    {
        private System.ComponentModel.IContainer components = null;
        private DataGridView dgvInventario;
        private TextBox txtProductoId;
        private TextBox txtProductoNombre;
        private TextBox txtCantidad;
        private TextBox txtMotivo;
        private TextBox txtBuscar;
        private Button btnEntrada;
        private Button btnSalida;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Text = "Control de Inventario";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            // Panel superior
            var panelSuperior = new Panel();
            panelSuperior.Dock = DockStyle.Top;
            panelSuperior.Height = 100;
            panelSuperior.BackColor = Color.FromArgb(236, 240, 241);

            var lblBuscar = new Label() { Text = "Buscar:", Location = new Point(15, 15), Size = new Size(60, 20) };
            this.txtBuscar = new TextBox() { Location = new Point(80, 15), Size = new Size(300, 20) };
            this.txtBuscar.TextChanged += new EventHandler(this.txtBuscar_TextChanged);

            var lblProducto = new Label() { Text = "Producto:", Location = new Point(15, 45), Size = new Size(70, 20) };
            this.txtProductoId = new TextBox() { Location = new Point(85, 45), Size = new Size(60, 20), ReadOnly = true, BackColor = Color.LightGray };
            this.txtProductoNombre = new TextBox() { Location = new Point(150, 45), Size = new Size(250, 20), ReadOnly = true, BackColor = Color.LightGray };

            var lblCantidad = new Label() { Text = "Cantidad:", Location = new Point(450, 45), Size = new Size(70, 20) };
            this.txtCantidad = new TextBox() { Location = new Point(520, 45), Size = new Size(100, 20) };

            var lblMotivo = new Label() { Text = "Motivo:", Location = new Point(640, 45), Size = new Size(60, 20) };
            this.txtMotivo = new TextBox() { Location = new Point(700, 45), Size = new Size(150, 20) };

            this.btnEntrada = new Button() { Text = "Entrada (+)", Location = new Point(450, 10), Size = new Size(100, 30), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White };
            this.btnSalida = new Button() { Text = "Salida (-)", Location = new Point(560, 10), Size = new Size(100, 30), BackColor = Color.FromArgb(192, 57, 43), ForeColor = Color.White };

            this.btnEntrada.Click += new EventHandler(this.btnEntrada_Click);
            this.btnSalida.Click += new EventHandler(this.btnSalida_Click);

            panelSuperior.Controls.Add(lblBuscar);
            panelSuperior.Controls.Add(this.txtBuscar);
            panelSuperior.Controls.Add(lblProducto);
            panelSuperior.Controls.Add(this.txtProductoId);
            panelSuperior.Controls.Add(this.txtProductoNombre);
            panelSuperior.Controls.Add(lblCantidad);
            panelSuperior.Controls.Add(this.txtCantidad);
            panelSuperior.Controls.Add(lblMotivo);
            panelSuperior.Controls.Add(this.txtMotivo);
            panelSuperior.Controls.Add(this.btnEntrada);
            panelSuperior.Controls.Add(this.btnSalida);

            // Grid
            this.dgvInventario = new DataGridView();
            this.dgvInventario.Dock = DockStyle.Fill;
            this.dgvInventario.AllowUserToAddRows = false;
            this.dgvInventario.ReadOnly = true;
            this.dgvInventario.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvInventario.BackgroundColor = Color.White;
            this.dgvInventario.Columns.Add("colId", "ID");
            this.dgvInventario.Columns.Add("colNombre", "Producto");
            this.dgvInventario.Columns.Add("colCodigo", "Codigo");
            this.dgvInventario.Columns.Add("colStock", "Stock");
            this.dgvInventario.Columns.Add("colStockMin", "Stock Min");
            this.dgvInventario.Columns.Add("colEstado", "Estado");
            this.dgvInventario.CellClick += new DataGridViewCellEventHandler(this.dgvInventario_CellClick);

            this.Controls.Add(this.dgvInventario);
            this.Controls.Add(panelSuperior);
        }
    }
}
