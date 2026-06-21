'use client';

import { useEffect, useState, useMemo } from 'react';
import { api, type Producto, type Variante } from '@/lib/api';

const PAGE_SIZE = 20;

export default function ImagenesPage() {
  const [productos, setProductos] = useState<Producto[]>([]);
  const [variantes, setVariantes] = useState<Variante[]>([]);
  const [cargando, setCargando] = useState(true);
  const [filtro, setFiltro] = useState<'todos' | 'sin-imagen'>('sin-imagen');
  const [busqueda, setBusqueda] = useState('');
  const [pagina, setPagina] = useState(0);
  const [uploading, setUploading] = useState<Record<string, boolean>>({});

  useEffect(() => {
    Promise.all([api.productos.getAll(), api.productos.variantes.getAll()])
      .then(([prods, vars]) => {
        setProductos(prods);
        setVariantes(vars);
      })
      .catch(() => {})
      .finally(() => setCargando(false));
  }, []);

  const variantesPorProducto = useMemo(() =>
    variantes.reduce<Record<number, Variante[]>>((acc, v) => {
      (acc[v.productoId] ??= []).push(v);
      return acc;
    }, {}),
    [variantes]
  );

  const filtered = useMemo(() => {
    const q = busqueda.toLowerCase().trim();
    const porBusqueda = q
      ? productos.filter(p => p.nombre.toLowerCase().includes(q) || (p.codigoBarras || '').toLowerCase().includes(q))
      : productos;
    if (filtro === 'sin-imagen') {
      return porBusqueda.filter(p =>
        !p.imagenUrl || (variantesPorProducto[p.id] || []).some(v => !v.imagenUrl)
      );
    }
    return porBusqueda;
  }, [productos, variantesPorProducto, filtro, busqueda]);

  const totalPaginas = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paginaSegura = Math.min(pagina, totalPaginas - 1);
  const paginaActiva = pagina !== paginaSegura ? 0 : pagina;
  const page = filtered.slice(paginaActiva * PAGE_SIZE, (paginaActiva + 1) * PAGE_SIZE);

  useEffect(() => { setPagina(0); }, [filtro]);

  const uploadProducto = async (id: number, file: File) => {
    const key = `p-${id}`;
    setUploading(u => ({ ...u, [key]: true }));
    try {
      const url = await api.storage.upload(file);
      const actual = await api.productos.getById(id);
      await api.productos.update(id, { ...actual, imagenUrl: url });
      setProductos(prev => prev.map(p => p.id === id ? { ...p, imagenUrl: url } : p));
    } catch (e: any) {
      alert(e?.message || 'Error al subir imagen');
    } finally {
      setUploading(u => ({ ...u, [key]: false }));
    }
  };

  const uploadVariante = async (v: Variante, file: File) => {
    const key = `v-${v.id}`;
    setUploading(u => ({ ...u, [key]: true }));
    try {
      const url = await api.storage.upload(file);
      await api.productos.variantes.update(v.id, { ...v, imagenUrl: url });
      setVariantes(prev => prev.map(x => x.id === v.id ? { ...x, imagenUrl: url } : x));
    } catch (e: any) {
      alert(e?.message || 'Error al subir imagen');
    } finally {
      setUploading(u => ({ ...u, [key]: false }));
    }
  };

  if (cargando) return <div className="text-gray-500">Cargando...</div>;

  const sinImagenCount = productos.filter(p =>
    !p.imagenUrl || (variantesPorProducto[p.id] || []).some(v => !v.imagenUrl)
  ).length;

  return (
    <div>
      <div className="flex items-center justify-between gap-2 mb-2">
        <h1 className="text-base font-bold">Imagenes</h1>
        <div className="flex gap-1.5">
          <button onClick={() => setFiltro('sin-imagen')} className={`px-2 py-1 rounded text-[11px] font-medium transition-colors ${filtro === 'sin-imagen' ? 'bg-blue-600 text-white' : 'bg-gray-200 text-gray-700 hover:bg-gray-300'}`}>
            Sin imagen ({sinImagenCount})
          </button>
          <button onClick={() => setFiltro('todos')} className={`px-2 py-1 rounded text-[11px] font-medium transition-colors ${filtro === 'todos' ? 'bg-blue-600 text-white' : 'bg-gray-200 text-gray-700 hover:bg-gray-300'}`}>
            Todos ({productos.length})
          </button>
        </div>
      </div>

      <div className="relative mb-2">
        <svg className="absolute left-2.5 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
        <input
          type="text"
          placeholder="Buscar producto por nombre o codigo..."
          value={busqueda}
          onChange={e => { setBusqueda(e.target.value); setPagina(0); }}
          className="w-full pl-8 pr-3 py-1.5 border border-gray-300 rounded text-xs focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      {page.length === 0 && (
        <div className="text-center py-20">
          <svg className="w-16 h-16 mx-auto text-gray-200 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          <p className="text-gray-400 text-sm">Todos los productos y variantes tienen imagen</p>
        </div>
      )}

      <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-3">
        {page.map(p => {
          const vars = variantesPorProducto[p.id] || [];
          const keyP = `p-${p.id}`;
          return (
            <div key={p.id} className="bg-white rounded-lg shadow-sm border border-gray-100 p-2 flex flex-col items-center text-center">
              <div className="w-full aspect-square rounded-lg bg-gray-50 overflow-hidden mb-1.5 flex items-center justify-center">
                {p.imagenUrl ? (
                  <img src={p.imagenUrl} alt={p.nombre} className="w-full h-full object-contain" />
                ) : (
                  <svg className="w-8 h-8 text-gray-200" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                )}
              </div>
              <p className="text-[11px] font-medium text-gray-900 truncate w-full leading-tight">{p.nombre}</p>
              <p className="text-[9px] text-gray-400 mb-1.5">#{p.codigoBarras || p.id}</p>
              <label className={`w-full text-center py-1 rounded-md text-[11px] font-medium cursor-pointer transition-colors ${uploading[keyP] ? 'bg-blue-100 text-blue-400' : 'bg-blue-50 text-blue-600 hover:bg-blue-100'}`}>
                {uploading[keyP] ? 'Subiendo...' : p.imagenUrl ? 'Cambiar foto' : 'Subir foto'}
                <input type="file" accept="image/*" className="hidden" disabled={uploading[keyP]} onChange={e => { const f = e.target.files?.[0]; if (f) uploadProducto(p.id, f); }} />
              </label>
              {vars.length > 0 && (
                <div className="w-full mt-2 pt-2 border-t border-gray-100 space-y-1">
                  <p className="text-[9px] text-gray-400 font-medium uppercase tracking-wide">{vars.length} tono{vars.length > 1 && 's'}</p>
                  {vars.map(v => {
                    const keyV = `v-${v.id}`;
                    return (
                      <div key={v.id} className="flex items-center gap-1.5">
                        <div className="w-3 h-3 rounded-full border border-gray-200 shrink-0" style={{ backgroundColor: v.colorHex }} />
                        <span className="text-[9px] text-gray-500 truncate flex-1 text-left">{v.nombre}</span>
                        {v.imagenUrl ? (
                          <img src={v.imagenUrl} alt={v.nombre} className="w-5 h-5 rounded object-cover border border-gray-200" />
                        ) : (
                          <label className={`text-[9px] cursor-pointer ${uploading[keyV] ? 'text-blue-400' : 'text-blue-600 hover:underline'}`}>
                            {uploading[keyV] ? '...' : '+ foto'}
                            <input type="file" accept="image/*" className="hidden" disabled={uploading[keyV]} onChange={e => { const f = e.target.files?.[0]; if (f) uploadVariante(v, f); }} />
                          </label>
                        )}
                      </div>
                    );
                  })}
                </div>
              )}
            </div>
          );
        })}
      </div>

      {totalPaginas > 1 && (
        <div className="flex items-center justify-center gap-1.5 mt-4">
          <button onClick={() => setPagina(p => Math.max(0, p - 1))} disabled={paginaActiva === 0} className="px-2 py-1 rounded text-[11px] bg-gray-200 text-gray-700 disabled:opacity-30 hover:bg-gray-300 transition-colors">
            ← Ant
          </button>
          <span className="text-[11px] text-gray-500">{paginaActiva + 1} / {totalPaginas}</span>
          <button onClick={() => setPagina(p => Math.min(totalPaginas - 1, p + 1))} disabled={paginaActiva >= totalPaginas - 1} className="px-2 py-1 rounded text-[11px] bg-gray-200 text-gray-700 disabled:opacity-30 hover:bg-gray-300 transition-colors">
            Sig →
          </button>
        </div>
      )}
    </div>
  );
}
