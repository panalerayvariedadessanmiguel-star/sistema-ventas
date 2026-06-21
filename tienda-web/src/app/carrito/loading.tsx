export default function CarritoLoading() {
  return (
    <div className="max-w-4xl mx-auto">
      <div className="h-9 w-40 bg-gray-200 rounded-lg animate-pulse mb-8" />
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2 space-y-3">
          {[1, 2, 3].map(i => (
            <div key={i} className="bg-white rounded-lg shadow-sm border border-gray-100 p-3 flex items-center gap-3">
              <div className="w-16 h-16 rounded-lg bg-gray-200 animate-pulse" />
              <div className="flex-1 space-y-2">
                <div className="h-4 w-3/4 bg-gray-200 rounded animate-pulse" />
                <div className="h-3 w-1/4 bg-gray-200 rounded animate-pulse" />
              </div>
              <div className="h-8 w-20 bg-gray-200 rounded-lg animate-pulse" />
              <div className="h-5 w-16 bg-gray-200 rounded animate-pulse" />
            </div>
          ))}
        </div>
        <div className="bg-white rounded-lg shadow p-5 space-y-4">
          <div className="h-6 w-24 bg-gray-200 rounded animate-pulse" />
          <div className="space-y-2">
            <div className="h-4 w-full bg-gray-200 rounded animate-pulse" />
            <div className="h-4 w-full bg-gray-200 rounded animate-pulse" />
            <div className="h-4 w-3/4 bg-gray-200 rounded animate-pulse" />
          </div>
          <div className="h-12 w-full bg-gray-200 rounded-lg animate-pulse" />
        </div>
      </div>
    </div>
  );
}
