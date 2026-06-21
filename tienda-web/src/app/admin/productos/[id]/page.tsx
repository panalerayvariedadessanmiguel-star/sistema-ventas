'use client';

import { useState, useEffect, FormEvent, useRef } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { api, Producto, Categoria, Variante } from '@/lib/api';

const COLOR_DICT: Record<string, string> = {
  // --- Genéricos ---
  linda: '#C62828', osado: '#D32F2F', nude: '#D7CCC8', cereza: '#B71C1C',
  coral: '#FF7F50', fresa: '#FF1744', frambuesa: '#C2185B', vino: '#880E4F',
  borgoña: '#880E4F', ciruela: '#4A148C', uva: '#6A1B9A', lavanda: '#CE93D8',
  lila: '#CE93D8', violeta: '#7B1FA2', purpura: '#6A1B9A', morado: '#6A1B9A',
  azul: '#1976D2', celeste: '#4FC3F7', turquesa: '#00BCD4', menta: '#A5D6A7',
  verde: '#388E3C', esmeralda: '#2E7D32', oliva: '#8D6E63', mostaza: '#FBC02D',
  amarillo: '#FDD835', dorado: '#FFD700', naranja: '#F57C00', durazno: '#FFCC80',
  salmon: '#FF8A65', rosa: '#F06292', rosado: '#F06292', melocoton: '#FFCC80',
  champan: '#FFF3E0', marfil: '#FFF8E1', beige: '#D7CCC8', crema: '#FFF9C4',
  blanco: '#FFFFFF', perla: '#F5F5F5', gris: '#9E9E9E', plata: '#B0BEC5',
  plateado: '#B0BEC5', negro: '#212121', cafe: '#5D4037', marron: '#5D4037',
  chocolate: '#3E2723', caramelo: '#8D6E63', miel: '#FFB300', cobre: '#BF360C',
  bronce: '#A1887F', topo: '#6D4C41', arena: '#BCAAA4', hueso: '#EFEBE9',
  // --- Masglo · GAMA ROJO ---
  ausente: '#B71C1C', 'sangre toro': '#660000', golosa: '#D32F2F',
  fiesta: '#E53935', fufurufa: '#C62828',   gomela: '#D50000', animada: '#E53935', insinuante: '#BF360C',
  // --- Masglo · GAMA ROSADO ---
  novia: '#F8BBD0', frances: '#FCE4EC', rebelde: '#F48FB1',
  bella: '#F8BBD0', actual: '#F06292', amable: '#F48FB1',
  amigable: '#F06292', amistosa: '#EC407A', atrevida: '#D81B60',
  candidata: '#F8BBD0', comica: '#F48FB1', ilusion: '#FCE4EC',
  imponente: '#E91E63', mimada: '#F48FB1', perfeccionista: '#FCE4EC',
  razonable: '#F8BBD0', tierna: '#F48FB1', campeona: '#E91E63',
  quisquillosa: '#EC407A', regia: '#F48FB1', fanatica: '#D81B60',
  burlona: '#F06292', talentosa: '#F48FB1', agradable: '#F8BBD0',
  atletica: '#E91E63', popular: '#F48FB1', seguidora: '#EC407A',
  baby: '#FCE4EC',
  // --- Masglo · GAMA MARRON ---
  matrimonio: '#E8C9B0', virginal: '#5D4037', 'primera dama': '#4E342E',
  expresiva: '#795548', nativa: '#6D4C41', intuitiva: '#5D4037',
  sofisticada: '#4E342E', materialista: '#3E2723', paciente: '#8D6E63',
  arisca: '#6D4C41', intrigante: '#5D4037', natural: '#A1887F',
  granizado: '#BCAAA4',
  // --- Masglo · GAMA VERDE ---
  abrumadora: '#2E7D32', adorable: '#43A047', activista: '#1B5E20',
  fresca: '#66BB6A',
  // --- Masglo · GAMA AZUL ---
  solidaria: '#1565C0', autentica: '#1976D2', tranquila: '#1E88E5',
  libre: '#0D47A1', ambientalista: '#1565C0', atractiva: '#1E88E5',
  // --- Masglo · GAMA MORADO ---
  amante: '#7B1FA2', tirana: '#6A1B9A', controladora: '#8E24AA',
  alborotada: '#9C27B0', timida: '#7B1FA2', seductora: '#6A1B9A',
  // --- Masglo · GAMA LILA ---
  soltera: '#CE93D8', perfecta: '#BA68C8',
  // --- Masglo · GAMA FUCSIA ---
  chic: '#D81B60', ajena: '#E91E63',
  // --- Masglo · GAMA BLANCO ---
  tiza: '#FAFAFA', ejecutiva: '#F5F5F5', nieve: '#FFFFFF',
  educada: '#F5F5F5', 'blanco nacar': '#FFF8E1', angelical: '#FFFDE7',
  // --- Masglo · GAMA AMARILLO ---
  astuta: '#FDD835', artesana: '#FBC02D', bailadora: '#FFEE58',
  insaciable: '#F9A825',
  // --- Masglo · GAMA GRIS ---
  'escarchado plata': '#BDBDBD', visionaria: '#9E9E9E', influencer: '#757575',
  // --- Masglo · GAMA VIOLETA ---
  querendona: '#7B1FA2', arriesgada: '#9575CD',
  // --- Nailen ---
  banana: '#FDD835', chicle: '#F48FB1', eclipse: '#263238', estacion: '#880E4F',
  festival: '#E53935', galactico: '#7C4DFF', magenta: '#E91E63',
  malva: '#BA68C8', marmol: '#E0E0E0', mirador: '#42A5F5',
  'oro rosa': '#F8BBD0', 'perla rosa': '#FCE4EC', poblado: '#66BB6A',
  nogal: '#4E342E', sandia: '#D32F2F', sepia: '#8D6E63', onix: '#212121',
};

function norm(s: string) {
  return s.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().trim();
}

function sugerirColor(nombre: string): string | null {
  const n = norm(nombre);
  if (COLOR_DICT[n]) return COLOR_DICT[n];
  for (const [key, hex] of Object.entries(COLOR_DICT)) {
    if (n.includes(norm(key))) return hex;
  }
  return null;
}

export default function EditarProducto() {
  const router = useRouter();
  const params = useParams();
  const id = Number(params.id);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [producto, setProducto] = useState<Producto | null>(null);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [form, setForm] = useState({ codigoBarras: '', nombre: '', descripcion: '', categoriaId: 0, precioCompra: 0, precioVenta: 0, stock: 0, stockMinimo: 0, imagenUrl: '', orden: 0, activo: true });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [uploading, setUploading] = useState(false);

  const [variantes, setVariantes] = useState<Variante[]>([]);
  const TALLAS = ['', 'XS', 'S', 'M', 'L', 'XL', 'XXL', 'XXXL', '4', '6', '8', '10', '12', '14', '16', 'Única'];
  const [vForm, setVForm] = useState({ nombre: '', colorHex: '#000000', talla: '' as string, stock: '' as string | number, imagenUrl: '' });
  const [editingV, setEditingV] = useState<number | null>(null);
  const [savingV, setSavingV] = useState(false);
  const [uploadingV, setUploadingV] = useState(false);
  const [soloTalla, setSoloTalla] = useState(false);

  const [loadingV, setLoadingV] = useState(false);

  function loadVariantes() {
    setLoadingV(true);
    api.productos.variantes.getByProducto(id)
      .then(setVariantes)
      .catch(() => {})
      .finally(() => setLoadingV(false));
  }

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

  async function handleVariantUpload(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploadingV(true);
    try {
      const url = await api.storage.upload(file);
      setVForm(prev => ({ ...prev, imagenUrl: url }));
    } catch (err: any) {
      setError(err.message);
    } finally {
      setUploadingV(false);
    }
  }

  useEffect(() => {
    Promise.all([api.productos.getById(id), api.categorias.getAll(), api.productos.variantes.getByProducto(id)])
      .then(([p, cats, v]) => {
        setProducto(p);
        setCategorias(cats);
        setVariantes(v);
        setForm({
          codigoBarras: p.codigoBarras,
          nombre: p.nombre,
          descripcion: p.descripcion,
          categoriaId: p.categoriaId,
          precioCompra: p.precioCompra,
          precioVenta: p.precioVenta,
          stock: p.stock,
          stockMinimo: p.stockMinimo,
          imagenUrl: p.imagenUrl,
          orden: p.orden,
          activo: p.activo,
        });
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [id]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    if (!form.nombre.trim()) { setError('El nombre es obligatorio'); return; }
    setSaving(true);
    try {
      await api.productos.update(id, { ...form, categoriaId: Number(form.categoriaId) });
      router.push('/admin/productos');
    } catch (err: any) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  async function handleSaveVariante() {
    const nombreFinal = soloTalla ? 'Única' : vForm.nombre.trim();
    if (!nombreFinal && !vForm.talla) return;
    setSavingV(true);
    try {
      const payload = {
        nombre: nombreFinal,
        colorHex: soloTalla ? '#9E9E9E' : vForm.colorHex,
        talla: vForm.talla || null,
        stock: vForm.stock === '' ? null : Number(vForm.stock),
        imagenUrl: vForm.imagenUrl || null,
      };
      if (editingV !== null) {
        await api.productos.variantes.update(editingV, payload);
      } else {
        await api.productos.variantes.create(id, payload);
      }
      setVForm({ nombre: '', colorHex: '#000000', talla: '', stock: '', imagenUrl: '' });
      setSoloTalla(false);
      setEditingV(null);
      loadVariantes();
    } catch (err: any) {
      alert(err.message);
    } finally {
      setSavingV(false);
    }
  }

  function handleEditVariante(v: Variante) {
    const esSoloTalla = v.nombre === 'Única' && v.colorHex === '#9E9E9E';
    setSoloTalla(esSoloTalla);
    setVForm({ nombre: esSoloTalla ? '' : v.nombre, colorHex: v.colorHex, talla: v.talla || '', stock: v.stock === null ? '' : v.stock, imagenUrl: v.imagenUrl || '' });
    setEditingV(v.id);
  }

  async function handleDeleteVariante(varianteId: number) {
    if (!confirm('Eliminar esta variante?')) return;
    try {
      await api.productos.variantes.delete(varianteId);
      loadVariantes();
    } catch (err: any) {
      alert(err.message);
    }
  }

  if (loading) return <div className="text-gray-500">Cargando...</div>;
  if (!producto) return <div className="text-red-600">Producto no encontrado</div>;

  return (
    <div className="max-w-4xl">
      <h1 className="text-2xl font-bold mb-6">Editar Producto: {producto.nombre}</h1>
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow p-6 space-y-4 h-fit">
          <h2 className="font-bold text-lg border-b pb-2">Informacion del Producto</h2>
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
                {categorias.map(c => <option key={c.id} value={c.id}>{c.nombre}</option>)}
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
            <div className="flex items-center gap-2">
              <input type="checkbox" id="activo" checked={form.activo} onChange={e => setForm({ ...form, activo: e.target.checked })} className="rounded" />
              <label htmlFor="activo" className="text-sm font-medium text-gray-700">Producto Activo</label>
            </div>
          </div>
          {error && <p className="text-red-600 text-sm">{error}</p>}
          <div className="flex gap-3">
            <button type="submit" disabled={saving} className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 disabled:opacity-50">
              {saving ? 'Guardando...' : 'Guardar Cambios'}
            </button>
            <button type="button" onClick={() => router.back()} className="bg-gray-200 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-300">Cancelar</button>
          </div>
        </form>

        <div className="bg-white rounded-lg shadow p-6 space-y-4 h-fit">
          <h2 className="font-bold text-lg border-b pb-2">Variantes (Colores)</h2>
          <p className="text-xs text-gray-500">Agrega variantes de color para que los clientes elijan en la web.</p>

          {variantes.length > 0 && (
            <div className="space-y-2">
              {variantes.map(v => (
                <div key={v.id} className="flex items-center gap-3 bg-gray-50 rounded-lg p-2.5">
                  {v.imagenUrl ? (
                    <img src={v.imagenUrl} alt={v.nombre} className="w-10 h-10 rounded-lg object-cover border border-gray-200 shrink-0" />
                  ) : v.nombre === 'Única' && v.colorHex === '#9E9E9E' ? (
                    <div className="w-7 h-7 rounded border border-gray-200 shrink-0 flex items-center justify-center text-xs text-gray-400 bg-gray-50">—</div>
                  ) : (
                    <div className="w-7 h-7 rounded-full border border-gray-200 shrink-0" style={{ backgroundColor: v.colorHex }} />
                  )}
                  <div className="flex-1 min-w-0">
                    {v.nombre === 'Única' && v.colorHex === '#9E9E9E' ? (
                      <p className="text-sm font-medium text-gray-900">{v.talla ? `Talla ${v.talla}` : 'Única'}{v.stock !== null ? ` (Stock: ${v.stock})` : ''}</p>
                    ) : (
                      <p className="text-sm font-medium text-gray-900">{v.nombre}{v.talla ? ` - Talla ${v.talla}` : ''}</p>
                    )}
                    {v.nombre !== 'Única' || v.colorHex !== '#9E9E9E' ? <p className="text-xs text-gray-400">{v.colorHex}{v.stock !== null ? ` | Stock: ${v.stock}` : ''}</p> : null}
                  </div>
                  <button onClick={() => handleEditVariante(v)} className="text-blue-600 text-xs hover:underline">Editar</button>
                  <button onClick={() => handleDeleteVariante(v.id)} className="text-red-600 text-xs hover:underline">Eliminar</button>
                </div>
              ))}
            </div>
          )}

          {loadingV && <p className="text-xs text-gray-400">Cargando variantes...</p>}

          <div className="border-t pt-4 space-y-3">
            <h3 className="text-sm font-medium text-gray-700">{editingV !== null ? 'Editar Variante' : 'Nueva Variante'}</h3>

            <label className="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" checked={soloTalla} onChange={e => { setSoloTalla(e.target.checked); if (e.target.checked) setVForm(p => ({ ...p, nombre: '', colorHex: '#9E9E9E' })); }} className="rounded" />
              <span className="text-xs font-medium text-gray-600">Solo talla (sin color)</span>
            </label>

            <div className="grid grid-cols-2 gap-3">
              {!soloTalla && (
                <div>
                  <label className="block text-xs font-medium text-gray-600">Nombre (ej: Rojo Cereza)</label>
                  <div className="flex gap-1 mt-1">
                    <input value={vForm.nombre} onChange={e => setVForm({ ...vForm, nombre: e.target.value })} className="block w-full border border-gray-300 rounded-md px-3 py-1.5 text-sm" placeholder="Nombre del color" />
                    <button type="button" onClick={() => { const h = sugerirColor(vForm.nombre); if (h) setVForm(p => ({ ...p, colorHex: h })); }} className="bg-gray-100 hover:bg-gray-200 text-xs text-gray-600 px-2 rounded-md border border-gray-300 shrink-0" title="Auto-color">🎨</button>
                  </div>
                </div>
              )}
              {!soloTalla && (
                <div>
                  <label className="block text-xs font-medium text-gray-600">Color</label>
                  <div className="flex gap-2 mt-1">
                    <input type="color" value={vForm.colorHex} onChange={e => setVForm({ ...vForm, colorHex: e.target.value })} className="w-9 h-9 rounded cursor-pointer border" />
                    <input value={vForm.colorHex} onChange={e => setVForm({ ...vForm, colorHex: e.target.value })} className="flex-1 border border-gray-300 rounded-md px-3 py-1.5 text-sm font-mono" placeholder="#FF0000" />
                  </div>
                </div>
              )}
              <div>
                <label className="block text-xs font-medium text-gray-600">Talla (opcional)</label>
                <select value={vForm.talla} onChange={e => setVForm({ ...vForm, talla: e.target.value })} className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-1.5 text-sm">
                  {TALLAS.map(t => <option key={t} value={t}>{t || 'Sin talla'}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600">Stock (opcional)</label>
                <input type="number" min={0} value={vForm.stock} onChange={e => setVForm({ ...vForm, stock: e.target.value === '' ? '' : Number(e.target.value) })} className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-1.5 text-sm" placeholder="Usar stock del producto" />
              </div>
              <div className="col-span-2">
                <label className="block text-xs font-medium text-gray-600">Imagen (opcional)</label>
                <div className="flex gap-2 mt-1">
                  <input type="file" accept="image/*" onChange={handleVariantUpload} className="block w-full text-sm text-gray-500 file:mr-3 file:py-1.5 file:px-3 file:rounded-md file:border-0 file:text-xs file:font-medium file:bg-blue-50 file:text-blue-600 hover:file:bg-blue-100" />
                  {uploadingV && <span className="text-blue-600 text-xs self-center">Subiendo...</span>}
                </div>
                {vForm.imagenUrl && <img src={vForm.imagenUrl} alt="Preview" className="mt-2 h-16 w-16 object-cover rounded" />}
                <input value={vForm.imagenUrl} onChange={e => setVForm({ ...vForm, imagenUrl: e.target.value })} className="mt-2 block w-full border border-gray-300 rounded-md px-3 py-1.5 text-sm" placeholder="O pega una URL manualmente" />
              </div>
            </div>
            <div className="flex gap-2">
              <button onClick={handleSaveVariante} disabled={savingV || (!soloTalla && !vForm.nombre.trim())} className="bg-blue-600 text-white px-3 py-1.5 rounded-md text-sm hover:bg-blue-700 disabled:opacity-50">
                {savingV ? 'Guardando...' : editingV !== null ? 'Actualizar' : 'Agregar'}
              </button>
              {editingV !== null && (
                <button onClick={() => { setVForm({ nombre: '', colorHex: '#000000', talla: '', stock: '', imagenUrl: '' }); setEditingV(null); }} className="bg-gray-200 text-gray-700 px-3 py-1.5 rounded-md text-sm hover:bg-gray-300">
                  Cancelar
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
