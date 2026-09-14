import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import os from 'node:os';
import { fileURLToPath } from 'node:url';
import { startTunnel } from 'untun';
import qrcode from 'qrcode-terminal';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const PORT = 8080;
const BUILDS_DIR = path.join(__dirname, 'Builds');

// MIME types para Unity WebGL
const mimeTypes = {
    '.html': 'text/html',
    '.js': 'application/javascript',
    '.css': 'text/css',
    '.json': 'application/json',
    '.png': 'image/png',
    '.jpg': 'image/jpeg',
    '.svg': 'image/svg+xml',
    '.ico': 'image/x-icon',
    '.wasm': 'application/wasm',
    '.data': 'application/octet-stream',
    '.zpt': 'application/octet-stream',
    '.zbin': 'application/octet-stream'
};

// 1. Crear servidor HTTP local
const server = http.createServer((req, res) => {
    let reqPath = req.url.split('?')[0];
    if (reqPath === '/' || reqPath === '') reqPath = '/index.html';

    const filePath = path.join(BUILDS_DIR, decodeURIComponent(reqPath));

    // Headers CORS y aislamiento para WebAR
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Cross-Origin-Opener-Policy', 'same-origin');
    res.setHeader('Cross-Origin-Embedder-Policy', 'require-corp');

    // Manejo de archivos comprimidos Brotli / Gzip de Unity
    if (filePath.endsWith('.br')) {
        res.setHeader('Content-Encoding', 'br');
        const ext = path.extname(filePath.slice(0, -3));
        res.setHeader('Content-Type', mimeTypes[ext] || 'application/octet-stream');
    } else if (filePath.endsWith('.gz')) {
        res.setHeader('Content-Encoding', 'gzip');
        const ext = path.extname(filePath.slice(0, -3));
        res.setHeader('Content-Type', mimeTypes[ext] || 'application/octet-stream');
    } else {
        const ext = path.extname(filePath);
        res.setHeader('Content-Type', mimeTypes[ext] || 'application/octet-stream');
    }

    fs.stat(filePath, (err, stats) => {
        if (err || !stats.isFile()) {
            res.writeHead(404, { 'Content-Type': 'text/plain' });
            res.end(`404 Not Found: ${reqPath}`);
            return;
        }

        res.writeHead(200);
        const stream = fs.createReadStream(filePath);
        stream.pipe(res);
    });
});

function start(port) {
    server.listen(port, '0.0.0.0', async () => {
        console.clear();
        console.log('\n======================================================');
        console.log('🚀 INICIANDO TUNEL HTTPS SEGURO PARA IPHONE / ANDROID...');
        console.log('======================================================\n');

        try {
            const tunnel = await startTunnel({ port });
            const tunnelUrl = await tunnel.getURL();

            console.clear();
            console.log('\n======================================================');
            console.log('✨ SERVIDOR WEBAR EN VIVO CON HTTPS VÁLIDO (CLOUDFLARE)');
            console.log('======================================================\n');
            console.log(`💻 Local (Laptop):      http://localhost:${port}`);
            console.log(`📱 iPhone / Android:    ${tunnelUrl}\n`);
            console.log('📲 Escaneá este código QR con la cámara de tu iPhone:\n');

            qrcode.generate(tunnelUrl, { small: true });

            console.log('\n======================================================');
            console.log('✅ Safari / Chrome abrirán la página de inmediato.');
            console.log('✅ Al pedir permisos, aceptá CÁMARA y GIROSCOPIO.');
            console.log('======================================================\n');
        } catch (tunnelError) {
            console.error('No se pudo iniciar el túnel automático:', tunnelError.message);
            console.log(`Podés acceder localmente en http://localhost:${port}`);
        }
    });
}

server.on('error', (err) => {
    if (err.code === 'EADDRINUSE') {
        start(PORT + 1);
    } else {
        console.error('Error en el servidor:', err);
    }
});

start(PORT);
