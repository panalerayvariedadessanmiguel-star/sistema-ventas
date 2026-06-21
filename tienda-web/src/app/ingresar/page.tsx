'use client';

import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { api } from '@/lib/api';
import { useCliente } from '@/lib/cliente-context';
import { useStore } from '@/lib/store-context';

export default function LoginPage() {
  const { login } = useCliente();
  const { whatsapp } = useStore();
  const router = useRouter();
  const [documento, setDocumento] = useState('');
  const [contrasena, setContrasena] = useState('');
  const [error, setError] = useState('');
  const [cargando, setCargando] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setCargando(true);
    try {
      const res = await api.clientes.login(documento, contrasena);
      login(res.token, {
        id: res.id, documento: res.documento, nombre: res.nombre,
        telefono: res.telefono, email: res.email, direccion: res.direccion,
      });
      router.push('/mis-pedidos');
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error al iniciar sesion');
    } finally {
      setCargando(false);
    }
  };

  return (
    <div className="max-w-sm mx-auto py-16">
      <h1 className="text-2xl font-bold text-gray-900 text-center mb-8">Iniciar sesion</h1>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Documento</label>
          <input
            type="text"
            value={documento}
            onChange={e => setDocumento(e.target.value)}
            className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Contrasena</label>
          <input
            type="password"
            value={contrasena}
            onChange={e => setContrasena(e.target.value)}
            className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>
        {error && <p className="text-red-600 text-sm">{error}</p>}
        <button
          type="submit"
          disabled={cargando}
          className="w-full bg-blue-600 text-white py-2.5 rounded-lg font-medium hover:bg-blue-700 disabled:bg-gray-300 transition-colors"
        >
          {cargando ? 'Entrando...' : 'Entrar'}
        </button>
        <div className="text-sm text-gray-500 text-center space-y-2">
          <p>
            No tienes cuenta?{' '}
            <Link href="/registro" className="text-blue-600 hover:underline">Registrate</Link>
          </p>
          {whatsapp && (
            <p>
              <a
                href={`https://wa.me/57${whatsapp.replace(/[^0-9]/g, '').replace(/^0/, '')}?text=${encodeURIComponent('Hola, olvide mi contrasena. Mi documento es: [tu documento]')}`}
                target="_blank"
                rel="noopener noreferrer"
                className="text-gray-500 hover:text-green-600"
              >
                Olvidaste tu contrasena?
              </a>
            </p>
          )}
        </div>
      </form>
    </div>
  );
}
