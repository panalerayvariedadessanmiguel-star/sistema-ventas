const { Client, LocalAuth } = require('whatsapp-web.js');
const qrcode = require('qrcode-terminal');
const qrcodeImage = require('qr-image');
const express = require('express');
const path = require('path');

const PORT = process.env.PORT || 3007;
const app = express();
app.use(express.json());

let client = null;
let ready = false;
let lastQr = null;

function initClient() {
  client = new Client({
    authStrategy: new LocalAuth(),
    puppeteer: {
      headless: true,
      executablePath: 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe',
      args: ['--no-sandbox', '--disable-setuid-sandbox']
    }
  });

  client.on('qr', (qr) => {
    ready = false;
    lastQr = qr;
    console.log('\n===== ESCANEA ESTE QR CON WHATSAPP =====');
    qrcode.generate(qr, { small: true });
    console.log('==========================================\n');
    // Save QR as PNG
    try {
      const qrStream = qrcodeImage.image(qr, { type: 'png', margin: 2 });
      const qrPath = path.join(__dirname, 'qr.png');
      const fs = require('fs');
      const writeStream = fs.createWriteStream(qrPath);
      qrStream.pipe(writeStream);
      console.log(`[whatsapp-service] QR guardado en ${qrPath}`);
    } catch (e) {
      console.error('[whatsapp-service] Error al guardar QR:', e.message);
    }
  });

  client.on('ready', () => {
    ready = true;
    lastQr = null;
    console.log('[whatsapp-service] Cliente WhatsApp listo');
  });

  client.on('disconnected', (reason) => {
    ready = false;
    console.log('[whatsapp-service] Desconectado:', reason);
    console.log('[whatsapp-service] Reconectando en 5 segundos...');
    setTimeout(initClient, 5000);
  });

  client.initialize();
}

app.get('/health', (req, res) => {
  res.json({ ok: true, ready, number: client ? client.info?.wid?.user || null : null });
});

app.get('/qr', (req, res) => {
  if (ready) return res.json({ ready: true, mensaje: 'WhatsApp ya esta vinculado' });
  if (!lastQr) return res.status(404).json({ error: 'QR no disponible aun' });
  try {
    const qrStream = qrcodeImage.image(lastQr, { type: 'png', margin: 2 });
    res.type('png');
    qrStream.pipe(res);
  } catch (e) {
    res.status(500).json({ error: e.message });
  }
});

app.post('/send', async (req, res) => {
  const { phone, message } = req.body;

  if (!ready) {
    return res.status(503).json({ error: 'WhatsApp no esta listo. Escanea el QR primero.', qrUrl: 'http://localhost:3007/qr' });
  }

  if (!phone || !message) {
    return res.status(400).json({ error: 'Faltan phone o message' });
  }

  try {
    const chatId = phone.includes('@c.us') ? phone : `${phone}@c.us`;
    const response = await client.sendMessage(chatId, message);
    console.log(`[whatsapp-service] Mensaje enviado a ${phone}: "${message.substring(0, 50)}..."`);
    res.json({ ok: true, id: response.id.id });
  } catch (err) {
    console.error('[whatsapp-service] Error al enviar:', err);
    res.status(500).json({ error: err.message });
  }
});

app.listen(PORT, () => {
  console.log(`[whatsapp-service] Escuchando en http://localhost:${PORT}`);
  initClient();
});
