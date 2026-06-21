'use client';

import { createContext, useContext, useState, useEffect, useCallback, ReactNode } from 'react';

interface StoreConfig {
  nombreTienda: string;
  slogan: string;
  heroTitle: string;
  heroSubtitle: string;
  telefono: string;
  direccion: string;
  ciudad: string;
  email: string;
  sitioWeb: string;
  horario: string;
  whatsapp: string;
  nit: string;
  domicilioCosto: string;
  domicilioGratisDesde: string;
  domicilioTiempoEstimado: string;
  colorPrincipal: string;
  colorSecundario: string;
  colorFondo: string;
  siteTitle: string;
  siteSubtitle: string;
  logo: string;
  qrBrebImg: string;
  brebLlave: string;
  bancoNombre: string;
  bancoTipoCuenta: string;
  bancoNumeroCuenta: string;
  bancoTitular: string;
  tarjetaInfo: string;
  tawktoPropertyId: string;
}

const defaults: StoreConfig = {
  nombreTienda: 'Mi Tienda Online',
  slogan: 'Tu tienda de confianza',
  heroTitle: 'Los mejores productos para ti',
  heroSubtitle: 'Encuentra todo lo que necesitas',
  telefono: '',
  direccion: '',
  ciudad: '',
  email: '',
  sitioWeb: '',
  horario: '',
  whatsapp: '',
  nit: '',
  domicilioCosto: '5000',
  domicilioGratisDesde: '0',
  domicilioTiempoEstimado: '2-5 dias habiles',
  colorPrincipal: '#3B82F6',
  colorSecundario: '#059669',
  colorFondo: '#F9FAFB',
  siteTitle: 'Bienvenido a Nuestra Tienda',
  siteSubtitle: 'Descubre nuestros productos exclusivos',
  logo: '',
  qrBrebImg: '',
  brebLlave: '',
  bancoNombre: '',
  bancoTipoCuenta: 'Ahorros',
  bancoNumeroCuenta: '',
  bancoTitular: '',
  tarjetaInfo: 'Paga con tarjeta debito o credito en la entrega',
  tawktoPropertyId: '',
};

type StoreContextValue = StoreConfig & { refreshConfig: () => void };

const StoreContext = createContext<StoreContextValue>({ ...defaults, refreshConfig: () => {} });

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5062/api';

async function fetchConfig(): Promise<StoreConfig> {
  const res = await fetch(`${API_URL}/configuracion/public`);
  return res.json();
}

function applyColors(data: StoreConfig) {
  const root = document.documentElement;
  root.style.setProperty('--color-principal', data.colorPrincipal);
  root.style.setProperty('--color-secundario', data.colorSecundario);
  root.style.setProperty('--color-fondo', data.colorFondo);
}

export function StoreProvider({ children }: { children: ReactNode }) {
  const [config, setConfig] = useState<StoreConfig>(defaults);

  const doFetch = useCallback(async () => {
    try {
      const data = await fetchConfig();
      setConfig({ ...defaults, ...data });
      applyColors(data);
    } catch { }
  }, []);

  useEffect(() => {
    doFetch();
  }, [doFetch]);

  return (
    <StoreContext.Provider value={{ ...config, refreshConfig: doFetch }}>
      {children}
    </StoreContext.Provider>
  );
}

export function useStore() {
  return useContext(StoreContext);
}
