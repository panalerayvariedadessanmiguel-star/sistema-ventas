using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Presentation
{
    public partial class FrmSeleccionProducto : Form
    {
        public Producto ProductoSeleccionado { get; private set; }

        public FrmSeleccionProducto(List<Producto> productos)
        {
            InitializeComponent();
            CargarProductos(productos);
        }

        private void CargarProductos(List<Producto> productos)
        {
            dgvSeleccion.Rows.Clear();
            foreach (var p in productos)
            {
                dgvSeleccion.Rows.Add(p.Id, p.Nombre, p.CodigoBarras, p.PrecioVenta.ToString("C2"), p.Stock);
            }
        }

        private void dgvSeleccion_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                ProductoSeleccionado = new Producto
                {
                    Id = (int)dgvSeleccion.Rows[e.RowIndex].Cells["colId"].Value,
                    Nombre = dgvSeleccion.Rows[e.RowIndex].Cells["colNombre"].Value.ToString(),
                    PrecioVenta = decimal.Parse(dgvSeleccion.Rows[e.RowIndex].Cells["colPrecio"].Value.ToString().Replace("$", "").Replace(",", "")),
                    Stock = (int)dgvSeleccion.Rows[e.RowIndex].Cells["colStock"].Value
                };
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }

    partial class FrmSeleccionProducto
    {
        private System.ComponentModel.IContainer components = null;
        private DataGridView dgvSeleccion;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Text = "Seleccionar Producto";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.dgvSeleccion = new DataGridView();
            this.dgvSeleccion.Dock = DockStyle.Fill;
            this.dgvSeleccion.AllowUserToAddRows = false;
            this.dgvSeleccion.ReadOnly = true;
            this.dgvSeleccion.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvSeleccion.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvSeleccion.BackgroundColor = Color.White;
            this.dgvSeleccion.Columns.Add("colId", "ID");
            this.dgvSeleccion.Columns.Add("colNombre", "Nombre");
            this.dgvSeleccion.Columns.Add("colCodigo", "Codigo");
            this.dgvSeleccion.Columns.Add("colPrecio", "Precio");
            this.dgvSeleccion.Columns.Add("colStock", "Stock");
            this.dgvSeleccion.CellDoubleClick += new DataGridViewCellEventHandler(this.dgvSeleccion_CellDoubleClick);

            this.Controls.Add(this.dgvSeleccion);
        }
    }
}
