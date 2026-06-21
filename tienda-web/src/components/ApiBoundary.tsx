'use client';

import { ReactNode } from 'react';

interface ApiBoundaryProps {
  loading: boolean;
  error: string | null;
  children: ReactNode;
  loader?: ReactNode;
  onRetry?: () => void;
}

export function ApiBoundary({ loading, error, children, loader, onRetry }: ApiBoundaryProps) {
  if (loading) {
    return loader ?? (
      <div className="flex justify-center py-20">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-center px-4">
        <svg className="w-16 h-16 text-red-300 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4.5c-.77-.833-2.694-.833-3.464 0L3.34 16.5c-.77.833.192 2.5 1.732 2.5z" />
        </svg>
        <p className="text-gray-700 font-medium mb-1">Algo salio mal</p>
        <p className="text-gray-500 text-sm mb-4 max-w-sm">{error}</p>
        {onRetry && (
          <button onClick={onRetry} className="px-5 py-2 bg-blue-600 text-white rounded-lg text-sm font-medium hover:bg-blue-700 transition-colors">
            Reintentar
          </button>
        )}
      </div>
    );
  }

  return <>{children}</>;
}
