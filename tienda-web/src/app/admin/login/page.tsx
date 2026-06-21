'use client';

import { useState, FormEvent } from 'react';
import { useRouter } from 'next/navigation';
import { api } from '@/lib/api';
import { useAdmin } from '@/lib/admin-context';

export default function AdminLogin() {
  const [doc, setDoc] = useState('');
  const [pass, setPass] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const router = useRouter();
  const { login } = useAdmin();

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const user = await api.auth.login(doc, pass);
      if (user.rol !== 'Administrador') {
        setError('Solo los administradores pueden acceder al panel');
        return;
      }
      login(user);
      router.push('/admin');
    } catch (err: any) {
      setError(err.message || 'Error al iniciar sesion');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100">
      <div className="bg-white p-8 rounded-lg shadow-md w-full max-w-md">
        <h1 className="text-2xl font-bold text-center mb-6">Panel de Administracion</h1>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">Documento</label>
            <input
              type="text" value={doc} onChange={e => setDoc(e.target.value)}
              className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2 focus:ring-blue-500 focus:border-blue-500"
              required
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">Contrasena</label>
            <input
              type="password" value={pass} onChange={e => setPass(e.target.value)}
              className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
          {error && <p className="text-red-600 text-sm">{error}</p>}
          <button
            type="submit" disabled={loading}
            className="w-full bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700 disabled:opacity-50"
          >
            {loading ? 'Ingresando...' : 'Ingresar'}
          </button>
          <a href="/" className="block text-center text-sm text-blue-600 hover:underline">Volver a la tienda</a>
        </form>
      </div>
    </div>
  );
}
