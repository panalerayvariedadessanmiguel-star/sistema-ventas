import { neon } from '@neondatabase/serverless';

export const runtime = 'edge';

export async function GET(
  _request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  const productId = parseInt(id, 10);
  if (isNaN(productId)) {
    return new Response('Invalid product ID', { status: 400 });
  }

  const sql = neon(process.env.DATABASE_URL!);
  const rows = await sql(
    'SELECT data, mimetype, filename FROM imagenes WHERE productoid = $1 ORDER BY id LIMIT 1',
    [productId]
  );

  if (!rows || rows.length === 0) {
    return new Response('Not found', { status: 404 });
  }

  const row = rows[0] as Record<string, unknown>;
  const rawData = row.data as string;
  const mimeType = (row.mimetype as string) || 'image/jpeg';
  const filename = (row.filename as string) || 'image.jpg';

  const hexMatch = typeof rawData === 'string' && rawData.match(/^\\x([0-9a-f]+)$/i);
  let buffer: Buffer;
  if (hexMatch) {
    buffer = Buffer.from(hexMatch[1], 'hex');
  } else if (typeof rawData === 'string' && /^[A-Za-z0-9+/=]+$/.test(rawData)) {
    buffer = Buffer.from(rawData, 'base64');
  } else {
    buffer = Buffer.from(rawData);
  }

  return new Response(buffer, {
    status: 200,
    headers: {
      'Content-Type': mimeType,
      'Cache-Control': 'public, max-age=31536000, immutable',
      'Content-Disposition': `inline; filename="${filename}"`,
    },
  });
}
