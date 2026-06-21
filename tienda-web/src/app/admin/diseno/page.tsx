'use client';

import { useEffect, useState, useRef } from 'react';
import { api } from '@/lib/api';
import { useStore } from '@/lib/store-context';

export default function AdminDiseno() {
  const [items, setItems] = useState<{ id: number; clave: string; valor: string; descripcion: string }[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState('');
  const logoFileRef = useRef<HTMLInputElement>(null);
  const [uploadingLogo, setUploadingLogo] = useState(false);
  const brebFileRef = useRef<HTMLInputElement>(null);
  const [uploadingBreb, setUploadingBreb] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem('admin_token');
    if (!token) { setLoading(false); return; }
    api.configuracion.getDiseno().then(setItems).catch(() => {}).finally(() => setLoading(false));
  }, []);

  function setValor(clave: string, valor: string) {
    setItems(prev => prev.map(i => i.clave === clave ? { ...i, valor } : i));
  }

  const { refreshConfig } = useStore();

  async function handleSave() {
    setSaving(true);
    setMsg('');
    try {
      await api.configuracion.update(items.map(i => ({ clave: i.clave, valor: i.valor })));
      refreshConfig();
      setMsg('Diseno actualizado exitosamente');
    } catch (err: any) {
      alert(err.message);
    } finally {
      setSaving(false);
    }
  }

  async function handleUploadLogo(file: File) {
    setUploadingLogo(true);
    try {
      const url = await api.storage.upload(file);
      setValor('SITE_LOGO', url);
    } catch (err: any) {
      alert(err.message);
    } finally {
      setUploadingLogo(false);
    }
  }

  async function handleUploadBreb(file: File) {
    setUploadingBreb(true);
    try {
      const url = await api.storage.upload(file);
      setValor('QR_BREB_IMG', url);
    } catch (err: any) {
      alert(err.message);
    } finally {
      setUploadingBreb(false);
    }
  }

  function getInputType(item: { clave: string; valor: string; descripcion: string }) {
    if (item.clave.startsWith('COLOR_')) return 'color';
    if (item.clave.includes('LOGO')) return 'url';
    return 'text';
  }

  if (loading) return <div className="text-gray-500">Cargando...</div>;

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">Diseno de la Tienda</h1>
      <p className="text-sm text-gray-500 mb-4">Personaliza la apariencia de tu tienda online. Los cambios se aplican al instante.</p>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
        {items.filter(i => i.clave.startsWith('COLOR_')).map(item => (
          <div key={item.id} className="bg-white rounded-lg shadow p-4">
            <label className="block text-sm font-medium text-gray-700 mb-2">{item.descripcion}</label>
            <div className="flex items-center gap-3">
              <input
                type="color" value={item.valor}
                onChange={e => setValor(item.clave, e.target.value)}
                className="w-12 h-12 rounded cursor-pointer border"
              />
              <input
                type="text" value={item.valor}
                onChange={e => setValor(item.clave, e.target.value)}
                className="flex-1 border border-gray-300 rounded-md px-3 py-2 text-sm font-mono"
              />
            </div>
          </div>
        ))}
      </div>

      <div className="bg-white rounded-lg shadow p-6 space-y-4 mb-6">
        <h2 className="font-bold text-lg border-b pb-2">Hero - Eslogan y Titulos</h2>
        {items.filter(i => i.clave === 'SLOGAN' || i.clave.startsWith('HERO_')).map(item => (
          <div key={item.id}>
            <label className="block text-sm font-medium text-gray-700 mb-1">{item.descripcion}</label>
            <input
              type={getInputType(item)}
              value={item.valor}
              onChange={e => setValor(item.clave, e.target.value)}
              className="block w-full border border-gray-300 rounded-md px-3 py-2"
              placeholder={item.descripcion}
            />
          </div>
        ))}
      </div>

      <div className="bg-white rounded-lg shadow p-6 space-y-4 mb-6">
        <h2 className="font-bold text-lg border-b pb-2">Informacion de Contacto</h2>
        {items.filter(i => i.clave.startsWith('INFO_')).map(item => (
          <div key={item.id}>
            <label className="block text-sm font-medium text-gray-700 mb-1">{item.descripcion}</label>
            <input
              type={item.clave === 'INFO_EMAIL' ? 'email' : 'text'}
              value={item.valor}
              onChange={e => setValor(item.clave, e.target.value)}
              className="block w-full border border-gray-300 rounded-md px-3 py-2"
              placeholder={item.descripcion}
            />
          </div>
        ))}
      </div>

      <div className="bg-white rounded-lg shadow p-6 space-y-4 mb-6">
        <h2 className="font-bold text-lg border-b pb-2">Textos</h2>
        {items.filter(i => i.clave.startsWith('SITE_') || i.clave === 'NOMBRE_EMPRESA').map(item => (
          <div key={item.id}>
            <label className="block text-sm font-medium text-gray-700 mb-1">{item.descripcion}</label>
            <input
              type={getInputType(item)}
              value={item.valor}
              onChange={e => setValor(item.clave, e.target.value)}
              className="block w-full border border-gray-300 rounded-md px-3 py-2"
              placeholder={item.descripcion}
            />
          </div>
        ))}
      </div>

      <div className="bg-white rounded-lg shadow p-6 space-y-4 mb-6">
        <h2 className="font-bold text-lg border-b pb-2">Pago con Davivienda Bre-B</h2>
        <p className="text-sm text-gray-500">Configura el codigo QR interoperable para recibir pagos desde cualquier banco via Bre-B.</p>
        {items.filter(i => i.clave.startsWith('QR_BREB_IMG') || i.clave === 'BREB_LLAVE').map(item => (
          <div key={item.id}>
            <label className="block text-sm font-medium text-gray-700 mb-1">{item.descripcion}</label>
            {item.clave === 'QR_BREB_IMG' ? (
              <div className="space-y-2">
                <div className="flex gap-2">
                  <input
                    type="url"
                    value={item.valor}
                    onChange={e => setValor(item.clave, e.target.value)}
                    className="flex-1 block border border-gray-300 rounded-md px-3 py-2"
                    placeholder="https://ejemplo.com/qr-breb.png"
                  />
                  <input ref={brebFileRef} type="file" accept="image/*" onChange={e => { const f = e.target.files?.[0]; if (f) handleUploadBreb(f); }} className="hidden" />
                  <button type="button" onClick={() => brebFileRef.current?.click()} disabled={uploadingBreb} className="bg-blue-600 text-white px-3 py-2 rounded-md text-sm hover:bg-blue-700 disabled:opacity-50 whitespace-nowrap">
                    {uploadingBreb ? 'Subiendo...' : 'Subir imagen'}
                  </button>
                </div>
                {item.valor && (
                  <img src={item.valor} alt="QR Bre-B" className="w-48 h-48 border rounded-lg object-contain" />
                )}
              </div>
            ) : (
              <input
                type="text"
                value={item.valor}
                onChange={e => setValor(item.clave, e.target.value)}
                className="block w-full border border-gray-300 rounded-md px-3 py-2"
                placeholder="3001234567"
              />
            )}
          </div>
        ))}
        <h3 className="font-semibold text-sm text-gray-700 mt-4">Datos bancarios (opcional)</h3>
        <p className="text-sm text-gray-500">Informacion de la cuenta para transferencia tradicional.</p>
        {items.filter(i => i.clave.startsWith('BANCO_')).map(item => (
          <div key={item.id}>
            <label className="block text-sm font-medium text-gray-700 mb-1">{item.descripcion}</label>
            <input
              type="text"
              value={item.valor}
              onChange={e => setValor(item.clave, e.target.value)}
              className="block w-full border border-gray-300 rounded-md px-3 py-2"
            />
          </div>
        ))}
      </div>

      <div className="bg-white rounded-lg shadow p-6 space-y-4 mb-6">
        <h2 className="font-bold text-lg border-b pb-2">Pago con Tarjeta</h2>
        <p className="text-sm text-gray-500">Informacion que se mostrara al cliente sobre pago con tarjeta debito o credito.</p>
        {items.filter(i => i.clave === 'TARJETA_INFO').map(item => (
          <div key={item.id}>
            <label className="block text-sm font-medium text-gray-700 mb-1">{item.descripcion}</label>
            <textarea
              value={item.valor}
              onChange={e => setValor(item.clave, e.target.value)}
              className="block w-full border border-gray-300 rounded-md px-3 py-2"
              rows={3}
            />
          </div>
        ))}
      </div>

      <div className="bg-white rounded-lg shadow p-6 space-y-4 mb-6">
        <h2 className="font-bold text-lg border-b pb-2">Chat en vivo</h2>
        <p className="text-sm text-gray-500">Configura el chat en vivo de Tawk.to. Crea una cuenta en tawk.to y pega tu Property ID.</p>
        {items.filter(i => i.clave === 'TAWKTO_PROPERTY_ID').map(item => (
          <div key={item.id}>
            <label className="block text-sm font-medium text-gray-700 mb-1">{item.descripcion}</label>
            <input
              type="text"
              value={item.valor}
              onChange={e => setValor(item.clave, e.target.value)}
              className="block w-full border border-gray-300 rounded-md px-3 py-2"
              placeholder="ej: 1234567890abc1234567890ab"
            />
          </div>
        ))}
      </div>

      <div className="bg-white rounded-lg shadow p-6 space-y-4 mb-6">
        <h2 className="font-bold text-lg border-b pb-2">Domicilio y Entregas</h2>
        {items.filter(i => i.clave.startsWith('DOMICILIO_')).map(item => (
          <div key={item.id}>
            <label className="block text-sm font-medium text-gray-700 mb-1">{item.descripcion}</label>
            <input
              type={item.clave === 'DOMICILIO_TIEMPO_ESTIMADO' ? 'text' : 'number'}
              value={item.valor}
              onChange={e => setValor(item.clave, e.target.value)}
              className="block w-full border border-gray-300 rounded-md px-3 py-2"
              placeholder={item.clave === 'DOMICILIO_TIEMPO_ESTIMADO' ? '2-5 dias habiles' : undefined}
            />
          </div>
        ))}
        <p className="text-xs text-gray-400">El costo de domicilio se aplica al total del carrito. Si pones un valor minimo, el domicilio sera gratis para pedidos que superen ese monto.</p>
      </div>

      <div className="bg-white rounded-lg shadow p-6 mb-6">
        <h2 className="font-bold text-lg border-b pb-2 mb-4">Vista Previa</h2>
        <div className="border rounded-lg overflow-hidden">
          <div className="p-4" style={{ backgroundColor: items.find(i => i.clave === 'COLOR_FONDO')?.valor || '#F9FAFB' }}>
            <div className="bg-white shadow rounded-lg p-4 mb-4">
              <div className="flex items-center justify-between">
                <span className="text-xl font-bold" style={{ color: items.find(i => i.clave === 'COLOR_PRINCIPAL')?.valor || '#3B82F6' }}>
                  {items.find(i => i.clave === 'NOMBRE_EMPRESA')?.valor || 'Mi Tienda'}
                </span>
              </div>
            </div>
            <div>
              <h3 className="text-2xl font-bold text-gray-900 mb-1">
                {items.find(i => i.clave === 'SITE_TITLE')?.valor || 'Bienvenido'}
              </h3>
              <p className="text-gray-600">
                {items.find(i => i.clave === 'SITE_SUBTITLE')?.valor || ''}
              </p>
            </div>
          </div>
        </div>
      </div>

      {msg && <p className="text-green-600 text-sm mb-4">{msg}</p>}

      <button
        onClick={handleSave}
        disabled={saving}
        className="bg-blue-600 text-white px-6 py-2 rounded-md hover:bg-blue-700 disabled:opacity-50"
      >
        {saving ? 'Guardando...' : 'Guardar Diseno'}
      </button>
    </div>
  );
}
