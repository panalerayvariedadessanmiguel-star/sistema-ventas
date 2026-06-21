'use client';

import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useCarrito } from '@/lib/carrito-context';
import { useStore } from '@/lib/store-context';
import { useAdmin } from '@/lib/admin-context';
import { useCliente } from '@/lib/cliente-context';
import { useState, useEffect } from 'react';

export default function Header() {
  const { cantidadItems } = useCarrito();
  const { nombreTienda, slogan, telefono, whatsapp, email, direccion, ciudad, colorPrincipal, logo, nit, horario } = useStore();
  const { user } = useAdmin();
  const { isLoggedIn, cliente, logout } = useCliente();
  const router = useRouter();
  const [hydrated, setHydrated] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  useEffect(() => { setHydrated(true); }, []);

  const handleLogout = () => {
    logout();
    setMenuOpen(false);
    router.push('/');
  };

  const navLinks = (
    <>
      <Link href="/" onClick={() => setMenuOpen(false)} className="text-gray-700 hover:text-blue-600 font-medium">
        Productos
      </Link>
      {user && (
        <Link href="/admin" onClick={() => setMenuOpen(false)} className="text-gray-700 hover:text-blue-600 font-medium">
          Admin
        </Link>
      )}
      {hydrated && isLoggedIn ? (
        <>
          <Link href="/mis-pedidos" onClick={() => setMenuOpen(false)} className="text-gray-700 hover:text-blue-600 font-medium">
            Mis pedidos
          </Link>
          <button onClick={handleLogout} className="text-gray-500 hover:text-red-600 text-left">
            Salir
          </button>
        </>
      ) : (
        <Link href="/ingresar" onClick={() => setMenuOpen(false)} className="text-gray-700 hover:text-blue-600 font-medium">
          Entrar
        </Link>
      )}
    </>
  );

  return (
    <header className="bg-white shadow-md sticky top-0 z-50">
              {(telefono || whatsapp || email || direccion || ciudad) && (
        <div className="bg-gray-50 border-b">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="flex items-center justify-center sm:justify-start gap-x-5 h-8 text-xs text-gray-600">
               {(direccion || ciudad) && (
                <a href={`https://www.google.com/maps/search/${encodeURIComponent([direccion, ciudad].filter(Boolean).join(', '))}`} target="_blank" rel="noopener noreferrer" className="inline-flex items-center gap-1 hover:text-red-700">
                  <svg className="w-3.5 h-3.5" fill="#EF4444" stroke="none" viewBox="0 0 24 24"><path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z"/></svg>
                  {[direccion, ciudad].filter(Boolean).join(', ')}
                </a>
              )}
              {telefono && (
                <a href={`tel:${telefono}`} className="inline-flex items-center gap-1 hover:text-blue-700">
                  <svg className="w-3.5 h-3.5" fill="#3B82F6" stroke="none" viewBox="0 0 24 24"><path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"/></svg>
                  {telefono}
                </a>
              )}
              {whatsapp && (
                <a href={`https://wa.me/57${whatsapp.replace(/[^0-9]/g, '').replace(/^(57|0)+/, '')}`} target="_blank" rel="noopener noreferrer" className="inline-flex items-center gap-1 hover:text-green-700 font-medium">
                  <svg className="w-3.5 h-3.5" viewBox="0 0 24 24"><path fill="#25D366" d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z"/></svg>
                  WhatsApp
                </a>
              )}
              {email && (
                <a href={`mailto:${email}`} className="inline-flex items-center gap-1 hover:text-blue-700">
                  <svg className="w-3.5 h-3.5" fill="#3B82F6" stroke="none" viewBox="0 0 24 24"><path d="M20 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z"/></svg>
                  {email}
                </a>
              )}
            </div>
          </div>
        </div>
      )}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center h-16">
          <Link href="/" className="flex flex-col select-none shrink-0" style={{ color: colorPrincipal }} onDoubleClick={(e) => { e.preventDefault(); router.push('/admin/login'); }}>
            <span className="text-2xl font-bold flex items-center gap-3">{logo && <img src={logo} alt={nombreTienda} className="h-10" />}{nombreTienda}{nit && <span className="text-sm text-gray-400 font-normal">NIT {nit}</span>}</span>
            {slogan && <span className="text-sm font-normal opacity-80">{slogan}</span>}
          </Link>
          <div className="flex-1 flex items-center justify-center gap-6">
            {horario && (
              <span className="hidden lg:flex items-center gap-1.5 text-xs text-gray-500">
                <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
                {horario}
              </span>
            )}
            <div className="hidden sm:flex items-center gap-4">
              {navLinks}
            </div>
          </div>
          <div className="flex items-center gap-2 shrink-0">
            <Link href="/carrito" className="relative text-gray-700 hover:text-blue-600 font-medium group">
              <svg className="w-6 h-6 transition-transform duration-300 group-hover:scale-110 group-hover:text-blue-600 animate-[bounce_2s_ease-in-out_infinite]" viewBox="0 0 24 24" fill="currentColor"><path d="M7 18c-1.1 0-1.99.9-1.99 2S5.9 22 7 22s2-.9 2-2-.9-2-2-2zm10 0c-1.1 0-1.99.9-1.99 2S15.9 22 17 22s2-.9 2-2-.9-2-2-2zM7.17 12.75l.03-.12.9-1.63h7.45c.75 0 1.41-.41 1.75-1.03l3.86-7.01L19.42 4h-.01l-1.1 2-2.76 5H8.53l-.13-.27L6.16 6l-.95-2-.94-2H1v2h2l3.6 7.59-1.35 2.45c-.16.28-.25.61-.25.96 0 1.1.9 2 2 2h12v-2H7.42c-.14 0-.25-.11-.25-.25zM9 2l-.75 2h3.25l.75-2z"/></svg>
              {hydrated && cantidadItems > 0 && (
                <span className="absolute -top-2 -right-4 bg-red-500 text-white text-xs rounded-full h-5 w-5 flex items-center justify-center font-bold animate-[ping_1.5s_ease-in-out_infinite]">
                  {cantidadItems}
                </span>
              )}
            </Link>
            <button onClick={() => setMenuOpen(!menuOpen)} className="sm:hidden p-2 text-gray-700 hover:text-blue-600">
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                {menuOpen ? (
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                ) : (
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                )}
              </svg>
            </button>
          </div>
        </div>
        {menuOpen && (
          <div className="sm:hidden border-t border-gray-100 bg-white">
            <div className="px-4 py-3 space-y-1">
              <Link href="/" onClick={() => setMenuOpen(false)} className="block px-3 py-2.5 rounded-lg text-gray-700 hover:text-blue-600 hover:bg-blue-50 font-medium transition-colors">
                Productos
              </Link>
              {user && (
                <Link href="/admin" onClick={() => setMenuOpen(false)} className="block px-3 py-2.5 rounded-lg text-gray-700 hover:text-blue-600 hover:bg-blue-50 font-medium transition-colors">
                  Admin
                </Link>
              )}
              {hydrated && isLoggedIn ? (
                <>
                  <Link href="/mis-pedidos" onClick={() => setMenuOpen(false)} className="block px-3 py-2.5 rounded-lg text-gray-700 hover:text-blue-600 hover:bg-blue-50 font-medium transition-colors">
                    Mis pedidos
                  </Link>
                  <button onClick={handleLogout} className="block w-full text-left px-3 py-2.5 rounded-lg text-gray-500 hover:text-red-600 hover:bg-red-50 font-medium transition-colors">
                    Salir
                  </button>
                </>
              ) : (
                <Link href="/ingresar" onClick={() => setMenuOpen(false)} className="block px-3 py-2.5 rounded-lg text-gray-700 hover:text-blue-600 hover:bg-blue-50 font-medium transition-colors">
                  Entrar
                </Link>
              )}
            </div>
          </div>
        )}
      </div>
    </header>
  );
}
