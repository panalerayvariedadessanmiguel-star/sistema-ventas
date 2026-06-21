using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaVentas.Presentation
{
    partial class FormPrincipal
    {
        private System.ComponentModel.IContainer components = null;
        private Panel panelMenu;
        private Panel panelContenido;
        private Button btnVentas;
        private Button btnProductos;
        private Button btnInventario;
        private Button btnStock;
        private Button btnConteoFisico;
        private Button btnCompras;
        private Button btnContabilidad;
        private Button btnReportes;
        private Button btnCerrarCaja;
        private Button btnUsuarios;
        private Button btnConfiguracion;
        private Label lblUsuario;
        private Label lblTitulo;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelMenu = new Panel();
            this.panelContenido = new Panel();
            this.lblTitulo = new Label();
            this.btnVentas = new Button();
            this.btnProductos = new Button();
            this.btnInventario = new Button();
            this.btnStock = new Button();
            this.btnConteoFisico = new Button();
            this.btnCompras = new Button();
            this.btnContabilidad = new Button();
            this.btnReportes = new Button();
            this.btnCerrarCaja = new Button();
            this.btnUsuarios = new Button();
            this.btnConfiguracion = new Button();
            this.lblUsuario = new Label();
            
            this.panelMenu.SuspendLayout();
            this.SuspendLayout();

            // Panel Menu
            this.panelMenu.BackColor = Color.FromArgb(44, 62, 80);
            this.panelMenu.Dock = DockStyle.Left;
            this.panelMenu.Width = 250;

            // Titulo (se agrega primero para que aparezca arriba)
            this.lblTitulo.Text = "SISTEMA DE VENTAS";
            this.lblTitulo.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            this.lblTitulo.ForeColor = Color.White;
            this.lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            this.lblTitulo.Dock = DockStyle.Top;
            this.lblTitulo.Height = 80;

            // Boton Ventas
            this.btnVentas.Text = "  Ventas";
            this.btnVentas.Font = new Font("Segoe UI", 12);
            this.btnVentas.BackColor = Color.FromArgb(52, 73, 94);
            this.btnVentas.ForeColor = Color.White;
            this.btnVentas.FlatStyle = FlatStyle.Flat;
            this.btnVentas.Dock = DockStyle.Top;
            this.btnVentas.Height = 55;
            this.btnVentas.TextAlign = ContentAlignment.MiddleLeft;
            this.btnVentas.Click += new EventHandler(this.btnVentas_Click);

            // Boton Productos
            this.btnProductos.Text = "  Productos";
            this.btnProductos.Font = new Font("Segoe UI", 12);
            this.btnProductos.BackColor = Color.FromArgb(52, 73, 94);
            this.btnProductos.ForeColor = Color.White;
            this.btnProductos.FlatStyle = FlatStyle.Flat;
            this.btnProductos.Dock = DockStyle.Top;
            this.btnProductos.Height = 55;
            this.btnProductos.TextAlign = ContentAlignment.MiddleLeft;
            this.btnProductos.Click += new EventHandler(this.btnProductos_Click);

            // Boton Inventario
            this.btnInventario.Text = "  Inventario";
            this.btnInventario.Font = new Font("Segoe UI", 12);
            this.btnInventario.BackColor = Color.FromArgb(52, 73, 94);
            this.btnInventario.ForeColor = Color.White;
            this.btnInventario.FlatStyle = FlatStyle.Flat;
            this.btnInventario.Dock = DockStyle.Top;
            this.btnInventario.Height = 55;
            this.btnInventario.TextAlign = ContentAlignment.MiddleLeft;
            this.btnInventario.Click += new EventHandler(this.btnInventario_Click);

            // Boton Stock
            this.btnStock.Text = "  Stock";
            this.btnStock.Font = new Font("Segoe UI", 12);
            this.btnStock.BackColor = Color.FromArgb(52, 73, 94);
            this.btnStock.ForeColor = Color.White;
            this.btnStock.FlatStyle = FlatStyle.Flat;
            this.btnStock.Dock = DockStyle.Top;
            this.btnStock.Height = 55;
            this.btnStock.TextAlign = ContentAlignment.MiddleLeft;
            this.btnStock.Click += new EventHandler(this.btnStock_Click);

            // Boton Conteo Fisico
            this.btnConteoFisico.Text = "  Conteo Fisico";
            this.btnConteoFisico.Font = new Font("Segoe UI", 12);
            this.btnConteoFisico.BackColor = Color.FromArgb(52, 73, 94);
            this.btnConteoFisico.ForeColor = Color.White;
            this.btnConteoFisico.FlatStyle = FlatStyle.Flat;
            this.btnConteoFisico.Dock = DockStyle.Top;
            this.btnConteoFisico.Height = 55;
            this.btnConteoFisico.TextAlign = ContentAlignment.MiddleLeft;
            this.btnConteoFisico.Click += new EventHandler(this.btnConteoFisico_Click);

            // Boton Compras
            this.btnCompras.Text = "  Compras";
            this.btnCompras.Font = new Font("Segoe UI", 12);
            this.btnCompras.BackColor = Color.FromArgb(52, 73, 94);
            this.btnCompras.ForeColor = Color.White;
            this.btnCompras.FlatStyle = FlatStyle.Flat;
            this.btnCompras.Dock = DockStyle.Top;
            this.btnCompras.Height = 55;
            this.btnCompras.TextAlign = ContentAlignment.MiddleLeft;
            this.btnCompras.Click += new EventHandler(this.btnCompras_Click);

            // Boton Contabilidad
            this.btnContabilidad.Text = "  Contabilidad";
            this.btnContabilidad.Font = new Font("Segoe UI", 12);
            this.btnContabilidad.BackColor = Color.FromArgb(52, 73, 94);
            this.btnContabilidad.ForeColor = Color.White;
            this.btnContabilidad.FlatStyle = FlatStyle.Flat;
            this.btnContabilidad.Dock = DockStyle.Top;
            this.btnContabilidad.Height = 55;
            this.btnContabilidad.TextAlign = ContentAlignment.MiddleLeft;
            this.btnContabilidad.Click += new EventHandler(this.btnContabilidad_Click);

            // Boton Reportes
            this.btnReportes.Text = "  Reportes";
            this.btnReportes.Font = new Font("Segoe UI", 12);
            this.btnReportes.BackColor = Color.FromArgb(52, 73, 94);
            this.btnReportes.ForeColor = Color.White;
            this.btnReportes.FlatStyle = FlatStyle.Flat;
            this.btnReportes.Dock = DockStyle.Top;
            this.btnReportes.Height = 55;
            this.btnReportes.TextAlign = ContentAlignment.MiddleLeft;
            this.btnReportes.Click += new EventHandler(this.btnReportes_Click);

            // Boton Cerrar Caja
            this.btnCerrarCaja.Text = "  Cerrar Caja";
            this.btnCerrarCaja.Font = new Font("Segoe UI", 12);
            this.btnCerrarCaja.BackColor = Color.FromArgb(192, 57, 43);
            this.btnCerrarCaja.ForeColor = Color.White;
            this.btnCerrarCaja.FlatStyle = FlatStyle.Flat;
            this.btnCerrarCaja.Dock = DockStyle.Top;
            this.btnCerrarCaja.Height = 55;
            this.btnCerrarCaja.TextAlign = ContentAlignment.MiddleLeft;
            this.btnCerrarCaja.Click += new EventHandler(this.btnCerrarCaja_Click);

            // Boton Usuarios
            this.btnUsuarios.Text = "  Usuarios";
            this.btnUsuarios.Font = new Font("Segoe UI", 12);
            this.btnUsuarios.BackColor = Color.FromArgb(52, 73, 94);
            this.btnUsuarios.ForeColor = Color.White;
            this.btnUsuarios.FlatStyle = FlatStyle.Flat;
            this.btnUsuarios.Dock = DockStyle.Top;
            this.btnUsuarios.Height = 55;
            this.btnUsuarios.TextAlign = ContentAlignment.MiddleLeft;
            this.btnUsuarios.Click += new EventHandler(this.btnUsuarios_Click);

            // Boton Configuracion
            this.btnConfiguracion.Text = "  Configuracion";
            this.btnConfiguracion.Font = new Font("Segoe UI", 12);
            this.btnConfiguracion.BackColor = Color.FromArgb(52, 73, 94);
            this.btnConfiguracion.ForeColor = Color.White;
            this.btnConfiguracion.FlatStyle = FlatStyle.Flat;
            this.btnConfiguracion.Dock = DockStyle.Bottom;
            this.btnConfiguracion.Height = 55;
            this.btnConfiguracion.TextAlign = ContentAlignment.MiddleLeft;
            this.btnConfiguracion.Click += new EventHandler(this.btnConfiguracion_Click);

            // Label Usuario
            this.lblUsuario.Text = "Usuario: Admin";
            this.lblUsuario.Font = new Font("Segoe UI", 10);
            this.lblUsuario.ForeColor = Color.FromArgb(189, 195, 199);
            this.lblUsuario.TextAlign = ContentAlignment.MiddleCenter;
            this.lblUsuario.Dock = DockStyle.Bottom;
            this.lblUsuario.Height = 30;

            // Agregar controles al panel (el orden de Add determina la posicion con DockStyle.Top)
            this.panelMenu.Controls.Add(this.lblUsuario);
            this.panelMenu.Controls.Add(this.btnConfiguracion);
            this.panelMenu.Controls.Add(this.btnUsuarios);
            this.panelMenu.Controls.Add(this.btnCerrarCaja);
            this.panelMenu.Controls.Add(this.btnReportes);
            this.panelMenu.Controls.Add(this.btnContabilidad);
            this.panelMenu.Controls.Add(this.btnCompras);
            this.panelMenu.Controls.Add(this.btnConteoFisico);
            this.panelMenu.Controls.Add(this.btnStock);
            this.panelMenu.Controls.Add(this.btnInventario);
            this.panelMenu.Controls.Add(this.btnProductos);
            this.panelMenu.Controls.Add(this.btnVentas);
            this.panelMenu.Controls.Add(this.lblTitulo);

            // Panel Contenido
            this.panelContenido.BackColor = Color.FromArgb(236, 240, 241);
            this.panelContenido.Dock = DockStyle.Fill;
            this.panelContenido.Padding = new Padding(20);

            var lblBienvenida = new Label();
            lblBienvenida.Text = "Bienvenido al Sistema de Ventas\n\nSeleccione una opcion del menu lateral";
            lblBienvenida.Font = new Font("Segoe UI", 20, FontStyle.Regular);
            lblBienvenida.ForeColor = Color.FromArgb(52, 73, 94);
            lblBienvenida.TextAlign = ContentAlignment.MiddleCenter;
            lblBienvenida.Dock = DockStyle.Fill;
            this.panelContenido.Controls.Add(lblBienvenida);

            // Form
            this.Controls.Add(this.panelContenido);
            this.Controls.Add(this.panelMenu);
            this.Name = "FormPrincipal";

            this.panelMenu.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
