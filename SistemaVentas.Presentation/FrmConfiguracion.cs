using System;
using System.Drawing;
using System.Windows.Forms;
using SistemaVentas.Data;

namespace SistemaVentas.Presentation
{
    public partial class FrmConfiguracion : Form
    {
        public FrmConfiguracion()
        {
            InitializeComponent();
            CargarConfiguracion();
        }

        private void CargarConfiguracion()
        {
            var connStr = DbConnection.GetCurrentConnectionString();
            txtServidor.Text = ObtenerValorConexion(connStr, "Server");
            txtBaseDatos.Text = ObtenerValorConexion(connStr, "Database");
            
            if (connStr.Contains("Integrated Security", StringComparison.OrdinalIgnoreCase))
            {
                rbWindowsAuth.Checked = true;
            }
            else
            {
                rbSqlAuth.Checked = true;
                txtUsuario.Text = ObtenerValorConexion(connStr, "User ID");
                txtPassword.Text = ObtenerValorConexion(connStr, "Password");
            }
        }

        private string ObtenerValorConexion(string connStr, string clave)
        {
            var partes = connStr.Split(';');
            foreach (var parte in partes)
            {
                if (parte.Trim().StartsWith(clave + "=", StringComparison.OrdinalIgnoreCase))
                {
                    return parte.Substring(clave.Length + 1).Trim();
                }
            }
            return "";
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtServidor.Text) || string.IsNullOrWhiteSpace(txtBaseDatos.Text))
            {
                MessageBox.Show("Servidor y Base de Datos son obligatorios.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var connStr = $"Server={txtServidor.Text};Database={txtBaseDatos.Text};TrustServerCertificate=true;";

            if (rbWindowsAuth.Checked)
            {
                connStr += "Integrated Security=true;";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtUsuario.Text))
                {
                    MessageBox.Show("El usuario es obligatorio para autenticación SQL.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                connStr += $"User ID={txtUsuario.Text};Password={txtPassword.Text};";
            }

            try
            {
                using var testConn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                testConn.Open();
                testConn.Close();
                
                DbConnection.UpdateConnectionString(connStr);
                MessageBox.Show("Conexión exitosa. Configuración guardada.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void rbWindowsAuth_CheckedChanged(object sender, EventArgs e)
        {
            txtUsuario.Enabled = rbSqlAuth.Checked;
            txtPassword.Enabled = rbSqlAuth.Checked;
        }
    }

    partial class FrmConfiguracion
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtServidor;
        private TextBox txtBaseDatos;
        private TextBox txtUsuario;
        private TextBox txtPassword;
        private RadioButton rbWindowsAuth;
        private RadioButton rbSqlAuth;
        private Button btnAceptar;
        private Button btnCancelar;
        private Label lblServidor;
        private Label lblBaseDatos;
        private Label lblUsuario;
        private Label lblPassword;
        private Label lblAuth;
        private GroupBox grpAutenticacion;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Text = "Configuración de Base de Datos";
            this.Size = new Size(450, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblServidor = new Label() { Text = "Servidor:", Location = new Point(30, 25), Size = new Size(100, 20) };
            txtServidor = new TextBox() { Location = new Point(140, 23), Size = new Size(250, 20) };

            lblBaseDatos = new Label() { Text = "Base de Datos:", Location = new Point(30, 55), Size = new Size(100, 20) };
            txtBaseDatos = new TextBox() { Location = new Point(140, 53), Size = new Size(250, 20) };

            lblAuth = new Label() { Text = "Autenticación:", Location = new Point(30, 90), Size = new Size(100, 20) };
            
            grpAutenticacion = new GroupBox() { Location = new Point(140, 85), Size = new Size(250, 60), Text = "" };
            rbWindowsAuth = new RadioButton() { Text = "Windows", Location = new Point(10, 15), Size = new Size(100, 20), Checked = true };
            rbSqlAuth = new RadioButton() { Text = "SQL Server", Location = new Point(120, 15), Size = new Size(100, 20) };
            rbWindowsAuth.CheckedChanged += rbWindowsAuth_CheckedChanged;
            rbSqlAuth.CheckedChanged += rbWindowsAuth_CheckedChanged;
            grpAutenticacion.Controls.Add(rbWindowsAuth);
            grpAutenticacion.Controls.Add(rbSqlAuth);

            lblUsuario = new Label() { Text = "Usuario:", Location = new Point(30, 160), Size = new Size(100, 20) };
            txtUsuario = new TextBox() { Location = new Point(140, 158), Size = new Size(250, 20), Enabled = false };

            lblPassword = new Label() { Text = "Contraseña:", Location = new Point(30, 190), Size = new Size(100, 20) };
            txtPassword = new TextBox() { Location = new Point(140, 188), Size = new Size(250, 20), Enabled = false, PasswordChar = '*' };

            btnAceptar = new Button() { Text = "Aceptar", Location = new Point(140, 260), Size = new Size(100, 35), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White };
            btnAceptar.Click += btnAceptar_Click;

            btnCancelar = new Button() { Text = "Cancelar", Location = new Point(250, 260), Size = new Size(100, 35), BackColor = Color.FromArgb(149, 165, 166), ForeColor = Color.White };
            btnCancelar.Click += btnCancelar_Click;

            this.Controls.Add(lblServidor);
            this.Controls.Add(txtServidor);
            this.Controls.Add(lblBaseDatos);
            this.Controls.Add(txtBaseDatos);
            this.Controls.Add(lblAuth);
            this.Controls.Add(grpAutenticacion);
            this.Controls.Add(lblUsuario);
            this.Controls.Add(txtUsuario);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnAceptar);
            this.Controls.Add(btnCancelar);
        }
    }
}
