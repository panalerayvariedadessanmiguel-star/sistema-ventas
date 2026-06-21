'use client';

import { useStore } from '@/lib/store-context';

export default function Footer() {
  const { nombreTienda, colorPrincipal, telefono, direccion, ciudad, email, sitioWeb, horario, whatsapp, nit, domicilioTiempoEstimado } = useStore();
  const hasContact = telefono || direccion || ciudad || email || sitioWeb || horario || whatsapp;

  return (
    <footer className="bg-white border-t py-12">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center mb-10">
          <h2 className="text-3xl font-bold" style={{ color: colorPrincipal }}>
            {nombreTienda}
          </h2>
          {nit && <p className="text-sm text-gray-400 mt-1">NIT {nit}</p>}
        </div>
        {hasContact && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 max-w-3xl mx-auto mb-8">
            {(direccion || ciudad) && (
              <div className="flex items-start gap-3 text-sm text-gray-600 min-w-0">
                <svg className="w-5 h-5 mt-0.5 shrink-0 text-red-500" fill="currentColor" viewBox="0 0 24 24"><path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z"/></svg>
                <span className="min-w-0 break-words">{[direccion, ciudad].filter(Boolean).join(', ')}</span>
              </div>
            )}
            {telefono && (
              <div className="flex items-center gap-3 text-sm min-w-0">
                <svg className="w-5 h-5 shrink-0 text-blue-500" fill="currentColor" viewBox="0 0 24 24"><path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"/></svg>
                <a href={`tel:${telefono}`} className="hover:text-blue-600 text-gray-600 min-w-0 break-words">{telefono}</a>
              </div>
            )}
            {whatsapp && (
              <div className="flex items-center gap-3 text-sm min-w-0">
                <svg className="w-5 h-5 shrink-0 text-green-500" viewBox="0 0 24 24"><path fill="currentColor" d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z"/></svg>
                <a href={`https://wa.me/57${whatsapp.replace(/[^0-9]/g, '').replace(/^(57|0)+/, '')}`} target="_blank" rel="noopener noreferrer" className="hover:text-green-700 text-gray-600 font-medium min-w-0 break-words">
                  WhatsApp
                </a>
              </div>
            )}
            {email && (
              <div className="flex items-center gap-3 text-sm min-w-0">
                <svg className="w-5 h-5 shrink-0 text-blue-500" fill="currentColor" viewBox="0 0 24 24"><path d="M20 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z"/></svg>
                <a href={`mailto:${email}`} className="hover:text-blue-600 text-gray-600 min-w-0 break-words">{email}</a>
              </div>
            )}
            {horario && (
              <div className="flex items-center gap-3 text-sm min-w-0">
                <svg className="w-5 h-5 shrink-0 text-amber-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
                <span className="text-gray-600 min-w-0 break-words">{horario}</span>
              </div>
            )}
            {sitioWeb && (
              <div className="flex items-center gap-3 text-sm min-w-0">
                <svg className="w-5 h-5 shrink-0 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9"/></svg>
                <a href={sitioWeb.startsWith('http') ? sitioWeb : `https://${sitioWeb}`} target="_blank" rel="noopener noreferrer" className="hover:text-blue-600 text-gray-600 min-w-0 break-words">{sitioWeb}</a>
              </div>
            )}
          </div>
        )}
        <p className="text-center text-yellow-700 text-sm font-medium mb-1">
          <svg className="w-4 h-4 inline align-text-bottom mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"/><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"/></svg>
          Solo entregas en Bogotá
        </p>
        {domicilioTiempoEstimado && (
          <p className="text-center text-yellow-600 text-xs mb-4">📬 Entrega estimada: {domicilioTiempoEstimado}</p>
        )}
        <p className="text-center text-sm text-gray-500 mt-8">
          &copy; {new Date().getFullYear()} {nombreTienda} - Todos los derechos reservados
        </p>
      </div>
    </footer>
  );
}
