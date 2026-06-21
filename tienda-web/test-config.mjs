const res = await fetch('http://localhost:5062/api/configuracion/public');
const data = await res.json();
console.log('qrNequiImg:', data.qrNequiImg);
console.log('All keys:', Object.keys(data));
