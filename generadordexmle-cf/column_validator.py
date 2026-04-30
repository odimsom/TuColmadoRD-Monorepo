"""
column_validator.py
Validación de columnas del Excel para detectar columnas faltantes o no reconocidas.
"""

import logging
import re

logger = logging.getLogger(__name__)

COLUMN_PATTERNS = {
    "base": [
        "TipoeCF", "eNCF", "FechaVencimientoSecuencia",
        "IndicadorNotaCredito", "IndicadorEnvioDiferido",
        "IndicadorMontoGravado", "TipoIngresos", "TipoPago",
        "FechaLimitePago", "TerminoPago",
        "TipoCuentaPago", "NumeroCuentaPago", "BancoPago",
        "FechaDesde", "FechaHasta", "TotalPaginas",
    ],
    "emisor": [
        "RNCEmisor", "RazonSocialEmisor", "NombreComercial", "Sucursal",
        "DireccionEmisor", "Municipio", "Provincia",
        "CorreoEmisor", "WebSite", "ActividadEconomica",
        "CodigoVendedor", "NumeroFacturaInterna", "NumeroPedidoInterno",
        "ZonaVenta", "RutaVenta", "InformacionAdicionalEmisor", "FechaEmision",
    ],
    "comprador": [
        "RNCComprador", "IdentificadorExtranjero", "RazonSocialComprador",
        "ContactoComprador", "CorreoComprador", "DireccionComprador",
        "MunicipioComprador", "ProvinciaComprador", "PaisComprador",
        "FechaEntrega", "ContactoEntrega", "DireccionEntrega",
        "TelefonoAdicional", "FechaOrdenCompra", "NumeroOrdenCompra",
        "CodigoInternoComprador", "ResponsablePago",
        "InformacionAdicionalComprador",
    ],
    "totales": [
        "MontoGravadoTotal", "MontoGravadoI1", "MontoGravadoI2", "MontoGravadoI3",
        "MontoExento", "ITBIS1", "ITBIS2", "ITBIS3",
        "TotalITBIS", "TotalITBIS1", "TotalITBIS2", "TotalITBIS3",
        "MontoImpuestoAdicional", "MontoTotal", "MontoNoFacturable",
        "MontoPeriodo", "SaldoAnterior", "MontoAvancePago", "ValorPagar",
        "TotalITBISRetenido", "TotalISRRetencion",
        "TotalITBISPercepcion", "TotalISRPercepcion",
    ],
    "firma": ["FechaHoraFirma"],
}

# Regex para columnas dinámicas válidas
DYNAMIC_PATTERNS = [
    re.compile(r"^TelefonoEmisor_[1-3]$"),
    re.compile(r"^FormaDePago_[1-7]_(FormaPago|MontoPago)$"),
    re.compile(r"^ImpAd_\d+_(TipoImpuesto|TasaImpuestoAdicional|"
               r"MontoImpuestoSelectivoConsumoEspecifico|"
               r"MontoImpuestoSelectivoConsumoAdvalorem|"
               r"OtrosImpuestosAdicionales)$"),
    re.compile(r"^OM_"),
    re.compile(r"^IA_"),
    re.compile(r"^TR_"),
    re.compile(r"^IR_"),
    re.compile(r"^Item_\d+_"),
    re.compile(r"^Sub_\d+_"),
    re.compile(r"^DR_\d+_"),
    re.compile(r"^Pag_\d+_"),
    re.compile(r"^OM_ImpAd_\d+_"),
    # Formato DGII corchetes (aceptar tras normalización o directamente)
    re.compile(r"^.+\[\d+\](\[\d+\])?$"),
]


def _all_known_columns() -> set[str]:
    """Retorna todas las columnas estáticas conocidas."""
    known = set()
    for group in COLUMN_PATTERNS.values():
        known.update(group)
    return known


def validate_columns(columns: list[str]) -> list[str]:
    """
    Verifica las columnas del Excel y retorna advertencias.
    """
    warnings = []
    known = _all_known_columns()
    col_set = set(columns)

    if "TipoeCF" not in col_set and "eNCF" not in col_set:
        warnings.append(
            "ADVERTENCIA: No se encontró 'TipoeCF' ni 'eNCF'. "
            "No se podrá determinar el tipo de e-CF."
        )

    has_items = any(c.startswith("Item_") for c in col_set)
    if not has_items:
        warnings.append(
            "ADVERTENCIA: No se encontraron columnas de Items (Item_1_NumeroLinea, etc.). "
            "Los XML generados tendrán DetallesItems vacío."
        )

    unrecognized = []
    for col in columns:
        if col in known:
            continue
        if any(p.match(col) for p in DYNAMIC_PATTERNS):
            continue
        unrecognized.append(col)

    if unrecognized:
        sample = unrecognized[:10]
        extra = f" (y {len(unrecognized) - 10} más)" if len(unrecognized) > 10 else ""
        warnings.append(
            f"ADVERTENCIA: Columnas no reconocidas: {', '.join(sample)}{extra}. "
            "Estas columnas serán ignoradas."
        )

    return warnings
