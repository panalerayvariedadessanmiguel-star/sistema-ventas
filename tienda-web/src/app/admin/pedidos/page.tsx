'use client';

import { useEffect, useState, useMemo } from 'react';
import { api, Venta, DetalleVenta } from '@/lib/api';
import { formatCOP } from '@/lib/formats';
import { useToast } from '@/lib/toast-context';

const ITEMS_PER_PAGE = 10;

export default function AdminPedidos() {
  const { toast } = useToast();
  const [ventas, setVentas] = useState<Venta[]>([]);
  const [selectedVenta, setSelectedVenta] = useState<{ venta: Venta; detalles: DetalleVenta[] } | null>(null);
  const [loading, setLoading] = useState(true);
  const [anulando, setAnulando] = useState<number | null>(null);
  const [confirmando, setConfirmando] = useState<number | null>(null);
  const [tab, setTab] = useState<'todos' | 'pendientes' | 'anuladas'>('todos');
  const [origenFilter, setOrigenFilter] = useState<'todos' | 'Web' | 'Fisico'>('todos');
  const [loadError, setLoadError] = useState('');
  const [search, setSearch] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [page, setPage] = useState(1);

  function load() {
    setLoading(true);
    setLoadError('');
    if (tab === 'anuladas') {
      api.ventas.getAll().then(all => setVentas(all.filter(v => v.anulada))).catch(e => { setLoadError('Error al cargar: ' + (e instanceof Error ? e.message : 'desconocido')); setVentas([]); }).finally(() => setLoading(false));
    } else {
      const fn = tab === 'pendientes' ? api.ventas.getPendientes : api.ventas.getAll;
      fn().then(setVentas).catch(e => { setLoadError('Error al cargar: ' + (e instanceof Error ? e.message : 'desconocido')); setVentas([]); }).finally(() => setLoading(false));
    }
  }

  useEffect(() => { load(); }, [tab]);

  const filtered = useMemo(() => {
    let result = ventas;
    if (search.trim()) {
      const q = search.toLowerCase();
      result = result.filter(v =>
        v.numeroVenta.toLowerCase().includes(q) ||
        (v.nombreCliente && v.nombreCliente.toLowerCase().includes(q)) ||
        (v.metodoPago && v.metodoPago.toLowerCase().includes(q))
      );
    }
    if (dateFrom) {
      const from = new Date(dateFrom);
      from.setHours(0, 0, 0, 0);
      result = result.filter(v => new Date(v.fechaVenta) >= from);
    }
    if (dateTo) {
      const to = new Date(dateTo);
      to.setHours(23, 59, 59, 999);
      result = result.filter(v => new Date(v.fechaVenta) <= to);
    }
    if (origenFilter !== 'todos') {
      result = result.filter(v => (v.origen || 'Fisico') === origenFilter);
    }
    return result;
  }, [ventas, search, dateFrom, dateTo, origenFilter]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / ITEMS_PER_PAGE));
  const paginated = filtered.slice((page - 1) * ITEMS_PER_PAGE, page * ITEMS_PER_PAGE);

  useEffect(() => { setPage(1); }, [search, tab, dateFrom, dateTo, origenFilter]);

  async function verDetalle(id: number) {
    try {
      const data = await api.ventas.getById(id);
      setSelectedVenta(data);
    } catch (err: any) {
      toast(err.message || 'Error al cargar detalle', 'error');
    }
  }

  async function handleAnular(id: number) {
    const motivo = prompt('Motivo de la anulacion:');
    if (!motivo || !motivo.trim()) return;
    setAnulando(id);
    try {
      await api.ventas.anular(id, motivo);
      toast('Venta anulada correctamente', 'success');
      load();
    } catch (err: any) {
      toast(err.message || 'Error al anular la venta', 'error');
    } finally {
      setAnulando(null);
    }
  }

  async function handleConfirmar(id: number) {
    if (!confirm('Confirmar el pago de esta venta? Se descontara del stock.')) return;
    setConfirmando(id);
    try {
      const res = await api.ventas.confirmarPago(id) as any;
      if (res.factura) {
        toast(`Pago confirmado — Factura: ${res.factura.nombreArchivo}`, 'success');
      } else {
        toast('Pago confirmado — stock actualizado', 'success');
      }
      load();
    } catch (err: any) {
      toast(err.message || 'Error al confirmar el pago', 'error');
    } finally {
      setConfirmando(null);
    }
  }

  function formatFecha(f: string) {
    return new Date(f).toLocaleString('es-CO', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });
  }

  if (loading) return <div className="text-gray-500">Cargando...</div>;

  return (
    <div>
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 mb-6">
        <h1 className="text-2xl font-bold">Pedidos / Ventas</h1>
        <div className="relative w-full sm:w-72">
          <svg className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
          <input
            type="text"
            placeholder="Buscar por # venta, cliente o pago..."
            value={search}
            onChange={e => setSearch(e.target.value)}
            className="pl-9 pr-3 py-2 border border-gray-300 rounded-md text-sm w-full focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      <div className="flex flex-wrap items-center gap-2 mb-4">
        <div className="flex items-center gap-2 text-sm">
          <label className="text-gray-500">Desde:</label>
          <input type="date" value={dateFrom} onChange={e => setDateFrom(e.target.value)} className="border border-gray-300 rounded-md px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
        </div>
        <div className="flex items-center gap-2 text-sm">
          <label className="text-gray-500">Hasta:</label>
          <input type="date" value={dateTo} onChange={e => setDateTo(e.target.value)} className="border border-gray-300 rounded-md px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
        </div>
        {(dateFrom || dateTo) && (
          <button onClick={() => { setDateFrom(''); setDateTo(''); }} className="text-xs text-blue-600 hover:underline">
            Limpiar filtro
          </button>
        )}
      </div>

      <div className="flex items-center gap-2 mb-1">
        <span className="text-xs text-gray-500 font-medium mr-1">Origen:</span>
        <button onClick={() => setOrigenFilter('todos')} className={`px-3 py-1 rounded-md text-xs font-medium transition-colors ${origenFilter === 'todos' ? 'bg-gray-800 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}>
          Todos
        </button>
        <button onClick={() => setOrigenFilter('Web')} className={`px-3 py-1 rounded-md text-xs font-medium transition-colors ${origenFilter === 'Web' ? 'bg-blue-600 text-white' : 'bg-blue-50 text-blue-700 hover:bg-blue-100'}`}>
          Web
        </button>
        <button onClick={() => setOrigenFilter('Fisico')} className={`px-3 py-1 rounded-md text-xs font-medium transition-colors ${origenFilter === 'Fisico' ? 'bg-gray-700 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}>
          Físico
        </button>
      </div>

      <div className="flex gap-2 mb-4">
        <button onClick={() => setTab('todos')} className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${tab === 'todos' ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}>
          Todas las ventas
        </button>
        <button onClick={() => setTab('pendientes')} className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors relative ${tab === 'pendientes' ? 'bg-amber-500 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}>
          Pendientes de pago
          {tab === 'pendientes' && ventas.length > 0 && (
            <span className="absolute -top-2 -right-2 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center font-bold">
              {ventas.length}
            </span>
          )}
        </button>
        <button onClick={() => setTab('anuladas')} className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors relative ${tab === 'anuladas' ? 'bg-red-600 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}>
          Anuladas
          {tab === 'anuladas' && ventas.length > 0 && (
            <span className="absolute -top-2 -right-2 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center font-bold">
              {ventas.length}
            </span>
          )}
        </button>
      </div>

      {selectedVenta && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50" onClick={() => setSelectedVenta(null)}>
          <div className="bg-white rounded-lg shadow-xl max-w-lg w-full mx-4 p-6" onClick={e => e.stopPropagation()}>
            <h2 className="text-lg font-bold mb-2">Venta: {selectedVenta.venta.numeroVenta}</h2>
            <p className="text-sm text-gray-500 mb-4">{formatFecha(selectedVenta.venta.fechaVenta)}</p>

            {selectedVenta.venta.nombreCliente && (
              <div className="bg-gray-50 rounded-lg p-3 mb-4 text-sm space-y-1">
                <p><span className="text-gray-500">Cliente:</span> <strong>{selectedVenta.venta.nombreCliente}</strong></p>
                {(selectedVenta.venta as any).documentoCliente && <p><span className="text-gray-500">Documento:</span> {(selectedVenta.venta as any).documentoCliente}</p>}
                {(selectedVenta.venta as any).telefonoCliente && <p><span className="text-gray-500">Telefono:</span> {(selectedVenta.venta as any).telefonoCliente}</p>}
                {(selectedVenta.venta as any).direccionCliente && <p><span className="text-gray-500">Direccion:</span> {(selectedVenta.venta as any).direccionCliente}</p>}
              </div>
            )}

            <table className="w-full text-sm mb-4">
              <thead><tr className="border-b text-left"><th className="py-2">Producto</th><th className="py-2">Cant</th><th className="py-2">Precio</th><th className="py-2">Total</th></tr></thead>
              <tbody>
                {selectedVenta.detalles.map((d, i) => (
                  <tr key={i} className="border-b">
                    <td className="py-2">{(d as any).nombreProducto}</td>
                    <td className="py-2">{d.cantidad}</td>
                    <td className="py-2">{formatCOP(d.precioUnitario)}</td>
                    <td className="py-2">{formatCOP(d.cantidad * d.precioUnitario)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div className="flex justify-between items-center">
              <div>
                <p className="text-sm">Pago: <strong>{selectedVenta.venta.metodoPago}</strong></p>
                <p className="text-sm">Total: <strong>{formatCOP(selectedVenta.venta.total)}</strong></p>
                <p className="text-sm">Origen: <strong>{selectedVenta.venta.origen === 'Web' ? 'Web' : 'Físico'}</strong></p>
                <p className="text-sm">Estado: <strong>{selectedVenta.venta.estado || 'Confirmada'}</strong></p>
                {selectedVenta.venta.anulada && selectedVenta.venta.motivoAnulacion && (
                  <p className="text-sm text-red-600 mt-1">Motivo anulacion: {selectedVenta.venta.motivoAnulacion}</p>
                )}
              </div>
              <button onClick={() => setSelectedVenta(null)} className="bg-gray-200 px-4 py-2 rounded-md hover:bg-gray-300 text-sm">Cerrar</button>
            </div>
          </div>
        </div>
      )}

      {loadError && <p className="text-red-600 text-sm mb-4">{loadError}</p>}

      {tab !== 'pendientes' && ventas.filter(v => v.estado === 'Pendiente' && !v.anulada).length > 0 && (
        <button onClick={() => setTab('pendientes')} className="w-full text-left mb-4 p-3 bg-amber-50 border border-amber-300 rounded-lg hover:bg-amber-100 transition-colors cursor-pointer">
          <div className="flex items-center gap-3">
            <span className="w-10 h-10 rounded-full bg-amber-200 flex items-center justify-center text-amber-700 text-lg font-bold animate-pulse shrink-0">
              {ventas.filter(v => v.estado === 'Pendiente' && !v.anulada).length}
            </span>
            <div>
              <p className="font-semibold text-amber-800">
                Tienes pedidos pendientes de pago
              </p>
              <p className="text-sm text-amber-600">
                Haz clic para revisarlos y confirmar pagos
              </p>
            </div>
            <svg className="w-5 h-5 text-amber-500 ml-auto shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </div>
        </button>
      )}

      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 text-left">
            <tr>
              <th className="px-4 py-3 font-medium"># Venta</th>
              <th className="px-4 py-3 font-medium">Fecha</th>
              <th className="px-4 py-3 font-medium">Cliente</th>
              <th className="px-4 py-3 font-medium">Total</th>
              <th className="px-4 py-3 font-medium">Pago</th>
              <th className="px-4 py-3 font-medium">Estado</th>
              <th className="px-4 py-3 font-medium">Origen</th>
              <th className="px-4 py-3 font-medium"></th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {paginated.map(v => (
              <tr key={v.id} className={`transition-colors ${v.estado === 'Pendiente' && !v.anulada ? 'bg-amber-50/50 hover:bg-amber-100' : v.anulada ? 'opacity-60 hover:bg-gray-50' : 'hover:bg-gray-50'}`}>
                <td className="px-4 py-3 font-medium">{v.numeroVenta}</td>
                <td className="px-4 py-3 text-gray-500">{formatFecha(v.fechaVenta)}</td>
                <td className="px-4 py-3">{v.nombreCliente || 'Mostrador'}</td>
                <td className="px-4 py-3">{formatCOP(v.total)}</td>
                <td className="px-4 py-3">{v.metodoPago}</td>
                <td className="px-4 py-3">
                  {v.anulada ? (
                    <span className="px-2 py-1 rounded-full text-xs bg-red-100 text-red-800" title={v.motivoAnulacion}>Anulada</span>
                  ) : v.estado === 'Pendiente' ? (
                    <span className="px-2 py-1 rounded-full text-xs bg-amber-200 text-amber-900 font-semibold animate-pulse">Pendiente</span>
                  ) : (
                    <span className="px-2 py-1 rounded-full text-xs bg-green-100 text-green-800">Confirmada</span>
                  )}
                </td>
                <td className="px-4 py-3">
                  <span className={`px-2 py-1 rounded-full text-xs ${v.origen === 'Web' ? 'bg-blue-100 text-blue-800' : 'bg-gray-100 text-gray-800'}`}>
                    {v.origen === 'Web' ? 'Web' : 'Físico'}
                  </span>
                </td>
                <td className="px-4 py-3 flex gap-2">
                  <button onClick={() => verDetalle(v.id)} className="text-blue-600 hover:underline text-sm">Detalle</button>
                  {v.estado === 'Pendiente' && !v.anulada && (
                    <button onClick={() => handleConfirmar(v.id)} disabled={confirmando === v.id} className="inline-flex items-center gap-1.5 text-green-600 hover:underline text-sm font-medium disabled:opacity-50 disabled:cursor-wait">
                      {confirmando === v.id && <svg className="animate-spin w-3.5 h-3.5" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"/></svg>}
                      {confirmando === v.id ? 'Confirmando...' : 'Confirmar pago'}
                    </button>
                  )}
                  {!v.anulada && (
                    <button onClick={() => handleAnular(v.id)} disabled={anulando === v.id} className="inline-flex items-center gap-1.5 text-red-600 hover:underline text-sm disabled:opacity-50 disabled:cursor-wait">
                      {anulando === v.id && <svg className="animate-spin w-3.5 h-3.5" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"/></svg>}
                      {anulando === v.id ? 'Anulando...' : 'Anular'}
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {filtered.length === 0 && (
          <p className="text-center text-gray-500 py-12">
            {search ? 'No se encontraron ventas con ese criterio' : tab === 'pendientes' ? 'No hay pedidos pendientes' : tab === 'anuladas' ? 'No hay pedidos anulados' : 'No hay ventas'}
          </p>
        )}
        {totalPages > 1 && (
          <div className="flex items-center justify-between px-4 py-3 border-t bg-gray-50 text-sm">
            <span className="text-gray-500">
              {filtered.length} venta{filtered.length !== 1 && 's'} — Página {page} de {totalPages}
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
