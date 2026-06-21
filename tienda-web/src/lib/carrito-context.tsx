'use client';

import { createContext, useContext, useState, useCallback, useEffect, ReactNode } from 'react';
import type { Producto, Variante } from './api';

const STORAGE_KEY = 'carrito_items';

export interface CarritoItem {
  producto: Producto;
  cantidad: number;
  variante?: { id: number; nombre: string; colorHex: string; talla?: string | null; stock?: number | null; imagenUrl?: string | null };
}

function itemKey(item: CarritoItem): string {
  return item.variante ? `${item.producto.id}-${item.variante.id}` : `${item.producto.id}`;
}

interface CarritoContextType {
  items: CarritoItem[];
  agregar: (producto: Producto, cantidad?: number, variante?: Variante) => void;
  quitar: (itemKey: string) => void;
  actualizarCantidad: (itemKey: string, cantidad: number) => void;
  limpiar: () => void;
  total: number;
  cantidadItems: number;
}

const CarritoContext = createContext<CarritoContextType | undefined>(undefined);

export function CarritoProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<CarritoItem[]>([]);

  useEffect(() => {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      if (saved) setItems(JSON.parse(saved));
    } catch { }
  }, []);

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
  }, [items]);

  const agregar = useCallback((producto: Producto, cantidad = 1, variante?: Variante) => {
    const key = variante ? `${producto.id}-${variante.id}` : `${producto.id}`;
    setItems(prev => {
      const existente = prev.find(i => itemKey(i) === key);
      if (existente) {
        return prev.map(i =>
          itemKey(i) === key
            ? { ...i, cantidad: Math.min(i.cantidad + cantidad, variante?.stock ?? producto.stock) }
            : i
        );
      }
      const nuevo: CarritoItem = { producto, cantidad: Math.min(cantidad, variante?.stock ?? producto.stock) };
      if (variante) nuevo.variante = { id: variante.id, nombre: variante.nombre, colorHex: variante.colorHex, talla: variante.talla, stock: variante.stock, imagenUrl: variante.imagenUrl };
      return [...prev, nuevo];
    });
  }, []);

  const quitar = useCallback((key: string) => {
    setItems(prev => prev.filter(i => itemKey(i) !== key));
  }, []);

  const actualizarCantidad = useCallback((key: string, cantidad: number) => {
    setItems(prev =>
      cantidad <= 0
        ? prev.filter(i => itemKey(i) !== key)
        : prev.map(i =>
            itemKey(i) === key
              ? { ...i, cantidad: Math.min(cantidad, i.variante?.stock ?? i.producto.stock) }
              : i
          )
    );
  }, []);

  const limpiar = useCallback(() => setItems([]), []);

  const total = items.reduce((sum, i) => sum + i.producto.precioVenta * i.cantidad, 0);
  const cantidadItems = items.reduce((sum, i) => sum + i.cantidad, 0);

  return (
    <CarritoContext.Provider value={{ items, agregar, quitar, actualizarCantidad, limpiar, total, cantidadItems }}>
      {children}
    </CarritoContext.Provider>
  );
}

export function useCarrito() {
  const ctx = useContext(CarritoContext);
  if (!ctx) throw new Error('useCarrito debe usarse dentro de CarritoProvider');
  return ctx;
}
