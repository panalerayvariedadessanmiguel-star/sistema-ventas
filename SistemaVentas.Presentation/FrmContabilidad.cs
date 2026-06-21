using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SistemaVentas.Business.Services;

namespace SistemaVentas.Presentation
{
    public partial class FrmContabilidad : Form
    {
        private readonly ContabilidadService _service;
        private DateTimePicker dtpFecha, dtpDesde, dtpHasta;
        private ComboBox cmbTipo, cmbCategoria;
        private TextBox txtConcepto, txtMonto;
        private DataGridView dgvTransacciones;
        private Label lblTotalIngresos, lblTotalGastos, lblNeto;
        private Button btnGuardar, btnEliminar, btnFiltrar, btnImportarVentas, btnImportarMovCaja;
        private bool _formateandoMonto = false;

        public FrmContabilidad()
        {
            _service = new ContabilidadService();
            InitializeComponent();
            CargarTransacciones();
        }

        private void CargarCategorias()
        {
            cmbCategoria.Items.Clear();
            string tipo = cmbTipo.SelectedItem?.ToString() ?? "Gasto";
            var cats = tipo == "Ingreso" ? ContabilidadService.CategoriasIngreso : ContabilidadService.CategoriasGasto;
            cmbCategoria.Items.AddRange(cats);
            if (cmbCategoria.Items.Count > 0) cmbCategoria.SelectedIndex = 0;
        }

        private void cmbTipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarCategorias();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (cmbTipo.SelectedItem == null || cmbCategoria.SelectedItem == null)
            { MessageBox.Show("Seleccione tipo y categoria", "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string textoMonto = txtMonto.Text.Replace(".", "").Trim();
            if (!decimal.TryParse(textoMonto, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.CurrentCulture, out decimal monto) || monto <= 0)
            { MessageBox.Show("Ingrese un monto valido", "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            _service.RegistrarConFecha(
                dtpFecha.Value,
                cmbTipo.SelectedItem.ToString(),
                cmbCategoria.SelectedItem.ToString(),
                txtConcepto.Text.Trim(),
                monto,
                "Admin");

            txtConcepto.Clear();
            txtMonto.Clear();
            CargarTransacciones();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvTransacciones.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(dgvTransacciones.SelectedRows[0].Cells["colId"].Value);
            if (MessageBox.Show("Eliminar esta transaccion?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _service.Eliminar(id);
                CargarTransacciones();
            }
        }

        private void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarTransacciones();
        }

        private void btnImportarVentas_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Importar todas las ventas pasadas como ingresos contables?", "Importar Ventas",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            try
            {
                int count = _service.ImportarVentasPasadas();
                MessageBox.Show($"Se importaron {count} ventas.", "Importacion Completa",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarTransacciones();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImportarMovCaja_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Importar todos los movimientos de caja como transacciones contables?", "Importar Movimientos",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            try
            {
                int count = _service.ImportarMovimientosCaja();
                MessageBox.Show($"Se importaron {count} movimientos.", "Importacion Completa",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarTransacciones();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtMonto_TextChanged(object sender, EventArgs e)
        {
            if (_formateandoMonto) return;
            _formateandoMonto = true;

            string texto = txtMonto.Text.Replace(".", "").Trim();
            if (decimal.TryParse(texto, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.CurrentCulture, out decimal monto))
            {
                int cursorPos = txtMonto.SelectionStart;
                int longAntes = txtMonto.Text.Length;
                txtMonto.Text = monto.ToString("N0");
                int longDesp = txtMonto.Text.Length;
                txtMonto.SelectionStart = Math.Max(0, cursorPos + (longDesp - longAntes));
            }

            _formateandoMonto = false;
        }

        private void CargarTransacciones()
        {
            var desde = dtpDesde.Value;
            var hasta = dtpHasta.Value;
            var lista = _service.GetTransacciones(desde, hasta);

            dgvTransacciones.Rows.Clear();
            foreach (var t in lista)
            {
                string montoStr = t.Monto.ToString("N2");
                dgvTransacciones.Rows.Add(t.Id, t.Fecha.ToShortDateString(), t.Tipo, t.Categoria, t.Concepto, montoStr);
            }

            decimal totalIng = _service.GetTotalIngresos(desde, hasta);
            decimal totalGas = _service.GetTotalGastos(desde, hasta);
            decimal neto = totalIng - totalGas;

            lblTotalIngresos.Text = $"Total Ingresos: ${totalIng:N2}";
            lblTotalGastos.Text = $"Total Gastos: ${totalGas:N2}";
            lblNeto.Text = neto >= 0 ? $"Utilidad: ${neto:N2}" : $"Perdida: ${neto:N2}";
            lblNeto.ForeColor = neto >= 0 ? Color.FromArgb(39, 174, 96) : Color.FromArgb(192, 57, 43);
        }

        private void InitializeComponent()
        {
            Text = "Contabilidad";
            Size = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(236, 240, 241);

            var panelRegistro = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Color.White, Padding = new Padding(10) };

            var lblFecha = new Label { Text = "Fecha:", Location = new Point(15, 15), Size = new Size(50, 20) };
            dtpFecha = new DateTimePicker { Location = new Point(70, 13), Size = new Size(110, 20), Format = DateTimePickerFormat.Short };

            var lblTipo = new Label { Text = "Tipo:", Location = new Point(200, 15), Size = new Size(40, 20) };
            cmbTipo = new ComboBox { Location = new Point(245, 13), Size = new Size(90, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTipo.Items.AddRange(new[] { "Ingreso", "Gasto" });
            cmbTipo.SelectedIndex = 1;
            cmbTipo.SelectedIndexChanged += cmbTipo_SelectedIndexChanged;

            var lblCat = new Label { Text = "Categoria:", Location = new Point(350, 15), Size = new Size(70, 20) };
            cmbCategoria = new ComboBox { Location = new Point(425, 13), Size = new Size(150, 20), DropDownStyle = ComboBoxStyle.DropDownList };

            var lblConc = new Label { Text = "Concepto:", Location = new Point(15, 50), Size = new Size(70, 20) };
            txtConcepto = new TextBox { Location = new Point(90, 48), Size = new Size(300, 20) };

            var lblMonto = new Label { Text = "Monto $:", Location = new Point(410, 50), Size = new Size(60, 20) };
            txtMonto = new TextBox { Location = new Point(475, 48), Size = new Size(100, 20), TextAlign = HorizontalAlignment.Right };
            txtMonto.TextChanged += txtMonto_TextChanged;

            btnGuardar = new Button { Text = "Guardar", Location = new Point(15, 80), Size = new Size(100, 28),
                BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White };
            btnGuardar.Click += btnGuardar_Click;

            btnEliminar = new Button { Text = "Eliminar", Location = new Point(125, 80), Size = new Size(100, 28),
                BackColor = Color.FromArgb(192, 57, 43), ForeColor = Color.White };
            btnEliminar.Click += btnEliminar_Click;

            panelRegistro.Controls.AddRange(new Control[] {
                lblFecha, dtpFecha, lblTipo, cmbTipo, lblCat, cmbCategoria,
                lblConc, txtConcepto, lblMonto, txtMonto, btnGuardar, btnEliminar
            });

            var panelFiltro = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(245, 245, 245), Padding = new Padding(10) };
            var lblDesde = new Label { Text = "Desde:", Location = new Point(15, 10), Size = new Size(50, 20) };
            dtpDesde = new DateTimePicker { Location = new Point(65, 8), Size = new Size(110, 20), Format = DateTimePickerFormat.Short };
            dtpDesde.Value = new DateTime(DateTime.Now.Year, 1, 1);

            var lblHasta = new Label { Text = "Hasta:", Location = new Point(195, 10), Size = new Size(50, 20) };
            dtpHasta = new DateTimePicker { Location = new Point(245, 8), Size = new Size(110, 20), Format = DateTimePickerFormat.Short };

            btnFiltrar = new Button { Text = "Filtrar", Location = new Point(370, 7), Size = new Size(80, 25),
                BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
            btnFiltrar.Click += btnFiltrar_Click;

            btnImportarVentas = new Button { Text = "Importar Ventas", Location = new Point(470, 7), Size = new Size(110, 25),
                BackColor = Color.FromArgb(155, 89, 182), ForeColor = Color.White, Font = new Font("Segoe UI", 8) };
            btnImportarVentas.Click += btnImportarVentas_Click;

            btnImportarMovCaja = new Button { Text = "Importar Caja", Location = new Point(590, 7), Size = new Size(110, 25),
                BackColor = Color.FromArgb(155, 89, 182), ForeColor = Color.White, Font = new Font("Segoe UI", 8) };
            btnImportarMovCaja.Click += btnImportarMovCaja_Click;

            panelFiltro.Controls.AddRange(new Control[] { lblDesde, dtpDesde, lblHasta, dtpHasta, btnFiltrar, btnImportarVentas, btnImportarMovCaja });

            dgvTransacciones = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };
            dgvTransacciones.Columns.Add("colId", "ID");
            dgvTransacciones.Columns.Add("colFecha", "Fecha");
            dgvTransacciones.Columns.Add("colTipo", "Tipo");
            dgvTransacciones.Columns.Add("colCategoria", "Categoria");
            dgvTransacciones.Columns.Add("colConcepto", "Concepto");
            dgvTransacciones.Columns.Add("colMonto", "Monto");
            dgvTransacciones.Columns["colId"].Visible = false;
            dgvTransacciones.Columns["colConcepto"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvTransacciones.Columns["colMonto"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            var panelResumen = new Panel { Dock = DockStyle.Bottom, Height = 35, BackColor = Color.FromArgb(44, 62, 80), Padding = new Padding(10) };
            lblTotalIngresos = new Label { Text = "Total Ingresos: $0.00", Location = new Point(15, 7), Size = new Size(200, 20), ForeColor = Color.White };
            lblTotalGastos = new Label { Text = "Total Gastos: $0.00", Location = new Point(280, 7), Size = new Size(200, 20), ForeColor = Color.White };
            lblNeto = new Label { Text = "Utilidad: $0.00", Location = new Point(550, 7), Size = new Size(200, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            panelResumen.Controls.AddRange(new Control[] { lblTotalIngresos, lblTotalGastos, lblNeto });

            panelRegistro.Controls.Add(new Label
            {
                Text = "Categorias de gasto sugeridas segun tus margenes: recuerda incluir los costos fijos y variables en tus registros.",
                Location = new Point(250, 83), Size = new Size(500, 20), ForeColor = Color.Gray, Font = new Font("Segoe UI", 8)
            });

            Controls.Add(dgvTransacciones);
            Controls.Add(panelFiltro);
            Controls.Add(panelRegistro);
            Controls.Add(panelResumen);

            CargarCategorias();
        }
    }
}
