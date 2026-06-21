using System;
using System.Drawing;
using System.Windows.Forms;
using SistemaVentas.Business.Services;

namespace SistemaVentas.Presentation
{
    public partial class FormPrincipal : Form
    {
        private string _usuarioActual;
        private int _cajaIdActual;
        private readonly CajaService _cajaService;
        private readonly POSSyncService _posSync;
        private readonly ProductSyncService _productSync;

        public FormPrincipal()
        {
            InitializeComponent();
            _cajaService = new CajaService();
            _posSync = new POSSyncService();
            _posSync.Start();
            _productSync = new ProductSyncService();
            _productSync.Start();
            this.FormClosed += (s, e) =>
            {
                _posSync.Stop();
                _productSync.Stop();
            };
            InicializarFormulario();
        }

        private void InicializarFormulario()
        {
            Text = "Sistema de Ventas - Menu Principal";
            Size = new Size(1200, 700);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
        }

        public void SetUsuario(string usuario, int cajaId)
        {
            _usuarioActual = usuario;
            _cajaIdActual = cajaId;
        }

        private void btnVentas_Click(object sender, EventArgs e)
        {
            var cajaAbierta = _cajaService.GetCajaAbierta();
            if (cajaAbierta == null)
            {
                MessageBox.Show("No hay una caja abierta. Debe abrir una caja para realizar ventas.", "Caja Cerrada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                AbrirCaja();
                return;
            }

            // Actualizar por si hubo cambios
            _cajaIdActual = cajaAbierta.Id;
            _usuarioActual = cajaAbierta.Usuario;

            var frm = new FrmVenta(_cajaIdActual, _usuarioActual);
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        private void AbrirCaja()
        {
            var frmApertura = new FrmAperturaCaja();
            if (frmApertura.ShowDialog() == DialogResult.OK)
            {
                _usuarioActual = frmApertura.Usuario;
                _cajaIdActual = frmApertura.CajaId;
                MessageBox.Show($"Caja abierta exitosamente para {_usuarioActual}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnProductos_Click(object sender, EventArgs e)
        {
            var frm = new FrmProductos();
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        private void btnInventario_Click(object sender, EventArgs e)
        {
            var usuario = _usuarioActual ?? Environment.UserName;
            var frm = new FrmInventario(usuario);
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        private void btnReportes_Click(object sender, EventArgs e)
        {
            var frm = new FrmReportes();
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        private void btnCerrarCaja_Click(object sender, EventArgs e)
        {
            var cajaAbierta = _cajaService.GetCajaAbierta();
            if (cajaAbierta == null)
            {
                MessageBox.Show("No hay una caja abierta actualmente.", "Caja Cerrada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var frm = new FrmCierreCaja(cajaAbierta.Id, cajaAbierta.Usuario);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                _cajaIdActual = 0;
                _usuarioActual = null;
                MessageBox.Show("Caja cerrada exitosamente. Para realizar nuevas ventas debe abrir una nueva caja.", "Caja Cerrada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnUsuarios_Click(object sender, EventArgs e)
        {
            var frm = new FrmUsuarios();
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        private void btnConfiguracion_Click(object sender, EventArgs e)
        {
            var frm = new FrmConfiguracion();
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        private void btnStock_Click(object sender, EventArgs e)
        {
            var frm = new FrmStock();
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        private void btnConteoFisico_Click(object sender, EventArgs e)
        {
            var frm = new FrmConteoFisico();
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        private void btnCompras_Click(object sender, EventArgs e)
        {
            var usuario = _usuarioActual ?? Environment.UserName;
            var frm = new FrmCompras(usuario);
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        private void btnContabilidad_Click(object sender, EventArgs e)
        {
            var frm = new FrmContabilidad();
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }
    }
}
