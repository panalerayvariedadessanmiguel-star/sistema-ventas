'use client';

import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { api } from '@/lib/api';
import { useCliente } from '@/lib/cliente-context';

export default function RegistroPage() {
  const { login } = useCliente();
  const router = useRouter();
  const [form, setForm] = useState({ documento: '', contrasena: '', nombre: '', telefono: '', direccion: '' });
  const [error, setError] = useState('');
  const [cargando, setCargando] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setCargando(true);
    try {
      const res = await api.clientes.registro(form);
      login(res.token, {
        id: res.id, documento: res.documento, nombre: res.nombre,
        telefono: res.telefono, email: res.email, direccion: res.direccion,
      });
      router.push('/mis-pedidos');
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error al registrarse');
    } finally {
      setCargando(false);
    }
  };

  const set = (campo: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(prev => ({ ...prev, [campo]: e.target.value }));

  return (
    <div className="max-w-sm mx-auto py-16">
      <h1 className="text-2xl font-bold text-gray-900 text-center mb-8">Crear cuenta</h1>
      <form onSubmit={handleSubmit} className="space-y-4">
        {(['nombre', 'documento', 'telefono', 'direccion', 'contrasena'] as const).map(campo => (
          <div key={campo}>
            <label className="block text-sm font-medium text-gray-700 mb-1 capitalize">
              {campo === 'contrasena' ? 'Contrasena' : campo}
            </label>
            <input
              type={campo === 'contrasena' ? 'password' : 'text'}
              value={form[campo]}
              onChange={set(campo)}
              className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            />
          </div>
        ))}
        {error && <p className="text-red-600 text-sm">{error}</p>}
        <button
          type="submit"
          disabled={cargando}
          className="w-full bg-blue-600 text-white py-2.5 rounded-lg font-medium hover:bg-blue-700 disabled:bg-gray-300 transition-colors"
        >
          {cargando ? 'Registrando...' : 'Crear cuenta'}
        </button>
        <p className="text-sm text-gray-500 text-center">
          Ya tienes cuenta?{' '}
          <Link href="/ingresar" className="text-blue-600 hover:underline">Inicia sesion</Link>
        </p>
      </form>
    </div>
  );
}
