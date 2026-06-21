using System;
using System.Drawing;
using System.Windows.Forms;
using SistemaVentas.Business.Services;

namespace SistemaVentas.Presentation
{
    public partial class FrmConfiguracionMargenes : Form
    {
        private readonly ConfiguracionService _service;
        private NumericUpDown nudMargenFijos;
        private NumericUpDown nudMargenVariables;
        private NumericUpDown nudMargenUtilidad;
        private Label lblResultado;

        public decimal MargenFijos { get; private set; }
        public decimal MargenVariables { get; private set; }
        public decimal MargenUtilidad { get; private set; }

        public FrmConfiguracionMargenes()
        {
            _service = new ConfiguracionService();
            InitializeComponent();
            CargarValores();
        }

        private void CargarValores()
        {
            nudMargenFijos.Value = _service.LeerDecimal("MARGEN_FIJOS", 25);
            nudMargenVariables.Value = _service.LeerDecimal("MARGEN_VARIABLES", 5);
            nudMargenUtilidad.Value = _service.LeerDecimal("MARGEN_UTILIDAD", 15);
            ActualizarVistaPrevia();
        }

        private void ActualizarVistaPrevia()
        {
            decimal total = nudMargenFijos.Value + nudMargenVariables.Value + nudMargenUtilidad.Value;

            if (total >= 100)
            {
                lblResultado.Text = "Los margenes suman 100% o mas. El precio de venta no puede calcularse.";
                lblResultado.ForeColor = Color.Red;
                return;
            }

            decimal precioCompra = 100;
            decimal precioVenta = total > 0 ? Math.Round(precioCompra / (1 - total / 100m), 2) : precioCompra;

            decimal costosFijos = Math.Round(precioVenta * nudMargenFijos.Value / 100m, 2);
            decimal costosVar = Math.Round(precioVenta * nudMargenVariables.Value / 100m, 2);
            decimal utilidad = Math.Round(precioVenta * nudMargenUtilidad.Value / 100m, 2);

            lblResultado.Text = $"Costo $100 → Venta: ${precioVenta:0.00} | " +
                                $"C.Fijos: ${costosFijos:0.00} ({nudMargenFijos.Value:0}%) | " +
                                $"C.Var: ${costosVar:0.00} ({nudMargenVariables.Value:0}%) | " +
                                $"Utilidad: ${utilidad:0.00} ({nudMargenUtilidad.Value:0}%)";
            lblResultado.ForeColor = Color.FromArgb(39, 174, 96);
        }

        private void Guardar_Click(object sender, EventArgs e)
        {
            _service.GuardarValor("MARGEN_FIJOS", nudMargenFijos.Value.ToString("0"));
            _service.GuardarValor("MARGEN_VARIABLES", nudMargenVariables.Value.ToString("0"));
            _service.GuardarValor("MARGEN_UTILIDAD", nudMargenUtilidad.Value.ToString("0"));

            MargenFijos = nudMargenFijos.Value;
            MargenVariables = nudMargenVariables.Value;
            MargenUtilidad = nudMargenUtilidad.Value;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void InitializeComponent()
        {
            this.Text = "Configuracion de Margenes de Precio";
            this.Size = new Size(500, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblFijos = new Label { Text = "Costos Fijos (salarios, arriendo, servicios):", Location = new Point(20, 20), Size = new Size(320, 20) };
            nudMargenFijos = new NumericUpDown { Location = new Point(350, 18), Size = new Size(80, 20), Minimum = 0, Maximum = 99, DecimalPlaces = 0, Value = 25 };
            nudMargenFijos.ValueChanged += (s, e) => ActualizarVistaPrevia();

            var lblVariables = new Label { Text = "Costos Variables (bolsas, cajas, bisuteria):", Location = new Point(20, 55), Size = new Size(320, 20) };
            nudMargenVariables = new NumericUpDown { Location = new Point(350, 53), Size = new Size(80, 20), Minimum = 0, Maximum = 99, DecimalPlaces = 0, Value = 5 };
            nudMargenVariables.ValueChanged += (s, e) => ActualizarVistaPrevia();

            var lblUtilidad = new Label { Text = "Margen de Utilidad (ganancia deseada):", Location = new Point(20, 90), Size = new Size(320, 20) };
            nudMargenUtilidad = new NumericUpDown { Location = new Point(350, 88), Size = new Size(80, 20), Minimum = 0, Maximum = 99, DecimalPlaces = 0, Value = 15 };
            nudMargenUtilidad.ValueChanged += (s, e) => ActualizarVistaPrevia();

            var lblTotal = new Label { Text = "Total margen:", Location = new Point(20, 125), Size = new Size(100, 20) };
            var lblTotalValor = new Label { Location = new Point(130, 125), Size = new Size(100, 20) };
            nudMargenFijos.ValueChanged += (s, e) => lblTotalValor.Text = $"{nudMargenFijos.Value + nudMargenVariables.Value + nudMargenUtilidad.Value}%";
            nudMargenVariables.ValueChanged += (s, e) => lblTotalValor.Text = $"{nudMargenFijos.Value + nudMargenVariables.Value + nudMargenUtilidad.Value}%";
            nudMargenUtilidad.ValueChanged += (s, e) => lblTotalValor.Text = $"{nudMargenFijos.Value + nudMargenVariables.Value + nudMargenUtilidad.Value}%";
            lblTotalValor.Text = $"{nudMargenFijos.Value + nudMargenVariables.Value + nudMargenUtilidad.Value}%";

            lblResultado = new Label { Location = new Point(20, 160), Size = new Size(450, 30), ForeColor = Color.FromArgb(39, 174, 96) };
            ActualizarVistaPrevia();

            var btnGuardar = new Button { Text = "Guardar", Location = new Point(120, 200), Size = new Size(100, 30), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White };
            btnGuardar.Click += Guardar_Click;

            var btnCancelar = new Button { Text = "Cancelar", Location = new Point(240, 200), Size = new Size(100, 30), BackColor = Color.FromArgb(149, 165, 166), ForeColor = Color.White };
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(lblFijos);
            Controls.Add(nudMargenFijos);
            Controls.Add(lblVariables);
            Controls.Add(nudMargenVariables);
            Controls.Add(lblUtilidad);
            Controls.Add(nudMargenUtilidad);
            Controls.Add(lblTotal);
            Controls.Add(lblTotalValor);
            Controls.Add(lblResultado);
            Controls.Add(btnGuardar);
            Controls.Add(btnCancelar);
        }
    }
}
