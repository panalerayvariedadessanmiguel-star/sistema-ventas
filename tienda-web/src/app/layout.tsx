import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { AdminProvider } from "@/lib/admin-context";
import { CarritoProvider } from "@/lib/carrito-context";
import { StoreProvider } from "@/lib/store-context";
import { ToastProvider } from "@/lib/toast-context";
import { ClienteProvider } from "@/lib/cliente-context";
import Header from "@/components/Header";
import Footer from "@/components/Footer";
import ScrollToTop from "@/components/ScrollToTop";
import WhatsAppFloat from "@/components/WhatsAppFloat";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Pañalera y Variedades San Miguel",
  description: "Tu tienda de confianza en Bogotá - Belleza, Papelería, Regalos, Ropa y más. Envíos a domicilio.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="es" className={`${geistSans.variable} ${geistMono.variable} h-full antialiased`}>
      <body className="min-h-full flex flex-col" style={{ backgroundColor: 'var(--color-fondo, #F9FAFB)' }}>
        <AdminProvider>
          <StoreProvider>
            <CarritoProvider>
              <ClienteProvider>
                <ToastProvider>
                <Header />
                <main className="flex-1 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 w-full">
                  {children}
                </main>
                <Footer />
                <ScrollToTop />
                <WhatsAppFloat />
              </ToastProvider>
              </ClienteProvider>
            </CarritoProvider>
          </StoreProvider>
        </AdminProvider>
      </body>
    </html>
  );
}
