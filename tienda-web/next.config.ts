import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  typescript: {
    ignoreBuildErrors: true,
  },
  images: {
    loader: 'custom',
    loaderFile: './src/lib/image-loader.ts',
    remotePatterns: [
      {
        protocol: 'http',
        hostname: 'localhost',
        port: '5063',
        pathname: '/api/storage/files/**',
      },
      {
        protocol: 'http',
        hostname: '127.0.0.1',
        port: '5063',
        pathname: '/api/storage/files/**',
      },
      {
        protocol: 'https',
        hostname: '**.supabase.co',
        pathname: '/storage/v1/**',
      },
      {
        protocol: 'https',
        hostname: 'www.masglo.com',
        pathname: '/**',
      },
    ],
  },

};

export default nextConfig;
