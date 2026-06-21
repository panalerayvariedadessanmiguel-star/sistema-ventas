interface CacheEntry<T> {
  data: T;
  expiry: number;
}

const store = new Map<string, CacheEntry<unknown>>();
const DEFAULT_TTL = 2 * 60 * 1000;

export function apiCacheGet<T>(key: string): T | undefined {
  const entry = store.get(key);
  if (!entry) return undefined;
  if (Date.now() > entry.expiry) { store.delete(key); return undefined; }
  return entry.data as T;
}

export function apiCacheSet<T>(key: string, data: T, ttl = DEFAULT_TTL): void {
  store.set(key, { data, expiry: Date.now() + ttl });
}

export function apiCacheInvalidate(prefix?: string): void {
  if (!prefix) { store.clear(); return; }
  for (const key of store.keys()) {
    if (key.startsWith(prefix)) store.delete(key);
  }
}

export function apiCacheKey(endpoint: string, options?: RequestInit): string {
  return `${options?.method || 'GET'}|${endpoint}`;
}
