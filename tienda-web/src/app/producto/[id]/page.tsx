'use client';

import { use, useState, useEffect, useMemo } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { api, type Producto, type Variante } from '@/lib/api';
import { useCarrito } from '@/lib/carrito-context';
import { useToast } from '@/lib/toast-context';
import { formatCOP } from '@/lib/formats';

interface ColorGroup {
  nombre: string;
  colorHex: string;
  tallas: Variante[];
}

function agruparPorColor(variantes: Variante[]): ColorGroup[] {
  const map = new Map<string, ColorGroup>();
  for (const v of variantes) {
    const key = `${v.nombre}|${v.colorHex}`;
    if (!map.has(key)) {
      map.set(key, { nombre: v.nombre, colorHex: v.colorHex, tallas: [] });
    }
    map.get(key)!.tallas.push(v);
  }
  return Array.from(map.values());
}

export default function ProductoPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const { agregar } = useCarrito();
  const { toast } = useToast();
  const [producto, setProducto] = useState<Producto | null>(null);
  const [relacionados, setRelacionados] = useState<Producto[]>([]);
  const [variantes, setVariantes] = useState<Variante[]>([]);
  const [colorSel, setColorSel] = useState<ColorGroup | null>(null);
  const [tallaSel, setTallaSel] = useState<Variante | null>(null);
  const [cargando, setCargando] = useState(true);
  const [cantidad, setCantidad] = useState(1);

  const grupos = useMemo(() => agruparPorColor(variantes), [variantes]);
  const esSoloTalla = useMemo(() => variantes.length > 0 && variantes.every(v => v.nombre === 'Única' && v.colorHex === '#9E9E9E'), [variantes]);
  const tieneTallas = useMemo(() => variantes.some(v => v.talla && v.talla.trim() !== ''), [variantes]);

  useEffect(() => {
    const fetchProducto = async () => {
      setCargando(true);
      try {
        const { producto: p, variantes: v, relacionados: rel } = await api.productos.getDetalle(parseInt(id));
        setProducto(p);
        setVariantes(v);
        setRelacionados(rel);
      } catch { } finally {
        setCargando(false);
      }
    };
    fetchProducto();
  }, [id]);

  useEffect(() => {
    if (esSoloTalla) {
      setColorSel(null);
      setTallaSel(variantes[0] || null);
    } else if (grupos.length > 0) {
      setColorSel(grupos[0]);
      setTallaSel(null);
    }
  }, [grupos, esSoloTalla, variantes]);

  useEffect(() => {
    if (colorSel) {
      const first = colorSel.tallas[0];
      setTallaSel(first || null);
    }
  }, [colorSel]);

  const varianteSel = tallaSel;

  if (cargando) {
    return (
      <div className="max-w-4xl mx-auto py-12">
        <div className="animate-pulse space-y-6">
          <div className="h-8 bg-gray-200 rounded w-1/3" />
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div className="aspect-square bg-gray-200 rounded-xl" />
            <div className="space-y-4">
              <div className="h-6 bg-gray-200 rounded w-2/3" />
              <div className="h-4 bg-gray-200 rounded w-1/4" />
              <div className="h-20 bg-gray-200 rounded" />
              <div className="h-10 bg-gray-200 rounded w-1/3" />
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!producto) {
    return (
      <div className="text-center py-20 max-w-sm mx-auto">
        <svg className="w-20 h-20 mx-auto text-gray-200 mb-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
        <h2 className="text-xl font-bold text-gray-900 mb-1">Producto no encontrado</h2>
        <p className="text-gray-500 text-sm mb-6">El producto que buscas no existe o fue eliminado.</p>
        <Link href="/" className="inline-block bg-blue-600 text-white px-6 py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700 transition-colors">
          Ver todos los productos
        </Link>
      </div>
    );
  }

  const handleAgregar = () => {
    agregar(producto, cantidad, varianteSel ?? undefined);
    const parts = [producto.nombre];
    if (varianteSel) {
      if (varianteSel.nombre === 'Única' && varianteSel.talla) {
        parts.push(`(Talla ${varianteSel.talla})`);
      } else if (varianteSel.nombre === 'Única') {
        parts.push(`(Única)`);
      } else {
        parts.push(`(${varianteSel.nombre}`);
        if (varianteSel.talla) parts.push(`Talla ${varianteSel.talla})`);
        else parts.push(')');
      }
    }
    toast(`${parts.join(' ')} agregado al carrito`);
  };

  const stockActual = varianteSel?.stock ?? producto?.stock ?? 0;

  return (
    <div className="max-w-4xl mx-auto">
      <nav className="text-sm text-gray-500 mb-6">
        <Link href="/" className="hover:text-blue-600 transition-colors">Inicio</Link>
        <span className="mx-2">/</span>
        <span className="text-gray-900">{producto.nombre}</span>
      </nav>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-12">
        <div className="bg-white rounded-xl border border-gray-100 p-4">
          <div className="relative w-full aspect-square">
          {(() => {
            const imgUrl = varianteSel?.imagenUrl || producto.imagenUrl;
            return imgUrl ? (
              <Image key={varianteSel?.id || producto.id} src={imgUrl} alt={producto.nombre} fill sizes="(max-width: 768px) 100vw, 50vw" className="object-contain transition-opacity duration-300" priority />
            ) : (
              <div className="w-full h-full bg-gradient-to-br from-gray-100 to-gray-200 rounded-lg flex items-center justify-center text-gray-300">
                <svg className="w-24 h-24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="m20 7-8-4-8 4m16 0-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                </svg>
              </div>
            );
          })()}
          </div>
        </div>

        <div className="flex flex-col justify-center">
          <span className="text-xs font-semibold text-blue-600 bg-blue-50 px-2 py-0.5 rounded-full w-fit mb-3 uppercase tracking-wide">
            {producto.nombreCategoria}
          </span>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">{producto.nombre}</h1>
          {producto.descripcion && (
            <p className="text-gray-600 leading-relaxed mb-4">{producto.descripcion}</p>
          )}
          <p className="text-3xl font-bold text-gray-900 mb-4">{formatCOP(producto.precioVenta)}</p>

          <div className="flex items-center gap-2 mb-6">
            <span className={`text-sm font-medium ${stockActual > 0 ? 'text-emerald-600' : 'text-red-500'}`}>
              {stockActual > 0 ? `${stockActual} unidades disponibles` : 'Agotado'}
            </span>
            {stockActual > 0 && stockActual <= producto.stockMinimo && (
              <span className="text-xs bg-amber-100 text-amber-800 px-2 py-0.5 rounded-full font-medium">Pocas unidades</span>
            )}
          </div>

          {producto.stock > 0 && variantes.length > 0 && (esSoloTalla ? (
            <div className="mb-6">
              <p className="text-sm font-medium text-gray-700 mb-2">Talla: <span className="text-gray-900">{tallaSel?.talla || 'Única'}</span></p>
              <div className="flex gap-2 flex-wrap">
                {variantes.map(v => (
                  <button
                    key={v.id}
                    onClick={() => setTallaSel(v)}
                    className={`min-w-[2.75rem] px-3 py-1.5 rounded-lg border-2 text-sm font-medium transition-all ${
                      tallaSel?.id === v.id
                        ? 'border-gray-900 bg-gray-900 text-white shadow-md'
                        : 'border-gray-300 text-gray-700 hover:border-gray-500 hover:bg-gray-50'
                    } ${(v.stock ?? 0) <= 0 ? 'opacity-40 line-through cursor-not-allowed' : ''}`}
                    disabled={(v.stock ?? 0) <= 0}
                  >
                    {v.talla || 'Única'}
                  </button>
                ))}
              </div>
            </div>
          ) : grupos.length > 0 && (
            <div className="mb-6 space-y-4">
              <div>
                <p className="text-sm font-medium text-gray-700 mb-2">Color: <span className="text-gray-900">{colorSel?.nombre}</span></p>
                <div className="flex gap-2 flex-wrap">
                  {grupos.map(g => (
                    <button
                      key={`${g.nombre}|${g.colorHex}`}
                      onClick={() => { setColorSel(g); setTallaSel(g.tallas[0] || null); }}
                      className={`w-9 h-9 rounded-full border-2 transition-all ${colorSel === g ? 'border-gray-900 scale-110 shadow-md' : 'border-gray-200 hover:scale-105'}`}
                      style={{ backgroundColor: g.colorHex }}
                      title={g.nombre}
                    />
                  ))}
                </div>
              </div>

              {tieneTallas && colorSel && colorSel.tallas.length > 0 && (
                <div>
                  <p className="text-sm font-medium text-gray-700 mb-2">Talla: <span className="text-gray-900">{tallaSel?.talla || 'Única'}</span></p>
                  <div className="flex gap-2 flex-wrap">
                    {colorSel.tallas.map(v => (
                      <button
                        key={v.id}
                        onClick={() => setTallaSel(v)}
                        className={`min-w-[2.75rem] px-3 py-1.5 rounded-lg border-2 text-sm font-medium transition-all ${
                          tallaSel?.id === v.id
                            ? 'border-gray-900 bg-gray-900 text-white shadow-md'
                            : 'border-gray-300 text-gray-700 hover:border-gray-500 hover:bg-gray-50'
                        } ${(v.stock ?? 0) <= 0 ? 'opacity-40 line-through cursor-not-allowed' : ''}`}
                        disabled={(v.stock ?? 0) <= 0}
                      >
                        {v.talla || 'Única'}
                      </button>
                    ))}
                  </div>
                </div>
              )}
            </div>
          ))}

          {producto.stock > 0 && (
            <>
              <div className="flex items-center gap-4 mb-6">
                <button
                  onClick={() => setCantidad(Math.max(1, cantidad - 1))}
                  className="w-10 h-10 rounded-full bg-gray-100 text-gray-700 text-xl font-bold hover:bg-gray-200 transition-colors flex items-center justify-center"
                >
                  −
                </button>
                <input
                  type="number"
                  min={1}
                  max={stockActual}
                  value={cantidad}
                  onChange={e => {
                    const val = parseInt(e.target.value) || 1;
                    setCantidad(Math.min(Math.max(1, val), stockActual));
                  }}
                  className="w-16 text-center text-lg font-bold border border-gray-300 rounded-lg py-1 [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
                />
                <button
                  onClick={() => setCantidad(Math.min(stockActual, cantidad + 1))}
                  className="w-10 h-10 rounded-full bg-gray-100 text-gray-700 text-xl font-bold hover:bg-gray-200 transition-colors flex items-center justify-center"
                >
                  +
                </button>
              </div>

              <button
                onClick={handleAgregar}
                className="w-full bg-gray-900 text-white py-3.5 rounded-xl font-semibold text-lg hover:bg-gray-800 transition-colors active:scale-[0.98]"
              >
                Agregar al carrito — {formatCOP(producto.precioVenta * cantidad)}
              </button>
            </>
          )}
        </div>
      </div>

      {relacionados.length > 0 && (
        <div className="mb-12">
          <h2 className="text-xl font-bold text-gray-900 mb-4">Productos relacionados</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {relacionados.map(r => (
              <Link
                key={r.id}
                href={`/producto/${r.id}`}
                className="bg-white rounded-xl border border-gray-100 p-3 hover:shadow-lg transition-all duration-200 group"
              >
                <div className="relative aspect-square bg-gray-50 rounded-lg overflow-hidden mb-2">
                  {r.imagenUrl ? (
                    <Image src={r.imagenUrl} alt={r.nombre} fill sizes="(max-width: 640px) 50vw, 25vw" className="object-contain p-2 group-hover:scale-105 transition-transform duration-300" />
                  ) : (
                    <div className="w-full h-full flex items-center justify-center bg-gradient-to-br from-gray-100 to-gray-200 text-gray-300">
                      <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="m20 7-8-4-8 4m16 0-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                      </svg>
                    </div>
                  )}
                </div>
                <p className="text-sm font-medium text-gray-900 truncate group-hover:text-blue-600 transition-colors">{r.nombre}</p>
                <p className="text-sm font-bold text-gray-900 mt-1">{formatCOP(r.precioVenta)}</p>
              </Link>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
