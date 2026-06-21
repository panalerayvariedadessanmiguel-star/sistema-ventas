'use client';

import { use, useState, useEffect } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { api, type Venta, type DetalleVenta } from '@/lib/api';
import { useCliente } from '@/lib/cliente-context';
import { formatCOP } from '@/lib/formats';

export default function PedidoDetallePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const { isLoggedIn } = useCliente();
  const router = useRouter();
  const [venta, setVenta] = useState<Venta | null>(null);
  const [detalles, setDetalles] = useState<DetalleVenta[]>([]);
  const [cargando, setCargando] = useState(true);

  useEffect(() => {
    if (!isLoggedIn) { router.replace('/ingresar'); return; }
    const fetch = async () => {
      try {
        const data = await api.ventas.getById(parseInt(id));
        setVenta(data.venta);
        setDetalles(data.detalles);
      } catch { } finally {
        setCargando(false);
      }
    };
    fetch();
  }, [id, isLoggedIn, router]);

  if (!isLoggedIn) return null;

  if (cargando) {
    return (
      <div className="max-w-2xl mx-auto py-12">
        <div className="animate-pulse space-y-4">
          <div className="h-6 bg-gray-200 rounded w-1/3" />
          <div className="h-4 bg-gray-200 rounded w-1/4" />
          <div className="h-32 bg-gray-200 rounded" />
        </div>
      </div>
    );
  }

  if (!venta) {
    return (
      <div className="text-center py-20">
        <h2 className="text-xl font-bold text-gray-900 mb-2">Pedido no encontrado</h2>
        <Link href="/mis-pedidos" className="text-blue-600 hover:underline">Volver a mis pedidos</Link>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto">
      <Link href="/mis-pedidos" className="text-sm text-blue-600 hover:underline mb-4 inline-block">&larr; Volver a mis pedidos</Link>

      <div className="bg-white rounded-lg border border-gray-100 p-6 mb-6">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h1 className="text-xl font-bold text-gray-900">{venta.numeroVenta}</h1>
            <p className="text-sm text-gray-500">{new Date(venta.fechaVenta).toLocaleDateString('es-CO', { day: 'numeric', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit' })}</p>
          </div>
          <span className={`text-sm font-medium px-3 py-1 rounded-full ${venta.anulada ? 'bg-red-100 text-red-700' : 'bg-green-100 text-green-700'}`}>
            {venta.anulada ? 'Anulado' : 'Confirmado'}
          </span>
        </div>

        <div className="space-y-3">
          {detalles.map(d => (
            <div key={d.productoId} className="flex items-center justify-between text-sm">
              <div>
                <p className="font-medium text-gray-900">{d.nombreProducto || `Producto #${d.productoId}`}</p>
                <p className="text-gray-500">{d.cantidad} x {formatCOP(d.precioUnitario)}</p>
              </div>
              <p className="font-medium text-gray-900">{formatCOP(d.subTotal)}</p>
            </div>
          ))}
        </div>

        <div className="border-t mt-4 pt-4 space-y-1 text-sm">
          <div className="flex justify-between text-gray-600">
            <span>Metodo de pago</span>
            <span>{venta.metodoPago}</span>
          </div>
          <div className="flex justify-between text-lg font-bold text-gray-900 border-t pt-2">
            <span>Total</span>
            <span>{formatCOP(venta.total)}</span>
          </div>
        </div>
      </div>
    </div>
  );
}
