using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using SistemaVentas.Business.Services;

namespace SistemaVentas.Presentation
{
    public partial class FrmCierreCaja : Form
    {
        private readonly CajaService _cajaService;
        private readonly int _cajaId;
        private readonly string _usuario;
        private decimal _montoEsperado;
        private decimal _montoInicial;
        private decimal _totalVentas;
        private decimal _diferencia;
        private bool _updatingAmount = false;

        public FrmCierreCaja(int cajaId, string usuario)
        {
            _cajaId = cajaId;
            _usuario = usuario;
            _cajaService = new CajaService();
            InitializeComponent();
            CargarInformacion();
        }

        private void CargarInformacion()
        {
            txtFechaHora.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            txtCajaInfo.Text = $"Caja #{_cajaId}";
            txtUsuarioInfo.Text = _usuario;

            try
            {
                _montoInicial = _cajaService.GetMontoInicial(_cajaId);
                _totalVentas = _cajaService.GetTotalVentas(_cajaId);
                _montoEsperado = _montoInicial + _totalVentas;

                txtMontoInicial.Text = _montoInicial.ToString("C0");
                txtTotalVentas.Text = _totalVentas.ToString("C0");
                txtMontoEsperado.Text = _montoEsperado.ToString("C0");
                txtMontoReal.Text = "";
                ActualizarDiferencia();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            txtMontoReal.Focus();
        }

        private void ActualizarDiferencia()
        {
            string textoLimpio = txtMontoReal.Text.Replace("$", "").Replace(".", "").Trim();
            if (decimal.TryParse(textoLimpio, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal montoReal))
            {
                _diferencia = montoReal - _montoEsperado;
                txtDiferencia.Text = _diferencia.ToString("C0");
                
                if (_diferencia > 0)
                {
                    txtDiferencia.BackColor = Color.FromArgb(212, 237, 218);
                    txtDiferencia.ForeColor = Color.FromArgb(21, 87, 36);
                    lblDiferenciaTexto.Text = "SOBRANTE";
                    lblDiferenciaTexto.ForeColor = Color.FromArgb(21, 87, 36);
                }
                else if (_diferencia < 0)
                {
                    txtDiferencia.BackColor = Color.FromArgb(248, 215, 218);
                    txtDiferencia.ForeColor = Color.FromArgb(114, 28, 36);
                    lblDiferenciaTexto.Text = "FALTANTE";
                    lblDiferenciaTexto.ForeColor = Color.FromArgb(114, 28, 36);
                }
                else
                {
                    txtDiferencia.BackColor = Color.FromArgb(209, 231, 221);
                    txtDiferencia.ForeColor = Color.FromArgb(11, 65, 33);
                    lblDiferenciaTexto.Text = "CUADRA";
                    lblDiferenciaTexto.ForeColor = Color.FromArgb(11, 65, 33);
                }
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            try
            {
                string textoLimpio = txtMontoReal.Text.Replace("$", "").Replace(".", "").Replace(",", "").Trim();
                if (!decimal.TryParse(textoLimpio, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal montoReal) || montoReal < 0)
                {
                    MessageBox.Show("Ingrese un monto real valido", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMontoReal.Focus();
                    return;
                }

                string mensaje = $"Monto esperado: {_montoEsperado:C2}\n" +
                                $"Monto real: {montoReal:C2}\n" +
                                $"Diferencia: {_diferencia:C2}\n\n" +
                                $"Desea cerrar la caja?";

                if (MessageBox.Show(mensaje, "Confirmar Cierre", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _cajaService.CerrarCaja(_cajaId, montoReal, txtObservaciones.Text.Trim());
                    MessageBox.Show("Caja cerrada exitosamente", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
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

        private void txtMontoReal_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void txtMontoReal_TextChanged(object sender, EventArgs e)
        {
            if (_updatingAmount) return;

            string texto = txtMontoReal.Text.Replace("$", "").Replace(".", "").Trim();
            if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal monto))
            {
                int cursorPos = txtMontoReal.SelectionStart;
                int longitudAntes = txtMontoReal.Text.Length;
                _updatingAmount = true;
                txtMontoReal.Text = monto.ToString("C0");
                _updatingAmount = false;
                int longitudDespues = txtMontoReal.Text.Length;
                int nuevaPos = cursorPos + (longitudDespues - longitudAntes);
                txtMontoReal.SelectionStart = Math.Max(0, nuevaPos);
            }
            else if (string.IsNullOrWhiteSpace(texto))
            {
                _updatingAmount = true;
                txtMontoReal.Text = "";
                _updatingAmount = false;
            }

            ActualizarDiferencia();
        }
    }

    partial class FrmCierreCaja
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitulo;
        private Panel panelHeader;
        private Panel panelBody;
        private Panel panelButtons;
        private Label lblFechaHora;
        private Label lblCajaId;
        private Label lblUsuario;
        private Label lblMontoInicial;
        private Label lblTotalVentas;
        private Label lblMontoEsperado;
        private Label lblMontoReal;
        private Label lblDiferencia;
        private Label lblDiferenciaTexto;
        private Label lblObservaciones;
        private TextBox txtFechaHora;
        private TextBox txtCajaInfo;
        private TextBox txtUsuarioInfo;
        private TextBox txtMontoInicial;
        private TextBox txtTotalVentas;
        private TextBox txtMontoEsperado;
        private TextBox txtMontoReal;
        private TextBox txtDiferencia;
        private TextBox txtObservaciones;
        private Button btnCerrar;
        private Button btnCancelar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Text = "Cierre de Caja";
            this.Size = new Size(500, 540);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 245);

            this.panelHeader = new Panel();
            this.panelBody = new Panel();
            this.panelButtons = new Panel();
            this.lblFechaHora = new Label();
            this.lblCajaId = new Label();
            this.lblUsuario = new Label();
            this.lblMontoEsperado = new Label();
            this.lblMontoReal = new Label();
            this.lblDiferencia = new Label();
            this.lblDiferenciaTexto = new Label();
            this.lblObservaciones = new Label();
            this.txtFechaHora = new TextBox();
            this.txtCajaInfo = new TextBox();
            this.txtUsuarioInfo = new TextBox();
            this.txtMontoEsperado = new TextBox();
            this.txtMontoReal = new TextBox();
            this.txtDiferencia = new TextBox();
            this.txtObservaciones = new TextBox();
            this.btnCerrar = new Button();
            this.btnCancelar = new Button();

            this.panelHeader = new Panel();
            this.panelHeader.Dock = DockStyle.Top;
            this.panelHeader.Height = 70;
            this.panelHeader.BackColor = Color.FromArgb(192, 57, 43);
            this.panelHeader.Padding = new Padding(20);

            this.lblTitulo = new Label();
            this.lblTitulo.Text = "CIERRE DE CAJA";
            this.lblTitulo.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            this.lblTitulo.ForeColor = Color.White;
            this.lblTitulo.Dock = DockStyle.Fill;
            this.lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            this.panelHeader.Controls.Add(this.lblTitulo);

            this.panelBody = new Panel();
            this.panelBody.Dock = DockStyle.Fill;
            this.panelBody.Padding = new Padding(30, 20, 30, 10);
            this.panelBody.BackColor = Color.FromArgb(250, 250, 252);

            int y = 10;
            int labelWidth = 150;
            int fieldLeft = 170;
            int fieldWidth = 270;

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

            this.lblCajaId = new Label();
            this.lblCajaId.Text = "Caja:";
            this.lblCajaId.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.lblCajaId.ForeColor = Color.FromArgb(80, 80, 90);
            this.lblCajaId.Location = new Point(10, y + 3);
            this.lblCajaId.Size = new Size(labelWidth, 28);
            this.lblCajaId.TextAlign = ContentAlignment.MiddleRight;

            this.txtCajaInfo = new TextBox();
            this.txtCajaInfo.Location = new Point(fieldLeft, y);
            this.txtCajaInfo.Size = new Size(fieldWidth, 28);
            this.txtCajaInfo.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            this.txtCajaInfo.ReadOnly = true;
            this.txtCajaInfo.BackColor = Color.FromArgb(235, 235, 240);
            this.txtCajaInfo.TextAlign = HorizontalAlignment.Center;
            y += 40;

            this.lblUsuario = new Label();
            this.lblUsuario.Text = "Usuario:";
            this.lblUsuario.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.lblUsuario.ForeColor = Color.FromArgb(80, 80, 90);
            this.lblUsuario.Location = new Point(10, y + 3);
            this.lblUsuario.Size = new Size(labelWidth, 28);
            this.lblUsuario.TextAlign = ContentAlignment.MiddleRight;

            this.txtUsuarioInfo = new TextBox();
            this.txtUsuarioInfo.Location = new Point(fieldLeft, y);
            this.txtUsuarioInfo.Size = new Size(fieldWidth, 28);
            this.txtUsuarioInfo.Font = new Font("Segoe UI", 11);
            this.txtUsuarioInfo.ReadOnly = true;
            this.txtUsuarioInfo.BackColor = Color.FromArgb(235, 235, 240);
            this.txtUsuarioInfo.TextAlign = HorizontalAlignment.Center;
            y += 40;

            this.lblMontoInicial = new Label();
            this.lblMontoInicial.Text = "Monto Apertura:";
            this.lblMontoInicial.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.lblMontoInicial.ForeColor = Color.FromArgb(80, 80, 90);
            this.lblMontoInicial.Location = new Point(10, y + 3);
            this.lblMontoInicial.Size = new Size(labelWidth, 28);
            this.lblMontoInicial.TextAlign = ContentAlignment.MiddleRight;

            this.txtMontoInicial = new TextBox();
            this.txtMontoInicial.Location = new Point(fieldLeft, y);
            this.txtMontoInicial.Size = new Size(fieldWidth, 28);
            this.txtMontoInicial.Font = new Font("Segoe UI", 11);
            this.txtMontoInicial.ReadOnly = true;
            this.txtMontoInicial.BackColor = Color.FromArgb(235, 235, 240);
            this.txtMontoInicial.TextAlign = HorizontalAlignment.Right;
            y += 40;

            this.lblTotalVentas = new Label();
            this.lblTotalVentas.Text = "Total Ventas:";
            this.lblTotalVentas.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.lblTotalVentas.ForeColor = Color.FromArgb(80, 80, 90);
            this.lblTotalVentas.Location = new Point(10, y + 3);
            this.lblTotalVentas.Size = new Size(labelWidth, 28);
            this.lblTotalVentas.TextAlign = ContentAlignment.MiddleRight;

            this.txtTotalVentas = new TextBox();
            this.txtTotalVentas.Location = new Point(fieldLeft, y);
            this.txtTotalVentas.Size = new Size(fieldWidth, 28);
            this.txtTotalVentas.Font = new Font("Segoe UI", 11);
            this.txtTotalVentas.ReadOnly = true;
            this.txtTotalVentas.BackColor = Color.FromArgb(235, 235, 240);
            this.txtTotalVentas.TextAlign = HorizontalAlignment.Right;
            y += 40;

            this.lblMontoEsperado.Text = "Monto Esperado:";
            this.lblMontoEsperado.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.lblMontoEsperado.ForeColor = Color.FromArgb(80, 80, 90);
            this.lblMontoEsperado.Location = new Point(10, y + 3);
            this.lblMontoEsperado.Size = new Size(labelWidth, 28);
            this.lblMontoEsperado.TextAlign = ContentAlignment.MiddleRight;

            this.txtMontoEsperado = new TextBox();
            this.txtMontoEsperado.Location = new Point(fieldLeft, y);
            this.txtMontoEsperado.Size = new Size(fieldWidth, 28);
            this.txtMontoEsperado.Font = new Font("Segoe UI", 11);
            this.txtMontoEsperado.ReadOnly = true;
            this.txtMontoEsperado.BackColor = Color.FromArgb(235, 235, 240);
            this.txtMontoEsperado.TextAlign = HorizontalAlignment.Right;
            y += 40;

            this.lblMontoReal.Text = "Monto Real (Conteo):";
            this.lblMontoReal.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.lblMontoReal.ForeColor = Color.FromArgb(44, 62, 80);
            this.lblMontoReal.Location = new Point(10, y + 3);
            this.lblMontoReal.Size = new Size(labelWidth, 28);
            this.lblMontoReal.TextAlign = ContentAlignment.MiddleRight;

            this.txtMontoReal = new TextBox();
            this.txtMontoReal.Location = new Point(fieldLeft, y);
            this.txtMontoReal.Size = new Size(fieldWidth, 28);
            this.txtMontoReal.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.txtMontoReal.TextAlign = HorizontalAlignment.Right;
            this.txtMontoReal.KeyPress += new KeyPressEventHandler(this.txtMontoReal_KeyPress);
            this.txtMontoReal.TextChanged += new EventHandler(this.txtMontoReal_TextChanged);
            y += 40;

            this.lblDiferencia.Text = "Diferencia:";
            this.lblDiferencia.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.lblDiferencia.ForeColor = Color.FromArgb(80, 80, 90);
            this.lblDiferencia.Location = new Point(10, y + 3);
            this.lblDiferencia.Size = new Size(labelWidth, 28);
            this.lblDiferencia.TextAlign = ContentAlignment.MiddleRight;

            this.txtDiferencia = new TextBox();
            this.txtDiferencia.Location = new Point(fieldLeft, y);
            this.txtDiferencia.Size = new Size(fieldWidth - 90, 28);
            this.txtDiferencia.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.txtDiferencia.ReadOnly = true;
            this.txtDiferencia.TextAlign = HorizontalAlignment.Right;
            this.txtDiferencia.BackColor = Color.FromArgb(235, 235, 240);

            this.lblDiferenciaTexto = new Label();
            this.lblDiferenciaTexto.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.lblDiferenciaTexto.Location = new Point(fieldLeft + fieldWidth - 80, y + 3);
            this.lblDiferenciaTexto.Size = new Size(80, 28);
            this.lblDiferenciaTexto.TextAlign = ContentAlignment.MiddleCenter;
            y += 40;

            this.lblObservaciones.Text = "Observaciones:";
            this.lblObservaciones.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.lblObservaciones.ForeColor = Color.FromArgb(80, 80, 90);
            this.lblObservaciones.Location = new Point(10, y + 3);
            this.lblObservaciones.Size = new Size(labelWidth, 28);
            this.lblObservaciones.TextAlign = ContentAlignment.TopRight;

            this.txtObservaciones = new TextBox();
            this.txtObservaciones.Location = new Point(fieldLeft, y);
            this.txtObservaciones.Size = new Size(fieldWidth, 50);
            this.txtObservaciones.Font = new Font("Segoe UI", 10);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.PlaceholderText = "Notas opcionales sobre el cierre...";

            this.panelBody.Controls.Add(this.lblFechaHora);
            this.panelBody.Controls.Add(this.txtFechaHora);
            this.panelBody.Controls.Add(this.lblCajaId);
            this.panelBody.Controls.Add(this.txtCajaInfo);
            this.panelBody.Controls.Add(this.lblUsuario);
            this.panelBody.Controls.Add(this.txtUsuarioInfo);
            this.panelBody.Controls.Add(this.lblMontoInicial);
            this.panelBody.Controls.Add(this.txtMontoInicial);
            this.panelBody.Controls.Add(this.lblTotalVentas);
            this.panelBody.Controls.Add(this.txtTotalVentas);
            this.panelBody.Controls.Add(this.lblMontoEsperado);
            this.panelBody.Controls.Add(this.txtMontoEsperado);
            this.panelBody.Controls.Add(this.lblMontoReal);
            this.panelBody.Controls.Add(this.txtMontoReal);
            this.panelBody.Controls.Add(this.lblDiferencia);
            this.panelBody.Controls.Add(this.txtDiferencia);
            this.panelBody.Controls.Add(this.lblDiferenciaTexto);
            this.panelBody.Controls.Add(this.lblObservaciones);
            this.panelBody.Controls.Add(this.txtObservaciones);

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

            this.btnCerrar = new Button();
            this.btnCerrar.Text = "CERRAR CAJA";
            this.btnCerrar.BackColor = Color.FromArgb(192, 57, 43);
            this.btnCerrar.ForeColor = Color.White;
            this.btnCerrar.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            this.btnCerrar.FlatStyle = FlatStyle.Flat;
            this.btnCerrar.Location = new Point(100, 12);
            this.btnCerrar.Size = new Size(140, 40);
            this.btnCerrar.Click += new EventHandler(this.btnCerrar_Click);
            this.btnCerrar.FlatAppearance.BorderSize = 0;

            this.panelButtons.Controls.Add(this.btnCancelar);
            this.panelButtons.Controls.Add(this.btnCerrar);

            this.Controls.Add(this.panelBody);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.panelHeader);
        }
    }
}
