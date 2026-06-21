export default function imageLoader({ src }: { src: string; width: number; quality?: number }) {
  const supabasePattern = /https?:\/\/[^/]+\/storage\/v1\/object\/public\/[^/]+\/(.+)/;
  const match = src.match(supabasePattern);
  if (match) {
    return `/api/storage/files/${match[1]}`;
  }
  if (src.startsWith('http') || src.startsWith('/')) {
    return src;
  }
  return src;
}
