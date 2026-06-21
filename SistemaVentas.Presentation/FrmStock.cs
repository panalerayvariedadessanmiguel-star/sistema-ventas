using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SistemaVentas.Business.Services;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Presentation
{
    public partial class FrmStock : Form
    {
        private readonly StockService _stockService;
        private readonly ProductoService _productoService;

        public FrmStock()
        {
            _stockService = new StockService();
            _productoService = new ProductoService();
            InitializeComponent();
            CargarAños();
            CargarMesActual();
            CargarStock();
        }

        private void CargarAños()
        {
            cmbAño.Items.Clear();
            int añoActual = DateTime.Now.Year;
            for (int i = añoActual; i >= añoActual - 5; i--)
            {
                cmbAño.Items.Add(i);
            }
            cmbAño.SelectedItem = añoActual;
        }

        private void CargarMesActual()
        {
            cmbMes.SelectedIndex = DateTime.Now.Month - 1;
        }

        private void CargarStock()
        {
            dgvStock.Rows.Clear();

            if (cmbAño.SelectedItem == null || cmbMes.SelectedIndex < 0)
                return;

            int año = (int)cmbAño.SelectedItem;
            int mes = cmbMes.SelectedIndex + 1;
            string buscar = txtBuscar.Text.Trim().ToLower();

            var lista = _stockService.GetByAñoMes(año, mes);

            if (lista != null && lista.Count > 0)
            {
                var resultados = string.IsNullOrEmpty(buscar)
                    ? lista
                    : lista.Where(s =>
                        (s.NombreProducto != null && s.NombreProducto.ToLower().Contains(buscar)) ||
                        (s.CodigoBarras != null && s.CodigoBarras.ToLower().Contains(buscar))
                       ).ToList();

                if (!resultados.Any() && !string.IsNullOrEmpty(buscar))
                {
                    var producto = _productoService.GetByCodigoBarras(buscar);
                    if (producto != null)
                        resultados = lista.Where(s => s.ProductoId == producto.Id).ToList();
                }

                foreach (var s in resultados)
                {
                    dgvStock.Rows.Add(s.Id, s.CodigoBarras, s.NombreProducto, s.Año, GetNombreMes(s.Mes),
                        s.CantidadInicial, s.CantidadEntrante, s.CantidadSaliente, s.CantidadFinal);
                }
            }
            else
            {
                var productos = _productoService.GetAll();
                var filtrados = string.IsNullOrEmpty(buscar)
                    ? productos
                    : productos.Where(p =>
                        p.Nombre.ToLower().Contains(buscar) ||
                        (p.CodigoBarras != null && p.CodigoBarras.ToLower().Contains(buscar))
                       ).ToList();

                if (!filtrados.Any() && !string.IsNullOrEmpty(buscar))
                {
                    var producto = _productoService.GetByCodigoBarras(buscar);
                    if (producto != null && !filtrados.Any(p => p.Id == producto.Id))
                        filtrados.Add(producto);
                }

                foreach (var p in filtrados)
                {
                    dgvStock.Rows.Add("", p.CodigoBarras, p.Nombre, año, GetNombreMes(mes),
                        p.Stock, 0, 0, p.Stock);
                }
            }
        }

        private string GetNombreMes(int mes)
        {
            string[] meses = { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                             "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
            return mes >= 1 && mes <= 12 ? meses[mes] : "";
        }

        private void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarStock();
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarStock();
        }

        private void btnTraspaso_Click(object sender, EventArgs e)
        {
            var resultado = MessageBox.Show(
                "Se trasladará la cantidad final del mes anterior como cantidad inicial del mes seleccionado.\n¿Desea continuar?",
                "Traspaso de Stock",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                try
                {
                    int año = (int)cmbAño.SelectedItem;
                    int mes = cmbMes.SelectedIndex + 1;
                    string mensaje = _stockService.TraspasoStockMes(año, mes);
                    MessageBox.Show(mensaje, "Traspaso Completado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarStock();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    partial class FrmStock
    {
        private DataGridView dgvStock;
        private ComboBox cmbAño;
        private ComboBox cmbMes;
        private TextBox txtBuscar;
        private Button btnFiltrar;
        private Button btnTraspaso;

        private void InitializeComponent()
        {
            this.Text = "Control de Stock Mensual";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            var panelSuperior = new Panel();
            panelSuperior.Dock = DockStyle.Top;
            panelSuperior.Height = 70;
            panelSuperior.BackColor = Color.FromArgb(236, 240, 241);

            var lblAño = new Label() { Text = "Año:", Location = new Point(15, 15), Size = new Size(50, 20) };
            this.cmbAño = new ComboBox() { Location = new Point(60, 15), Size = new Size(80, 20), DropDownStyle = ComboBoxStyle.DropDownList };

            var lblMes = new Label() { Text = "Mes:", Location = new Point(160, 15), Size = new Size(50, 20) };
            this.cmbMes = new ComboBox() { Location = new Point(210, 15), Size = new Size(120, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbMes.Items.AddRange(new string[] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" });

            this.btnFiltrar = new Button() { Text = "Filtrar", Location = new Point(350, 13), Size = new Size(80, 25), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };

            var lblBuscar = new Label() { Text = "Buscar:", Location = new Point(460, 15), Size = new Size(60, 20) };
            this.txtBuscar = new TextBox() { Location = new Point(520, 15), Size = new Size(250, 20) };

            this.btnTraspaso = new Button() { Text = "Traspaso Mes", Location = new Point(790, 13), Size = new Size(110, 25), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White };

            btnFiltrar.Click += new EventHandler(this.btnFiltrar_Click);
            txtBuscar.TextChanged += new EventHandler(this.txtBuscar_TextChanged);
            btnTraspaso.Click += new EventHandler(this.btnTraspaso_Click);

            panelSuperior.Controls.Add(lblAño);
            panelSuperior.Controls.Add(this.cmbAño);
            panelSuperior.Controls.Add(lblMes);
            panelSuperior.Controls.Add(this.cmbMes);
            panelSuperior.Controls.Add(this.btnFiltrar);
            panelSuperior.Controls.Add(lblBuscar);
            panelSuperior.Controls.Add(this.txtBuscar);
            panelSuperior.Controls.Add(this.btnTraspaso);

            this.dgvStock = new DataGridView();
            this.dgvStock.Dock = DockStyle.Fill;
            this.dgvStock.AllowUserToAddRows = false;
            this.dgvStock.ReadOnly = true;
            this.dgvStock.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvStock.BackgroundColor = Color.White;
            this.dgvStock.Columns.Add("colId", "ID");
            this.dgvStock.Columns.Add("colCodigo", "Codigo");
            this.dgvStock.Columns.Add("colProducto", "Producto");
            this.dgvStock.Columns.Add("colAño", "Año");
            this.dgvStock.Columns.Add("colMes", "Mes");
            this.dgvStock.Columns.Add("colInicial", "Cant. Inicial");
            this.dgvStock.Columns.Add("colEntrante", "Cant. Entrante");
            this.dgvStock.Columns.Add("colSaliente", "Cant. Saliente");
            this.dgvStock.Columns.Add("colFinal", "Cant. Final");
            this.dgvStock.Columns["colId"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgvStock.Columns["colCodigo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvStock.Columns["colProducto"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dgvStock.Columns["colAño"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgvStock.Columns["colMes"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvStock.Columns["colInicial"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgvStock.Columns["colEntrante"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgvStock.Columns["colSaliente"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgvStock.Columns["colFinal"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;

            this.Controls.Add(this.dgvStock);
            this.Controls.Add(panelSuperior);
        }
    }
}
