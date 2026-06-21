'use client';

import { useState, useEffect, useCallback } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
import { api, type Producto, type Categoria } from '@/lib/api';
import { useApi } from '@/lib/useApi';
import { ApiBoundary } from './ApiBoundary';
import ProductCard from './ProductCard';

const ITEMS_PER_PAGE = 20;

export default function ProductList() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [categoriaActiva, setCategoriaActiva] = useState<number | null>(
    searchParams.get('cat') ? Number(searchParams.get('cat')) : null
  );
  const [searchTerm, setSearchTerm] = useState('');
  const [pagina, setPagina] = useState(1);

  const actualizarURL = useCallback((cat: number | null, page: number, scroll = false) => {
    const params = new URLSearchParams();
    if (cat !== null) params.set('cat', String(cat));
    if (page > 1) params.set('page', String(page));
    const qs = params.toString();
    router.replace(qs ? `/?${qs}` : '/', { scroll: false });
    if (scroll) document.getElementById('productos')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }, [router]);

  useEffect(() => {
    const cat = searchParams.get('cat');
    const page = searchParams.get('page');
    if (cat !== null) setCategoriaActiva(Number(cat));
    else setCategoriaActiva(null);
    if (page !== null) setPagina(Number(page));
  }, [searchParams]);

  useEffect(() => { setPagina(1); }, [categoriaActiva, searchTerm]);

  const categoriasQuery = useApi<Categoria[]>(() => api.categorias.getAll(), []);
  const productosQuery = useApi<Producto[]>(
    () => categoriaActiva !== null
      ? api.productos.getByCategoria(categoriaActiva)
      : api.productos.getAll(),
    [categoriaActiva]
  );

  const filtered = (productosQuery.data || []).filter(p =>
    !searchTerm.trim() || p.nombre.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const totalPaginas = Math.max(1, Math.ceil(filtered.length / ITEMS_PER_PAGE));
  const paginados = filtered.slice((pagina - 1) * ITEMS_PER_PAGE, pagina * ITEMS_PER_PAGE);

  return (
    <div id="productos">
      <div className="mb-8 flex justify-center">
        <div className="relative w-full max-w-lg">
          <svg className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
          <input
            type="text"
            placeholder="Buscar productos..."
            autoComplete="off"
            value={searchTerm}
            onChange={e => { setSearchTerm(e.target.value); actualizarURL(categoriaActiva, 1); }}
            className="w-full pl-12 pr-4 py-3.5 border-2 border-blue-400 rounded-xl focus:outline-none focus:ring-4 focus:ring-blue-200 focus:border-blue-600 text-base shadow-md bg-white"
          />
        </div>
      </div>

      <div className="flex gap-2 mb-8 overflow-x-auto pb-2">
        <button
          onClick={() => { setCategoriaActiva(null); setSearchTerm(''); actualizarURL(null, 1); }}
          className={`px-4 py-2 rounded-full text-sm font-medium whitespace-nowrap transition-colors ${
            categoriaActiva === null
              ? 'bg-blue-600 text-white'
              : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
          }`}
        >
          Todos
        </button>
        {(categoriasQuery.data || []).map(cat => (
          <button
            key={cat.id}
            onClick={() => { setCategoriaActiva(cat.id); actualizarURL(cat.id, 1); }}
            className={`px-4 py-2 rounded-full text-sm font-medium whitespace-nowrap transition-colors ${
              categoriaActiva === cat.id
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            {cat.nombre}
          </button>
        ))}
      </div>

      <ApiBoundary loading={productosQuery.loading} error={productosQuery.error} onRetry={productosQuery.refetch}>
        {filtered.length === 0 ? (
          <div className="text-center py-20 max-w-sm mx-auto">
            <svg className="w-20 h-20 mx-auto text-gray-200 mb-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
            </svg>
            <h3 className="text-lg font-semibold text-gray-900 mb-1">
              {searchTerm ? 'Sin resultados para tu busqueda' : 'No hay productos disponibles'}
            </h3>
            <p className="text-gray-500 text-sm mb-6">
              {searchTerm
                ? `No encontramos "${searchTerm}". Prueba con otro termino.`
                : 'Aun no hay productos publicados. Vuelve pronto.'}
            </p>
            {searchTerm && (
              <button onClick={() => { setSearchTerm(''); actualizarURL(categoriaActiva, 1); }} className="bg-blue-600 text-white px-6 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 transition-colors">
                Ver todos los productos
              </button>
            )}
          </div>
        ) : (
          <>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
              {paginados.map(p => (
                <ProductCard key={p.id} producto={p} />
              ))}
            </div>
            {totalPaginas > 1 && (
              <div className="flex items-center justify-center gap-2 mt-10">
                <button
                  disabled={pagina <= 1}
                  onClick={() => { const np = Math.max(1, pagina - 1); setPagina(np); actualizarURL(categoriaActiva, np); }}
                  className="px-4 py-2 rounded-lg text-sm font-medium border transition-colors disabled:opacity-30 disabled:cursor-not-allowed border-gray-300 text-gray-700 hover:bg-gray-100"
                >
                  Anterior
                </button>
                {Array.from({ length: totalPaginas }, (_, i) => i + 1).map(p => (
                  <button
                    key={p}
                    onClick={() => { setPagina(p); actualizarURL(categoriaActiva, p, true); }}
                    className={`w-10 h-10 rounded-lg text-sm font-medium transition-colors ${
                      p === pagina
                        ? 'bg-blue-600 text-white'
                        : 'border border-gray-300 text-gray-700 hover:bg-gray-100'
                    }`}
                  >
                    {p}
                  </button>
                ))}
                <button
                  disabled={pagina >= totalPaginas}
                  onClick={() => { const np = Math.min(totalPaginas, pagina + 1); setPagina(np); actualizarURL(categoriaActiva, np, true); }}
                  className="px-4 py-2 rounded-lg text-sm font-medium border transition-colors disabled:opacity-30 disabled:cursor-not-allowed border-gray-300 text-gray-700 hover:bg-gray-100"
                >
                  Siguiente
                </button>
              </div>
            )}
          </>
        )}
      </ApiBoundary>
    </div>
  );
}
