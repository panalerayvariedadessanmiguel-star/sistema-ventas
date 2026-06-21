using System.Net.Http.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SistemaVentas.WebAPI.Models;
using SistemaVentas.WebAPI.Repositories;

namespace SistemaVentas.WebAPI.Services;

public class NotificacionService
{
    private readonly IConfiguration _config;
    private readonly ILogger<NotificacionService> _logger;
    private readonly ConfiguracionRepository _configRepo;
    private readonly HttpClient _httpClient;

    public NotificacionService(IConfiguration config, ILogger<NotificacionService> logger, ConfiguracionRepository configRepo, HttpClient httpClient)
    {
        _config = config;
        _logger = logger;
        _configRepo = configRepo;
        _httpClient = httpClient;
    }

    public async Task NotificarPedidoConfirmadoAsync(Venta venta, List<DetalleVenta> detalles)
    {
        await EnviarEmailAsync(venta, detalles);
        await EnviarWhatsAppAsync(venta, detalles);
    }

    private async Task EnviarEmailAsync(Venta venta, List<DetalleVenta> detalles)
    {
        var smtpSection = _config.GetSection("Smtp");
        var host = smtpSection["Host"];
        var portStr = smtpSection["Port"];
        var username = smtpSection["Username"];
        var password = smtpSection["Password"];
        var fromEmail = smtpSection["FromEmail"] ?? username;
        var fromName = smtpSection["FromName"] ?? "Tienda Virtual";

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fromEmail))
        {
            _logger.LogWarning("SMTP no configurado. Omitiendo notificacion email.");
            return;
        }

        var port = int.TryParse(portStr, out var p) ? p : 587;

        var configs = (await _configRepo.GetAllAsync()).ToDictionary(c => c.Clave, c => c.Valor);
        configs.TryGetValue("NOMBRE_EMPRESA", out var empresa);
        empresa ??= "Tienda";
        configs.TryGetValue("INFO_EMAIL", out var toEmail);
        toEmail ??= "";
        if (string.IsNullOrEmpty(toEmail))
        {
            _logger.LogWarning("INFO_EMAIL no configurado. Omitiendo notificacion email.");
            return;
        }

        var detallesHtml = string.Join("", detalles.Select(d =>
            $"<tr><td>{d.NombreProducto}</td><td>{d.Cantidad}</td><td>${d.PrecioUnitario:N0}</td><td>${d.Total:N0}</td></tr>"));

        var body = $"""
<h2>Nuevo Pedido Confirmado</h2>
<p><strong>Venta:</strong> {venta.NumeroVenta}</p>
<p><strong>Fecha:</strong> {venta.FechaVenta:dd/MM/yyyy HH:mm}</p>
<p><strong>Cliente:</strong> {venta.NombreCliente}</p>
<p><strong>Documento:</strong> {venta.DocumentoCliente}</p>
<p><strong>Telefono:</strong> {venta.TelefonoCliente}</p>
<p><strong>Direccion:</strong> {venta.DireccionCliente}</p>
<p><strong>Metodo de pago:</strong> {venta.MetodoPago}</p>

<h3>Productos</h3>
<table border='1' cellpadding='6' cellspacing='0' style='border-collapse:collapse;width:100%'>
<thead style='background:#f3f3f3'><tr><th>Producto</th><th>Cant</th><th>Precio</th><th>Total</th></tr></thead>
<tbody>{detallesHtml}</tbody>
</table>

<p><strong>Total:</strong> ${venta.Total:N0}</p>
<hr>
<p style='color:#666'>{empresa}</p>
""";

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = $"Nuevo pedido {venta.NumeroVenta} - {venta.NombreCliente}";
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email de notificacion enviado a {Email} para venta {Numero}", toEmail, venta.NumeroVenta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificacion email para venta {Numero}", venta.NumeroVenta);
        }
    }

    private async Task EnviarWhatsAppAsync(Venta venta, List<DetalleVenta> detalles)
    {
        try
        {
            var configs = (await _configRepo.GetAllAsync()).ToDictionary(c => c.Clave, c => c.Valor);
            configs.TryGetValue("INFO_WHATSAPP", out var phone);
            if (string.IsNullOrEmpty(phone))
            {
                _logger.LogWarning("INFO_WHATSAPP no configurado. Omitiendo notificacion WhatsApp.");
                return;
            }

            phone = phone.Replace("+", "").Replace(" ", "").Replace("-", "");
            if (!phone.StartsWith("57")) phone = "57" + phone;

            var resumen = string.Join("\n", detalles.Select(d => $"  - {d.NombreProducto} x{d.Cantidad} = ${d.Total:N0}"));
            var message = $"""
*Nuevo Pedido - {venta.NumeroVenta}*
Cliente: {venta.NombreCliente}
Documento: {venta.DocumentoCliente}
Telefono: {venta.TelefonoCliente}
Direccion: {venta.DireccionCliente}
Pago: {venta.MetodoPago}

Productos:
{resumen}

*Total: ${venta.Total:N0}*
""";

            var response = await _httpClient.PostAsJsonAsync("http://localhost:3007/send", new { phone, message });

            if (response.IsSuccessStatusCode)
                _logger.LogInformation("WhatsApp notificacion enviada a {Phone} para venta {Numero}", phone, venta.NumeroVenta);
            else
                _logger.LogWarning("WhatsApp servicio respondio {Status} para venta {Numero}: {Body}",
                    (int)response.StatusCode, venta.NumeroVenta, await response.Content.ReadAsStringAsync());
        }
        catch (HttpRequestException)
        {
            _logger.LogWarning("WhatsApp service no disponible en localhost:3007. Omitiendo notificacion WhatsApp.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificacion WhatsApp para venta {Numero}", venta.NumeroVenta);
        }
    }
}
