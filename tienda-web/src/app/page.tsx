'use client';

import ProductList from "@/components/ProductList";
import { useStore } from "@/lib/store-context";

export default function Home() {
  const { heroTitle, heroSubtitle, colorPrincipal, colorFondo, domicilioTiempoEstimado } = useStore();

  return (
    <div>
      <div className="relative mb-10 rounded-2xl overflow-hidden border shadow-2xl" style={{ borderColor: `${colorPrincipal}30` }}>
        <div className="absolute inset-0" style={{ background: `linear-gradient(135deg, ${colorPrincipal}20, ${colorFondo})` }} />
        <div className="absolute inset-0 opacity-[0.04]" style={{ backgroundImage: `url("data:image/svg+xml,%3Csvg width='80' height='80' viewBox='0 0 80 80' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='${colorPrincipal.replace('#', '%23')}'%3E%3Ccircle cx='40' cy='12' r='3'/%3E%3Ccircle cx='12' cy='40' r='3'/%3E%3Ccircle cx='68' cy='40' r='3'/%3E%3Ccircle cx='40' cy='68' r='3'/%3E%3Cpath d='M40 28l4 8-4 8-4-8z'/%3E%3C/g%3E%3C/svg%3E")`, backgroundSize: '80px 80px' }} />
        <div className="relative z-10 p-10 text-center">
          <div className="absolute top-6 left-6 w-24 h-24 rounded-full opacity-10" style={{ backgroundColor: colorPrincipal }} />
          <div className="absolute bottom-6 right-6 w-40 h-40 rounded-full opacity-10" style={{ backgroundColor: colorPrincipal }} />
          <div className="absolute top-1/2 right-12 w-5 h-5 rotate-45 border-2 rounded-sm opacity-20" style={{ borderColor: colorPrincipal }} />
          <div className="absolute bottom-12 left-12 w-3 h-3 rotate-12 opacity-15" style={{ backgroundColor: colorPrincipal, clipPath: 'polygon(50% 0%, 100% 50%, 50% 100%, 0% 50%)' }} />
          <div className="absolute top-20 right-20 w-2 h-2 rounded-full opacity-30" style={{ backgroundColor: colorPrincipal }} />
          <div className="absolute bottom-20 left-1/3 w-1.5 h-1.5 rounded-full opacity-25" style={{ backgroundColor: colorPrincipal }} />
          <h1 className="text-4xl sm:text-5xl font-extrabold tracking-tight leading-tight text-gray-900 mb-3" style={{ color: colorPrincipal }}>
            {heroTitle}
          </h1>
          {heroSubtitle && <p className="text-lg sm:text-xl text-gray-600 max-w-2xl mx-auto">{heroSubtitle}</p>}
          <p className="text-yellow-700 text-sm font-medium mt-2">🚚 Solo entregas en Bogota</p>
          {domicilioTiempoEstimado && <p className="text-yellow-600 text-xs mt-1">📬 Entrega estimada: {domicilioTiempoEstimado}</p>}
        </div>
        <svg className="absolute bottom-0 left-0 w-full h-10 text-white" viewBox="0 0 1440 60" preserveAspectRatio="none" fill="currentColor" opacity="0.3">
          <path d="M0 60h1440V38c-160-28-320-42-480-42S480 10 320 38 160 60 0 60z"/>
        </svg>
      </div>
      <ProductList />
    </div>
  );
}
