import { Pool } from 'pg';

const DATABASE_URL = 'postgresql://neondb_owner:npg_TJ4QN1xmzeFp@ep-ancient-credit-aicwplyb-pooler.c-4.us-east-1.aws.neon.tech/neondb?sslmode=require';

const pool = new Pool({ connectionString: DATABASE_URL });

export async function GET(
  _request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  const productId = parseInt(id, 10);
  if (isNaN(productId)) {
    return new Response('Invalid product ID', { status: 400 });
  }

  try {
    const result = await pool.query(
      'SELECT data, mimetype, filename FROM imagenes WHERE id = $1',
      [productId]
    );

    if (result.rows.length === 0) {
      return new Response('Not found', { status: 404 });
    }

    const row = result.rows[0];
    const rawData = row.data;
    const mimeType = row.mimetype || 'image/jpeg';
    const filename = row.filename || 'image.jpg';

    let bytes: Buffer;
    if (typeof rawData === 'string' && rawData.startsWith('\\x')) {
      bytes = Buffer.from(rawData.slice(2), 'hex');
    } else if (typeof rawData === 'string') {
      bytes = Buffer.from(rawData);
    } else if (rawData instanceof Buffer) {
      bytes = rawData;
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
  } catch (error) {
    console.error('Image fetch error:', error);
    return new Response('Internal error', { status: 500 });
  }
}
