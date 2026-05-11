import { readFileSync } from 'fs';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const RESEND_API_KEY = process.env.RESEND_API_KEY || 're_4BAnL44t_6A4jzx7GohdJLm4oXteXrh29';
const FROM = 'noreply@tucolmadord.com';
const TO = 'borrome941@gmail.com';
const API = 'https://api.resend.com/emails';

async function send(subject, html, label) {
  process.stdout.write(`\n📧 Enviando: ${label}...\n`);
  const res = await fetch(API, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${RESEND_API_KEY}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ from: FROM, to: [TO], subject, html }),
  });

  const data = await res.json();

  if (res.ok) {
    process.stdout.write(`   ✅ Enviado — ID: ${data.id}\n`);
    return { ok: true, id: data.id };
  } else {
    process.stdout.write(`   ❌ Error ${res.status}: ${JSON.stringify(data)}\n`);
    return { ok: false, error: data };
  }
}

const verifyEmailHtml = `<!DOCTYPE html><html lang="es"><head><meta charset="UTF-8"/><style>
body{font-family:-apple-system,sans-serif;background:#f4f7fb;margin:0}
.c{max-width:480px;margin:40px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.08)}
.h{background:linear-gradient(135deg,#1a2744,#0e1a38);padding:32px;text-align:center;color:#fff;font-size:22px;font-weight:700}
.h span{color:#3b82f6}.b{padding:40px 32px;color:#4b5563;line-height:1.6}
.code{background:#f0f4ff;border:2px solid #3b82f6;border-radius:10px;padding:20px;text-align:center;font-size:36px;font-weight:700;letter-spacing:10px;color:#1a2744;font-family:monospace;margin:20px 0}
.note{font-size:12px;color:#9ca3af;text-align:center}.f{background:#f9fafb;padding:20px 32px;text-align:center;border-top:1px solid #e5e7eb;font-size:12px;color:#9ca3af}
</style></head><body><div class="c">
<div class="h">TuColmado<span>RD</span></div>
<div class="b"><h2 style="color:#1a2744;margin:0 0 12px">Verifica tu correo</h2>
<p>Hola, gracias por registrar <strong>Colmado El Beta</strong>. Tu código de verificación es:</p>
<div class="code">847291</div>
<div class="note">Válido por 15 minutos</div></div>
<div class="f">&copy; 2026 TuColmado RD &middot; Todos los derechos reservados</div>
</div></body></html>`;

const betaWelcomeHtml = `<!DOCTYPE html><html lang="es"><head><meta charset="UTF-8"/><style>
body{font-family:-apple-system,sans-serif;background:#f4f7fb;margin:0}
.c{max-width:520px;margin:40px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.08)}
.h{background:linear-gradient(135deg,#1a2744,#0e1a38);padding:32px;text-align:center}
.logo{color:#fff;font-size:24px;font-weight:700}.logo span{color:#3b82f6}
.badge{display:inline-block;background:#3b82f6;color:#fff;font-size:11px;font-weight:700;letter-spacing:1px;padding:4px 12px;border-radius:20px;margin-top:10px}
.b{padding:36px 32px;color:#374151;line-height:1.7}
h2{color:#1a2744;margin:0 0 16px;font-size:20px}
.highlight{background:#f0f9ff;border-left:4px solid #3b82f6;padding:16px 20px;border-radius:0 8px 8px 0;margin:20px 0;color:#1e40af;font-weight:500}
.list{padding-left:20px;color:#4b5563}.list li{margin-bottom:8px}
.f{background:#f9fafb;padding:20px 32px;text-align:center;border-top:1px solid #e5e7eb;font-size:12px;color:#9ca3af}
</style></head><body><div class="c">
<div class="h">
  <div class="logo">TuColmado<span>RD</span></div>
  <span class="badge">BETA TESTER</span>
</div>
<div class="b">
  <h2>¡Felicidades! 🎊</h2>
  <p>Eres parte del grupo selecto de beta testers de <strong>TuColmado RD</strong>. Tu participación es clave para construir la mejor herramienta de gestión para colmados dominicanos.</p>
  <div class="highlight">Tu código de verificación es: <strong style="font-size:20px;letter-spacing:4px">847291</strong></div>
  <p>Como beta tester tendrás:</p>
  <ul class="list">
    <li>Acceso anticipado a todas las funciones nuevas</li>
    <li>Canal directo con el equipo de desarrollo</li>
    <li>Tu feedback moldeará el producto final</li>
    <li>Plan gratuito durante toda la fase beta</li>
  </ul>
  <p>Cualquier duda o sugerencia, escríbenos directamente. Estamos para acompañarte en cada paso.</p>
  <p style="color:#6b7280;font-size:14px">— El equipo de TuColmado RD 🇩🇴</p>
</div>
<div class="f">&copy; 2026 TuColmado RD &middot; Santo Domingo, República Dominicana</div>
</div></body></html>`;

process.stdout.write('🚀 Iniciando pruebas de correo con Resend\n');
process.stdout.write(`   From: ${FROM}\n`);
process.stdout.write(`   To:   ${TO}\n`);
process.stdout.write(`   Key:  ${RESEND_API_KEY.slice(0, 12)}...\n`);

const results = await Promise.all([
  send(
    'Tu código de verificación – TuColmado RD',
    verifyEmailHtml,
    'verify-email (código: 847291)',
  ),
  send(
    '¡Felicidades por unirte como Beta Tester de TuColmado RD! 🎉',
    betaWelcomeHtml,
    'beta-welcome (código + bienvenida)',
  ),
]);

process.stdout.write('\n─────────────────────────────────────\n');
const passed = results.filter(r => r.ok).length;
process.stdout.write(`Resultado: ${passed}/${results.length} correos enviados\n`);
if (passed < results.length) {
  process.stdout.write('⚠️  Verifica que el dominio tucolmadord.com esté verificado en Resend.\n');
  process.exit(1);
}
