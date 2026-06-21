export default function ProductoLoading() {
  return (
    <div className="max-w-6xl mx-auto grid grid-cols-1 md:grid-cols-2 gap-8">
      <div className="aspect-square bg-gray-200 rounded-xl animate-pulse" />
      <div className="space-y-4">
        <div className="h-4 w-20 bg-gray-200 rounded-full animate-pulse" />
        <div className="h-8 w-3/4 bg-gray-200 rounded-lg animate-pulse" />
        <div className="h-4 w-full bg-gray-200 rounded animate-pulse" />
        <div className="h-4 w-2/3 bg-gray-200 rounded animate-pulse" />
        <div className="h-10 w-40 bg-gray-200 rounded-lg animate-pulse mt-6" />
        <div className="h-12 w-full bg-gray-200 rounded-lg animate-pulse mt-4" />
      </div>
    </div>
  );
}
