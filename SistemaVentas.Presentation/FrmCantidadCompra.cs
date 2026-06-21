using System;
using System.Drawing;
using System.Windows.Forms;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Presentation
{
    public partial class FrmCantidadCompra : Form
    {
        public int Cantidad { get; private set; }
        public decimal PrecioUnitario { get; private set; }

        private readonly Producto _producto;
        private readonly TextBox _txtCantidad;
        private readonly TextBox _txtPrecio;
        private readonly Label _lblTotal;

        public FrmCantidadCompra(Producto producto)
        {
            _producto = producto;
            Text = "Agregar a Compra";
            ClientSize = new Size(380, 290);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = false;
            BackColor = Color.White;

            var panelHeader = new Panel { BackColor = Color.FromArgb(44, 62, 80), Size = new Size(380, 50), Location = new Point(0, 0) };

            var lblTitulo = new Label
            {
                Text = "Agregar Producto a Compra",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(360, 25),
                Location = new Point(15, 8),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblProducto = new Label
            {
                Text = producto.Nombre,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(200, 200, 200),
                Size = new Size(360, 18),
                Location = new Point(15, 28),
                TextAlign = ContentAlignment.MiddleLeft
            };

            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Controls.Add(lblProducto);

            var panelBody = new Panel { Size = new Size(340, 180), Location = new Point(20, 60) };

            var fontLabel = new Font("Segoe UI", 10);
            var colorLabel = Color.FromArgb(100, 100, 100);

            var lblCodigo = new Label { Text = "Codigo:", Font = fontLabel, ForeColor = colorLabel, Size = new Size(80, 25), Location = new Point(0, 0), TextAlign = ContentAlignment.MiddleLeft };
            var valCodigo = new Label { Text = producto.CodigoBarras ?? "---", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80), Size = new Size(250, 25), Location = new Point(90, 0), TextAlign = ContentAlignment.MiddleLeft };

            var lblStock = new Label { Text = "Stock actual:", Font = fontLabel, ForeColor = colorLabel, Size = new Size(80, 25), Location = new Point(0, 28), TextAlign = ContentAlignment.MiddleLeft };
            var valStock = new Label { Text = $"{producto.Stock} unidades", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(39, 174, 96), Size = new Size(250, 25), Location = new Point(90, 28), TextAlign = ContentAlignment.MiddleLeft };

            var lblCant = new Label { Text = "Cantidad:", Font = fontLabel, ForeColor = colorLabel, Size = new Size(80, 25), Location = new Point(0, 65), TextAlign = ContentAlignment.MiddleLeft };
            _txtCantidad = new TextBox
            {
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(100, 25),
                Location = new Point(90, 63),
                TextAlign = HorizontalAlignment.Center,
                Text = "1"
            };
            _txtCantidad.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            _txtCantidad.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Confirmar(); };
            _txtCantidad.TextChanged += (s, e) => ActualizarTotal();

            var lblPrecio = new Label { Text = "Precio compra:", Font = fontLabel, ForeColor = colorLabel, Size = new Size(80, 25), Location = new Point(0, 100), TextAlign = ContentAlignment.MiddleLeft };
            _txtPrecio = new TextBox
            {
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(100, 25),
                Location = new Point(90, 98),
                TextAlign = HorizontalAlignment.Center,
                Text = producto.PrecioCompra > 0 ? producto.PrecioCompra.ToString("N0") : ""
            };
            _txtPrecio.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Confirmar(); };
            _txtPrecio.TextChanged += (s, e) => ActualizarTotal();

            var lblTotalLabel = new Label { Text = "Total:", Font = new Font("Segoe UI", 11), ForeColor = colorLabel, Size = new Size(50, 25), Location = new Point(0, 140), TextAlign = ContentAlignment.MiddleLeft };
            _lblTotal = new Label { Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(39, 174, 96), Size = new Size(200, 30), Location = new Point(60, 138), TextAlign = ContentAlignment.MiddleLeft };

            panelBody.Controls.AddRange(new Control[] { lblCodigo, valCodigo, lblStock, valStock, lblCant, _txtCantidad, lblPrecio, _txtPrecio, lblTotalLabel, _lblTotal });

            ActualizarTotal();

            var panelBotones = new Panel { Size = new Size(380, 55), Location = new Point(0, 245), BackColor = Color.FromArgb(245, 245, 245) };

            var btnAceptar = new Button
            {
                Text = "Agregar",
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(130, 35),
                Location = new Point(55, 10),
                FlatStyle = FlatStyle.Flat
            };
            btnAceptar.Click += (s, e) => Confirmar();

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(130, 35),
                Location = new Point(195, 10),
                FlatStyle = FlatStyle.Flat
            };
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            CancelButton = btnCancelar;

            panelBotones.Controls.Add(btnAceptar);
            panelBotones.Controls.Add(btnCancelar);

            Controls.Add(panelHeader);
            Controls.Add(panelBody);
            Controls.Add(panelBotones);

            Shown += (s, e) => _txtCantidad.Focus();
        }

        private void Confirmar()
        {
            if (!int.TryParse(_txtCantidad.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Ingrese una cantidad valida", "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtCantidad.Focus();
                return;
            }
            string precioText = _txtPrecio.Text.Replace(".", "").Replace(",", ".");
            if (!decimal.TryParse(precioText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal precio) || precio <= 0)
            {
                MessageBox.Show("Ingrese un precio de compra valido", "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtPrecio.Focus();
                return;
            }
            Cantidad = qty;
            PrecioUnitario = precio;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ActualizarTotal()
        {
            if (!int.TryParse(_txtCantidad.Text, out int cantidad)) cantidad = 0;
            string precioText = _txtPrecio.Text.Replace(".", "").Replace(",", ".");
            if (!decimal.TryParse(precioText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal precio)) precio = 0;
            _lblTotal.Text = (cantidad * precio).ToString("N0");
        }
    }
}
