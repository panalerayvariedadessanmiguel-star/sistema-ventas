'use client';

import { useEffect } from 'react';
import { useStore } from '@/lib/store-context';

export default function TawkToChat() {
  const { tawktoPropertyId } = useStore();

  useEffect(() => {
    if (!tawktoPropertyId) return;
    const s1 = document.createElement('script');
    const s0 = document.getElementsByTagName('script')[0];
    s1.async = true;
    s1.src = `https://embed.tawk.to/${tawktoPropertyId}/default`;
    s1.charset = 'UTF-8';
    s1.setAttribute('crossorigin', '*');
    s0?.parentNode?.insertBefore(s1, s0);
    return () => {
      s1.remove();
    };
  }, [tawktoPropertyId]);

  return null;
}
