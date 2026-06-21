'use client';

import { useState, FormEvent, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { api, Categoria } from '@/lib/api';

export default function NuevoProducto() {
  const router = useRouter();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [form, setForm] = useState({ codigoBarras: '', nombre: '', descripcion: '', categoriaId: 0, precioCompra: 0, precioVenta: 0, stock: 0, stockMinimo: 0, imagenUrl: '', orden: 0 });
  const [uploading, setUploading] = useState(false);

  useEffect(() => {
    api.categorias.getAll().then(setCategorias).catch(() => {});
  }, []);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function handleFileSelect(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploading(true);
    try {
      const url = await api.storage.upload(file);
      setForm(prev => ({ ...prev, imagenUrl: url }));
    } catch (err: any) {
      setError(err.message);
    } finally {
      setUploading(false);
    }
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    if (!form.nombre.trim()) { setError('El nombre es obligatorio'); return; }
    if (form.categoriaId === 0) { setError('Seleccione una categoria'); return; }
    setLoading(true);
    try {
      await api.productos.create({ ...form, categoriaId: Number(form.categoriaId) });
      router.push('/admin/productos');
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="max-w-2xl">
      <h1 className="text-2xl font-bold mb-6">Nuevo Producto</h1>
      <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow p-6 space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">Nombre *</label>
            <input value={form.nombre} onChange={e => setForm({ ...form, nombre: e.target.value })} className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2" required />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">Codigo de Barras</label>
            <input value={form.codigoBarras} onChange={e => setForm({ ...form, codigoBarras: e.target.value })} className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2" />
          </div>
          <div className="col-span-2">
            <label className="block text-sm font-medium text-gray-700">Descripcion</label>
            <textarea value={form.descripcion} onChange={e => setForm({ ...form, descripcion: e.target.value })} className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2" rows={2} />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">Categoria *</label>
            <select value={form.categoriaId} onChange={e => setForm({ ...form, categoriaId: Number(e.target.value) })} className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2" required>
              <option value={0}>Seleccione...</option>
              {categorias?.map(c => <option key={c.id} value={c.id}>{c.nombre}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">Stock</label>
            <input type="number" min={0} value={form.stock} onChange={e => setForm({ ...form, stock: Number(e.target.value) })} className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">Precio Compra</label>
            <input type="number" min={0} step="0.01" value={form.precioCompra} onChange={e => setForm({ ...form, precioCompra: Number(e.target.value) })} className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">Precio Venta</label>
            <input type="number" min={0} step="0.01" value={form.precioVenta} onChange={e => setForm({ ...form, precioVenta: Number(e.target.value) })} className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2" />
          </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Stock Minimo</label>
              <input type="number" min={0} value={form.stockMinimo} onChange={e => setForm({ ...form, stockMinimo: Number(e.target.value) })} className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Orden</label>
              <input type="number" min={0} value={form.orden} onChange={e => setForm({ ...form, orden: Number(e.target.value) })} className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2" />
              <p className="text-[10px] text-gray-400 mt-0.5">Menor numero = aparece primero</p>
            </div>
            <div className="col-span-2">
            <label className="block text-sm font-medium text-gray-700">Imagen</label>
            <div className="flex gap-2 mt-1">
              <input ref={fileInputRef} type="file" accept="image/*" onChange={handleFileSelect} className="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-medium file:bg-blue-50 file:text-blue-600 hover:file:bg-blue-100" />
              {uploading && <span className="text-blue-600 text-sm self-center">Subiendo...</span>}
            </div>
            {form.imagenUrl && <img src={form.imagenUrl} alt="Preview" className="mt-2 h-24 w-24 object-cover rounded" />}
            <input value={form.imagenUrl} onChange={e => setForm({ ...form, imagenUrl: e.target.value })} className="mt-2 block w-full border border-gray-300 rounded-md px-3 py-2 text-sm" placeholder="O pega una URL manualmente" />
          </div>
        </div>
        {error && <p className="text-red-600 text-sm">{error}</p>}
        <div className="flex gap-3">
          <button type="submit" disabled={loading} className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 disabled:opacity-50">
            {loading ? 'Guardando...' : 'Guardar'}
          </button>
          <button type="button" onClick={() => router.back()} className="bg-gray-200 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-300">Cancelar</button>
        </div>
      </form>
    </div>
  );
}
