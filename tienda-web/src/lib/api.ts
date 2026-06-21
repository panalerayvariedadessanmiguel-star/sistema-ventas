import { apiCacheGet, apiCacheSet, apiCacheKey, apiCacheInvalidate } from './api-cache';

export function invalidateProductCache() { apiCacheInvalidate('GET|/productos'); }
export function invalidateCategoryCache() { apiCacheInvalidate('GET|/categorias'); }

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5062/api';

export interface Producto {
  id: number;
  codigoBarras: string;
  nombre: string;
  descripcion: string;
  categoriaId: number;
  precioCompra: number;
  precioVenta: number;
  stock: number;
  stockMinimo: number;
  fechaCreacion: string;
  activo: boolean;
  nombreCategoria: string;
  imagenUrl: string;
  orden: number;
}

export interface Categoria {
  id: number;
  nombre: string;
  descripcion: string;
  activo: boolean;
}

export interface Cliente {
  id: number;
  documento: string;
  nombre: string;
  telefono: string;
  email: string;
  direccion: string;
}

export interface Venta {
  id: number;
  numeroVenta: string;
  clienteId: number | null;
  fechaVenta: string;
  subTotal: number;
  impuesto: number;
  total: number;
  metodoPago: string;
  nombreCliente: string;
  documentoCliente?: string;
  telefonoCliente?: string;
  direccionCliente?: string;
  origen?: string;
  anulada?: boolean;
  motivoAnulacion?: string;
  estado?: string;
}

export interface DetalleVenta {
  productoId: number;
  cantidad: number;
  precioUnitario: number;
  costoUnitario: number;
  subTotal: number;
  nombreProducto?: string;
}

export interface ClienteAuthResponse {
  id: number;
  documento: string;
  nombre: string;
  telefono: string;
  email: string;
  direccion: string;
  token: string;
}

export interface Variante {
  id: number;
  productoId: number;
  nombre: string;
  colorHex: string;
  talla: string | null;
  stock: number | null;
  imagenUrl: string | null;
  activo: boolean;
  orden: number;
}

export interface RegistrarVentaDto {
  clienteId: number | null;
  metodoPago: string;
  montoPagado: number;
  cambio: number;
  usuario: string;
  domicilio: number;
  detalles: DetalleVenta[];
}

async function fetchApi<T>(endpoint: string, options?: RequestInit): Promise<T> {
  const cacheKey = apiCacheKey(endpoint, options);
  if (!options?.method || options.method === 'GET') {
    const cached = apiCacheGet<T>(cacheKey);
    if (cached) return cached;
  }
  const res = await fetch(`${API_URL}${endpoint}`, {
    ...options,
    headers: { 'Content-Type': 'application/json', ...options?.headers as Record<string, string> },
  });
  if (!res.ok) {
    const error = await res.text();
    let msg = 'Error en la peticion';
    try { const j = JSON.parse(error); msg = j.mensaje || j.title || msg; } catch { msg = error || msg; }
    throw new Error(msg);
  }
  const text = await res.text();
  const data = text ? JSON.parse(text) : null;
  if (!options?.method || options.method === 'GET') apiCacheSet(cacheKey, data);
  else if (endpoint.startsWith('/productos')) invalidateProductCache();
  else if (endpoint.startsWith('/categorias')) invalidateCategoryCache();
  return data as T;
}

async function uploadFile(file: File): Promise<string> {
  const token = typeof window !== 'undefined' ? localStorage.getItem('admin_token') : null;
  const formData = new FormData();
  formData.append('file', file);
  const res = await fetch(`${API_URL}/storage/upload`, {
    method: 'POST',
    headers: token ? { 'xAdminToken': token } : {},
    body: formData,
  });
  if (!res.ok) {
    const error = await res.text();
    let msg = 'Error al subir imagen';
    try { const j = JSON.parse(error); msg = j.mensaje || msg; } catch { msg = error || msg; }
    throw new Error(msg);
  }
  const data = await res.json();
  return data.url;
}

function adminHeaders(): Record<string, string> {
  if (typeof window === 'undefined') return {};
  const token = localStorage.getItem('admin_token');
  return token ? { 'xAdminToken': token } : {};
}

function clienteHeaders(): Record<string, string> {
  if (typeof window === 'undefined') return {};
  const token = localStorage.getItem('cliente_token');
  return token ? { 'xClienteToken': token } : {};
}

export const api = {
  productos: {
    getAll: () => fetchApi<Producto[]>('/productos'),
    getById: (id: number) => fetchApi<Producto>(`/productos/${id}`),
    getDetalle: (id: number) => fetchApi<{ producto: Producto; variantes: Variante[]; relacionados: Producto[] }>(`/productos/${id}/detalle`),
    getByCategoria: (categoriaId: number) => fetchApi<Producto[]>(`/productos/categoria/${categoriaId}`),
    search: (q: string) => fetchApi<Producto[]>(`/productos/buscar?q=${encodeURIComponent(q)}`),
    create: (dto: Partial<Producto>) =>
      fetchApi<Producto>('/productos', { method: 'POST', body: JSON.stringify(dto), headers: adminHeaders() }),
    update: (id: number, dto: Partial<Producto>) =>
      fetchApi<Producto>(`/productos/${id}`, { method: 'PUT', body: JSON.stringify(dto), headers: adminHeaders() }),
    delete: (id: number) =>
      fetchApi<void>(`/productos/${id}`, { method: 'DELETE', headers: adminHeaders() }),
    variantes: {
      getAll: () => fetchApi<Variante[]>('/productos/variantes'),
      getByProducto: (productoId: number) => fetchApi<Variante[]>(`/productos/${productoId}/variantes`),
      create: (productoId: number, dto: { nombre: string; colorHex: string; talla?: string | null; stock?: number | null; imagenUrl?: string | null; orden?: number }) =>
        fetchApi<Variante>(`/productos/${productoId}/variantes`, { method: 'POST', body: JSON.stringify(dto), headers: adminHeaders() }),
      update: (varianteId: number, dto: { nombre: string; colorHex: string; talla?: string | null; stock?: number | null; imagenUrl?: string | null; activo?: boolean; orden?: number }) =>
        fetchApi<Variante>(`/productos/variantes/${varianteId}`, { method: 'PUT', body: JSON.stringify(dto), headers: adminHeaders() }),
      delete: (varianteId: number) =>
        fetchApi<void>(`/productos/variantes/${varianteId}`, { method: 'DELETE', headers: adminHeaders() }),
    },
  },
  categorias: {
    getAll: (incluirInactivos = false) =>
      fetchApi<Categoria[]>(`/categorias${incluirInactivos ? '?incluirInactivos=true' : ''}`),
    create: (dto: Partial<Categoria>) =>
      fetchApi<Categoria>('/categorias', { method: 'POST', body: JSON.stringify(dto), headers: adminHeaders() }),
    update: (id: number, dto: Partial<Categoria>) =>
      fetchApi<Categoria>(`/categorias/${id}`, { method: 'PUT', body: JSON.stringify(dto), headers: adminHeaders() }),
  },
  clientes: {
    getAll: () => fetchApi<Cliente[]>('/clientes'),
    create: (cliente: Partial<Cliente>) =>
      fetchApi<Cliente>('/clientes', { method: 'POST', body: JSON.stringify(cliente) }),
    update: (id: number, cliente: Partial<Cliente>) =>
      fetchApi<Cliente>(`/clientes/${id}`, { method: 'PUT', body: JSON.stringify(cliente) }),
    login: (documento: string, contrasena: string) =>
      fetchApi<ClienteAuthResponse>('/clientes/login', { method: 'POST', body: JSON.stringify({ documento, contrasena }) }),
    registro: (cliente: { documento: string; contrasena: string; nombre: string; telefono: string; direccion: string }) =>
      fetchApi<ClienteAuthResponse>('/clientes/registro', { method: 'POST', body: JSON.stringify(cliente) }),
    changePassword: (nuevaContrasena: string) =>
      fetchApi<{ mensaje: string }>('/clientes/password', { method: 'PUT', body: JSON.stringify({ nuevaContrasena }), headers: clienteHeaders() }),
  },
  ventas: {
    getAll: () => fetchApi<Venta[]>('/ventas'),
    getById: (id: number) => fetchApi<{ venta: Venta; detalles: DetalleVenta[] }>(`/ventas/${id}`),
    registrar: (venta: RegistrarVentaDto) =>
      fetchApi<Venta>('/ventas', { method: 'POST', body: JSON.stringify(venta) }),
    misPedidos: () =>
      fetchApi<Venta[]>('/ventas/mis-pedidos', { headers: clienteHeaders() }),
    anular: (id: number, motivo: string) =>
      fetchApi<{ mensaje: string }>(`/ventas/${id}/anular`, { method: 'PUT', body: JSON.stringify({ motivo }), headers: adminHeaders() }),
    getPendientes: () =>
      fetchApi<Venta[]>('/ventas/pendientes', { headers: adminHeaders() }),
    confirmarPago: (id: number) =>
      fetchApi<{ mensaje: string }>(`/ventas/${id}/confirmar-pago`, { method: 'PATCH', headers: adminHeaders() }),
  },
  auth: {
    login: (documento: string, contrasena: string) =>
      fetchApi<{ id: number; nombres: string; apellidos: string; documento: string; rol: string; token: string }>(
        '/auth/login', { method: 'POST', body: JSON.stringify({ documento, contrasena }) }
      ),
    changePassword: (nuevaContrasena: string) =>
      fetchApi<{ mensaje: string }>('/auth/password', { method: 'PUT', body: JSON.stringify({ nuevaContrasena }), headers: adminHeaders() }),
  },
  configuracion: {
    getAll: () => fetchApi<{ id: number; clave: string; valor: string; descripcion: string }[]>('/configuracion'),
    getDiseno: () => fetchApi<{ id: number; clave: string; valor: string; descripcion: string }[]>('/configuracion/diseno', { headers: adminHeaders() }),
    update: (items: { clave: string; valor: string }[]) =>
      fetchApi<{ mensaje: string }>('/configuracion', { method: 'PUT', body: JSON.stringify(items), headers: adminHeaders() }),
    getPublic: () => fetchApi<{
      nombreTienda: string; slogan: string; heroTitle: string; heroSubtitle: string;
      telefono: string; direccion: string; ciudad: string; email: string;
      sitioWeb: string; horario: string; whatsapp: string; nit: string;
      domicilioCosto: string; domicilioGratisDesde: string; domicilioTiempoEstimado: string;
      colorPrincipal: string; colorSecundario: string; colorFondo: string;
      siteTitle: string; siteSubtitle: string; logo: string;
      qrBrebImg: string; brebLlave: string;
      bancoNombre: string; bancoTipoCuenta: string; bancoNumeroCuenta: string; bancoTitular: string;
      tarjetaInfo: string; tawktoPropertyId: string;
    }>('/configuracion/public'),
  },
  storage: {
    upload: uploadFile,
  },
};
