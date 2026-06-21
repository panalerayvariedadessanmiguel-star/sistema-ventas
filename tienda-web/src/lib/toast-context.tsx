'use client';

import { createContext, useContext, useState, useCallback, ReactNode } from 'react';

interface Toast {
  id: number;
  message: string;
  type?: 'success' | 'error' | 'info';
}

interface ToastContextType {
  toast: (message: string, type?: Toast['type']) => void;
}

const ToastContext = createContext<ToastContextType | undefined>(undefined);

let nextId = 0;

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const toast = useCallback((message: string, type: Toast['type'] = 'success') => {
    const id = nextId++;
    setToasts(prev => [...prev, { id, message, type }]);
    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== id));
    }, 3000);
  }, []);

  const remove = useCallback((id: number) => {
    setToasts(prev => prev.filter(t => t.id !== id));
  }, []);

  return (
    <ToastContext.Provider value={{ toast }}>
      {children}
      <div className="fixed bottom-24 right-6 z-[100] flex flex-col gap-2 pointer-events-none">
        {toasts.map(t => (
          <div
            key={t.id}
            onClick={() => remove(t.id)}
            className={`pointer-events-auto cursor-pointer animate-[fadeIn_0.2s_ease-out] px-4 py-2.5 rounded-lg shadow-lg text-sm font-medium transition-all duration-300 ${
              t.type === 'error'
                ? 'bg-red-600 text-white'
                : t.type === 'info'
                ? 'bg-blue-600 text-white'
                : 'bg-green-600 text-white'
            }`}
          >
            {t.message}
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast debe usarse dentro de ToastProvider');
  return ctx;
}
