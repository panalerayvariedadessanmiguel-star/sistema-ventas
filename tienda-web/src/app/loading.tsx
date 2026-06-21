export default function RootLoading() {
  return (
    <div className="flex items-center justify-center py-32">
      <div className="flex flex-col items-center gap-4">
        <div className="relative w-12 h-12">
          <div className="absolute inset-0 border-4 border-gray-200 rounded-full" />
          <div className="absolute inset-0 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
        </div>
        <p className="text-sm text-gray-500 animate-pulse">Cargando...</p>
      </div>
    </div>
  );
}
