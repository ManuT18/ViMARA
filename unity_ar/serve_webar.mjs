import https from 'node:https';
import fs from 'node:fs';
import path from 'node:path';
import os from 'node:os';
import { fileURLToPath } from 'node:url';
import selfsigned from 'selfsigned';
import qrcode from 'qrcode-terminal';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const PORT = 8443;
const BUILDS_DIR = path.join(__dirname, 'Builds');

// 1. Obtener la IP local de la red LAN
function getLocalIP() {
    const interfaces = os.networkInterfaces();
    for (const name of Object.keys(interfaces)) {
        for (const iface of interfaces[name]) {
            if (iface.family === 'IPv4' && !iface.internal && iface.address.startsWith('192.168.')) {
                return iface.address;
            }
        }
    }
    for (const name of Object.keys(interfaces)) {
        for (const iface of interfaces[name]) {
            if (iface.family === 'IPv4' && !iface.internal) {
                return iface.address;
            }
        }
    }
    return 'localhost';
}

const localIP = getLocalIP();

// 2. Generar certificado SSL autofirmado
console.log('🔒 Generando certificado SSL autofirmado para WebAR...');
const attrs = [
    { name: 'commonName', value: localIP },
    { name: 'countryName', value: 'AR' },
    { name: 'organizationName', value: 'ViMARA' },
    { shortName: 'OU', value: 'AR Dev' }
];

const pems = selfsigned.generate(attrs, {
    days: 30,
    keySize: 2048,
    extensions: [
        {
            name: 'subjectAltName',
            altNames: [
                { type: 2, value: 'localhost' },
                { type: 7, ip: localIP },
                { type: 7, ip: '127.0.0.1' }
            ]
        }
    ]
});

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

// 3. Crear servidor HTTPS con soporte de compresión Unity WebGL
const server = https.createServer({ key: pems.private, cert: pems.cert }, (req, res) => {
    let reqPath = req.url.split('?')[0];
    if (reqPath === '/' || reqPath === '') reqPath = '/index.html';

    const filePath = path.join(BUILDS_DIR, decodeURIComponent(reqPath));

    // Headers CORS y Cache para WebAR
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

function startServer(port) {
    server.listen(port, '0.0.0.0', () => {
        const mobileUrl = `https://${localIP}:${port}`;
        const localUrl = `https://localhost:${port}`;

        console.clear();
        console.log('\n======================================================');
        console.log('🚀 SERVIDOR LOCAL HTTPS WEBAR INICIADO CON ÉXITO');
        console.log('======================================================\n');
        console.log(`💻 Local (en esta laptop):  ${localUrl}`);
        console.log(`📱 Celular (en red Wi-Fi):   ${mobileUrl}\n`);
        console.log('📲 Escaneá este código QR con tu celular:\n');

        qrcode.generate(mobileUrl, { small: true });

        console.log('\n======================================================');
        console.log('⚠️  IMPORTANTE EN EL CELULAR:');
        console.log('1. Conectá el celular a la misma red Wi-Fi que la laptop.');
        console.log('2. Al abrir el enlace, el navegador avisará "La conexión no es privada".');
        console.log('3. Tocá "Avanzado" -> "Continuar a ' + localIP + ' (no seguro)".');
        console.log('4. Concedé permisos de CÁMARA y SENSORES (giroscopio).');
        console.log('======================================================\n');
    });
}

server.on('error', (err) => {
    if (err.code === 'EADDRINUSE') {
        console.log(`⚠️  Puerto ${PORT} en uso, intentando con el puerto ${PORT + 1}...`);
        startServer(PORT + 1);
    } else {
        console.error('Error en el servidor HTTPS:', err);
    }
});

startServer(PORT);
