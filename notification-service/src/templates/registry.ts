import { NotificationChannel } from "../domain/enums/notification-channel.enum";
import { OperationResult } from "../domain/result/operation-result";
import { NotificationDomainError, UnsupportedTemplateError } from "../domain/errors/notification-error";

export interface RenderedNotification {
  subject?: string;
  body: string;
}

type TemplateRenderer = (data: Record<string, string>) => RenderedNotification;

const LOGO_SVG = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="38" height="38" fill="none" style="display:inline-block;vertical-align:middle;margin-right:10px;margin-bottom:4px"><rect x="3" y="3" width="12" height="12" rx="2" stroke="#93c5fd" stroke-width="2.5" fill="none"/><rect x="9" y="9" width="12" height="12" rx="2" stroke="#fca5a5" stroke-width="2.5" fill="none"/></svg>`;

const HEADER_STYLES = `body{font-family:'Inter',-apple-system,BlinkMacSystemFont,sans-serif;background:#f8fafc;margin:0;padding:0}
.wrap{max-width:520px;margin:40px auto;background:#fff;border-radius:10px;overflow:hidden;box-shadow:0 10px 40px rgba(0,71,171,.08);border:1px solid #e2e8f0}
.h{background:#0047ab;padding:32px 32px;text-align:center;color:#fff;font-size:22px;font-weight:900;letter-spacing:-0.02em}
.h .accent{color:#e41e26}
.badge{display:inline-block;background:rgba(255,255,255,0.15);color:#fff;font-size:9px;font-weight:900;letter-spacing:2px;text-transform:uppercase;padding:5px 14px;border-radius:4px;margin-top:12px;border:1px solid rgba(255,255,255,0.2)}
.b{padding:40px 32px;color:#1e293b;line-height:1.6;font-size:15px}
h2{color:#0047ab;margin:0 0 16px;font-size:20px;font-weight:900;text-transform:uppercase;font-style:italic;letter-spacing:-0.01em}
.code-box{background:#f0f7ff;border:2px solid #0047ab;border-radius:6px;padding:24px 16px;text-align:center;font-size:42px;font-weight:900;letter-spacing:10px;color:#0047ab;font-family:monospace;margin:24px 0}
.note{font-size:11px;color:#94a3b8;text-align:center;margin-top:8px;font-weight:500;text-transform:uppercase;letter-spacing:1px}
.highlight{background:#f8fafc;border-left:4px solid #e41e26;padding:16px 20px;border-radius:0 6px 6px 0;margin:24px 0;color:#0f172a;font-weight:600}
.list{padding-left:20px;color:#475569;margin:16px 0}.list li{margin-bottom:10px}
.steps{list-style:none;padding:0;margin:20px 0}.steps li{display:flex;align-items:flex-start;margin-bottom:16px;gap:16px}
.step-num{flex-shrink:0;width:26px;height:26px;background:#0047ab;color:#fff;border-radius:4px;display:flex;align-items:center;justify-content:center;font-size:12px;font-weight:900;margin-top:2px}
.cta{display:block;background:#e41e26;color:#fff;text-decoration:none;padding:16px 28px;border-radius:6px;text-align:center;font-weight:900;margin:28px 0;font-size:14px;text-transform:uppercase;letter-spacing:1px}
.divider{border:none;border-top:1px solid #f1f5f9;margin:28px 0}
.f{background:#f8fafc;padding:24px 32px;text-align:center;border-top:1px solid #f1f5f9;font-size:11px;color:#64748b;font-weight:500;letter-spacing:0.5px}
.success-icon{text-align:center;font-size:48px;margin:8px 0 16px}`;

function emailHeader(badge?: string): string {
  return `<div class="h">TuColmado<span class="accent">RD</span>${badge ? `<br><span class="badge">${badge}</span>` : ''}</div>`;
}

function emailFooter(): string {
  return `<div class="f">&copy; 2026 TuColmado RD &middot; Santo Domingo, República Dominicana<br>¿Necesitas ayuda? Escríbenos a <a href="mailto:soporte@tucolmadord.com" style="color:#3b82f6;text-decoration:none">soporte@tucolmadord.com</a></div>`;
}

const EMAIL_TEMPLATES: Record<string, TemplateRenderer> = {
  'verify-email': (d) => ({
    subject: 'Tu código de verificación – TuColmado RD',
    body: `<!DOCTYPE html><html lang="es"><head><meta charset="UTF-8"/><meta name="viewport" content="width=device-width"/><style>${HEADER_STYLES}</style></head><body>
<div class="wrap">
  ${emailHeader()}
  <div class="b">
    <h2>Verifica tu correo electrónico</h2>
    <p>Hola, gracias por registrar <strong>${d['businessName'] || 'tu negocio'}</strong> en TuColmado RD. Ingresa el siguiente código para activar tu cuenta:</p>
    <div class="code-box">${d['code'] || '------'}</div>
    <div class="note">Válido por 15 minutos. No lo compartas con nadie.</div>
  </div>
  ${emailFooter()}
</div></body></html>`,
  }),

  'beta-welcome': (d) => ({
    subject: '¡Bienvenido como Beta Tester de TuColmado RD! 🎉',
    body: `<!DOCTYPE html><html lang="es"><head><meta charset="UTF-8"/><meta name="viewport" content="width=device-width"/><style>${HEADER_STYLES}</style></head><body>
<div class="wrap">
  ${emailHeader('BETA TESTER')}
  <div class="b">
    <h2>¡Felicidades${d['firstName'] ? ', ' + d['firstName'] : ''}! 🎊</h2>
    <p>Eres parte del grupo selecto de beta testers de <strong>TuColmado RD</strong>. Tu participación es clave para construir la mejor herramienta de gestión para colmados dominicanos.</p>
    <p>Primero, verifica tu correo con el siguiente código:</p>
    <div class="code-box">${d['code'] || '------'}</div>
    <div class="note">Código válido por 15 minutos.</div>
    <hr class="divider"/>
    <p><strong>Como beta tester tendrás acceso a:</strong></p>
    <ul class="list">
      <li>Acceso anticipado a todas las funciones nuevas</li>
      <li>Canal directo con el equipo de desarrollo</li>
      <li>Tu feedback moldeará el producto final</li>
      <li>Plan gratuito durante toda la fase beta</li>
    </ul>
    <p>Cualquier duda o sugerencia, escríbenos directamente. Estamos para acompañarte en cada paso.</p>
    <p style="color:#6b7280;font-size:14px">— El equipo de TuColmado RD 🇩🇴</p>
  </div>
  ${emailFooter()}
</div></body></html>`,
  }),

  'account-verified': (d) => ({
    subject: '¡Tu cuenta está activa! Bienvenido a TuColmado RD 🎉',
    body: `<!DOCTYPE html><html lang="es"><head><meta charset="UTF-8"/><meta name="viewport" content="width=device-width"/><style>${HEADER_STYLES}</style></head><body>
<div class="wrap">
  ${emailHeader()}
  <div class="b">
    <div class="success-icon">✅</div>
    <h2 style="text-align:center">¡Tu correo ha sido verificado!</h2>
    <p style="text-align:center">Hola${d['firstName'] ? ' <strong>' + d['firstName'] + '</strong>' : ''}, tu cuenta en TuColmado RD ya está activa y lista para usar.</p>
    <div class="highlight">
      Tu colmado <strong>${d['businessName'] || 'tu negocio'}</strong> ya está registrado en nuestra plataforma. ¡Empieza a gestionarlo ahora!
    </div>
    <p><strong>¿Qué puedes hacer ahora?</strong></p>
    <ul class="list">
      <li>Gestionar tu inventario de productos</li>
      <li>Registrar ventas y controlar caja</li>
      <li>Generar reportes de tu negocio</li>
      <li>Administrar empleados y turnos</li>
    </ul>
    <a href="https://app.tucolmadord.com" class="cta">Ir a la plataforma →</a>
    <p style="font-size:13px;color:#6b7280;text-align:center">¿Tienes preguntas? Estamos aquí para ayudarte.</p>
  </div>
  ${emailFooter()}
</div></body></html>`,
  }),

  'app-tutorial': (d) => ({
    subject: '¡Ya tienes la app! Aquí tu guía rápida 📱',
    body: `<!DOCTYPE html><html lang="es"><head><meta charset="UTF-8"/><meta name="viewport" content="width=device-width"/><style>${HEADER_STYLES}</style></head><body>
<div class="wrap">
  ${emailHeader()}
  <div class="b">
    <h2>¡Hola${d['firstName'] ? ' ' + d['firstName'] : ''}! Tu app está lista 🚀</h2>
    <p>Gracias por descargar TuColmado RD. Te dejamos una guía rápida para que arranques sin complicaciones:</p>
    <ul class="steps">
      <li><span class="step-num">1</span><span><strong>Inicia sesión</strong> con el correo y contraseña que usaste al registrarte en la plataforma web.</span></li>
      <li><span class="step-num">2</span><span><strong>Configura tu POS</strong> — ve a Configuración y selecciona tu terminal. Cada dispositivo funciona de forma independiente.</span></li>
      <li><span class="step-num">3</span><span><strong>Registra una venta</strong> — toca el botón "Nueva venta", agrega los productos y selecciona el método de pago.</span></li>
      <li><span class="step-num">4</span><span><strong>Cierra tu turno</strong> — al terminar el día, cierra el turno desde el menú principal para generar el resumen.</span></li>
      <li><span class="step-num">5</span><span><strong>Revisa los reportes</strong> — en la sección Reportes puedes ver ventas, caja y balance en tiempo real.</span></li>
    </ul>
    <a href="https://tucolmadord.com/guia.pdf" class="cta">📄 Descargar guía completa en PDF</a>
    <hr class="divider"/>
    <p style="font-size:13px;color:#6b7280;text-align:center">¿Necesitas ayuda con algo más? Escríbenos a <a href="mailto:soporte@tucolmadord.com" style="color:#3b82f6;text-decoration:none">soporte@tucolmadord.com</a></p>
  </div>
  ${emailFooter()}
</div></body></html>`,
  }),

  'welcome': (d) => ({
    subject: `¡Bienvenido a TuColmado RD, ${d['firstName'] || ''}!`,
    body: `<p>Hola ${d['firstName'] || 'usuario'},</p><p>Tu cuenta está activa. ¡Empieza a gestionar tu colmado!</p>`,
  }),

  'shift-summary': (d) => ({
    subject: `Resumen de turno – ${d['date'] || ''}`,
    body: `<p>Turno cerrado por <strong>${d['cashier'] || ''}</strong>.<br>Total vendido: ${d['total'] || '0'}.</p>`,
  }),
};

const SMS_TEMPLATES: Record<string, TemplateRenderer> = {
  'verify-email': (d) => ({
    body: `Tu código de verificación TuColmado RD es: ${d['code'] || '------'}. Válido 15 min.`,
  }),

  'welcome': (d) => ({
    body: `Hola ${d['firstName'] || ''}! Tu cuenta en TuColmado RD está activa.`,
  }),

  'account-verified': (d) => ({
    body: `¡Tu cuenta TuColmado RD está activa! Entra en app.tucolmadord.com y empieza a gestionar tu colmado.`,
  }),
};

const TEMPLATE_REGISTRY: Record<NotificationChannel, Record<string, TemplateRenderer>> = {
  [NotificationChannel.EMAIL]: EMAIL_TEMPLATES,
  [NotificationChannel.SMS]:   SMS_TEMPLATES,
  [NotificationChannel.PUSH]:  {},
};

export function renderTemplate(
  channel: NotificationChannel,
  templateId: string,
  data: Record<string, string>,
): OperationResult<RenderedNotification, NotificationDomainError> {
  const renderer = TEMPLATE_REGISTRY[channel]?.[templateId];
  if (!renderer) {
    return OperationResult.fail(new UnsupportedTemplateError());
  }
  return OperationResult.ok(renderer(data));
}
