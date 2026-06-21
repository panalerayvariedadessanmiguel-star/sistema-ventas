'use client';

import Link from 'next/link';
import Image from 'next/image';
import { useCarrito } from '@/lib/carrito-context';
import { useStore } from '@/lib/store-context';
import { useCliente } from '@/lib/cliente-context';
import { api } from '@/lib/api';
import { formatCOP } from '@/lib/formats';
import { useState, useEffect } from 'react';

export default function CarritoPage() {
  const { items, actualizarCantidad, quitar, total, limpiar } = useCarrito();
  const { qrBrebImg, brebLlave, bancoNombre, bancoTipoCuenta, bancoNumeroCuenta, bancoTitular, nombreTienda, domicilioCosto, domicilioGratisDesde, domicilioTiempoEstimado, whatsapp, refreshConfig } = useStore();
  const { cliente } = useCliente();
  const costoDomicilio = parseInt(domicilioCosto) || 0;
  const gratisDesde = parseInt(domicilioGratisDesde) || 0;
  const domicilioGratis = gratisDesde > 0 && total >= gratisDesde;
  const domicilio = domicilioGratis ? 0 : costoDomicilio;
  const totalConDomicilio = total + domicilio;
  const [nombre, setNombre] = useState('');
  const [documento, setDocumento] = useState('');
  const [telefono, setTelefono] = useState('');
  const [direccion, setDireccion] = useState('');
  const [procesando, setProcesando] = useState(false);
  const [exito, setExito] = useState(false);
  const [error, setError] = useState('');
  const [metodoPago, setMetodoPago] = useState('breb');
  const [numeroVenta, setNumeroVenta] = useState('');
  const [ventaGuardada, setVentaGuardada] = useState<any>(null);
  const [totalFinal, setTotalFinal] = useState(0);
  const [pagoFinal, setPagoFinal] = useState('');
  const [confirmoPago, setConfirmoPago] = useState(false);
  const waNumero = (whatsapp || '').replace(/[^0-9]/g, '').replace(/^(57|0)+/, '');

  useEffect(() => { refreshConfig(); }, [refreshConfig]);

  useEffect(() => {
    if (cliente) {
      setNombre(cliente.nombre);
      setTelefono(cliente.telefono);
      setDireccion(cliente.direccion);
    }
  }, [cliente]);

  const handlePagar = async () => {
    if (!nombre.trim()) { setError('Ingresa tu nombre'); return; }
    if (!documento.trim()) { setError('Ingresa tu documento'); return; }
    if (items.length === 0) { setError('El carrito esta vacio'); return; }

    setProcesando(true);
    setError('');

    try {
      let clienteId: number | null = null;

      const clientes = await api.clientes.getAll();
      const clienteExistente = clientes.find(c =>
        c.documento.toLowerCase() === documento.trim().toLowerCase()
      );

      if (clienteExistente) {
        clienteId = clienteExistente.id;
        if (direccion && direccion !== clienteExistente.direccion) {
          await api.clientes.update(clienteExistente.id, { direccion, telefono });
        }
      } else {
        const nuevo = await api.clientes.create({ nombre, telefono, direccion, documento: documento.trim() });
        clienteId = nuevo.id;
      }

      const detalles = items.map(i => ({
        productoId: i.producto.id,
        cantidad: i.cantidad,
        precioUnitario: i.producto.precioVenta,
        costoUnitario: i.producto.precioCompra,
        subTotal: i.producto.precioVenta * i.cantidad,
      }));

      const pagoLabel = metodoPago === 'transferencia' ? 'Transferencia Davivienda' : 'Bre-B QR';
      const venta = await api.ventas.registrar({
        clienteId,
        metodoPago: pagoLabel,
        montoPagado: totalConDomicilio,
        cambio: 0,
        usuario: 'Web',
        domicilio: domicilio,
        detalles,
      });

      setNumeroVenta(venta.numeroVenta);
      setVentaGuardada(venta);
      setTotalFinal(totalConDomicilio);
      setPagoFinal(pagoLabel);
      setExito(true);
      limpiar();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error al procesar la venta');
    } finally {
      setProcesando(false);
    }
  };

  if (exito) {
    return (
      <div className="text-center py-16 max-w-md mx-auto">
        <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-6">
          <svg className="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M5 13l4 4L19 7" />
          </svg>
        </div>
        <h2 className="text-2xl font-bold text-gray-900 mb-1">Pedido Recibido</h2>
        {numeroVenta && (
          <p className="text-sm text-gray-500 mb-4">No. Pedido: <span className="font-semibold text-gray-700">{numeroVenta}</span></p>
        )}
        <p className="text-gray-600 mb-6">Gracias por tu compra en <strong>{nombreTienda}</strong>.</p>
        {direccion && (
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 mb-4 text-sm text-left">
            <p className="font-medium text-blue-800">Direccion de entrega:</p>
            <p className="text-blue-700">{direccion}</p>
          </div>
        )}
        {ventaGuardada?.metodoPago?.includes('Transferencia') && (
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 mb-4 text-sm text-left">
            <p className="font-medium text-blue-800">Datos de la cuenta Davivienda:</p>
            <p className="text-blue-700 text-xs mt-1">Ahorros {bancoNumeroCuenta} — {bancoTitular}</p>
          </div>
        )}
        <div className={`rounded-lg p-4 mb-6 text-sm text-left ${ventaGuardada?.metodoPago?.includes('Transferencia') ? 'bg-yellow-50 border border-yellow-200' : 'bg-green-50 border border-green-200'}`}>
          <p className={`font-medium ${ventaGuardada?.metodoPago?.includes('Transferencia') ? 'text-yellow-800' : 'text-green-800'}`}>
            Envía el comprobante de pago por WhatsApp para confirmar tu pedido
          </p>
        </div>
        {waNumero && (
          <a
            href={`https://wa.me/57${waNumero}?text=${encodeURIComponent(
              `Hola, ya realice el pago de mi pedido ${numeroVenta} por ${formatCOP(totalFinal)}${pagoFinal.includes('Transferencia') ? ` por transferencia Davivienda` : ''}. Adjunto comprobante. Mi direccion es: ${direccion || 'No especificada'}.`
            )}`}
            target="_blank"
            rel="noopener noreferrer"
            className="block w-full bg-green-500 text-white py-3 rounded-lg font-medium hover:bg-green-600 transition-colors mb-3"
          >
            Enviar comprobante por WhatsApp
          </a>
        )}
        <Link href="/" className="inline-block bg-blue-600 text-white px-8 py-3 rounded-lg font-medium hover:bg-blue-700 transition-colors">
          Seguir comprando
        </Link>
      </div>
    );
  }

  if (items.length === 0) {
    return (
      <div className="text-center py-20 max-w-sm mx-auto">
        <svg className="w-20 h-20 mx-auto text-gray-300 mb-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 100 4 2 2 0 000-4z" />
        </svg>
        <h2 className="text-2xl font-bold text-gray-900 mb-2">Tu carrito esta vacio</h2>
        <p className="text-gray-500 mb-8">Agrega productos desde nuestra tienda</p>
        <Link href="/" className="inline-block bg-blue-600 text-white px-8 py-3 rounded-lg font-medium hover:bg-blue-700 transition-colors">
          Ver productos
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto">
      <h1 className="text-3xl font-bold text-gray-900 mb-8">Tu Carrito</h1>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2 space-y-3">
          {items.map(item => {
            const key = item.variante ? `${item.producto.id}-${item.variante.id}` : `${item.producto.id}`;
            return (
            <div key={key} className="bg-white rounded-lg shadow-sm border border-gray-100 p-3 flex items-center gap-3">
              <div className="relative w-16 h-16 rounded-lg bg-gray-50 overflow-hidden shrink-0 border border-gray-100">
                {item.variante?.imagenUrl ? (
                  <Image src={item.variante.imagenUrl} alt={item.variante.nombre} fill sizes="64px" className="object-cover" />
                ) : item.producto.imagenUrl ? (
                  <Image src={item.producto.imagenUrl} alt={item.producto.nombre} fill sizes="64px" className="object-cover" />
                ) : (
                  <div className="w-full h-full flex items-center justify-center bg-gradient-to-br from-gray-100 to-gray-200 text-gray-300">
                    <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="m20 7-8-4-8 4m16 0-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                    </svg>
                  </div>
                )}
              </div>
              <div className="flex-1 min-w-0">
                <h3 className="font-medium text-gray-900 text-sm truncate">{item.producto.nombre}</h3>
                <div className="flex items-center gap-1.5 mt-0.5">
                  {item.variante && (
                    <>
                      <div className="w-3 h-3 rounded-full border border-gray-200 shrink-0" style={{ backgroundColor: item.variante.colorHex }} />
                      <span className="text-xs text-gray-500">{item.variante.nombre}{item.variante.talla ? ` - Talla ${item.variante.talla}` : ''}</span>
                    </>
                  )}
                  {!item.variante && <span className="text-xs text-gray-500">{formatCOP(item.producto.precioVenta)} c/u</span>}
                </div>
              </div>
              <div className="flex items-center gap-1">
                <button
                  onClick={() => actualizarCantidad(key, item.cantidad - 1)}
                  className="w-7 h-7 rounded-full bg-gray-100 hover:bg-gray-200 font-bold text-gray-600 transition-colors text-sm"
                >
                  -
                </button>
                <span className="w-7 text-center font-medium text-gray-900 text-sm">{item.cantidad}</span>
                <button
                  onClick={() => actualizarCantidad(key, item.cantidad + 1)}
                  className="w-7 h-7 rounded-full bg-gray-100 hover:bg-gray-200 font-bold text-gray-600 transition-colors text-sm"
                >
                  +
                </button>
              </div>
              <div className="text-right min-w-[72px]">
                <p className="font-bold text-gray-900 text-sm">{formatCOP(item.producto.precioVenta * item.cantidad)}</p>
              </div>
              <button
                onClick={() => quitar(key)}
                className="text-gray-300 hover:text-red-500 transition-colors p-1"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
              </button>
            </div>
            );
          })}
        </div>

        <div className="bg-white rounded-lg shadow p-5 h-fit space-y-4">
          <div>
            <div className="flex justify-between items-baseline mb-3">
              <h2 className="text-xl font-bold text-gray-900">Resumen</h2>
              <span className="text-sm text-gray-500">{items.length} {items.length === 1 ? 'producto' : 'productos'}</span>
            </div>
            <div className="space-y-1.5 text-sm">
              <div className="flex justify-between text-gray-600">
                <span>Subtotal</span>
                <span>{formatCOP(total)}</span>
              </div>
              {domicilio > 0 && (
                <div className="flex justify-between text-gray-600">
                  <span>Domicilio</span>
                  <span>{formatCOP(domicilio)}</span>
                </div>
              )}
              {domicilioGratis && (
                <div className="flex justify-between text-green-600 font-medium">
                  <span>Domicilio</span>
                  <span>Gratis 🎉</span>
                </div>
              )}
              {!domicilioGratis && gratisDesde > 0 && (
                <div className="text-xs text-gray-400 text-center">
                  Domicilio gratis desde {formatCOP(gratisDesde)}
                </div>
              )}
              <div className="flex justify-between text-base font-bold text-gray-900 border-t pt-1.5">
                <span>Total</span>
                <span>{formatCOP(totalConDomicilio)}</span>
              </div>
            </div>
          </div>

          <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-2 text-center space-y-0.5">
            <p className="text-yellow-700 text-xs font-medium">🚚 Solo entregas en Bogota</p>
            {domicilioTiempoEstimado && <p className="text-yellow-600 text-[11px]">📬 Entrega estimada: {domicilioTiempoEstimado}</p>}
          </div>

          <div className="space-y-2">
            <input
              type="text"
              placeholder="Nombre completo"
              value={nombre}
              onChange={e => setNombre(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <input
              type="text"
              placeholder="Documento (cedula / NIT)"
              value={documento}
              onChange={e => setDocumento(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <input
              type="tel"
              placeholder="Telefono"
              value={telefono}
              onChange={e => setTelefono(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <input
              type="text"
              placeholder="Direccion de entrega"
              value={direccion}
              onChange={e => setDireccion(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-2">Pago</p>
            <div className="flex flex-col gap-2">
              <button type="button" onClick={() => { setMetodoPago('breb'); setConfirmoPago(false); }} className={`flex items-center justify-center gap-2 py-4 px-4 rounded-xl text-sm font-medium border transition-colors w-full relative ${metodoPago === 'breb' ? 'bg-white text-gray-900 border-2 border-green-600 ring-1 ring-green-600' : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'}`}>
                {metodoPago === 'breb' && (
                  <span className="absolute top-1 right-1 w-5 h-5 bg-green-600 rounded-full flex items-center justify-center">
                    <svg className="w-3 h-3 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" /></svg>
                  </span>
                )}
                <img src="/logos/davivienda-icon.svg" alt="Davivienda" className="h-8 w-8 object-contain shrink-0" />
                <span className="font-medium">Davivienda - Bre-B QR</span>
                <span className="text-xs text-gray-400">Paga desde cualquier banco</span>
              </button>
              <button type="button" onClick={() => { setMetodoPago('transferencia'); setConfirmoPago(false); }} className={`flex items-center justify-center gap-2 py-4 px-4 rounded-xl text-sm font-medium border transition-colors w-full relative ${metodoPago === 'transferencia' ? 'bg-white text-gray-900 border-2 border-blue-600 ring-1 ring-blue-600' : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'}`}>
                {metodoPago === 'transferencia' && (
                  <span className="absolute top-1 right-1 w-5 h-5 bg-blue-600 rounded-full flex items-center justify-center">
                    <svg className="w-3 h-3 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" /></svg>
                  </span>
                )}
                <svg className="w-7 h-7 text-blue-600 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" /></svg>
                <span className="font-medium">Davivienda - Transferencia</span>
                <span className="text-xs text-gray-400">Deposito en cuenta</span>
              </button>
            </div>

            {metodoPago === 'breb' && (
              <div className="bg-green-50 border border-green-200 rounded-lg p-5 text-sm text-green-700 space-y-5 mt-3">
                <p className="font-semibold text-green-800 text-center text-base">Paga con Bre-B con cualquier banco</p>
                <div className="flex justify-center">
                  {qrBrebImg ? (
                    <img src={qrBrebImg} alt="QR Bre-B Davivienda" className="w-44 h-44 border-2 border-green-300 rounded-xl shadow-sm" />
                  ) : (
                    <div className="w-44 h-44 bg-gray-100 rounded-xl flex items-center justify-center text-gray-400 border-2 border-dashed border-gray-300">
                      QR no configurado
                    </div>
                  )}
                </div>
                <div className="space-y-4">
                  <div className="flex items-start gap-3">
                    <span className="w-7 h-7 rounded-full bg-green-600 text-white text-sm font-bold flex items-center justify-center shrink-0">1</span>
                    <div>
                      <p className="font-semibold text-green-800">Abre tu app bancaria</p>
                      <p className="text-green-600 text-xs">Cualquier banco del pais</p>
                    </div>
                  </div>
                  <div className="flex items-start gap-3">
                    <span className="w-7 h-7 rounded-full bg-green-600 text-white text-sm font-bold flex items-center justify-center shrink-0">2</span>
                    <div>
                      <p className="font-semibold text-green-800">Escanea el QR</p>
                      <p className="text-green-600 text-xs">Ingresa la llave si no te abre directo: <strong className="text-green-800">{brebLlave || '---'}</strong></p>
                    </div>
                  </div>
                  <div className="flex items-start gap-3">
                    <span className="w-7 h-7 rounded-full bg-green-600 text-white text-sm font-bold flex items-center justify-center shrink-0">3</span>
                    <div>
                      <p className="font-semibold text-green-800">Ingresa el valor exacto</p>
                      <p className="text-green-600 text-xs">Digita: <strong className="text-green-800 text-base">{formatCOP(totalConDomicilio)}</strong></p>
                    </div>
                  </div>
                </div>

                <label className="flex items-start gap-3 cursor-pointer group">
                  <input
                    type="checkbox"
                    checked={confirmoPago}
                    onChange={e => setConfirmoPago(e.target.checked)}
                    className="mt-0.5 w-4 h-4 rounded border-green-400 text-green-600 focus:ring-green-500 shrink-0"
                  />
                  <span className="text-xs text-green-700 group-hover:text-green-900 leading-relaxed">
                    He realizado el pago por <strong>Bre-B QR</strong> por <strong>{formatCOP(totalConDomicilio)}</strong> y quiero solicitar mi pedido
                  </span>
                </label>

              </div>
            )}

            {metodoPago === 'transferencia' && (
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-5 text-sm text-blue-700 space-y-4 mt-3">
                <p className="font-semibold text-blue-800 text-center text-base">Transfiere directo a Davivienda</p>
                <div className="bg-white/60 rounded-lg p-4 space-y-2 text-sm">
                  {bancoNombre && <p className="flex justify-between"><span className="text-blue-600">Banco:</span><span className="font-medium text-blue-800">{bancoNombre}</span></p>}
                  {bancoTipoCuenta && <p className="flex justify-between"><span className="text-blue-600">Tipo:</span><span className="font-medium text-blue-800">{bancoTipoCuenta}</span></p>}
                  {bancoNumeroCuenta && (
                    <div className="flex justify-between items-center">
                      <span className="text-blue-600">Cuenta:</span>
                      <span className="font-bold text-blue-900 text-base tracking-wide select-all">{bancoNumeroCuenta}</span>
                    </div>
                  )}
                  {bancoTitular && <p className="flex justify-between"><span className="text-blue-600">Titular:</span><span className="font-medium text-blue-800">{bancoTitular}</span></p>}
                </div>
                <div className="flex items-start gap-3">
                  <span className="w-7 h-7 rounded-full bg-blue-600 text-white text-sm font-bold flex items-center justify-center shrink-0">1</span>
                  <div>
                    <p className="font-semibold text-blue-800">Transfiere el valor exacto</p>
                    <p className="text-blue-600 text-xs">Total: <strong className="text-blue-800 text-base">{formatCOP(totalConDomicilio)}</strong></p>
                  </div>
                </div>
                <div className="flex items-start gap-3">
                  <span className="w-7 h-7 rounded-full bg-blue-600 text-white text-sm font-bold flex items-center justify-center shrink-0">2</span>
                  <div>
                    <p className="font-semibold text-blue-800">Envianos el comprobante</p>
                    <p className="text-blue-600 text-xs">Por WhatsApp para confirmar tu pedido</p>
                  </div>
                </div>

                <label className="flex items-start gap-3 cursor-pointer group">
                  <input
                    type="checkbox"
                    checked={confirmoPago}
                    onChange={e => setConfirmoPago(e.target.checked)}
                    className="mt-0.5 w-4 h-4 rounded border-blue-400 text-blue-600 focus:ring-blue-500 shrink-0"
                  />
                  <span className="text-xs text-blue-700 group-hover:text-blue-900 leading-relaxed">
                    He realizado la transferencia por <strong>{formatCOP(totalConDomicilio)}</strong> a la cuenta Davivienda y quiero solicitar mi pedido
                  </span>
                </label>
              </div>
            )}
          </div>

          {error && <p className="text-red-600 text-sm">{error}</p>}

          <div className="relative">
            {procesando && (
              <div className="absolute inset-0 bg-white/80 rounded-lg flex items-center justify-center z-10">
                <div className="flex items-center gap-2 text-sm text-gray-500">
                  <svg className="animate-spin h-4 w-4 text-blue-600" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                  </svg>
                  Procesando pedido...
                </div>
              </div>
            )}
          <button
            onClick={handlePagar}
            disabled={procesando || !confirmoPago}
            className="w-full bg-green-600 text-white py-3 rounded-lg font-medium hover:bg-green-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors"
          >
            {procesando ? 'Procesando...' : 'Solicitar Pedido'}
          </button>
          </div>
        </div>
      </div>
    </div>
  );
}
