'use client';

import { createContext, useContext, useState, useEffect, useCallback, ReactNode } from 'react';

interface ClienteData {
  id: number;
  documento: string;
  nombre: string;
  telefono: string;
  email: string;
  direccion: string;
}

interface ClienteContextType {
  cliente: ClienteData | null;
  token: string | null;
  login: (token: string, data: ClienteData) => void;
  logout: () => void;
  isLoggedIn: boolean;
}

const ClienteContext = createContext<ClienteContextType | undefined>(undefined);

const TOKEN_KEY = 'cliente_token';
const DATA_KEY = 'cliente_data';

export function ClienteProvider({ children }: { children: ReactNode }) {
  const [cliente, setCliente] = useState<ClienteData | null>(null);
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    const savedToken = localStorage.getItem(TOKEN_KEY);
    const savedData = localStorage.getItem(DATA_KEY);
    if (savedToken && savedData) {
      try {
        setToken(savedToken);
        setCliente(JSON.parse(savedData));
      } catch {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(DATA_KEY);
      }
    }
  }, []);

  const login = useCallback((newToken: string, data: ClienteData) => {
    setToken(newToken);
    setCliente(data);
    localStorage.setItem(TOKEN_KEY, newToken);
    localStorage.setItem(DATA_KEY, JSON.stringify(data));
  }, []);

  const logout = useCallback(() => {
    setToken(null);
    setCliente(null);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(DATA_KEY);
  }, []);

  return (
    <ClienteContext.Provider value={{ cliente, token, login, logout, isLoggedIn: !!token }}>
      {children}
    </ClienteContext.Provider>
  );
}

export function useCliente() {
  const ctx = useContext(ClienteContext);
  if (!ctx) throw new Error('useCliente debe usarse dentro de ClienteProvider');
  return ctx;
}
