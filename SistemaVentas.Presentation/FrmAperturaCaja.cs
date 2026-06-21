using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using SistemaVentas.Business.Services;

namespace SistemaVentas.Presentation
{
    public partial class FrmAperturaCaja : Form
    {
        private readonly CajaService _cajaService;
        private ComboBox cmbUsuario;

        public int CajaId { get; private set; }
        public string Usuario { get; private set; }

        public FrmAperturaCaja()
        {
            InitializeComponent();
            _cajaService = new CajaService();
            CargarVendedores();
            txtFechaHora.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            cmbUsuario.Focus();
        }

        private void CargarVendedores()
        {
            try
            {
                var vendedores = _cajaService.GetVendedores();
                cmbUsuario.Items.Clear();
                foreach (var v in vendedores)
                {
                    cmbUsuario.Items.Add(v);
                }
                if (cmbUsuario.Items.Count > 0)
                    cmbUsuario.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar vendedores: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAbrir_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cmbUsuario.Text))
                {
                    MessageBox.Show("Seleccione un vendedor", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbUsuario.Focus();
                    return;
                }

                string montoLimpio = txtMontoInicial.Text.Replace("$", "").Replace(".", "").Replace(",", "").Trim();
                if (!decimal.TryParse(montoLimpio, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal montoInicial) || montoInicial < 0)
                {
                    MessageBox.Show("Ingrese un monto inicial valido", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMontoInicial.Focus();
                    return;
                }

                CajaId = _cajaService.AbrirCaja(
                    cmbUsuario.Text.Trim(),
                    montoInicial,
                    txtObservaciones.Text.Trim(),
                    (int)numCaja.Value);

                Usuario = cmbUsuario.Text.Trim();

                MessageBox.Show("Caja abierta exitosamente", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void txtMontoInicial_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private bool _updatingAmount = false;

        private void txtMontoInicial_TextChanged(object sender, EventArgs e)
        {
            if (_updatingAmount) return;

            string text = txtMontoInicial.Text.Replace("$", "").Replace(",", "").Replace(".", "").Trim();
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal amount))
            {
                _updatingAmount = true;
                int cursorPos = txtMontoInicial.SelectionStart;
                int oldLength = txtMontoInicial.Text.Length;
                txtMontoInicial.Text = amount.ToString("C0");
                int newLength = txtMontoInicial.Text.Length;
                int newPos = cursorPos + (newLength - oldLength);
                txtMontoInicial.SelectionStart = Math.Max(0, newPos);
                _updatingAmount = false;
            }
            else if (string.IsNullOrWhiteSpace(text))
            {
                _updatingAmount = true;
                txtMontoInicial.Text = "";
                _updatingAmount = false;
            }
        }
    }

    partial class FrmAperturaCaja
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitulo;
        private Panel panelHeader;
        private Panel panelBody;
        private Panel panelButtons;
        private Label lblUsuario;
        private Label lblMonto;
        private Label lblObservaciones;
        private Label lblFechaHora;
        private Label lblNumeroCaja;
        private TextBox txtMontoInicial;
        private TextBox txtObservaciones;
        private TextBox txtFechaHora;
        private NumericUpDown numCaja;
        private Button btnAbrir;
        private Button btnCancelar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Text = "Apertura de Caja";
            this.Size = new Size(500, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 245);

            // Panel Header
            this.panelHeader = new Panel();
            this.panelHeader.Dock = DockStyle.Top;
            this.panelHeader.Height = 70;
            this.panelHeader.BackColor = Color.FromArgb(44, 62, 80);
            this.panelHeader.Padding = new Padding(20);

            this.lblTitulo = new Label();
            this.lblTitulo.Text = "APERTURA DE CAJA";
            this.lblTitulo.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            this.lblTitulo.ForeColor = Color.White;
            this.lblTitulo.Dock = DockStyle.Fill;
            this.lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            this.panelHeader.Controls.Add(this.lblTitulo);

            // Panel Body
            this.panelBody = new Panel();
            this.panelBody.Dock = DockStyle.Fill;
            this.panelBody.Padding = new Padding(30, 20, 30, 10);
            this.panelBody.BackColor = Color.FromArgb(250, 250, 252);

            int y = 10;
            int labelWidth = 140;
            int fieldLeft = 160;
            int fieldWidth = 280;

            // Fecha/Hora (solo lectura)
            this.lblFechaHora = new Label();
            this.lblFechaHora.Text = "Fecha y Hora:";
            this.lblFechaHora.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.lblFechaHora.ForeColor = Color.FromArgb(80, 80, 90);
            this.lblFechaHora.Location = new Point(10, y + 3);
            this.lblFechaHora.Size = new Size(labelWidth, 28);
            this.lblFechaHora.TextAlign = ContentAlignment.MiddleRight;

            this.txtFechaHora = new TextBox();
            this.txtFechaHora.Location = new Point(fieldLeft, y);
            this.txtFechaHora.Size = new Size(fieldWidth, 28);
            this.txtFechaHora.Font = new Font("Segoe UI", 10);
            this.txtFechaHora.ReadOnly = true;
            this.txtFechaHora.BackColor = Color.FromArgb(235, 235, 240);
            this.txtFechaHora.TextAlign = HorizontalAlignment.Center;
            y += 40;

            // Numero de Caja
            this.lblNumeroCaja = new Label();
            this.lblNumeroCaja.Text = "Numero de Caja:";
            this.lblNumeroCaja.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.lblNumeroCaja.ForeColor = Color.FromArgb(80, 80, 90);
            this.lblNumeroCaja.Location = new Point(10, y + 3);
            this.lblNumeroCaja.Size = new Size(labelWidth, 28);
            this.lblNumeroCaja.TextAlign = ContentAlignment.MiddleRight;

            this.numCaja = new NumericUpDown();
            this.numCaja.Location = new Point(fieldLeft, y);
            this.numCaja.Size = new Size(80, 28);
            this.numCaja.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            this.numCaja.Minimum = 1;
            this.numCaja.Maximum = 99;
            this.numCaja.Value = 1;
            this.numCaja.TextAlign = HorizontalAlignment.Center;
            y += 40;

            // Usuario
            this.lblUsuario = new Label();
            this.lblUsuario.Text = "Usuario / Cajero:";
            this.lblUsuario.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.lblUsuario.ForeColor = Color.FromArgb(80, 80, 90);
            this.lblUsuario.Location = new Point(10, y + 3);
            this.lblUsuario.Size = new Size(labelWidth, 28);
            this.lblUsuario.TextAlign = ContentAlignment.MiddleRight;

            this.cmbUsuario = new ComboBox();
            this.cmbUsuario.Location = new Point(fieldLeft, y);
            this.cmbUsuario.Size = new Size(fieldWidth, 28);
            this.cmbUsuario.Font = new Font("Segoe UI", 11);
            this.cmbUsuario.DropDownStyle = ComboBoxStyle.DropDownList;
            y += 40;

            // Monto Inicial
            this.lblMonto = new Label();
            this.lblMonto.Text = "Monto Inicial en Caja:";
            this.lblMonto.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.lblMonto.ForeColor = Color.FromArgb(80, 80, 90);
            this.lblMonto.Location = new Point(10, y + 3);
            this.lblMonto.Size = new Size(labelWidth, 28);
            this.lblMonto.TextAlign = ContentAlignment.MiddleRight;

            this.txtMontoInicial = new TextBox();
            this.txtMontoInicial.Location = new Point(fieldLeft, y);
            this.txtMontoInicial.Size = new Size(fieldWidth, 28);
            this.txtMontoInicial.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.txtMontoInicial.Text = 0.ToString("C0");
            this.txtMontoInicial.TextAlign = HorizontalAlignment.Right;
            this.txtMontoInicial.KeyPress += new KeyPressEventHandler(this.txtMontoInicial_KeyPress);
            this.txtMontoInicial.TextChanged += new EventHandler(this.txtMontoInicial_TextChanged);
            y += 40;

            // Observaciones
            this.lblObservaciones = new Label();
            this.lblObservaciones.Text = "Observaciones:";
            this.lblObservaciones.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.lblObservaciones.ForeColor = Color.FromArgb(80, 80, 90);
            this.lblObservaciones.Location = new Point(10, y + 3);
            this.lblObservaciones.Size = new Size(labelWidth, 28);
            this.lblObservaciones.TextAlign = ContentAlignment.TopRight;

            this.txtObservaciones = new TextBox();
            this.txtObservaciones.Location = new Point(fieldLeft, y);
            this.txtObservaciones.Size = new Size(fieldWidth, 60);
            this.txtObservaciones.Font = new Font("Segoe UI", 10);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.PlaceholderText = "Notas opcionales sobre la apertura...";

            this.panelBody.Controls.Add(this.lblFechaHora);
            this.panelBody.Controls.Add(this.txtFechaHora);
            this.panelBody.Controls.Add(this.lblNumeroCaja);
            this.panelBody.Controls.Add(this.numCaja);
            this.panelBody.Controls.Add(this.lblUsuario);
            this.panelBody.Controls.Add(this.cmbUsuario);
            this.panelBody.Controls.Add(this.lblMonto);
            this.panelBody.Controls.Add(this.txtMontoInicial);
            this.panelBody.Controls.Add(this.lblObservaciones);
            this.panelBody.Controls.Add(this.txtObservaciones);

            // Panel Buttons
            this.panelButtons = new Panel();
            this.panelButtons.Dock = DockStyle.Bottom;
            this.panelButtons.Height = 65;
            this.panelButtons.BackColor = Color.FromArgb(235, 235, 240);
            this.panelButtons.Padding = new Padding(20, 10, 20, 10);

            this.btnCancelar = new Button();
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.BackColor = Color.FromArgb(149, 165, 166);
            this.btnCancelar.ForeColor = Color.White;
            this.btnCancelar.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            this.btnCancelar.FlatStyle = FlatStyle.Flat;
            this.btnCancelar.Location = new Point(260, 12);
            this.btnCancelar.Size = new Size(140, 40);
            this.btnCancelar.Click += new EventHandler(this.btnCancelar_Click);
            this.btnCancelar.FlatAppearance.BorderSize = 0;

            this.btnAbrir = new Button();
            this.btnAbrir.Text = "ABRIR CAJA";
            this.btnAbrir.BackColor = Color.FromArgb(39, 174, 96);
            this.btnAbrir.ForeColor = Color.White;
            this.btnAbrir.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            this.btnAbrir.FlatStyle = FlatStyle.Flat;
            this.btnAbrir.Location = new Point(100, 12);
            this.btnAbrir.Size = new Size(140, 40);
            this.btnAbrir.Click += new EventHandler(this.btnAbrir_Click);
            this.btnAbrir.FlatAppearance.BorderSize = 0;

            this.panelButtons.Controls.Add(this.btnCancelar);
            this.panelButtons.Controls.Add(this.btnAbrir);

            this.Controls.Add(this.panelBody);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.panelHeader);
        }
    }
}
