const fs = require('fs');
const path = require('path');
const pdf = require('pdf-parse');

const dir = "C:\\Users\\Familia_Jica\\Desktop\\Pañalera y Variedades San Miguel\\Facturas de Venta Web";
const files = fs.readdirSync(dir).filter(f => f.endsWith('.pdf'));

(async () => {
  for (const file of files) {
    const buf = fs.readFileSync(path.join(dir, file));
    const data = await pdf(buf);
    console.log(`===== ${file} =====`);
    console.log(data.text);
    console.log();
  }
})().catch(err => console.error(err));
