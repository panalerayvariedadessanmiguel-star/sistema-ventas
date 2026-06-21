'use client';

import { useEffect, useState } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import Link from 'next/link';
import { useAdmin } from '@/lib/admin-context';
import { api } from '@/lib/api';

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const { user, logout, ready } = useAdmin();
  const router = useRouter();
  const pathname = usePathname();
  const [pendientes, setPendientes] = useState(0);

  useEffect(() => {
    if (ready && !user && pathname !== '/admin/login') {
      router.push('/admin/login');
    }
  }, [user, pathname, router, ready]);

  useEffect(() => {
    if (!user) return;
    async function fetchPendientes() {
      try {
        const ventas = await api.ventas.getPendientes();
        setPendientes(ventas.length);
      } catch { }
    }
    fetchPendientes();
    const interval = setInterval(fetchPendientes, 15000);
    return () => clearInterval(interval);
  }, [user]);

  if (!ready) return null;

  if (!user) {
    return <>{pathname === '/admin/login' ? children : null}</>;
  }

  const links = [
    { href: '/admin', label: 'Dashboard', icon: '📊' },
    { href: '/admin/productos', label: 'Productos', icon: '📦' },
    { href: '/admin/productos/imagenes', label: 'Imagenes', icon: '🖼️' },
    { href: '/admin/categorias', label: 'Categorias', icon: '🏷️' },
    { href: '/admin/pedidos', label: 'Pedidos', icon: '🛒' },
    { href: '/admin/diseno', label: 'Diseno', icon: '🎨' },
    { href: '/admin/configuracion', label: 'Configuracion', icon: '⚙️' },
  ];

  return (
    <div className="min-h-screen flex bg-gray-100">
      <aside className="w-64 bg-white shadow-md flex flex-col">
        <div className="p-4 border-b">
          <h2 className="font-bold text-lg">Admin Panel</h2>
          <p className="text-sm text-gray-500">{user.nombres} {user.apellidos}</p>
        </div>
        <nav className="flex-1 p-2 space-y-1">
          {links.map(l => (
            <Link
              key={l.href}
              href={l.href}
              className={`flex items-center gap-3 px-3 py-2 rounded-md text-sm relative ${
                pathname === l.href || (l.href !== '/admin' && pathname.startsWith(l.href))
                  ? 'bg-blue-50 text-blue-700 font-medium'
                  : 'text-gray-700 hover:bg-gray-50'
              }`}
            >
              <span>{l.icon}</span> {l.label}
              {l.href === '/admin/pedidos' && pendientes > 0 && (
                <span className="ml-auto bg-red-500 text-white text-xs font-bold rounded-full min-w-[20px] h-5 flex items-center justify-center px-1 animate-pulse">
                  {pendientes}
                </span>
              )}
            </Link>
          ))}
        </nav>
        <div className="p-4 border-t">
          <button onClick={() => { logout(); router.push('/admin/login'); }} className="text-sm text-red-600 hover:underline w-full text-left">
            Cerrar Sesion
          </button>
          <Link href="/" className="block text-sm text-blue-600 hover:underline mt-1">Ir a la Tienda</Link>
        </div>
      </aside>
      <main className="flex-1 p-6 overflow-auto">
        {children}
      </main>
    </div>
  );
}
