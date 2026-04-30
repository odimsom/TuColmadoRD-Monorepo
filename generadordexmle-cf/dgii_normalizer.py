"""
dgii_normalizer.py
Normaliza columnas del Excel DGII (formato corchetes) al formato interno
usado por ecf_builder.py (formato guiones bajos con prefijos de sección).

Formatos DGII soportados:
  Corchete simple:  CantidadItem[1], FormaPago[1], TelefonoEmisor[1]
  Corchete doble:   Subcantidad[1][5], TipoCodigo[2][1], TipoImpuesto[1][1]
  Sin corchete:     FechaEmbarque, Conductor, TipoMoneda, NCFModificado

Formato interno (ecf_builder.py):
  Item_1_CantidadItem, FormaDePago_1_FormaPago, TelefonoEmisor_1
  Item_1_Sub_5_Subcantidad, Item_2_Cod_1_TipoCodigo, Item_1_ImpTabla_1_TipoImpuesto
  IA_FechaEmbarque, TR_Conductor, OM_TipoMoneda, IR_NCFModificado
"""

import re

_DOUBLE_BRACKET = re.compile(r'^(.+?)\[(\d+)\]\[(\d+)\]$')
_SINGLE_BRACKET = re.compile(r'^(.+?)\[(\d+)\]$')

# Prefijos del formato interno — si una columna ya tiene uno, no se transforma
_INTERNAL_PREFIXES = (
    'Item_', 'Sub_', 'DR_', 'Pag_',
    'IA_', 'TR_', 'OM_', 'IR_',
    'ImpAd_', 'FormaDePago_', 'TelefonoEmisor_',
)

# Erratas conocidas en plantillas DGII oficiales
_DGII_ALIASES = {
    'MontosubRecargo': 'MontoSubRecargo',
}

# ---------------------------------------------------------------------------
# Campos sin corchetes que necesitan prefijo de sección
# (Solo se aplican cuando se detecta formato DGII en el Excel)
# ---------------------------------------------------------------------------

_IA_FIELDS = frozenset({
    'FechaEmbarque', 'NumeroEmbarque', 'NumeroContenedor', 'NumeroReferencia',
    'NombrePuertoEmbarque', 'CondicionesEntrega', 'TotalFob', 'Seguro',
    'Flete', 'OtrosGastos', 'TotalCif', 'RegimenAduanero',
    'NombrePuertoSalida', 'NombrePuertoDesembarque',
    'PesoBruto', 'PesoNeto', 'UnidadPesoBruto', 'UnidadPesoNeto',
    'CantidadBulto', 'UnidadBulto', 'VolumenBulto', 'UnidadVolumen',
})

_TR_FIELDS = frozenset({
    'ViaTransporte', 'PaisOrigen', 'DireccionDestino', 'PaisDestino',
    'RNCIdentificacionCompaniaTransportista', 'NombreCompaniaTransportista',
    'NumeroViaje', 'Conductor', 'DocumentoTransporte', 'Ficha', 'Placa',
    'RutaTransporte', 'ZonaTransporte', 'NumeroAlbaran',
})

_OM_FIELDS = frozenset({
    'TipoMoneda', 'TipoCambio',
    'MontoGravadoTotalOtraMoneda', 'MontoGravado1OtraMoneda',
    'MontoGravado2OtraMoneda', 'MontoGravado3OtraMoneda',
    'MontoExentoOtraMoneda', 'TotalITBISOtraMoneda',
    'TotalITBIS1OtraMoneda', 'TotalITBIS2OtraMoneda',
    'TotalITBIS3OtraMoneda', 'MontoImpuestoAdicionalOtraMoneda',
    'MontoTotalOtraMoneda',
})

_IR_FIELDS = frozenset({
    'NCFModificado', 'RNCOtroContribuyente', 'FechaNCFModificado',
    'CodigoModificacion', 'RazonModificacion',
})

_SUBTOTALES_FIELDS = frozenset({
    'NumeroSubTotal', 'DescripcionSubtotal', 'Orden',
    'SubTotalMontoGravadoTotal', 'SubTotalMontoGravadoI1',
    'SubTotalMontoGravadoI2', 'SubTotalMontoGravadoI3',
    'SubTotaITBIS', 'SubTotaITBIS1', 'SubTotaITBIS2', 'SubTotaITBIS3',
    'SubTotalImpuestoAdicional', 'SubTotalExento',
    'MontoSubTotal', 'Lineas',
})

# ---------------------------------------------------------------------------
# Corchete simple [N] — enrutamiento por nombre de campo
# ---------------------------------------------------------------------------

_FORMA_PAGO_FIELDS = frozenset({'FormaPago', 'MontoPago'})

# Item → Item_N_FieldName
_ITEM_DIRECT_FIELDS = frozenset({
    'NumeroLinea', 'IndicadorFacturacion', 'NombreItem',
    'IndicadorBienoServicio', 'DescripcionItem', 'CantidadItem',
    'UnidadMedida', 'CantidadReferencia', 'UnidadReferencia',
    'GradosAlcohol', 'PrecioUnitarioReferencia',
    'FechaElaboracion', 'FechaVencimientoItem',
    'PrecioUnitarioItem', 'DescuentoMonto', 'RecargoMonto', 'MontoItem',
})

# Item Retencion → Item_N_Ret_FieldName
_ITEM_RET_FIELDS = frozenset({
    'IndicadorAgenteRetencionoPercepcion',
    'MontoITBISRetenido', 'MontoISRRetenido',
})

# Item Mineria → Item_N_Mineria_FieldName
_ITEM_MINERIA_FIELDS = frozenset({
    'PesoNetoKilogramo', 'PesoNetoMineria', 'TipoAfiliacion', 'Liquidacion',
})

# Item OtraMoneda → Item_N_OM_FieldName
_ITEM_OM_FIELDS = frozenset({
    'PrecioOtraMoneda', 'DescuentoOtraMoneda',
    'RecargoOtraMoneda', 'MontoItemOtraMoneda',
})

# ImpuestosAdicionales Totales → ImpAd_N_FieldName
_IMPAD_FIELDS = frozenset({
    'TipoImpuesto', 'TasaImpuestoAdicional',
    'MontoImpuestoSelectivoConsumoEspecifico',
    'MontoImpuestoSelectivoConsumoAdvalorem',
    'OtrosImpuestosAdicionales',
})

# ImpuestosAdicionales OtraMoneda → OM_ImpAd_N_InternalName
_OM_IMPAD_MAP = {
    'TipoImpuestoOtraMoneda': 'TipoImpuesto',
    'TasaImpuestoAdicionalOtraMoneda': 'TasaImpuestoAdicional',
    'MontoImpuestoSelectivoConsumoEspecificoOtraMoneda': 'MontoEspecifico',
    'MontoImpuestoSelectivoConsumoAdvaloremOtraMoneda': 'MontoAdvalorem',
    'OtrosImpuestosAdicionalesOtraMoneda': 'OtrosMontos',
}

# DescuentosORecargos → DR_N_InternalName
_DR_FIELDS = frozenset({
    'NumeroLineaDoR', 'TipoAjuste', 'IndicadorNorma1007',
    'DescripcionDescuentooRecargo', 'TipoValor',
    'ValorDescuentooRecargo', 'MontoDescuentooRecargo',
    'MontoDescuentooRecargoOtraMoneda',
    'IndicadorFacturacionDescuentooRecargo',
})

_DR_NAME_MAP = {'NumeroLineaDoR': 'NumeroLinea'}

# Paginacion → Pag_N_FieldName
_PAG_FIELDS = frozenset({
    'PaginaNo', 'NoLineaDesde', 'NoLineaHasta',
    'SubtotalMontoGravadoPagina', 'SubtotalMontoGravado1Pagina',
    'SubtotalMontoGravado2Pagina', 'SubtotalMontoGravado3Pagina',
    'SubtotalExentoPagina',
    'SubtotalItbisPagina', 'SubtotalItbis1Pagina',
    'SubtotalItbis2Pagina', 'SubtotalItbis3Pagina',
    'SubtotalImpuestoAdicionalPagina',
    'SubtotalImpuestoSelectivoConsumoEspecificoPagina',
    'SubtotalOtrosImpuesto',
    'MontoSubtotalPagina', 'SubtotalMontoNoFacturablePagina',
})

# ---------------------------------------------------------------------------
# Doble corchete [N][M] — sub-tablas de items
# ---------------------------------------------------------------------------

_ITEM_COD_FIELDS = frozenset({'TipoCodigo', 'CodigoItem'})
_ITEM_SUB_FIELDS = frozenset({'Subcantidad', 'CodigoSubcantidad'})
_ITEM_SUBDESC_FIELDS = frozenset({
    'TipoSubDescuento', 'SubDescuentoPorcentaje', 'MontoSubDescuento',
})
_ITEM_SUBREC_FIELDS = frozenset({
    'TipoSubRecargo', 'SubRecargoPorcentaje', 'MontoSubRecargo',
})

# Paginacion double-bracket (solo primer sub-índice soportado actualmente)
_PAG_DOUBLE_FIELDS = frozenset({
    'SubtotalImpuestoSelectivoConsumoEspecificoPagina',
    'SubtotalOtrosImpuesto',
})


# ---------------------------------------------------------------------------
# Lógica de normalización
# ---------------------------------------------------------------------------

def _normalize_column(col: str, add_prefixes: bool) -> str:
    """Convierte un nombre de columna DGII (corchetes) al formato interno."""

    # Si ya usa formato interno, no tocar
    if any(col.startswith(p) for p in _INTERNAL_PREFIXES):
        return col

    # --- Doble corchete: FieldName[N][M] ---
    m = _DOUBLE_BRACKET.match(col)
    if m:
        field_raw, n, idx = m.group(1), m.group(2), m.group(3)
        field = _DGII_ALIASES.get(field_raw, field_raw)

        if field in _ITEM_COD_FIELDS:
            return f'Item_{n}_Cod_{idx}_{field}'
        if field in _ITEM_SUB_FIELDS:
            return f'Item_{n}_Sub_{idx}_{field}'
        if field in _ITEM_SUBDESC_FIELDS:
            return f'Item_{n}_SubDesc_{idx}_{field}'
        if field in _ITEM_SUBREC_FIELDS:
            return f'Item_{n}_SubRec_{idx}_{field}'
        if field == 'TipoImpuesto':
            return f'Item_{n}_ImpTabla_{idx}_TipoImpuesto'
        # Pag double-bracket: solo M=1 soportado
        if field in _PAG_DOUBLE_FIELDS and idx == '1':
            return f'Pag_{n}_{field}'
        return col

    # --- Corchete simple: FieldName[N] ---
    m = _SINGLE_BRACKET.match(col)
    if m:
        field_raw, n = m.group(1), m.group(2)
        field = _DGII_ALIASES.get(field_raw, field_raw)

        if field == 'TelefonoEmisor':
            return f'TelefonoEmisor_{n}'
        if field in _FORMA_PAGO_FIELDS:
            return f'FormaDePago_{n}_{field}'
        if field in _ITEM_DIRECT_FIELDS:
            return f'Item_{n}_{field}'
        if field in _ITEM_RET_FIELDS:
            return f'Item_{n}_Ret_{field}'
        if field in _ITEM_MINERIA_FIELDS:
            return f'Item_{n}_Mineria_{field}'
        if field in _ITEM_OM_FIELDS:
            return f'Item_{n}_OM_{field}'
        if field in _IMPAD_FIELDS:
            return f'ImpAd_{n}_{field}'
        if field in _OM_IMPAD_MAP:
            return f'OM_ImpAd_{n}_{_OM_IMPAD_MAP[field]}'
        if field in _DR_FIELDS:
            internal = _DR_NAME_MAP.get(field, field)
            return f'DR_{n}_{internal}'
        if field in _PAG_FIELDS:
            return f'Pag_{n}_{field}'
        return col

    # --- Sin corchetes: prefijos de sección (solo si formato DGII detectado) ---
    if add_prefixes:
        if col in _IA_FIELDS:
            return f'IA_{col}'
        if col in _TR_FIELDS:
            return f'TR_{col}'
        if col in _OM_FIELDS:
            return f'OM_{col}'
        if col in _IR_FIELDS:
            return f'IR_{col}'
        if col in _SUBTOTALES_FIELDS:
            return f'Sub_1_{col}'

    return col


def normalize_dgii_columns(df):
    """
    Detecta si el Excel usa formato DGII (corchetes) y normaliza las columnas
    al formato interno con guiones bajos y prefijos de sección.
    Las columnas que ya usan formato interno no se modifican.

    Returns:
        tuple: (DataFrame con columnas renombradas, dict {original: nuevo})
    """
    has_brackets = any('[' in str(c) for c in df.columns)

    rename_map = {}
    for col in df.columns:
        new_col = _normalize_column(col, add_prefixes=has_brackets)
        if new_col != col:
            rename_map[col] = new_col

    if rename_map:
        df = df.rename(columns=rename_map)

    return df, rename_map
