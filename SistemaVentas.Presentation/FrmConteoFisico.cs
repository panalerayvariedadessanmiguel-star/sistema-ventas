using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Text;
using Dapper;
using SistemaVentas.Business.Services;
using SistemaVentas.Data;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Presentation
{
    public partial class FrmConteoFisico : Form
    {
        private readonly ConteoFisicoService _service;
        private int _conteoIdActual = 0;
        private int _conteo2IdActual = 0;

        private ComboBox cmbConteo1Orig;
        private DataGridView dgvConteo2;
        private Button btnCargarDiferencias;
        private Button btnGuardarConteo2;

        public FrmConteoFisico()
        {
            _service = new ConteoFisicoService();
            InitializeComponent();
            CargarConteos();
        }

        private void CargarConteos()
        {
            dgvConteos.Rows.Clear();
            var conteos = _service.GetAll();
            foreach (var c in conteos)
            {
                string desc = c.TipoConteo == 2 ? $"{c.ConteoOriginalDesc} - Reconteo" : "Original";
                dgvConteos.Rows.Add(c.Id, c.Fecha.ToShortDateString(), c.Usuario, c.Estado, desc, c.Observaciones);
            }
        }

        private void btnNuevoConteo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsuario.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Ingrese usuario y contraseña de administrador", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string adminUser = LeerConfig("ADMIN_USER");
            string adminPass = LeerConfig("ADMIN_PASS");

            if (txtUsuario.Text != adminUser || txtPassword.Text != adminPass)
            {
                MessageBox.Show("Usuario o contraseña incorrectos. Solo administradores pueden realizar conteos.", "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Clear();
                txtUsuario.Focus();
                return;
            }

            _conteoIdActual = _service.CrearConteo(txtUsuario.Text, txtObsConteo.Text);
            MessageBox.Show("Conteo creado con ID: " + _conteoIdActual + ". Ahora registre los productos.", "Conteo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            tabControl1.SelectedIndex = 1;
            CargarProductosConteo();
        }

        private string LeerConfig(string clave)
        {
            using var connection = DbConnection.GetConnection();
            return connection.QueryFirstOrDefault<string>(
                "SELECT Valor FROM Configuracion WHERE Clave = @Clave",
                new { Clave = clave });
        }

        private void CargarProductosConteo()
        {
            dgvProductosConteo.Rows.Clear();
            var productos = _service.GetProductosParaConteo();
            foreach (var p in productos)
            {
                dgvProductosConteo.Rows.Add(p.Id, p.CodigoBarras, p.Nombre, p.Stock, 0, 0);
            }
        }

        private void dgvProductosConteo_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                var row = dgvProductosConteo.Rows[e.RowIndex];
                int stockSistema = Convert.ToInt32(row.Cells[3].Value);
                int stockFisico = Convert.ToInt32(row.Cells[4].Value ?? 0);
                row.Cells[5].Value = stockFisico - stockSistema;
            }
        }

        private void btnGuardarConteo_Click(object sender, EventArgs e)
        {
            if (_conteoIdActual == 0)
            {
                MessageBox.Show("Debe crear un nuevo conteo primero", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                foreach (DataGridViewRow row in dgvProductosConteo.Rows)
                {
                    if (row.Cells[0].Value == null) continue;
                    int productoId = Convert.ToInt32(row.Cells[0].Value);
                    int stockFisico = Convert.ToInt32(row.Cells[4].Value ?? 0);
                    _service.RegistrarConteoProducto(_conteoIdActual, productoId, stockFisico);
                }

                _service.FinalizarConteo(_conteoIdActual);

                var conteo = _service.GetById(_conteoIdActual);
                var detalles = _service.GetDetalles(_conteoIdActual);
                GuardarReporteConteo(conteo, detalles);

                MessageBox.Show("Conteo guardado exitosamente", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _conteoIdActual = 0;
                CargarConteos();
                tabControl1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GuardarReporteConteo(ConteoFisico conteo, List<DetalleConteoFisico> detalles)
        {
            try
            {
                string carpeta = @"C:\Users\Familia_Jica\Desktop\Pañalera y Variedades San Miguel\Reportes\Conteos Fisicos";
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                string fechaStr = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
                string tipo = conteo.TipoConteo == 2 ? "Reconteo" : "ConteoFisico";
                string nombreArchivo = $"{tipo}_{conteo.Id}_{fechaStr}.txt";
                string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                var sb = new StringBuilder();
                sb.AppendLine("==================================================");
                sb.AppendLine(conteo.TipoConteo == 2
                    ? "       REPORTE DE RECONTEO FISICO (CONTE0 2)"
                    : "       REPORTE DETALLADO DE CONTE0 FISICO");
                sb.AppendLine("==================================================");
                sb.AppendLine();
                sb.AppendLine($"ID Conteo: {conteo.Id}");
                sb.AppendLine($"Tipo: {(conteo.TipoConteo == 2 ? "Reconteo (Conteo 2)" : "Original (Conteo 1)")}");
                if (conteo.ConteoOriginalId.HasValue)
                    sb.AppendLine($"Conteo Original ID: {conteo.ConteoOriginalId.Value}");
                sb.AppendLine($"Fecha: {conteo.Fecha:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine($"Usuario: {conteo.Usuario}");
                sb.AppendLine($"Estado: {conteo.Estado}");
                sb.AppendLine($"Observaciones: {conteo.Observaciones}");
                sb.AppendLine();
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine("DETALLE DE PRODUCTOS");
                sb.AppendLine("--------------------------------------------------");

                if (conteo.TipoConteo == 2)
                {
                    sb.AppendLine("Codigo      | Producto                        | Sistema | FísicoC1 | DifC1 | FísicoC2 | DifC2 | Valor Fal  | Valor Sob");
                    sb.AppendLine("------------|---------------------------------|---------|----------|-------|----------|-------|------------|-----------");
                }
                else
                {
                    sb.AppendLine("Codigo      | Producto                        | Sistema | Fisico | Dif   | Valor Fal  | Valor Sob");
                    sb.AppendLine("------------|---------------------------------|---------|--------|-------|------------|-----------");
                }

                if (conteo.TipoConteo == 2)
                {
                    foreach (var d in detalles)
                    {
                        string codigo = (d.CodigoBarras ?? "").PadRight(12).Substring(0, 12);
                        string nombre = (d.NombreProducto ?? "").PadRight(33).Substring(0, 33);
                        string fisC1 = d.StockFisicoOriginal.HasValue ? d.StockFisicoOriginal.Value.ToString().PadLeft(8) : "   N/A  ";
                        string difC1 = d.DiferenciaOriginal.HasValue ? d.DiferenciaOriginal.Value.ToString().PadLeft(5) : "  N/A ";
                        sb.AppendLine($"{codigo} | {nombre} | {d.StockSistema,7} | {fisC1} | {difC1} | {d.StockFisico,8} | {d.Diferencia,5} | {d.ValorFaltante,10:C2} | {d.ValorSobrante,9:C2}");
                    }
                }
                else
                {
                    foreach (var d in detalles)
                    {
                        string codigo = (d.CodigoBarras ?? "").PadRight(12).Substring(0, 12);
                        string nombre = (d.NombreProducto ?? "").PadRight(33).Substring(0, 33);
                        sb.AppendLine($"{codigo} | {nombre} | {d.StockSistema,7} | {d.StockFisico,6} | {d.Diferencia,5} | {d.ValorFaltante,10:C2} | {d.ValorSobrante,9:C2}");
                    }
                }

                sb.AppendLine();
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine($"Total productos contados: {detalles.Count}");
                int totalFaltante = detalles.Count(d => d.Diferencia < 0);
                int totalSobrante = detalles.Count(d => d.Diferencia > 0);
                sb.AppendLine($"Productos con faltante: {totalFaltante}");
                sb.AppendLine($"Productos con sobrante: {totalSobrante}");
                sb.AppendLine();
                sb.AppendLine("RESUMEN MONETARIO");
                sb.AppendLine($"Valor total faltante: {conteo.ValorFaltante:C2}");
                sb.AppendLine($"Valor total sobrante: {conteo.ValorSobrante:C2}");
                sb.AppendLine("==================================================");

                File.WriteAllText(rutaCompleta, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar reporte: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgvConteos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int conteoId = Convert.ToInt32(dgvConteos.Rows[e.RowIndex].Cells[0].Value);
                CargarDetalleConteo(conteoId);
                tabControl1.SelectedIndex = 2;
            }
        }

        private void CargarDetalleConteo(int conteoId)
        {
            dgvDetalleConteo.Rows.Clear();
            var conteo = _service.GetById(conteoId);
            if (conteo == null) return;

            if (conteo.TipoConteo == 2 && conteo.ConteoOriginalId.HasValue)
            {
                var detallesOrig = _service.GetDetalles(conteo.ConteoOriginalId.Value);
                var origPorProducto = new System.Collections.Generic.Dictionary<int, DetalleConteoFisico>();
                foreach (var d in detallesOrig)
                    origPorProducto[d.ProductoId] = d;

                foreach (var d in conteo.Detalles)
                {
                    string codigo = d.CodigoBarras ?? "";
                    string nombre = d.NombreProducto ?? "";
                    int? fisOrig = null;
                    int? difOrig = null;
                    if (origPorProducto.TryGetValue(d.ProductoId, out var orig))
                    {
                        fisOrig = orig.StockFisico;
                        difOrig = orig.Diferencia;
                    }
                    dgvDetalleConteo.Rows.Add(d.Id, codigo, nombre,
                        d.StockSistema,
                        fisOrig.HasValue ? fisOrig.ToString() : "N/A",
                        difOrig.HasValue ? difOrig.ToString() : "N/A",
                        d.StockFisico,
                        d.Diferencia);
                }
            }
            else
            {
                foreach (var d in conteo.Detalles)
                {
                    dgvDetalleConteo.Rows.Add(d.Id, d.CodigoBarras, d.NombreProducto,
                        d.StockSistema, d.StockFisico, d.Diferencia,
                        d.StockFisico, d.Diferencia);
                }
            }
        }

        private void CargarConteosFinalizados()
        {
            cmbConteo1Orig.Items.Clear();
            cmbConteo1Orig.DisplayMember = "DisplayText";
            cmbConteo1Orig.ValueMember = "Id";

            var conteos = _service.GetConteosFinalizadosTipo1();
            foreach (var c in conteos)
            {
                cmbConteo1Orig.Items.Add(new
                {
                    Id = c.Id,
                    DisplayText = $"C{c.Id} - {c.Fecha:dd/MM/yyyy} - {c.Usuario}"
                });
            }
        }

        private void btnCargarDiferencias_Click(object sender, EventArgs e)
        {
            if (cmbConteo1Orig.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un Conteo 1 finalizado.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dynamic selected = cmbConteo1Orig.SelectedItem;
            int conteoId = selected.Id;

            var diferencias = _service.GetDetallesConDiferencia(conteoId);
            if (diferencias.Count == 0)
            {
                MessageBox.Show("No hay productos con diferencias en este conteo.", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            dgvConteo2.Rows.Clear();
            foreach (var d in diferencias)
            {
                dgvConteo2.Rows.Add(d.ProductoId, d.CodigoBarras, d.NombreProducto,
                    d.StockSistema, d.StockFisico, d.Diferencia, 0, 0);
            }
        }

        private void dgvConteo2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 6)
            {
                var row = dgvConteo2.Rows[e.RowIndex];
                if (row.Cells[3].Value == null) return;
                int stockSistema = Convert.ToInt32(row.Cells[3].Value);
                int nuevoFisico = Convert.ToInt32(row.Cells[6].Value ?? 0);
                row.Cells[7].Value = nuevoFisico - stockSistema;
            }
        }

        private void btnGuardarConteo2_Click(object sender, EventArgs e)
        {
            if (cmbConteo1Orig.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un Conteo 1 finalizado.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtUsuario.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Ingrese usuario y contraseña de administrador", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string adminUser = LeerConfig("ADMIN_USER");
            string adminPass = LeerConfig("ADMIN_PASS");

            if (txtUsuario.Text != adminUser || txtPassword.Text != adminPass)
            {
                MessageBox.Show("Usuario o contraseña incorrectos.", "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Clear();
                txtUsuario.Focus();
                return;
            }

            try
            {
                dynamic selected = cmbConteo1Orig.SelectedItem;
                int conteoOriginalId = selected.Id;

                _conteo2IdActual = _service.CrearConteo(txtUsuario.Text,
                    "Reconteo desde Conteo " + conteoOriginalId + ": " + txtObsConteo.Text,
                    tipoConteo: 2, conteoOriginalId: conteoOriginalId);

                foreach (DataGridViewRow row in dgvConteo2.Rows)
                {
                    if (row.Cells[0].Value == null) continue;
                    int productoId = Convert.ToInt32(row.Cells[0].Value);
                    int stockFisico = Convert.ToInt32(row.Cells[6].Value ?? 0);
                    _service.RegistrarConteoProducto(_conteo2IdActual, productoId, stockFisico);
                }

                _service.FinalizarConteo(_conteo2IdActual);

                var conteo2 = _service.GetById(_conteo2IdActual);
                var origDetalles = _service.GetDetalles(conteoOriginalId);
                var origPorProducto = new System.Collections.Generic.Dictionary<int, DetalleConteoFisico>();
                foreach (var d in origDetalles)
                    origPorProducto[d.ProductoId] = d;

                foreach (var d in conteo2.Detalles)
                {
                    if (origPorProducto.TryGetValue(d.ProductoId, out var orig))
                    {
                        d.StockFisicoOriginal = orig.StockFisico;
                        d.DiferenciaOriginal = orig.Diferencia;
                    }
                }

                GuardarReporteConteo(conteo2, conteo2.Detalles);

                MessageBox.Show("Reconteo (Conteo 2) guardado exitosamente", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _conteo2IdActual = 0;
                CargarConteos();
                CargarConteosFinalizados();
                tabControl1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    partial class FrmConteoFisico
    {
        private TabControl tabControl1;
        private DataGridView dgvConteos;
        private DataGridView dgvProductosConteo;
        private DataGridView dgvDetalleConteo;
        private TextBox txtUsuario;
        private TextBox txtPassword;
        private TextBox txtObsConteo;
        private Button btnNuevoConteo;
        private Button btnGuardarConteo;

        private void InitializeComponent()
        {
            this.Text = "Conteo Fisico vs Sistema";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;

            tabControl1 = new TabControl();
            tabControl1.Dock = DockStyle.Fill;

            // Tab 1: Lista de Conteos
            var tab1 = new TabPage("Conteos");

            var panelSuperior = new Panel();
            panelSuperior.Dock = DockStyle.Top;
            panelSuperior.Height = 100;
            panelSuperior.BackColor = Color.FromArgb(236, 240, 241);

            var lblUsuario = new Label() { Text = "Usuario:", Location = new Point(15, 15), Size = new Size(60, 20) };
            txtUsuario = new TextBox() { Location = new Point(80, 15), Size = new Size(150, 20) };

            var lblPassword = new Label() { Text = "Contraseña:", Location = new Point(250, 15), Size = new Size(80, 20) };
            txtPassword = new TextBox() { Location = new Point(340, 15), Size = new Size(150, 20), PasswordChar = '*' };

            var lblObs = new Label() { Text = "Observaciones:", Location = new Point(510, 15), Size = new Size(100, 20) };
            txtObsConteo = new TextBox() { Location = new Point(620, 15), Size = new Size(250, 20) };

            btnNuevoConteo = new Button() { Text = "Nuevo Conteo", Location = new Point(900, 13), Size = new Size(120, 25), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White };
            btnNuevoConteo.Click += new EventHandler(this.btnNuevoConteo_Click);

            panelSuperior.Controls.Add(lblUsuario);
            panelSuperior.Controls.Add(txtUsuario);
            panelSuperior.Controls.Add(lblPassword);
            panelSuperior.Controls.Add(txtPassword);
            panelSuperior.Controls.Add(lblObs);
            panelSuperior.Controls.Add(txtObsConteo);
            panelSuperior.Controls.Add(btnNuevoConteo);

            dgvConteos = new DataGridView();
            dgvConteos.Dock = DockStyle.Fill;
            dgvConteos.AllowUserToAddRows = false;
            dgvConteos.ReadOnly = true;
            dgvConteos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvConteos.BackgroundColor = Color.White;
            dgvConteos.Columns.Add("colIdConteo", "ID");
            dgvConteos.Columns.Add("colFecha", "Fecha");
            dgvConteos.Columns.Add("colUsuarioConteo", "Usuario");
            dgvConteos.Columns.Add("colEstado", "Estado");
            dgvConteos.Columns.Add("colTipo", "Tipo");
            dgvConteos.Columns.Add("colObs", "Observaciones");
            dgvConteos.CellClick += new DataGridViewCellEventHandler(this.dgvConteos_CellClick);

            tab1.Controls.Add(dgvConteos);
            tab1.Controls.Add(panelSuperior);

            // Tab 2: Registro de Conteo
            var tab2 = new TabPage("Registro de Conteo");

            dgvProductosConteo = new DataGridView();
            dgvProductosConteo.Dock = DockStyle.Fill;
            dgvProductosConteo.AllowUserToAddRows = false;
            dgvProductosConteo.BackgroundColor = Color.White;
            dgvProductosConteo.Columns.Add("colIdProd", "ID");
            dgvProductosConteo.Columns.Add("colCodProd", "Codigo");
            dgvProductosConteo.Columns.Add("colNomProd", "Producto");
            dgvProductosConteo.Columns.Add("colSis", "Stock Sistema");
            dgvProductosConteo.Columns.Add("colFis", "Stock Fisico");
            dgvProductosConteo.Columns.Add("colDif", "Diferencia");
            dgvProductosConteo.Columns["colIdProd"].Visible = false;
            dgvProductosConteo.CellEndEdit += new DataGridViewCellEventHandler(this.dgvProductosConteo_CellEndEdit);

            btnGuardarConteo = new Button() { Text = "Guardar Conteo", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
            btnGuardarConteo.Click += new EventHandler(this.btnGuardarConteo_Click);

            tab2.Controls.Add(dgvProductosConteo);
            tab2.Controls.Add(btnGuardarConteo);

            // Tab 3: Ver Diferencias
            var tab3 = new TabPage("Ver Diferencias");

            dgvDetalleConteo = new DataGridView();
            dgvDetalleConteo.Dock = DockStyle.Fill;
            dgvDetalleConteo.AllowUserToAddRows = false;
            dgvDetalleConteo.ReadOnly = true;
            dgvDetalleConteo.BackgroundColor = Color.White;
            dgvDetalleConteo.Columns.Add("colIdDet", "ID");
            dgvDetalleConteo.Columns.Add("colCodDet", "Codigo");
            dgvDetalleConteo.Columns.Add("colNomDet", "Producto");
            dgvDetalleConteo.Columns.Add("colSisDet", "Stock Sistema");
            dgvDetalleConteo.Columns.Add("colFisC1Det", "Fisico C1");
            dgvDetalleConteo.Columns.Add("colDifC1Det", "Dif C1");
            dgvDetalleConteo.Columns.Add("colFisC2Det", "Fisico C2");
            dgvDetalleConteo.Columns.Add("colDifC2Det", "Dif C2");

            tab3.Controls.Add(dgvDetalleConteo);

            // Tab 4: Conteo 2 (Reconteo)
            var tab4 = new TabPage("Conteo 2 (Reconteo)");

            var panelC2Top = new Panel();
            panelC2Top.Dock = DockStyle.Top;
            panelC2Top.Height = 50;
            panelC2Top.BackColor = Color.FromArgb(236, 240, 241);

            var lblSelConteo = new Label() { Text = "Conteo 1 Finalizado:", Location = new Point(15, 15), Size = new Size(120, 20) };

            cmbConteo1Orig = new ComboBox() { Location = new Point(140, 13), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            btnCargarDiferencias = new Button() { Text = "Cargar Diferencias", Location = new Point(460, 12), Size = new Size(130, 25), BackColor = Color.FromArgb(243, 156, 18), ForeColor = Color.White };
            btnCargarDiferencias.Click += new EventHandler(this.btnCargarDiferencias_Click);

            panelC2Top.Controls.Add(lblSelConteo);
            panelC2Top.Controls.Add(cmbConteo1Orig);
            panelC2Top.Controls.Add(btnCargarDiferencias);

            dgvConteo2 = new DataGridView();
            dgvConteo2.Dock = DockStyle.Fill;
            dgvConteo2.AllowUserToAddRows = false;
            dgvConteo2.BackgroundColor = Color.White;
            dgvConteo2.Columns.Add("colIdProdC2", "ID");
            dgvConteo2.Columns.Add("colCodC2", "Codigo");
            dgvConteo2.Columns.Add("colNomC2", "Producto");
            dgvConteo2.Columns.Add("colSisC2", "Stock Sistema");
            dgvConteo2.Columns.Add("colFisC1", "Stock Fisico (C1)");
            dgvConteo2.Columns.Add("colDifC1", "Diferencia (C1)");
            dgvConteo2.Columns.Add("colFisC2", "Nuevo Stock Fisico");
            dgvConteo2.Columns.Add("colDifC2", "Nueva Diferencia");
            dgvConteo2.Columns["colIdProdC2"].Visible = false;
            dgvConteo2.Columns["colFisC1"].ReadOnly = true;
            dgvConteo2.Columns["colDifC1"].ReadOnly = true;
            dgvConteo2.Columns["colSisC2"].ReadOnly = true;
            dgvConteo2.Columns["colDifC2"].ReadOnly = true;
            dgvConteo2.CellEndEdit += new DataGridViewCellEventHandler(this.dgvConteo2_CellEndEdit);

            btnGuardarConteo2 = new Button() { Text = "Guardar Conteo 2", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White };
            btnGuardarConteo2.Click += new EventHandler(this.btnGuardarConteo2_Click);

            tab4.Controls.Add(dgvConteo2);
            tab4.Controls.Add(panelC2Top);
            tab4.Controls.Add(btnGuardarConteo2);

            tabControl1.TabPages.Add(tab1);
            tabControl1.TabPages.Add(tab2);
            tabControl1.TabPages.Add(tab3);
            tabControl1.TabPages.Add(tab4);

            this.Controls.Add(tabControl1);

            CargarConteosFinalizados();
        }
    }
}
