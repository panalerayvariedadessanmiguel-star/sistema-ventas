export default function IngresarLoading() {
  return (
    <div className="max-w-md mx-auto mt-12 space-y-4">
      <div className="h-8 w-48 bg-gray-200 rounded-lg animate-pulse mx-auto" />
      <div className="bg-white rounded-lg shadow p-6 space-y-4">
        <div className="h-10 w-full bg-gray-200 rounded-lg animate-pulse" />
        <div className="h-10 w-full bg-gray-200 rounded-lg animate-pulse" />
        <div className="h-12 w-full bg-gray-200 rounded-lg animate-pulse" />
      </div>
    </div>
  );
}
