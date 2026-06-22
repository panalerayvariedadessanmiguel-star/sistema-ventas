'use client';

import Link from 'next/link';
import Image from 'next/image';
import { useState, useEffect, useMemo, useCallback } from 'react';
import type { Producto, Variante } from '@/lib/api';
import { api } from '@/lib/api';
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

interface Props {
  producto: Producto;
}

export default function ProductCard({ producto }: Props) {
  const { agregar } = useCarrito();
  const { toast } = useToast();
  const [showModal, setShowModal] = useState(false);
  const [cantidad, setCantidad] = useState(1);
  const [variantes, setVariantes] = useState<Variante[]>([]);
  const [varianteSel, setVarianteSel] = useState<Variante | null>(null);
  const [colorSel, setColorSel] = useState<ColorGroup | null>(null);
  const [loadingV, setLoadingV] = useState(false);

  const grupos = useMemo(() => agruparPorColor(variantes), [variantes]);
  const esSoloTalla = useMemo(() =>
    variantes.length > 0 && variantes.every(v => v.nombre === 'Única' && v.colorHex === '#9E9E9E'),
  [variantes]);
  const tieneTallas = useMemo(() =>
    variantes.some(v => v.talla && v.talla.trim() !== ''),
  [variantes]);

  const cargarVariantes = useCallback(() => {
    if (variantes.length > 0 || loadingV) return;
    setLoadingV(true);
    api.productos.variantes.getByProducto(producto.id)
      .then(v => {
        setVariantes(v);
        if (v.length > 0) {
          if (v.every(x => x.nombre === 'Única' && x.colorHex === '#9E9E9E')) {
            setVarianteSel(v[0]);
          } else {
            const g = agruparPorColor(v);
            if (g.length > 0) { setColorSel(g[0]); setVarianteSel(g[0].tallas[0] || null); }
          }
        }
      })
      .catch(() => {})
      .finally(() => setLoadingV(false));
  }, [producto.id, variantes.length, loadingV]);

  const stockActual = varianteSel?.stock ?? producto.stock;
  const imagenMostrar = producto.imagenUrl || variantes.find(v => v.imagenUrl)?.imagenUrl || '';

  const handleAgregar = () => {
    if (cantidad > 0 && cantidad <= stockActual) {
      agregar(producto, cantidad, varianteSel ?? undefined);
      const label = varianteSel
        ? esSoloTalla
          ? `(Talla ${varianteSel.talla || 'Única'})`
          : `${varianteSel.nombre}${varianteSel.talla ? ` - Talla ${varianteSel.talla}` : ''}`
        : '';
      toast(`${producto.nombre} ${label} agregado al carrito`);
      setShowModal(false);
      setCantidad(1);
      setVarianteSel(null);
      setColorSel(null);
    }
  };

  return (
    <>
      <div className="group bg-white rounded-xl shadow-sm hover:shadow-2xl hover:-translate-y-1 transition-all duration-300 overflow-hidden border border-gray-100 hover:border-blue-100 flex flex-col">
        <div className="relative overflow-hidden bg-gradient-to-br from-gray-50 to-gray-100 aspect-[1/1]">
          <Link href={`/producto/${producto.id}`}>
          {imagenMostrar ? (
            <Image
              src={imagenMostrar}
              alt={producto.nombre}
              fill
              sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 25vw"
              className="object-contain p-2 group-hover:scale-110 transition-transform duration-700 ease-out"
            />
          ) : (
            <div className="w-full h-full flex flex-col items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100 gap-2">
              <svg className="w-16 h-16 text-gray-200 group-hover:text-gray-300 transition-colors duration-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={0.5} d="m20 7-8-4-8 4m16 0-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
              </svg>
              <span className="text-xs text-gray-200 group-hover:text-gray-300 transition-colors duration-300 font-medium">Sin imagen</span>
            </div>
          )}
          <div className="absolute inset-0 bg-gradient-to-t from-black/0 via-black/0 to-black/0 group-hover:from-black/10 group-hover:via-transparent group-hover:to-transparent transition-all duration-500" />
          <div className="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-300">
            <span className="bg-white/90 text-gray-900 text-xs font-semibold px-4 py-2 rounded-full shadow-lg backdrop-blur-sm translate-y-2 group-hover:translate-y-0 transition-transform duration-300">
              Ver detalle
            </span>
          </div>
          </Link>
          {producto.stock <= 0 && (
            <div className="absolute inset-0 bg-black/50 flex items-center justify-center backdrop-blur-[2px]">
              <span className="bg-red-500 text-white px-4 py-1.5 rounded-full text-sm font-bold uppercase tracking-wider shadow-lg">Agotado</span>
            </div>
          )}
          {producto.stock > 0 && producto.stock <= producto.stockMinimo && (
            <div className="absolute top-2 left-2 animate-pulse">
              <span className="bg-amber-400 text-amber-900 px-2 py-0.5 rounded text-xs font-semibold shadow-sm">Pocas und.</span>
            </div>
          )}
        </div>
        <div className="p-4 flex flex-col flex-1">
          <div className="flex items-center justify-between mb-2">
            <span className="text-[11px] font-semibold text-blue-600 bg-blue-50 px-2 py-0.5 rounded-full uppercase tracking-wide">
              {producto.nombreCategoria}
            </span>
            {producto.stock > 0 && (
              <span className="text-[11px] text-emerald-600 font-medium">{producto.stock} und.</span>
            )}
          </div>
          <h3 className="font-semibold text-gray-900 leading-tight">
            <Link href={`/producto/${producto.id}`} className="hover:text-blue-600 transition-colors">
              {producto.nombre}
            </Link>
          </h3>
          {producto.descripcion && (
            <p className="mt-1 text-xs text-gray-400 line-clamp-2 leading-relaxed">{producto.descripcion}</p>
          )}
          {variantes.length > 1 && (
            <div className="flex items-center gap-1.5 mt-2">
              <div className="flex -space-x-1">
                {variantes.slice(0, 5).map(v => (
                  <div key={v.id} className="w-4 h-4 rounded-full border-2 border-white shadow-sm" style={{ backgroundColor: v.colorHex }} title={v.nombre} />
                ))}
              </div>
              <span className="text-[11px] text-gray-400">{variantes.length} tonos</span>
            </div>
          )}
          <div className="mt-auto pt-3 flex items-center justify-between">
            <span className="text-2xl font-bold text-gray-900 tracking-tight">
              {formatCOP(producto.precioVenta)}
            </span>
          </div>
          <button
            onClick={() => { setShowModal(true); cargarVariantes(); }}
            disabled={producto.stock <= 0}
            className="mt-3 w-full bg-gray-900 text-white py-2.5 px-4 rounded-lg text-sm font-medium hover:bg-gray-800 disabled:bg-gray-100 disabled:text-gray-300 disabled:cursor-not-allowed transition-all duration-200 active:scale-[0.97] hover:shadow-lg hover:shadow-gray-900/20"
          >
            {producto.stock > 0 ? 'Agregar al carrito' : 'No disponible'}
          </button>
        </div>
      </div>

      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm" onClick={() => { setShowModal(false); setCantidad(1); }}>
          <div className="bg-white rounded-2xl shadow-2xl p-6 w-80 mx-4 relative" onClick={e => e.stopPropagation()}>
            <button
              onClick={() => { setShowModal(false); setCantidad(1); }}
              className="absolute top-3 right-3 w-7 h-7 rounded-full bg-gray-100 text-gray-500 hover:bg-gray-200 hover:text-gray-700 transition-colors flex items-center justify-center text-sm font-bold"
            >
              ✕
            </button>
            {producto.imagenUrl && !varianteSel?.imagenUrl && (
              <div className="relative w-full h-32 mb-2">
                <Image src={producto.imagenUrl} alt={producto.nombre} fill sizes="320px" className="object-cover rounded-lg" />
              </div>
            )}
            {varianteSel?.imagenUrl && (
              <div className="relative w-full h-32 mb-2">
                <Image src={varianteSel.imagenUrl} alt={varianteSel.nombre} fill sizes="320px" className="object-cover rounded-lg" />
              </div>
            )}
            <h3 className="text-lg font-bold text-gray-900 text-center">{producto.nombre}</h3>
            <p className="text-center text-gray-500 text-sm">{producto.nombreCategoria}</p>
            <p className="text-center text-2xl font-bold text-gray-900 mt-2">{formatCOP(producto.precioVenta)}</p>

            {variantes.length > 0 && esSoloTalla && (
              <div className="mt-4">
                <p className="text-xs text-gray-500 text-center mb-2">Talla: <span className="font-medium text-gray-700">{varianteSel?.talla || 'Única'}</span></p>
                <div className="flex justify-center gap-2 flex-wrap">
                  {variantes.map(v => (
                    <button
                      key={v.id}
                      onClick={() => setVarianteSel(v)}
                      className={`min-w-[2.5rem] px-3 py-1.5 rounded-lg border-2 text-xs font-medium transition-all ${
                        varianteSel?.id === v.id
                          ? 'border-gray-900 bg-gray-900 text-white shadow-md'
                          : 'border-gray-200 text-gray-600 hover:border-gray-400 hover:bg-gray-50'
                      } ${(v.stock ?? 0) <= 0 ? 'opacity-40 line-through cursor-not-allowed' : ''}`}
                      disabled={(v.stock ?? 0) <= 0}
                    >
                      {v.talla || 'Única'}
                    </button>
                  ))}
                </div>
              </div>
            )}
            {variantes.length > 0 && !esSoloTalla && grupos.length > 0 && (
              <div className="mt-4 space-y-3">
                <div>
                  <p className="text-xs text-gray-500 text-center mb-2">Color: <span className="font-medium text-gray-700">{colorSel?.nombre}</span></p>
                  <div className="flex justify-center gap-2">
                    {grupos.map(g => (
                      <button
                        key={`${g.nombre}|${g.colorHex}`}
                        onClick={() => { setColorSel(g); setVarianteSel(g.tallas[0] || null); }}
                        className={`w-8 h-8 rounded-full border-2 transition-all ${
                          colorSel === g ? 'border-gray-900 scale-110 shadow-md' : 'border-gray-200 hover:scale-105'
                        }`}
                        style={{ backgroundColor: g.colorHex }}
                        title={g.nombre}
                      />
                    ))}
                  </div>
                </div>
                {tieneTallas && colorSel && colorSel.tallas.length > 0 && (
                  <div>
                    <p className="text-xs text-gray-500 text-center mb-2">Talla: <span className="font-medium text-gray-700">{varianteSel?.talla || 'Única'}</span></p>
                    <div className="flex justify-center gap-2 flex-wrap">
                      {colorSel.tallas.map(v => (
                        <button
                          key={v.id}
                          onClick={() => setVarianteSel(v)}
                          className={`min-w-[2.5rem] px-3 py-1.5 rounded-lg border-2 text-xs font-medium transition-all ${
                            varianteSel?.id === v.id
                              ? 'border-gray-900 bg-gray-900 text-white shadow-md'
                              : 'border-gray-200 text-gray-600 hover:border-gray-400 hover:bg-gray-50'
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
            )}

            <div className="flex items-center justify-center gap-4 mt-4">
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

            <p className="text-xs text-gray-400 text-center mt-2">Disponible: {stockActual}</p>

            <button
              onClick={handleAgregar}
              className="mt-4 w-full bg-blue-600 text-white py-3 px-4 rounded-xl font-semibold text-lg hover:bg-blue-700 transition-all active:scale-[0.98]"
            >
              Agregar {cantidad > 1 && `(${cantidad})`} — {formatCOP(producto.precioVenta * cantidad)}
            </button>
          </div>
        </div>
      )}
    </>
  );
}
