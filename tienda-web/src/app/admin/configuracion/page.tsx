'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';

export default function AdminConfiguracion() {
  const [items, setItems] = useState<{ id: number; clave: string; valor: string; descripcion: string }[]>([]);
  const [newPass, setNewPass] = useState('');
  const [confirmPass, setConfirmPass] = useState('');
  const [passMsg, setPassMsg] = useState('');
  const [savingPass, setSavingPass] = useState(false);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState('');

  useEffect(() => {
    api.configuracion.getAll().then(setItems).catch(() => {}).finally(() => setLoading(false));
  }, []);

  function setValor(clave: string, valor: string) {
    setItems(prev => prev.map(i => i.clave === clave ? { ...i, valor } : i));
  }

  async function handleSave() {
    setSaving(true);
    setMsg('');
    try {
      const updates = items.map(i => ({ clave: i.clave, valor: i.valor }));
      await api.configuracion.update(updates);
      setMsg('Configuracion guardada exitosamente');
    } catch (err: any) {
      alert(err.message);
    } finally {
      setSaving(false);
    }
  }

  const generalItems = items.filter(i => i.id <= 1000 && !i.clave.startsWith('DOMICILIO_'));
  const domicilioItems = items.filter(i => i.clave.startsWith('DOMICILIO_'));
  const marginItems = items.filter(i => i.id > 1000 && !i.clave.startsWith('DOMICILIO_'));

  if (loading) return <div className="text-gray-500">Cargando...</div>;

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">Configuracion de la Tienda</h1>

      <div className="bg-white rounded-lg shadow p-6 space-y-4 mb-6">
        <h2 className="font-bold text-lg border-b pb-2">Informacion General</h2>
        {generalItems.map(item => (
          <div key={item.id}>
            <label className="block text-sm font-medium text-gray-700 mb-1">{item.descripcion}</label>
            <input
              value={item.valor}
              onChange={e => setValor(item.clave, e.target.value)}
              className="block w-full border border-gray-300 rounded-md px-3 py-2"
            />
          </div>
        ))}
      </div>

      {domicilioItems.length > 0 && (
        <div className="bg-white rounded-lg shadow p-6 space-y-4 mb-6">
          <h2 className="font-bold text-lg border-b pb-2">Domicilio y Entregas</h2>
          {domicilioItems.map(item => (
            <div key={item.id}>
              <label className="block text-sm font-medium text-gray-700 mb-1">{item.descripcion}</label>
              <input
                type="number"
                value={item.valor}
                onChange={e => setValor(item.clave, e.target.value)}
                className="block w-full border border-gray-300 rounded-md px-3 py-2"
              />
            </div>
          ))}
        </div>
      )}

      <div className="bg-white rounded-lg shadow p-6 space-y-4 mb-6">
        <h2 className="font-bold text-lg border-b pb-2">Margenes</h2>
        {marginItems.map(item => (
          <div key={item.id}>
            <label className="block text-sm font-medium text-gray-700 mb-1">{item.descripcion} (%)</label>
            <input
              value={item.valor}
              onChange={e => setValor(item.clave, e.target.value)}
              className="block w-full border border-gray-300 rounded-md px-3 py-2"
            />
          </div>
        ))}
      </div>

      {msg && <p className="text-green-600 text-sm mb-4">{msg}</p>}

      <button
        onClick={handleSave}
        disabled={saving}
        className="bg-blue-600 text-white px-6 py-2 rounded-md hover:bg-blue-700 disabled:opacity-50"
      >
        {saving ? 'Guardando...' : 'Guardar Configuracion'}
      </button>

      <div className="bg-white rounded-lg shadow p-6 space-y-4 mt-6">
        <h2 className="font-bold text-lg border-b pb-2">Cambiar Contrasena de Administrador</h2>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Nueva Contrasena</label>
            <input type="password" value={newPass} onChange={e => setNewPass(e.target.value)} className="block w-full border border-gray-300 rounded-md px-3 py-2" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Confirmar Contrasena</label>
            <input type="password" value={confirmPass} onChange={e => setConfirmPass(e.target.value)} className="block w-full border border-gray-300 rounded-md px-3 py-2" />
          </div>
        </div>
        {passMsg && <p className={`text-sm ${passMsg.includes('exitosa') ? 'text-green-600' : 'text-red-600'}`}>{passMsg}</p>}
        <button
          onClick={async () => {
            if (!newPass || newPass !== confirmPass) { setPassMsg('Las contrasenas no coinciden'); return; }
            setSavingPass(true); setPassMsg('');
            try {
              const r = await api.auth.changePassword(newPass);
              setPassMsg(r.mensaje); setNewPass(''); setConfirmPass('');
            } catch (err: any) { setPassMsg(err.message); }
            finally { setSavingPass(false); }
          }}
          disabled={savingPass}
          className="bg-green-600 text-white px-6 py-2 rounded-md hover:bg-green-700 disabled:opacity-50"
        >
          {savingPass ? 'Guardando...' : 'Cambiar Contrasena'}
        </button>
      </div>
    </div>
  );
}
