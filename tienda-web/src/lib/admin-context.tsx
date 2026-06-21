'use client';

import { createContext, useContext, useState, useEffect, ReactNode } from 'react';

interface AdminUser {
  id: number;
  nombres: string;
  apellidos: string;
  documento: string;
  rol: string;
  token: string;
}

interface AdminContextType {
  user: AdminUser | null;
  login: (user: AdminUser) => void;
  logout: () => void;
  isAdmin: boolean;
  ready: boolean;
}

const AdminContext = createContext<AdminContextType | undefined>(undefined);

export function AdminProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AdminUser | null>(null);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    const stored = localStorage.getItem('admin_user');
    const token = localStorage.getItem('admin_token');
    if (stored && token) {
      try { setUser(JSON.parse(stored)); } catch { localStorage.removeItem('admin_user'); localStorage.removeItem('admin_token'); }
    }
    setReady(true);
  }, []);

  const login = (u: AdminUser) => {
    setUser(u);
    localStorage.setItem('admin_user', JSON.stringify(u));
    localStorage.setItem('admin_token', u.token);
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem('admin_user');
    localStorage.removeItem('admin_token');
  };

  const isAdmin = user?.rol === 'Administrador';

  return (
    <AdminContext.Provider value={{ user, login, logout, isAdmin, ready }}>
      {children}
    </AdminContext.Provider>
  );
}

export function useAdmin() {
  const ctx = useContext(AdminContext);
  if (!ctx) throw new Error('useAdmin debe usarse dentro de AdminProvider');
  return ctx;
}
