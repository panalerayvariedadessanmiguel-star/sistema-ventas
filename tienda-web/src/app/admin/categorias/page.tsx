'use client';

import { useEffect, useState } from 'react';
import { api, Categoria } from '@/lib/api';

export default function AdminCategorias() {
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [loading, setLoading] = useState(true);
  const [editId, setEditId] = useState<number | null>(null);
  const [nombre, setNombre] = useState('');
  const [descripcion, setDescripcion] = useState('');

  function load() {
    setLoading(true);
    api.categorias.getAll(true).then(setCategorias).catch(() => {}).finally(() => setLoading(false));
  }

  useEffect(() => { load(); }, []);

  async function handleCreate() {
    if (!nombre.trim()) return;
    try {
      await api.categorias.create({ nombre, descripcion });
      setNombre('');
      setDescripcion('');
      load();
    } catch (err: any) { alert(err.message); }
  }

  async function handleUpdate(id: number) {
    if (!nombre.trim()) return;
    try {
      await api.categorias.update(id, { nombre, descripcion, activo: true });
      setEditId(null);
      setNombre('');
      setDescripcion('');
      load();
    } catch (err: any) { alert(err.message); }
  }

  function startEdit(c: Categoria) {
    setEditId(c.id);
    setNombre(c.nombre);
    setDescripcion(c.descripcion);
  }

  if (loading) return <div className="text-gray-500">Cargando...</div>;

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">Categorias</h1>

      <div className="bg-white rounded-lg shadow p-4 mb-6">
        <h2 className="font-medium mb-3">{editId ? 'Editar Categoria' : 'Nueva Categoria'}</h2>
        <div className="flex gap-3 items-end">
          <div className="flex-1">
            <label className="block text-xs text-gray-500 mb-1">Nombre</label>
            <input value={nombre} onChange={e => setNombre(e.target.value)} className="block w-full border border-gray-300 rounded-md px-3 py-2 text-sm" placeholder="Nombre de la categoria" />
          </div>
          <div className="flex-1">
            <label className="block text-xs text-gray-500 mb-1">Descripcion</label>
            <input value={descripcion} onChange={e => setDescripcion(e.target.value)} className="block w-full border border-gray-300 rounded-md px-3 py-2 text-sm" placeholder="Descripcion" />
          </div>
          <button
            onClick={() => editId ? handleUpdate(editId) : handleCreate()}
            className="bg-blue-600 text-white px-4 py-2 rounded-md text-sm hover:bg-blue-700"
          >
            {editId ? 'Actualizar' : 'Agregar'}
          </button>
          {editId && (
            <button onClick={() => { setEditId(null); setNombre(''); setDescripcion(''); }} className="bg-gray-200 text-gray-700 px-4 py-2 rounded-md text-sm hover:bg-gray-300">
              Cancelar
            </button>
          )}
        </div>
      </div>

      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 text-left">
            <tr>
              <th className="px-4 py-3 font-medium">Nombre</th>
              <th className="px-4 py-3 font-medium">Descripcion</th>
              <th className="px-4 py-3 font-medium">Estado</th>
              <th className="px-4 py-3 font-medium">Acciones</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {categorias.map(c => (
              <tr key={c.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">{c.nombre}</td>
                <td className="px-4 py-3 text-gray-500">{c.descripcion}</td>
                <td className="px-4 py-3">
                  <span className={`px-2 py-1 rounded-full text-xs ${c.activo ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                    {c.activo ? 'Activo' : 'Inactivo'}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <button onClick={() => startEdit(c)} className="text-blue-600 hover:underline text-sm">Editar</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
