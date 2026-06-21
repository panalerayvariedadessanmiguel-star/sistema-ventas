using System;
using System.Drawing;
using System.Windows.Forms;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Presentation
{
    public partial class FrmCantidad : Form
    {
        public int Cantidad { get; private set; }

        private readonly Producto _producto;
        private readonly ProductoVariante? _variante;
        private readonly TextBox _txtCantidad;
        private readonly Label _lblTotal;

        private int StockDisponible => _variante?.Stock ?? _producto.Stock;

        public FrmCantidad(Producto producto, ProductoVariante? variante = null)
        {
            _producto = producto;
            _variante = variante;
            this.Text = "Cantidad";
            this.ClientSize = new Size(380, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(0);

            var y = 0;

            var panelHeader = new Panel();
            panelHeader.BackColor = Color.FromArgb(44, 62, 80);
            panelHeader.Size = new Size(380, 60);
            panelHeader.Location = new Point(0, 0);

            var lblTitulo = new Label();
            lblTitulo.Text = "Agregar al Carrito";
            lblTitulo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Size = new Size(360, 25);
            lblTitulo.Location = new Point(15, 8);
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;

            var lblProducto = new Label();
            lblProducto.Text = producto.Nombre;
            lblProducto.Font = new Font("Segoe UI", 9);
            lblProducto.ForeColor = Color.FromArgb(200, 200, 200);
            lblProducto.Size = new Size(360, 18);
            lblProducto.Location = new Point(15, 28);
            lblProducto.TextAlign = ContentAlignment.MiddleLeft;

            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Controls.Add(lblProducto);

            if (variante != null)
            {
                var lblVariante = new Label();
                lblVariante.Text = variante.Nombre == "Única" && variante.ColorHex == "#9E9E9E"
                    ? $"Talla: {variante.Talla ?? "Única"}"
                    : $"{variante.Nombre}{(variante.Talla != null ? $" - Talla {variante.Talla}" : "")}";
                lblVariante.Font = new Font("Segoe UI", 8);
                lblVariante.ForeColor = Color.FromArgb(180, 180, 180);
                lblVariante.Size = new Size(360, 16);
                lblVariante.Location = new Point(15, 44);
                lblVariante.TextAlign = ContentAlignment.MiddleLeft;
                panelHeader.Controls.Add(lblVariante);
            }

            y = 70;

            var panelBody = new Panel();
            panelBody.Size = new Size(340, 160);
            panelBody.Location = new Point(20, y);
            y += 165;

            var labelWidth = 90;
            var valueWidth = 230;
            var rowH = 28;

            var fontLabel = new Font("Segoe UI", 10, FontStyle.Regular);
            var fontValue = new Font("Segoe UI", 10, FontStyle.Bold);
            var colorLabel = Color.FromArgb(100, 100, 100);
            var colorValue = Color.FromArgb(44, 62, 80);
            var xLabel = 0;
            var xValue = labelWidth;

            int stockDisplay = StockDisponible;
            string stockText = variante?.Stock.HasValue == true ? $"{stockDisplay} disponibles" : $"{producto.Stock} disponibles";
            AddInfoRow(panelBody, "Código:", producto.CodigoBarras ?? "---", fontLabel, fontValue, colorLabel, colorValue, xLabel, xValue, 0, labelWidth, valueWidth, rowH);
            AddInfoRow(panelBody, "Stock:", stockText, fontLabel, fontValue, colorLabel, Color.FromArgb(stockDisplay > 0 ? 39 : 192, stockDisplay > 0 ? 174 : 57, stockDisplay > 0 ? 96 : 43), xLabel, xValue, rowH, labelWidth, valueWidth, rowH);
            AddInfoRow(panelBody, "Precio:", producto.PrecioVenta.ToString("C0"), fontLabel, fontValue, colorLabel, colorValue, xLabel, xValue, rowH * 2, labelWidth, valueWidth, rowH);

            var sep = new Label();
            sep.BorderStyle = BorderStyle.FixedSingle;
            sep.Size = new Size(340, 1);
            sep.Location = new Point(0, rowH * 3 + 4);
            panelBody.Controls.Add(sep);

            var lblCantLabel = new Label();
            lblCantLabel.Text = "Cantidad:";
            lblCantLabel.Font = fontLabel;
            lblCantLabel.ForeColor = colorLabel;
            lblCantLabel.Size = new Size(labelWidth, rowH);
            lblCantLabel.Location = new Point(xLabel, rowH * 3 + 14);
            lblCantLabel.TextAlign = ContentAlignment.MiddleLeft;
            panelBody.Controls.Add(lblCantLabel);

            _txtCantidad = new TextBox();
            _txtCantidad.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            _txtCantidad.ForeColor = Color.FromArgb(44, 62, 80);
            _txtCantidad.Size = new Size(90, 30);
            _txtCantidad.Location = new Point(xValue, rowH * 3 + 10);
            _txtCantidad.TextAlign = HorizontalAlignment.Center;
            _txtCantidad.Text = "";
            _txtCantidad.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            _txtCantidad.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Confirmar(); };
            _txtCantidad.TextChanged += (s, e) => ActualizarTotal();
            panelBody.Controls.Add(_txtCantidad);

            var lblTotalLabel = new Label();
            lblTotalLabel.Text = "Total:";
            lblTotalLabel.Font = new Font("Segoe UI", 11);
            lblTotalLabel.ForeColor = colorLabel;
            lblTotalLabel.Size = new Size(50, rowH);
            lblTotalLabel.Location = new Point(xValue + 100, rowH * 3 + 14);
            lblTotalLabel.TextAlign = ContentAlignment.MiddleLeft;
            panelBody.Controls.Add(lblTotalLabel);

            _lblTotal = new Label();
            _lblTotal.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            _lblTotal.ForeColor = Color.FromArgb(39, 174, 96);
            _lblTotal.Size = new Size(150, rowH + 4);
            _lblTotal.Location = new Point(xValue + 145, rowH * 3 + 10);
            _lblTotal.TextAlign = ContentAlignment.MiddleLeft;
            panelBody.Controls.Add(_lblTotal);

            ActualizarTotal();

            var panelBotones = new Panel();
            panelBotones.Size = new Size(380, 55);
            panelBotones.Location = new Point(0, y);
            panelBotones.BackColor = Color.FromArgb(245, 245, 245);

            var btnAceptar = new Button();
            btnAceptar.Text = "Agregar";
            btnAceptar.BackColor = Color.FromArgb(39, 174, 96);
            btnAceptar.ForeColor = Color.White;
            btnAceptar.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnAceptar.Size = new Size(130, 35);
            btnAceptar.Location = new Point(55, 10);
            btnAceptar.FlatStyle = FlatStyle.Flat;
            btnAceptar.FlatAppearance.BorderSize = 0;
            btnAceptar.Click += (s, e) => Confirmar();

            var btnCancelar = new Button();
            btnCancelar.Text = "Cancelar";
            btnCancelar.BackColor = Color.FromArgb(149, 165, 166);
            btnCancelar.ForeColor = Color.White;
            btnCancelar.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnCancelar.Size = new Size(130, 35);
            btnCancelar.Location = new Point(195, 10);
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            this.CancelButton = btnCancelar;

            panelBotones.Controls.Add(btnAceptar);
            panelBotones.Controls.Add(btnCancelar);

            this.Controls.Add(panelHeader);
            this.Controls.Add(panelBody);
            this.Controls.Add(panelBotones);
        }

        private void Confirmar()
        {
            if (!int.TryParse(_txtCantidad.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Ingrese una cantidad válida", "Cantidad requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtCantidad.Focus();
                return;
            }
            int stockDisp = StockDisponible;
            if (qty > stockDisp)
            {
                MessageBox.Show($"Stock insuficiente. Disponible: {stockDisp}", "Sin stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtCantidad.Focus();
                return;
            }
            Cantidad = qty;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ActualizarTotal()
        {
            if (!int.TryParse(_txtCantidad.Text, out int cantidad))
            {
                _lblTotal.Text = "$0";
                return;
            }
            var total = _producto.PrecioVenta * cantidad;
            _lblTotal.Text = total.ToString("C0");
        }

        private void AddInfoRow(Panel parent, string label, string value, Font fontLabel, Font fontValue,
            Color colorLabel, Color colorValue, int xLabel, int xValue, int y, int wLabel, int wValue, int h)
        {
            var lbl = new Label();
            lbl.Text = label;
            lbl.Font = fontLabel;
            lbl.ForeColor = colorLabel;
            lbl.Size = new Size(wLabel, h);
            lbl.Location = new Point(xLabel, y);
            lbl.TextAlign = ContentAlignment.MiddleLeft;
            parent.Controls.Add(lbl);

            var val = new Label();
            val.Text = value;
            val.Font = fontValue;
            val.ForeColor = colorValue;
            val.Size = new Size(wValue, h);
            val.Location = new Point(xValue, y);
            val.TextAlign = ContentAlignment.MiddleLeft;
            parent.Controls.Add(val);
        }
    }
}
