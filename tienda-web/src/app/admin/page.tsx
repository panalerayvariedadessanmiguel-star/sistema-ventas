'use client';

import { useEffect, useState, useMemo } from 'react';
import { api, Venta } from '@/lib/api';
import { formatCOP } from '@/lib/formats';

const MONTHS = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];

export default function AdminDashboard() {
  const [loading, setLoading] = useState(true);
  const [productos, setProductos] = useState(0);
  const [categorias, setCategorias] = useState(0);
  const [ventas, setVentas] = useState<Venta[]>([]);

  useEffect(() => {
    Promise.all([api.productos.getAll(), api.categorias.getAll(), api.ventas.getAll()])
      .then(([prods, cats, vents]) => {
        setProductos(prods.length);
        setCategorias(cats.length);
        setVentas(vents);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const confirmed = useMemo(() => ventas.filter(v => v.estado === 'Confirmada' && !v.anulada), [ventas]);
  const pendientes = useMemo(() => ventas.filter(v => v.estado === 'Pendiente' && !v.anulada), [ventas]);
  const anuladas = useMemo(() => ventas.filter(v => v.anulada), [ventas]);

  const monthlySales = useMemo(() => {
    const groups: Record<string, { count: number; total: number }> = {};
    for (const v of confirmed) {
      const d = new Date(v.fechaVenta);
      const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
      if (!groups[key]) groups[key] = { count: 0, total: 0 };
      groups[key].count++;
      groups[key].total += v.total;
    }
    return Object.entries(groups)
      .sort(([a], [b]) => a.localeCompare(b))
      .slice(-6);
  }, [confirmed]);

  const maxMonthCount = Math.max(...monthlySales.map(([, v]) => v.count), 1);

  const paymentMethods = useMemo(() => {
    const groups: Record<string, number> = {};
    for (const v of confirmed) {
      const key = v.metodoPago || 'Otro';
      groups[key] = (groups[key] || 0) + 1;
    }
    return Object.entries(groups).sort(([, a], [, b]) => b - a);
  }, [confirmed]);

  const maxPayment = Math.max(...paymentMethods.map(([, v]) => v), 1);

  const cards = [
    { label: 'Productos', value: productos, color: 'bg-blue-500' },
    { label: 'Categorias', value: categorias, color: 'bg-green-500' },
    { label: 'Ventas', value: confirmed.length, color: 'bg-purple-500' },
    { label: 'Pendientes', value: pendientes.length, color: 'bg-yellow-500' },
    { label: 'Anuladas', value: anuladas.length, color: 'bg-red-500' },
  ];

  if (loading) return <div className="text-gray-500">Cargando...</div>;

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">Dashboard</h1>

      {/* Stats cards */}
      <div className="grid grid-cols-2 lg:grid-cols-5 gap-3 mb-6">
        {cards.map(c => (
          <div key={c.label} className="bg-white rounded-lg shadow p-4">
            <div className={`w-10 h-10 ${c.color} rounded-lg flex items-center justify-center text-white text-lg font-bold mb-2`}>
              {c.value}
            </div>
            <p className="text-gray-600 text-xs">{c.label}</p>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Monthly sales */}
        <div className="bg-white rounded-lg shadow p-5">
          <h2 className="text-sm font-bold text-gray-900 mb-4">Ventas por mes</h2>
          {monthlySales.length === 0 ? (
            <div className="text-center py-8">
              <svg className="w-12 h-12 mx-auto text-gray-200 mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
              </svg>
              <p className="text-gray-400 text-sm">Aun no hay ventas confirmadas</p>
              <p className="text-gray-300 text-xs mt-1">Las ventas apareceran aqui cuando los clientes confirmen sus pedidos.</p>
            </div>
          ) : (
            <div className="space-y-2">
              {monthlySales.map(([key, { count, total }]) => {
                const pct = (count / maxMonthCount) * 100;
                const [year, month] = key.split('-');
                const label = `${MONTHS[parseInt(month) - 1]} ${year}`;
                return (
                  <div key={key}>
                    <div className="flex justify-between text-xs text-gray-600 mb-1">
                      <span>{label}</span>
                      <span className="font-medium">{count} venta{count !== 1 && 's'} — {formatCOP(total)}</span>
                    </div>
                    <div className="w-full bg-gray-100 rounded-full h-5 overflow-hidden">
                      <div
                        className="bg-purple-500 h-full rounded-full transition-all duration-500 flex items-center justify-end pr-2 text-[10px] text-white font-medium"
                        style={{ width: `${Math.max(pct, 4)}%` }}
                      >
                        {pct > 15 && count}
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        {/* Payment methods */}
        <div className="bg-white rounded-lg shadow p-5">
          <h2 className="text-sm font-bold text-gray-900 mb-4">Metodos de pago</h2>
          {paymentMethods.length === 0 ? (
            <div className="text-center py-8">
              <svg className="w-12 h-12 mx-auto text-gray-200 mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
              </svg>
              <p className="text-gray-400 text-sm">Sin datos de pago aun</p>
              <p className="text-gray-300 text-xs mt-1">Los metodos de pago se mostraran cuando haya ventas confirmadas.</p>
            </div>
          ) : (
            <div className="space-y-2">
              {paymentMethods.map(([method, count]) => {
                const pct = (count / maxPayment) * 100;
                const totalPct = (count / confirmed.length) * 100;
                return (
                  <div key={method}>
                    <div className="flex justify-between text-xs text-gray-600 mb-1">
                      <span>{method}</span>
                      <span className="font-medium">{count} ({totalPct.toFixed(0)}%)</span>
                    </div>
                    <div className="w-full bg-gray-100 rounded-full h-5 overflow-hidden">
                      <div
                        className="bg-blue-500 h-full rounded-full transition-all duration-500"
                        style={{ width: `${Math.max(pct, 4)}%` }}
                      />
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
