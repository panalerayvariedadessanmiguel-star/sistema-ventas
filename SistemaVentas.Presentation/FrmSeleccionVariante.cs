using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SistemaVentas.Data.Models;
using SistemaVentas.Data.Repositories;

namespace SistemaVentas.Presentation
{
    public partial class FrmSeleccionVariante : Form
    {
        public ProductoVariante VarianteSeleccionado { get; private set; }

        private readonly ProductoVarianteRepository _varianteRepo;
        private readonly Producto _producto;
        private List<ProductoVariante> _variantes;
        private readonly DataGridView _dgvVariantes;

        public FrmSeleccionVariante(Producto producto)
        {
            _producto = producto;
            _varianteRepo = new ProductoVarianteRepository();
            _variantes = new List<ProductoVariante>();

            this.Text = $"Seleccionar variante - {producto.Nombre}";
            this.ClientSize = new Size(520, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false;
            this.BackColor = Color.White;

            var y = 0;

            var panelHeader = new Panel();
            panelHeader.BackColor = Color.FromArgb(44, 62, 80);
            panelHeader.Size = new Size(520, 50);
            panelHeader.Location = new Point(0, 0);

            var lblTitulo = new Label();
            lblTitulo.Text = "Seleccionar variante";
            lblTitulo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Size = new Size(500, 25);
            lblTitulo.Location = new Point(15, 8);

            var lblProducto = new Label();
            lblProducto.Text = producto.Nombre;
            lblProducto.Font = new Font("Segoe UI", 9);
            lblProducto.ForeColor = Color.FromArgb(200, 200, 200);
            lblProducto.Size = new Size(500, 18);
            lblProducto.Location = new Point(15, 28);

            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Controls.Add(lblProducto);
            y = 60;

            _dgvVariantes = new DataGridView();
            _dgvVariantes.Location = new Point(15, y);
            _dgvVariantes.Size = new Size(490, 240);
            _dgvVariantes.AllowUserToAddRows = false;
            _dgvVariantes.ReadOnly = true;
            _dgvVariantes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvVariantes.MultiSelect = false;
            _dgvVariantes.BackgroundColor = Color.White;
            _dgvVariantes.RowHeadersVisible = false;
            _dgvVariantes.Columns.Add("Color", "Color");
            _dgvVariantes.Columns.Add("Talla", "Talla");
            _dgvVariantes.Columns.Add("Stock", "Stock");
            _dgvVariantes.Columns["Color"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvVariantes.Columns["Talla"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            _dgvVariantes.Columns["Stock"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            _dgvVariantes.CellDoubleClick += (s, e) => Confirmar();
            _dgvVariantes.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Confirmar(); };

            y += 250;

            var panelBotones = new Panel();
            panelBotones.Size = new Size(520, 55);
            panelBotones.Location = new Point(0, y);
            panelBotones.BackColor = Color.FromArgb(245, 245, 245);

            var btnAceptar = new Button();
            btnAceptar.Text = "Seleccionar";
            btnAceptar.BackColor = Color.FromArgb(39, 174, 96);
            btnAceptar.ForeColor = Color.White;
            btnAceptar.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnAceptar.Size = new Size(140, 35);
            btnAceptar.Location = new Point(95, 10);
            btnAceptar.FlatStyle = FlatStyle.Flat;
            btnAceptar.FlatAppearance.BorderSize = 0;
            btnAceptar.Click += (s, e) => Confirmar();

            var btnCancelar = new Button();
            btnCancelar.Text = "Sin variante";
            btnCancelar.BackColor = Color.FromArgb(149, 165, 166);
            btnCancelar.ForeColor = Color.White;
            btnCancelar.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnCancelar.Size = new Size(140, 35);
            btnCancelar.Location = new Point(250, 10);
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, e) =>
            {
                VarianteSeleccionado = null;
                DialogResult = DialogResult.OK;
                Close();
            };
            this.CancelButton = btnCancelar;

            panelBotones.Controls.Add(btnAceptar);
            panelBotones.Controls.Add(btnCancelar);

            this.Controls.Add(panelHeader);
            this.Controls.Add(_dgvVariantes);
            this.Controls.Add(panelBotones);

            CargarVariantes();
        }

        private void CargarVariantes()
        {
            _variantes = _varianteRepo.GetByProducto(_producto.Id);
            _dgvVariantes.Rows.Clear();
            foreach (var v in _variantes)
            {
                string colorDisplay = v.Nombre;
                if (v.Nombre == "Única" && v.ColorHex == "#9E9E9E")
                    colorDisplay = "Solo talla";
                _dgvVariantes.Rows.Add(colorDisplay, v.Talla ?? "Única",
                    v.Stock.HasValue ? v.Stock.ToString() : "Ilimitado");
            }

            if (_variantes.Count > 0)
                _dgvVariantes.Rows[0].Selected = true;
        }

        private void Confirmar()
        {
            if (_dgvVariantes.SelectedRows.Count == 0) return;
            int index = _dgvVariantes.SelectedRows[0].Index;
            if (index < 0 || index >= _variantes.Count) return;

            VarianteSeleccionado = _variantes[index];
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
