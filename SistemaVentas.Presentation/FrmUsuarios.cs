using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using SistemaVentas.Business.Services;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Presentation
{
    public partial class FrmUsuarios : Form
    {
        private readonly UsuarioService _usuarioService;
        private DataGridView dgvUsuarios;
        private TextBox txtNombres;
        private TextBox txtApellidos;
        private TextBox txtDocumento;
        private ComboBox cmbTipoDocumento;
        private TextBox txtContraseña;
        private ComboBox cmbRol;
        private TextBox txtSalario;
        private Button btnNuevo;
        private Button btnGuardar;
        private Button btnEliminar;
        private Button btnCancelar;
        private int idSeleccionado = 0;
        private bool _isFormattingSalario = false;

        public FrmUsuarios()
        {
            _usuarioService = new UsuarioService();
            InitializeComponent();
            CargarComboTipos();
            CargarGrid();
        }

        private void txtSalario_TextChanged(object sender, EventArgs e)
        {
            if (_isFormattingSalario) return;
            _isFormattingSalario = true;

            var text = txtSalario.Text;
            var cursorPos = txtSalario.SelectionStart;
            var beforeCursor = text.Substring(0, Math.Min(cursorPos, text.Length));
            var digitCountBeforeCursor = beforeCursor.Count(char.IsDigit);

            var digits = new string(text.Where(char.IsDigit).ToArray());

            if (digits.Length > 0 && decimal.TryParse(digits, out var value))
            {
                txtSalario.Text = value.ToString("N0", CultureInfo.GetCultureInfo("es-CO"));

                var newPos = 0;
                var digitsSeen = 0;
                for (var i = 0; i < txtSalario.Text.Length; i++)
                {
                    if (char.IsDigit(txtSalario.Text[i]))
                    {
                        digitsSeen++;
                        if (digitsSeen > digitCountBeforeCursor) break;
                    }
                    newPos++;
                }
                txtSalario.SelectionStart = Math.Min(newPos, txtSalario.Text.Length);
                txtSalario.SelectionLength = 0;
            }

            _isFormattingSalario = false;
        }

        private void CargarComboTipos()
        {
            cmbTipoDocumento.Items.AddRange(new[] { "CC", "NIT", "CE", "PP", "TI", "RC" });
            cmbTipoDocumento.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRol.Items.AddRange(new[] { "Administrador", "Cajero" });
            cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRol.SelectedIndex = 1;
        }

        private void CargarGrid()
        {
            var usuarios = _usuarioService.GetAll();
            dgvUsuarios.DataSource = usuarios.Select(u => new
            {
                u.Id,
                u.Nombres,
                u.Apellidos,
                u.Documento,
                u.TipoDocumento,
                u.Rol,
                Salario = u.Salario.HasValue ? u.Salario.Value.ToString("N0") : "",
                u.Activo,
                u.FechaCreacion
            }).ToList();
        }

        private void Limpiar()
        {
            idSeleccionado = 0;
            txtNombres.Text = "";
            txtApellidos.Text = "";
            txtDocumento.Text = "";
            cmbTipoDocumento.SelectedIndex = -1;
            txtContraseña.Text = "";
            cmbRol.SelectedIndex = 1;
            txtSalario.Text = "";
        }

        private void btnNuevo_Click(object sender, EventArgs e) => Limpiar();

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombres.Text) || string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                MessageBox.Show("Nombres y documento son obligatorios.");
                return;
            }

            var usuario = new Usuario
            {
                Id = idSeleccionado,
                Nombres = txtNombres.Text.Trim(),
                Apellidos = txtApellidos.Text.Trim(),
                Documento = txtDocumento.Text.Trim(),
                TipoDocumento = cmbTipoDocumento.Text,
                Contraseña = string.IsNullOrEmpty(txtContraseña.Text) ? "12345" : txtContraseña.Text,
                Rol = cmbRol.Text,
                Salario = decimal.TryParse(txtSalario.Text.Replace(".", ""), out var sal) ? sal : null,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            if (idSeleccionado == 0)
            {
                _usuarioService.Insert(usuario);
            }
            else
            {
                _usuarioService.Update(usuario);
            }

            Limpiar();
            CargarGrid();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (idSeleccionado == 0) return;
            _usuarioService.Delete(idSeleccionado);
            Limpiar();
            CargarGrid();
        }

        private void btnCancelar_Click(object sender, EventArgs e) => Limpiar();

        private void dgvUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 0) return;
            var row = dgvUsuarios.SelectedRows[0];
            idSeleccionado = (int)row.Cells["Id"].Value;
            txtNombres.Text = row.Cells["Nombres"].Value.ToString();
            txtApellidos.Text = row.Cells["Apellidos"].Value.ToString();
            txtDocumento.Text = row.Cells["Documento"].Value.ToString();
            cmbTipoDocumento.Text = row.Cells["TipoDocumento"].Value.ToString();
            cmbRol.Text = row.Cells["Rol"].Value.ToString();
            txtSalario.Text = row.Cells["Salario"].Value?.ToString();
            txtContraseña.Text = "";
        }

        private void InitializeComponent()
        {
            this.Text = "Gestión de Usuarios";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblNombres = new Label { Text = "Nombres:", Location = new Point(20, 20), Size = new Size(80, 23) };
            txtNombres = new TextBox { Location = new Point(110, 20), Size = new Size(200, 23) };
            var lblApellidos = new Label { Text = "Apellidos:", Location = new Point(320, 20), Size = new Size(80, 23) };
            txtApellidos = new TextBox { Location = new Point(410, 20), Size = new Size(200, 23) };

            var lblDocumento = new Label { Text = "Documento:", Location = new Point(20, 60), Size = new Size(80, 23) };
            txtDocumento = new TextBox { Location = new Point(110, 60), Size = new Size(200, 23) };
            var lblTipo = new Label { Text = "Tipo Doc:", Location = new Point(320, 60), Size = new Size(80, 23) };
            cmbTipoDocumento = new ComboBox { Location = new Point(410, 60), Size = new Size(200, 23) };

            var lblContraseña = new Label { Text = "Contraseña:", Location = new Point(20, 100), Size = new Size(80, 23) };
            txtContraseña = new TextBox { Location = new Point(110, 100), Size = new Size(200, 23), PasswordChar = '*' };
            var lblRol = new Label { Text = "Rol:", Location = new Point(320, 100), Size = new Size(80, 23) };
            cmbRol = new ComboBox { Location = new Point(410, 100), Size = new Size(200, 23) };

            var lblSalario = new Label { Text = "Salario:", Location = new Point(20, 140), Size = new Size(80, 23) };
            txtSalario = new TextBox { Location = new Point(110, 140), Size = new Size(200, 23) };
            txtSalario.TextChanged += txtSalario_TextChanged;

            btnNuevo = new Button { Text = "Nuevo", Location = new Point(20, 190), Size = new Size(100, 35) };
            btnGuardar = new Button { Text = "Guardar", Location = new Point(130, 190), Size = new Size(100, 35) };
            btnEliminar = new Button { Text = "Eliminar", Location = new Point(240, 190), Size = new Size(100, 35) };
            btnCancelar = new Button { Text = "Cancelar", Location = new Point(350, 190), Size = new Size(100, 35) };

            btnNuevo.Click += btnNuevo_Click;
            btnGuardar.Click += btnGuardar_Click;
            btnEliminar.Click += btnEliminar_Click;
            btnCancelar.Click += btnCancelar_Click;

            dgvUsuarios = new DataGridView
            {
                Location = new Point(20, 240),
                Size = new Size(740, 200),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = true
            };
            dgvUsuarios.SelectionChanged += dgvUsuarios_SelectionChanged;

            this.Controls.AddRange(new Control[] { lblNombres, txtNombres, lblApellidos, txtApellidos,
                lblDocumento, txtDocumento, lblTipo, cmbTipoDocumento, lblContraseña, txtContraseña,
                lblRol, cmbRol, lblSalario, txtSalario, btnNuevo, btnGuardar, btnEliminar, btnCancelar, dgvUsuarios });
        }
    }
}
