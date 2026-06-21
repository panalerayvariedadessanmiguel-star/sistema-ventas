'use client';

import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useState, useEffect } from 'react';
import { api, type Venta } from '@/lib/api';
import { useCliente } from '@/lib/cliente-context';
import { formatCOP } from '@/lib/formats';

export default function MisPedidosPage() {
  const { isLoggedIn, cliente } = useCliente();
  const router = useRouter();
  const [ventas, setVentas] = useState<Venta[]>([]);
  const [cargando, setCargando] = useState(true);
  const [showPasswordForm, setShowPasswordForm] = useState(false);
  const [nuevaContrasena, setNuevaContrasena] = useState('');
  const [passwordMsg, setPasswordMsg] = useState('');
  const [passwordError, setPasswordError] = useState('');

  useEffect(() => {
    if (!isLoggedIn) { router.replace('/ingresar'); return; }
    const fetch = async () => {
      try {
        const data = await api.ventas.misPedidos();
        setVentas(data);
      } catch { } finally {
        setCargando(false);
      }
    };
    fetch();
  }, [isLoggedIn, router]);

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setPasswordMsg('');
    setPasswordError('');
    if (nuevaContrasena.length < 4) { setPasswordError('Minimo 4 caracteres'); return; }
    try {
      const res = await api.clientes.changePassword(nuevaContrasena);
      setPasswordMsg(res.mensaje);
      setNuevaContrasena('');
      setShowPasswordForm(false);
    } catch (err: unknown) {
      setPasswordError(err instanceof Error ? err.message : 'Error al cambiar contrasena');
    }
  };

  if (!isLoggedIn) return null;

  return (
    <div className="max-w-3xl mx-auto">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Mis pedidos</h1>
          <p className="text-gray-500 text-sm">Hola, {cliente?.nombre}</p>
        </div>
        <button
          onClick={() => setShowPasswordForm(!showPasswordForm)}
          className="text-sm text-gray-500 hover:text-blue-600"
        >
          Cambiar contrasena
        </button>
      </div>

      {showPasswordForm && (
        <form onSubmit={handleChangePassword} className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 mb-6 flex items-end gap-3">
          <div className="flex-1">
            <label className="block text-xs text-gray-600 mb-1">Nueva contrasena</label>
            <input
              type="password"
              value={nuevaContrasena}
              onChange={e => setNuevaContrasena(e.target.value)}
              className="w-full px-3 py-2 border border-yellow-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-yellow-500"
              required
            />
          </div>
          <button
            type="submit"
            className="bg-yellow-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-yellow-700 transition-colors shrink-0"
          >
            Guardar
          </button>
        </form>
      )}
      {passwordMsg && <p className="text-green-600 text-sm mb-4">{passwordMsg}</p>}
      {passwordError && <p className="text-red-600 text-sm mb-4">{passwordError}</p>}

      {cargando ? (
        <div className="space-y-3">
          {[1, 2, 3].map(i => (
            <div key={i} className="h-20 bg-gray-100 rounded-lg animate-pulse" />
          ))}
        </div>
      ) : ventas.length === 0 ? (
        <div className="text-center py-16">
          <p className="text-gray-500 mb-4">No tienes pedidos aun</p>
          <Link href="/" className="inline-block bg-blue-600 text-white px-6 py-2.5 rounded-lg font-medium hover:bg-blue-700 transition-colors">
            Ir a la tienda
          </Link>
        </div>
      ) : (
        <div className="space-y-3">
          {ventas.map(v => (
            <Link
              key={v.id}
              href={`/mis-pedidos/${v.id}`}
              className="block bg-white rounded-lg border border-gray-100 p-4 hover:shadow-md transition-shadow"
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium text-gray-900">{v.numeroVenta}</p>
                  <p className="text-sm text-gray-500">{new Date(v.fechaVenta).toLocaleDateString('es-CO', { day: 'numeric', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit' })}</p>
                </div>
                <div className="text-right">
                  <p className="font-bold text-gray-900">{formatCOP(v.total)}</p>
                  <span className={`text-xs font-medium ${v.anulada ? 'text-red-500' : 'text-green-600'}`}>
                    {v.anulada ? 'Anulado' : 'Confirmado'}
                  </span>
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
