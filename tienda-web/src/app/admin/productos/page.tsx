'use client';

import { useEffect, useState, useMemo } from 'react';
import Link from 'next/link';
import { api, Producto } from '@/lib/api';
import { formatCOP } from '@/lib/formats';
import { useToast } from '@/lib/toast-context';

const ITEMS_PER_PAGE = 10;

export default function AdminProductos() {
  const { toast } = useToast();
  const [productos, setProductos] = useState<Producto[]>([]);
  const [loading, setLoading] = useState(true);
  const [deleting, setDeleting] = useState<number | null>(null);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);

  function load() {
    setLoading(true);
    api.productos.getAll().then(setProductos).catch(() => {}).finally(() => setLoading(false));
  }

  useEffect(() => { load(); }, []);

  const filtered = useMemo(() => {
    if (!search.trim()) return productos;
    const q = search.toLowerCase();
    return productos.filter(p =>
      p.nombre.toLowerCase().includes(q) ||
      (p.codigoBarras && p.codigoBarras.toLowerCase().includes(q)) ||
      (p.nombreCategoria && p.nombreCategoria.toLowerCase().includes(q))
    );
  }, [productos, search]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / ITEMS_PER_PAGE));
  const paginated = filtered.slice((page - 1) * ITEMS_PER_PAGE, page * ITEMS_PER_PAGE);

  useEffect(() => { setPage(1); }, [search]);

  async function handleDelete(id: number) {
    if (!confirm('¿Esta seguro de eliminar este producto?')) return;
    setDeleting(id);
    try {
      await api.productos.delete(id);
      toast('Producto eliminado correctamente', 'success');
      load();
    } catch (err: any) {
      toast(err.message || 'Error al eliminar el producto', 'error');
    } finally {
      setDeleting(null);
    }
  }

  if (loading) return <div className="text-gray-500">Cargando...</div>;

  return (
    <div>
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 mb-6">
        <h1 className="text-2xl font-bold">Productos</h1>
        <div className="flex items-center gap-3 w-full sm:w-auto">
          <div className="relative flex-1 sm:flex-initial">
            <svg className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
            <input
              type="text"
              placeholder="Buscar producto..."
              value={search}
              onChange={e => setSearch(e.target.value)}
              className="pl-9 pr-3 py-2 border border-gray-300 rounded-md text-sm w-full sm:w-64 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <Link href="/admin/productos/nuevo" className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 text-sm whitespace-nowrap">
            + Nuevo Producto
          </Link>
        </div>
      </div>
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 text-left">
            <tr>
              <th className="px-4 py-3 font-medium">Imagen</th>
              <th className="px-4 py-3 font-medium">Nombre</th>
              <th className="px-4 py-3 font-medium">Codigo</th>
              <th className="px-4 py-3 font-medium">Categoria</th>
              <th className="px-4 py-3 font-medium">Precio Venta</th>
              <th className="px-4 py-3 font-medium">Stock</th>
              <th className="px-4 py-3 font-medium">Acciones</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {paginated.map(p => (
              <tr key={p.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  {p.imagenUrl ? (
                    <img src={p.imagenUrl} alt={p.nombre} className="w-10 h-10 rounded object-cover" />
                  ) : (
                    <div className="w-10 h-10 rounded bg-gradient-to-br from-gray-100 to-gray-200 flex items-center justify-center text-gray-300">
                      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="m20 7-8-4-8 4m16 0-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                      </svg>
                    </div>
                  )}
                </td>
                <td className="px-4 py-3">{p.nombre}</td>
                <td className="px-4 py-3 text-gray-500">{p.codigoBarras}</td>
                <td className="px-4 py-3">{p.nombreCategoria}</td>
                <td className="px-4 py-3">{formatCOP(p.precioVenta)}</td>
                <td className="px-4 py-3">
                  <span className={`${p.stock <= p.stockMinimo ? 'text-red-600 font-medium' : ''}`}>{p.stock}</span>
                </td>
                <td className="px-4 py-3 flex gap-2">
                  <Link href={`/admin/productos/${p.id}`} className="text-blue-600 hover:underline">Editar</Link>
                  <button onClick={() => handleDelete(p.id)} disabled={deleting === p.id} className="inline-flex items-center gap-1.5 text-red-600 hover:underline disabled:opacity-50 disabled:cursor-wait">
                    {deleting === p.id && <svg className="animate-spin w-3.5 h-3.5" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"/></svg>}
                    {deleting === p.id ? 'Eliminando...' : 'Eliminar'}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {filtered.length === 0 && (
          <p className="text-center text-gray-500 py-12">
            {search ? 'No se encontraron productos con ese criterio' : 'No hay productos registrados'}
          </p>
        )}
        {totalPages > 1 && (
          <div className="flex items-center justify-between px-4 py-3 border-t bg-gray-50 text-sm">
            <span className="text-gray-500">
              {filtered.length} producto{filtered.length !== 1 && 's'} — Página {page} de {totalPages}
            </span>
            <div className="flex gap-1">
              <button
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-3 py-1.5 rounded border border-gray-300 bg-white text-gray-700 hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
              >
                Anterior
              </button>
              {Array.from({ length: totalPages }, (_, i) => i + 1).map(p => (
                <button
                  key={p}
                  onClick={() => setPage(p)}
                  className={`px-3 py-1.5 rounded border text-sm ${p === page ? 'bg-blue-600 text-white border-blue-600' : 'border-gray-300 bg-white text-gray-700 hover:bg-gray-100'}`}
                >
                  {p}
                </button>
              ))}
              <button
                onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                className="px-3 py-1.5 rounded border border-gray-300 bg-white text-gray-700 hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
              >
                Siguiente
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
