import { neon } from '@neondatabase/serverless';

const DATABASE_URL = 'postgresql://neondb_owner:npg_TJ4QN1xmzeFp@ep-ancient-credit-aicwplyb-pooler.c-4.us-east-1.aws.neon.tech/neondb?sslmode=require';

export async function GET(
  _request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  const productId = parseInt(id, 10);
  if (isNaN(productId)) {
    return new Response('Invalid product ID', { status: 400 });
  }

  const sql = neon(DATABASE_URL);
  const rows = await sql(
    'SELECT data, mimetype, filename FROM imagenes WHERE productoid = $1 ORDER BY id LIMIT 1',
    [productId]
  );

  if (!rows || rows.length === 0) {
    return new Response('Not found', { status: 404 });
  }

  const row = rows[0] as Record<string, unknown>;
  const rawData = row.data;
  const mimeType = (row.mimetype as string) || 'image/jpeg';
  const filename = (row.filename as string) || 'image.jpg';

  let bytes: Buffer;
  if (typeof rawData === 'string' && rawData.startsWith('\\x')) {
    bytes = Buffer.from(rawData.slice(2), 'hex');
  } else if (typeof rawData === 'string') {
    bytes = Buffer.from(rawData, 'base64');
  } else if (rawData instanceof Uint8Array) {
    bytes = Buffer.from(rawData);
  } else {
    bytes = Buffer.from(String(rawData));
  }

  return new Response(bytes, {
    status: 200,
    headers: {
      'Content-Type': mimeType,
      'Cache-Control': 'public, max-age=31536000, immutable',
    },
  });
}
