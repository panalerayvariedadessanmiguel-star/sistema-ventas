const fs = require('fs');
const path = require('path');

const dir = 'C:\\Users\\Familia_Jica\\Desktop\\Pañalera y Variedades San Miguel\\Facturas de Venta Web';
const files = fs.readdirSync(dir).filter(f => f.endsWith('.pdf')).sort();

for (const file of files) {
  console.log('\n=== ' + file + ' ===');
  const buf = fs.readFileSync(path.join(dir, file));
  const text = buf.toString('latin1');
  
  // Extract text between parentheses in PDF (standard PDF text objects)
  const matches = text.match(/\(([^)]*)\)/g);
  if (matches) {
    for (const m of matches) {
      const clean = m.slice(1, -1).replace(/\\[0-9]{3}/g, '').trim();
      if (clean.length > 1) console.log(clean);
    }
  }
}
