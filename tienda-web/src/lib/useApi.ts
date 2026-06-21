'use client';

import { useState, useEffect, useCallback, useRef } from 'react';

interface UseApiResult<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
  refetch: () => void;
}

export function useApi<T>(
  fetcher: () => Promise<T>,
  deps: unknown[] = []
): UseApiResult<T> {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const fetcherRef = useRef(fetcher);
  fetcherRef.current = fetcher;

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);

    fetcherRef.current()
      .then(result => { if (!cancelled) { setData(result); setLoading(false); } })
      .catch(e => { if (!cancelled) { setError(e instanceof Error ? e.message : 'Error inesperado'); setLoading(false); } });

    return () => { cancelled = true; };
  }, deps);

  const refetch = useCallback(() => {
    setLoading(true);
    setError(null);
    fetcherRef.current()
      .then(result => { setData(result); setLoading(false); })
      .catch(e => { setError(e instanceof Error ? e.message : 'Error inesperado'); setLoading(false); });
  }, []);

  return { data, loading, error, refetch };
}
