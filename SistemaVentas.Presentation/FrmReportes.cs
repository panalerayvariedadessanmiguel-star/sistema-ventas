using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using SistemaVentas.Business.Services;

namespace SistemaVentas.Presentation
{
    public partial class FrmReportes : Form
    {
        private readonly ReporteService _reporteService;

        public FrmReportes()
        {
            InitializeComponent();
            _reporteService = new ReporteService();
            CargarAnios();
            CargarMeses();
        }

        private void GenerarReporte()
        {
            try
            {
                int anio = (cmbAnio.SelectedItem != null) ? (int)cmbAnio.SelectedItem : DateTime.Now.Year;
                int mes = (cmbMes.SelectedIndex >= 0) ? cmbMes.SelectedIndex + 1 : DateTime.Now.Month;

                var detalles = _reporteService.GetDetalleUtilidades(anio, mes);
                dgvDetalle.Rows.Clear();

                decimal totalVentas = 0;
                decimal totalUtilidad = 0;

                var cultura = new CultureInfo("es-CO");

                foreach (var d in detalles)
                {
                    dgvDetalle.Rows.Add(d.Producto, d.CantidadVendida,
                        Convert.ToDecimal(d.TotalVentas).ToString("C0", cultura),
                        Convert.ToDecimal(d.TotalCosto).ToString("C0", cultura),
                        Convert.ToDecimal(d.Utilidad).ToString("C0", cultura));
                    totalVentas += d.TotalVentas;
                    totalUtilidad += d.Utilidad;
                }

                lblTotalVentas.Text = $"Total Ventas: {totalVentas.ToString("C0", cultura)}";
                lblTotalUtilidad.Text = $"Total Utilidad: {totalUtilidad.ToString("C0", cultura)}";
                lblTotalTransacciones.Text = $"Transacciones: {detalles.Count}";
                lblMargen.Text = totalVentas > 0 ? $"Margen: {(totalUtilidad / totalVentas) * 100:F1}%" : "Margen: 0%";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerarReporteDiario()
        {
            try
            {
                DateTime fecha = dtpFecha.Value.Date;
                var ventas = _reporteService.GetVentasDiarias(fecha);
                dgvVentasDiarias.Rows.Clear();
                decimal totalVentas = 0;

                foreach (var v in ventas)
                {
                    dgvVentasDiarias.Rows.Add(v.Id, v.NumeroVenta, Convert.ToDateTime(v.FechaVenta).ToString("dd/MM/yyyy HH:mm"), v.NombreCliente, v.Total.ToString("C0"), v.MetodoPago, v.Usuario);
                    totalVentas += v.Total;
                }

                lblTotalVentasDiario.Text = $"Total Ventas: {totalVentas:C0}";
                lblTotalTransDiario.Text = $"Transacciones: {ventas.Count}";

                // Mostrar todos los productos vendidos ese día
                var productos = _reporteService.GetProductosVendidosDiarios(fecha);
                dgvDetalleVenta.Rows.Clear();
                foreach (var p in productos)
                {
                    dgvDetalleVenta.Rows.Add(p.CodigoBarras, p.Producto, p.Cantidad, Convert.ToDecimal(p.PrecioUnitario).ToString("C0"), Convert.ToDecimal(p.SubTotal).ToString("C0"));
                }

                // Obtener categoría con mayor utilidad
                var categorias = _reporteService.GetUtilidadPorCategoriaDiaria(fecha);
                if (categorias.Count > 0)
                {
                    var topCategoria = categorias[0];
                    lblTopCategoria.Text = $"Mejor Categoría: {topCategoria.Categoria} ({Convert.ToDecimal(topCategoria.Utilidad):C0})";
                }
                else
                {
                    lblTopCategoria.Text = "Mejor Categoría: N/A";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGenerar_Click(object sender, EventArgs e) { GenerarReporte(); }
        private void btnGenerarDiario_Click(object sender, EventArgs e) { GenerarReporteDiario(); }
        private void btnExportarDiario_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime fecha = dtpFecha.Value.Date;
                var ventas = _reporteService.GetVentasDiarias(fecha);
                var productos = _reporteService.GetProductosVendidosDiarios(fecha);
                decimal totalVentas = 0;

                foreach (var v in ventas)
                {
                    totalVentas += v.Total;
                }

                var cultura = new CultureInfo("es-CO");

                string directorio = @"C:\Users\Familia_Jica\Desktop\Pañalera y Variedades San Miguel\Reportes\Reportes Diarios";
                if (!Directory.Exists(directorio))
                    Directory.CreateDirectory(directorio);

                string nombreArchivo = $"Reporte_Diario_{fecha:yyyy-MM-dd}.txt";
                string rutaCompleta = Path.Combine(directorio, nombreArchivo);

                using (var writer = new StreamWriter(rutaCompleta, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine($"REPORTE DE VENTAS DIARIAS - {fecha:dd/MM/yyyy}");
                    writer.WriteLine(new string('=', 70));
                    writer.WriteLine();

                    writer.WriteLine("RESUMEN:");
                    writer.WriteLine($"  Fecha: {fecha:dd/MM/yyyy}");
                    writer.WriteLine($"  Total Ventas: {totalVentas.ToString("C0", cultura)}");
                    writer.WriteLine($"  Transacciones: {ventas.Count}");
                    writer.WriteLine();
                    writer.WriteLine(new string('-', 70));
                    writer.WriteLine();

                    writer.WriteLine("DETALLE DE VENTAS:");
                    writer.WriteLine($"{"No. Venta",-12} {"Fecha",-20} {"Cliente",-25} {"Total",-12} {"Método Pago",-15} {"Usuario",-15}");
                     writer.WriteLine(new string('-', 100));

                      foreach (var v in ventas)
                      {
                          string numeroVenta = v.NumeroVenta?.ToString() ?? "";
                          string fechaVenta = Convert.ToDateTime(v.FechaVenta).ToString("dd/MM/yyyy HH:mm");
                          string cliente = (v.NombreCliente?.ToString() ?? "").Length > 25 ?
                              (v.NombreCliente?.ToString() ?? "").Substring(0, 22) + "..." :
                              (v.NombreCliente?.ToString() ?? "");
                          string metodoPago = v.MetodoPago?.ToString() ?? "";
                          string usuario = v.Usuario?.ToString() ?? "";

                          writer.WriteLine($"{numeroVenta,-12} {fechaVenta,-20} {cliente,-25} {v.Total.ToString("C0", cultura),-12} {metodoPago,-15} {usuario,-15}");
                      }

                    writer.WriteLine();
                    writer.WriteLine(new string('-', 70));
                    writer.WriteLine();

                    writer.WriteLine("DETALLE DE PRODUCTOS VENDIDOS:");
                    writer.WriteLine($"{"Código",-15} {"Producto",-30} {"Cant.",-10} {"Precio Unit.",-15} {"SubTotal",-15}");
                    writer.WriteLine(new string('-', 90));

                    foreach (var p in productos)
                    {
                        string codigo = (p.CodigoBarras?.ToString() ?? "").Length > 15 ?
                            (p.CodigoBarras?.ToString() ?? "").Substring(0, 12) + "..." :
                            (p.CodigoBarras?.ToString() ?? "");
                        string producto = (p.Producto?.ToString() ?? "").Length > 30 ?
                            (p.Producto?.ToString() ?? "").Substring(0, 27) + "..." :
                            (p.Producto?.ToString() ?? "");

                         writer.WriteLine($"{codigo,-15} {producto,-30} {p.Cantidad,-10} {Convert.ToDecimal(p.PrecioUnitario).ToString("C0", cultura),-15} {Convert.ToDecimal(p.SubTotal).ToString("C0", cultura),-15}");
                    }

                    writer.WriteLine();
                    writer.WriteLine(new string('=', 70));
                    writer.WriteLine($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                }

                MessageBox.Show($"Archivo TXT generado en:\n{rutaCompleta}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvVentasDiarias_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgvVentasDiarias.SelectedRows.Count > 0)
                {
                    var fila = dgvVentasDiarias.SelectedRows[0];
                    if (fila.Cells["colId"].Value != null)
                    {
                        int ventaId = Convert.ToInt32(fila.Cells["colId"].Value);
                        var detalles = _reporteService.GetDetalleVenta(ventaId);
                        dgvDetalleVenta.Rows.Clear();

                        foreach (var d in detalles)
                        {
                            dgvDetalleVenta.Rows.Add(d.CodigoBarras, d.Producto, d.Cantidad, Convert.ToDecimal(d.PrecioUnitario).ToString("C0"), Convert.ToDecimal(d.SubTotal).ToString("C0"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar detalle: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportarPDF_Click(object sender, EventArgs e)
        {
            try
            {
                int anio = (cmbAnio.SelectedItem != null) ? (int)cmbAnio.SelectedItem : DateTime.Now.Year;
                int mes = (cmbMes.SelectedIndex >= 0) ? cmbMes.SelectedIndex + 1 : DateTime.Now.Month;

                var detalles = _reporteService.GetDetalleUtilidades(anio, mes);
                decimal totalVentas = 0;
                decimal totalUtilidad = 0;

                foreach (var d in detalles)
                {
                    totalVentas += d.TotalVentas;
                    totalUtilidad += d.Utilidad;
                }

                int totalTransacciones = detalles.Count;
                decimal margen = totalVentas > 0 ? (totalUtilidad / totalVentas) * 100 : 0;

                string directorio = @"C:\Users\Familia_Jica\Desktop\Pañalera y Variedades San Miguel\Reportes\Reportes Mensuales";
                if (!Directory.Exists(directorio))
                    Directory.CreateDirectory(directorio);

                string nombreArchivo = $"Reporte_Mensual_{anio}_{mes:00}.txt";
                string rutaCompleta = Path.Combine(directorio, nombreArchivo);

                var cultura = new CultureInfo("es-CO");

                // Generar archivo de texto
                using (var writer = new StreamWriter(rutaCompleta, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine($"REPORTE MENSUAL DE UTILIDADES - {mes}/{anio}");
                    writer.WriteLine(new string('=', 60));
                    writer.WriteLine();

                    writer.WriteLine("RESUMEN:");
                    writer.WriteLine($"  Total Ventas: {totalVentas.ToString("C0", cultura)}");
                    writer.WriteLine($"  Total Utilidad: {totalUtilidad.ToString("C0", cultura)}");
                    writer.WriteLine($"  Transacciones: {totalTransacciones}");
                    writer.WriteLine($"  Margen: {margen:F1}%");
                    writer.WriteLine();
                    writer.WriteLine(new string('-', 60));
                    writer.WriteLine();

                    writer.WriteLine("DETALLE POR PRODUCTO:");
                    writer.WriteLine($"{"Producto",-30} {"Cant.",-10} {"Ventas",-15} {"Costo",-15} {"Utilidad",-15}");
                    writer.WriteLine(new string('-', 85));

                    foreach (var d in detalles)
                    {
                        string producto = (d.Producto?.ToString() ?? "").Length > 30 ?
                            (d.Producto?.ToString() ?? "").Substring(0, 27) + "..." :
                            (d.Producto?.ToString() ?? "");

                        writer.WriteLine($"{producto,-30} {d.CantidadVendida,-10} {Convert.ToDecimal(d.TotalVentas).ToString("C0", cultura),-15} {Convert.ToDecimal(d.TotalCosto).ToString("C0", cultura),-15} {Convert.ToDecimal(d.Utilidad).ToString("C0", cultura),-15}");
                    }

                    writer.WriteLine();
                    writer.WriteLine(new string('=', 60));
                    writer.WriteLine($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                }

                MessageBox.Show($"Archivo TXT generado en:\n{rutaCompleta}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarAnios()
        {
            for (int i = DateTime.Now.Year; i >= DateTime.Now.Year - 5; i--) cmbAnio.Items.Add(i);
            cmbAnio.SelectedItem = DateTime.Now.Year;
        }

        private void CargarMeses()
        {
            cmbMes.Items.AddRange(new string[] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" });
            cmbMes.SelectedIndex = DateTime.Now.Month - 1;
        }
    }

    partial class FrmReportes
    {
        private System.ComponentModel.IContainer components = null;
        private ComboBox cmbAnio;
        private ComboBox cmbMes;
        private Button btnGenerar;
        private Label lblTotalVentas;
        private Label lblTotalUtilidad;
        private Label lblTotalTransacciones;
        private Label lblMargen;
        private Label lblTopCategoria;
        private DataGridView dgvDetalle;
        private DateTimePicker dtpFecha;
        private Button btnGenerarDiario;
        private Label lblTotalVentasDiario;
        private Label lblTotalTransDiario;
        private DataGridView dgvVentasDiarias;
        private DataGridView dgvDetalleVenta;
        private TabControl tabControl;
        private Button btnExportarPDF;
        private Button btnExportarDiario;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Text = "Reportes";
            this.Size = new Size(950, 700);
            this.StartPosition = FormStartPosition.CenterParent;

            tabControl = new TabControl() { Dock = DockStyle.Fill };

            var tabMensual = new TabPage("Reporte Mensual") { BackColor = Color.White };

            var panelFiltros = new Panel() { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(236, 240, 241) };
            var lblAnio = new Label() { Text = "Año:", Location = new Point(20, 20), Size = new Size(50, 20) };
            cmbAnio = new ComboBox() { Location = new Point(70, 18), Size = new Size(100, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            var lblMes = new Label() { Text = "Mes:", Location = new Point(190, 20), Size = new Size(50, 20) };
            cmbMes = new ComboBox() { Location = new Point(240, 18), Size = new Size(180, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            btnGenerar = new Button() { Text = "Generar Reporte", Location = new Point(440, 15), Size = new Size(150, 30), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
            btnGenerar.Click += btnGenerar_Click;
            btnExportarPDF = new Button() { Text = "Exportar TXT", Location = new Point(600, 15), Size = new Size(150, 30), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White };
            btnExportarPDF.Click += btnExportarPDF_Click;
            panelFiltros.Controls.AddRange(new Control[] { lblAnio, cmbAnio, lblMes, cmbMes, btnGenerar, btnExportarPDF });

            var panelResumen = new Panel() { Dock = DockStyle.Top, Height = 120, BackColor = Color.FromArgb(44, 62, 80) };
            lblTotalVentas = new Label() { Text = "Total Ventas: $0.00", Location = new Point(30, 20), AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            lblTotalUtilidad = new Label() { Text = "Total Utilidad: $0.00", Location = new Point(310, 20), AutoSize = true, ForeColor = Color.FromArgb(46, 204, 113), Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            lblTotalTransacciones = new Label() { Text = "Transacciones: 0", Location = new Point(590, 20), AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            lblMargen = new Label() { Text = "Margen: 0%", Location = new Point(30, 70), AutoSize = true, ForeColor = Color.FromArgb(241, 196, 15), Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            panelResumen.Controls.AddRange(new Control[] { lblTotalVentas, lblTotalUtilidad, lblTotalTransacciones, lblMargen });

            dgvDetalle = new DataGridView() { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, BackgroundColor = Color.White };
            dgvDetalle.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn() { Name = "colProducto", HeaderText = "Producto" },
                new DataGridViewTextBoxColumn() { Name = "colCantidad", HeaderText = "Cant. Vendida" },
                new DataGridViewTextBoxColumn() { Name = "colVentas", HeaderText = "Total Ventas" },
                new DataGridViewTextBoxColumn() { Name = "colCosto", HeaderText = "Total Costo" },
                new DataGridViewTextBoxColumn() { Name = "colUtilidad", HeaderText = "Utilidad" }
            });

            tabMensual.Controls.AddRange(new Control[] { dgvDetalle, panelResumen, panelFiltros });

            var tabDiario = new TabPage("Ventas Diarias") { BackColor = Color.White };

            var panelFiltrosDiario = new Panel() { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(236, 240, 241) };
            var lblFecha = new Label() { Text = "Fecha:", Location = new Point(20, 20), Size = new Size(50, 20) };
            dtpFecha = new DateTimePicker() { Location = new Point(70, 18), Size = new Size(150, 20), Value = DateTime.Now, Format = DateTimePickerFormat.Short };
            btnGenerarDiario = new Button() { Text = "Generar Reporte", Location = new Point(230, 15), Size = new Size(150, 30), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
            btnGenerarDiario.Click += btnGenerarDiario_Click;
            btnExportarDiario = new Button() { Text = "Exportar TXT", Location = new Point(390, 15), Size = new Size(150, 30), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White };
            btnExportarDiario.Click += btnExportarDiario_Click;
            panelFiltrosDiario.Controls.AddRange(new Control[] { lblFecha, dtpFecha, btnGenerarDiario, btnExportarDiario });

            var panelResumenDiario = new Panel() { Dock = DockStyle.Top, Height = 90, BackColor = Color.FromArgb(44, 62, 80) };
            lblTotalVentasDiario = new Label() { Text = "Total Ventas: $0.00", Location = new Point(30, 20), AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            lblTotalTransDiario = new Label() { Text = "Transacciones: 0", Location = new Point(350, 20), AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            lblTopCategoria = new Label() { Text = "Mejor Categoría: N/A", Location = new Point(30, 55), AutoSize = true, ForeColor = Color.FromArgb(241, 196, 15), Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            panelResumenDiario.Controls.AddRange(new Control[] { lblTotalVentasDiario, lblTotalTransDiario, lblTopCategoria });

            dgvVentasDiarias = new DataGridView() { Dock = DockStyle.Top, Height = 200, AllowUserToAddRows = false, ReadOnly = true, BackgroundColor = Color.White };
            dgvVentasDiarias.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn() { Name = "colId", HeaderText = "Id", Visible = false },
                new DataGridViewTextBoxColumn() { Name = "colNumVenta", HeaderText = "No. Venta" },
                new DataGridViewTextBoxColumn() { Name = "colFecha", HeaderText = "Fecha" },
                new DataGridViewTextBoxColumn() { Name = "colCliente", HeaderText = "Cliente" },
                new DataGridViewTextBoxColumn() { Name = "colTotal", HeaderText = "Total" },
                new DataGridViewTextBoxColumn() { Name = "colMetodoPago", HeaderText = "Método Pago" },
                new DataGridViewTextBoxColumn() { Name = "colUsuario", HeaderText = "Usuario" }
            });
            dgvVentasDiarias.SelectionChanged += dgvVentasDiarias_SelectionChanged;

            var panelDetalleVenta = new Panel() { Dock = DockStyle.Fill, BackColor = Color.White };
            var lblDetalle = new Label() { Text = "Productos Vendidos el Día", Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            dgvDetalleVenta = new DataGridView() { Location = new Point(10, 35), Size = new Size(750, 200), AllowUserToAddRows = false, ReadOnly = true, BackgroundColor = Color.White };
            dgvDetalleVenta.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn() { Name = "colCodigo", HeaderText = "Código" },
                new DataGridViewTextBoxColumn() { Name = "colProducto", HeaderText = "Producto" },
                new DataGridViewTextBoxColumn() { Name = "colCantidad", HeaderText = "Cantidad" },
                new DataGridViewTextBoxColumn() { Name = "colPrecio", HeaderText = "Precio Unit." },
                new DataGridViewTextBoxColumn() { Name = "colSubTotal", HeaderText = "SubTotal" }
            });
            panelDetalleVenta.Controls.AddRange(new Control[] { lblDetalle, dgvDetalleVenta });

            tabDiario.Controls.AddRange(new Control[] { panelDetalleVenta, dgvVentasDiarias, panelResumenDiario, panelFiltrosDiario });

            tabControl.TabPages.Add(tabMensual);
            tabControl.TabPages.Add(tabDiario);
            this.Controls.Add(tabControl);
        }
    }
}
