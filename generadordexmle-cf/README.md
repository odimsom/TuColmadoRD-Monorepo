# Generador de XML e-CF (DGII – República Dominicana)

Convierte un archivo Excel (.xlsx) en XMLs firmables para los Comprobantes Fiscales Electrónicos (e-CF) de la DGII. Cada fila del Excel genera un archivo XML nombrado con el valor de la columna **ENCF**.

---

## Requisitos

- Python 3.10 o superior
- Conexión a internet (solo para instalar dependencias)

---

## Instalación

```bash
# 1. Clonar o descargar la carpeta scripphy
# 2. Abrir una terminal en esa carpeta y ejecutar:
pip install -r requirements.txt
```

---

## Ejecutar la aplicación

```bash
python app.py
```

Luego abrir en el navegador:

```
http://localhost:5000
```

---

## Cómo usar la interfaz

1. **Descargar la plantilla** — haz clic en el botón *"Descargar plantilla"* para obtener el archivo `plantilla_ecf.xlsx` con todas las columnas disponibles.
2. **Llenar el Excel** — cada fila es un comprobante. Las celdas vacías se omiten del XML (no genera etiquetas en blanco).
3. **Subir el archivo** — arrastra el `.xlsx` al área de carga o usa el botón de selección.
4. **Convertir** — haz clic en *"Convertir a XML"*.
5. **Descargar el ZIP** — recibirás `ecf_generados.zip` con un XML por fila, nombrado `{ENCF}.xml` (ej: `E310000000001.xml`).

> Si alguna fila falla, el ZIP incluye un archivo `errores.txt` con el detalle por número de fila.

---

## Formato del Excel

### Columnas obligatorias mínimas

| Columna | Descripción |
|---------|-------------|
| `ENCF` | Número de comprobante (ej: `E310000000001`). Define también el nombre del archivo XML. |
| `TipoeCF` | Tipo de e-CF (31, 32, 33, 34, 41, 43, 44, 45, 46, 47). **Opcional si ENCF empieza por `Exx`** — se auto-detecta. |
| `FechaEmision` | Fecha de emisión (`DD-MM-AAAA`) |
| `RNCEmisor` | RNC del emisor |
| `RazonSocialEmisor` | Razón social del emisor |
| `TipoIngresos` | Código del tipo de ingresos |
| `TipoPago` | Código del tipo de pago |
| `MontoTotal` | Monto total del comprobante |
| `ValorPagar` | Valor a pagar |
| `Item_1_NumeroLinea` | Número de línea del primer ítem (inicia la secuencia de ítems) |
| `Item_1_NombreItem` | Nombre del ítem |
| `Item_1_CantidadItem` | Cantidad |
| `Item_1_PrecioUnitarioItem` | Precio unitario |
| `Item_1_MontoItem` | Monto del ítem |

### Convención de nombres de columnas

```
# Teléfonos del emisor (hasta 3)
TelefonoEmisor_1 / TelefonoEmisor_2 / TelefonoEmisor_3

# Formas de pago (hasta 7)
FormaDePago_1_FormaPago / FormaDePago_1_MontoPago
FormaDePago_2_FormaPago / FormaDePago_2_MontoPago  ...

# Sección InformacionesAdicionales
IA_FechaEmbarque / IA_PesoBruto / IA_NombrePuertoEmbarque ...

# Sección Transporte
TR_Conductor / TR_Placa / TR_ViaTransporte / TR_PaisDestino ...

# OtraMoneda
OM_TipoMoneda / OM_TipoCambio / OM_MontoTotalOtraMoneda ...

# Ítems (hasta 30, prefijo Item_N_)
Item_1_DescripcionItem / Item_1_UnidadMedida / Item_1_DescuentoMonto ...

# Retención por ítem
Item_1_Ret_IndicadorAgenteRetencionoPercepcion
Item_1_Ret_MontoITBISRetenido / Item_1_Ret_MontoISRRetenido

# Códigos por ítem (hasta 5)
Item_1_Cod_1_TipoCodigo / Item_1_Cod_1_CodigoItem ...

# InformacionReferencia (requerido solo en tipo 34)
IR_NCFModificado / IR_FechaNCFModificado / IR_CodigoModificacion
IR_RNCOtroContribuyente / IR_RazonModificacion
```

---

## Tipos de e-CF soportados

| Tipo | Nombre | FechaVencSec | Comprador | Nota |
|------|--------|:---:|:---:|------|
| 31 | Factura con Valor Fiscal | ✅ | Req | Estructura completa |
| 32 | Factura de Consumo | — | Opc | Sin FechaVencimientoSecuencia |
| 33 | Nota de Débito | ✅ | Opc | Con retención opcional |
| 34 | Nota de Crédito | — | Opc | `IR_*` **requeridos**, IndicadorNotaCredito |
| 41 | Compras | ✅ | Req | Retención por ítem **requerida** |
| 43 | Gastos Menores | ✅ | — | Sin sección Comprador |
| 44 | Regímenes Especiales | ✅ | Req | ImpuestosAdicionales con OtrosImpuestos |
| 45 | Gubernamental | ✅ | Req | Igual al 31 con retención |
| 46 | Exportaciones | ✅ | Req | IA y Transporte extendidos, PaisComprador |
| 47 | Compras al Exterior | ✅ | Opc | Comprador mínimo, retención ISR por ítem req. |

---

## Estructura de archivos

```
scripphy/
├── app.py              # Servidor Flask (endpoints: /, /upload, /template)
├── ecf_builder.py      # Generador de XML por tipo de e-CF
├── requirements.txt    # Dependencias Python
├── static/             # Plantilla XLSX (se genera automáticamente)
└── templates/
    └── index.html      # Interfaz web
```

---

## Notas importantes

- **Fechas**: usar formato `DD-MM-AAAA` (ej: `31-12-2025`).
- **Valores numéricos**: no incluir símbolos de moneda ni separadores de miles. Usar punto `.` como decimal.
- **Enteros**: valores como `18` pueden escribirse como `18` o `18.0`; el sistema elimina el `.0`.
- **Celdas vacías**: son ignoradas por completo — no se genera ninguna etiqueta XML vacía.
- **TipoeCF**: si la columna no existe o está vacía, se auto-detecta desde los primeros 3 caracteres del ENCF (`E31...` → tipo 31).
- El XML generado **no incluye firma digital** — debe firmarse con el certificado electrónico de la DGII antes de enviar.
