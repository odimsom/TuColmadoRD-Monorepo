"""
ecf_builder.py
Generador de XML para e-CF (Comprobantes Fiscales Electrónicos) de la DGII.
Cada función build_ecf_XX recibe un dict con los datos de la fila del Excel
y devuelve la cadena XML formateada, omitiendo etiquetas vacías.

Convención de columnas del Excel:
  - Campos simples:      NombreCampo (ej: TipoeCF, RNCEmisor)
  - Repeated en tabla:   Prefijo_N_Campo (ej: TelefonoEmisor_1, FormaDePago_1_FormaPago)
  - Secciones:           Prefijo_Campo   (ej: IA_PesoBruto, TR_Conductor, OM_TipoMoneda)
  - Items:               Item_N_Campo    (ej: Item_1_NombreItem, Item_1_OM_PrecioOtraMoneda)
  - Retencion items:     Item_N_Ret_Campo
  - Mineria items:       Item_N_Mineria_Campo
  - Codigos item:        Item_N_Cod_N_TipoCodigo / Item_N_Cod_N_CodigoItem
  - SubDesc items:       Item_N_SubDesc_N_Tipo, Item_N_SubDesc_N_Pct, Item_N_SubDesc_N_Monto
  - SubRec items:        Item_N_SubRec_N_Tipo, Item_N_SubRec_N_Pct, Item_N_SubRec_N_Monto
  - Subcantidad items:   Item_N_Sub_N_Subcantidad, Item_N_Sub_N_CodigoSubcantidad
  - TablaImpuesto items: Item_N_ImpTabla_N_TipoImpuesto
  - InfRef:              IR_NCFModificado, IR_FechaNCFModificado, etc.
  - OtraMoneda ImpAd:    OM_ImpAd_N_TipoImpuesto, OM_ImpAd_N_Tasa, etc.
"""

import xml.etree.ElementTree as ET
from xml.dom import minidom
import math

MAX_ITEMS       = 30
MAX_FP          = 7     # FormasDePago
MAX_IA_TOT      = 20    # ImpuestosAdicionales en Totales
MAX_IA_TOT_OM   = 20    # ImpuestosAdicionales OtraMoneda
MAX_COD_ITEM    = 5     # CodigosItem por item
MAX_SUBCANT     = 5     # SubcantidadesItem por item
MAX_SUBDESC     = 12    # SubDescuentos por item
MAX_SUBREC      = 12    # SubRecargos por item
MAX_IMPTABLA    = 2     # ImpuestosAdicionales en TablaImpuesto por item
MAX_SUBTOTALES  = 20    # Subtotales
MAX_DR          = 20    # DescuentosORecargos
MAX_PAGINAS     = 100   # Paginacion


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

# Valores que se consideran vacíos (incluyendo placeholders de Excel)
_EMPTY_VALUES = {
    'nan', 'none', 'nat',
    '#e',                       # placeholder DGII / exportadores internos
    '#n', '#n/a', 'n/a',        # variantes comunes
    '#value!', '#ref!', '#null!', '#div/0!', '#name?',  # errores de Excel
}


def _is_empty(v) -> bool:
    if v is None:
        return True
    if isinstance(v, float) and math.isnan(v):
        return True
    s = str(v).strip()
    return s == '' or s.lower() in _EMPTY_VALUES


def _clean(v) -> str | None:
    """Devuelve string limpio o None si está vacío."""
    if _is_empty(v):
        return None
    s = str(v).strip()
    # Quitar .0 de enteros almacenados como float
    if s.endswith('.0') and s[:-2].lstrip('-').isdigit():
        s = s[:-2]
    return s


def v(row: dict, col: str) -> str | None:
    """Shorthand: devuelve valor limpio de la fila para la columna dada."""
    return _clean(row.get(col))


def add(parent: ET.Element, tag: str, value: str | None) -> ET.Element | None:
    """Agrega <tag>value</tag> a parent SOLO si value no es None/vacío."""
    if value is not None:
        el = ET.SubElement(parent, tag)
        el.text = value
        return el
    return None


def _pretty(root: ET.Element) -> str:
    """Devuelve XML bien indentado como cadena."""
    raw = ET.tostring(root, encoding='utf-8', xml_declaration=True)
    dom = minidom.parseString(raw)
    pretty = dom.toprettyxml(indent='    ', encoding='utf-8')
    # minidom agrega cabecera doble; la limpiamos
    lines = pretty.decode('utf-8').splitlines()
    # Quitar línea extra en blanco que genera minidom
    lines = [l for l in lines if l.strip()]
    return '\n'.join(lines)


# ---------------------------------------------------------------------------
# Sub-secciones reutilizables
# ---------------------------------------------------------------------------

def _build_tabla_telefonos(parent: ET.Element, row: dict):
    telefonos = [v(row, f'TelefonoEmisor_{i}') for i in range(1, 4)]
    telefonos = [t for t in telefonos if t]
    if telefonos:
        tabla = ET.SubElement(parent, 'TablaTelefonoEmisor')
        for t in telefonos:
            add(tabla, 'TelefonoEmisor', t)


def _build_tabla_formas_pago(parent: ET.Element, row: dict):
    formas = []
    for i in range(1, MAX_FP + 1):
        fp = v(row, f'FormaDePago_{i}_FormaPago')
        mp = v(row, f'FormaDePago_{i}_MontoPago')
        if fp and mp:
            formas.append((fp, mp))
    if formas:
        tabla = ET.SubElement(parent, 'TablaFormasPago')
        for fp, mp in formas:
            forma = ET.SubElement(tabla, 'FormaDePago')
            add(forma, 'FormaPago', fp)
            add(forma, 'MontoPago', mp)


def _build_emisor(parent: ET.Element, row: dict,
                  has_codigo_vendedor: bool = True,
                  has_zona_ruta: bool = True):
    emisor = ET.SubElement(parent, 'Emisor')
    add(emisor, 'RNCEmisor',          v(row, 'RNCEmisor'))
    add(emisor, 'RazonSocialEmisor',  v(row, 'RazonSocialEmisor'))
    add(emisor, 'NombreComercial',    v(row, 'NombreComercial'))
    add(emisor, 'Sucursal',           v(row, 'Sucursal'))
    add(emisor, 'DireccionEmisor',    v(row, 'DireccionEmisor'))
    add(emisor, 'Municipio',          v(row, 'Municipio'))
    add(emisor, 'Provincia',          v(row, 'Provincia'))
    _build_tabla_telefonos(emisor, row)
    add(emisor, 'CorreoEmisor',       v(row, 'CorreoEmisor'))
    add(emisor, 'WebSite',            v(row, 'WebSite'))
    add(emisor, 'ActividadEconomica', v(row, 'ActividadEconomica'))
    if has_codigo_vendedor:
        add(emisor, 'CodigoVendedor',     v(row, 'CodigoVendedor'))
    add(emisor, 'NumeroFacturaInterna',   v(row, 'NumeroFacturaInterna'))
    add(emisor, 'NumeroPedidoInterno',    v(row, 'NumeroPedidoInterno'))
    if has_zona_ruta:
        add(emisor, 'ZonaVenta',      v(row, 'ZonaVenta'))
        add(emisor, 'RutaVenta',      v(row, 'RutaVenta'))
    add(emisor, 'InformacionAdicionalEmisor', v(row, 'InformacionAdicionalEmisor'))
    add(emisor, 'FechaEmision',       v(row, 'FechaEmision'))
    return emisor


def _build_comprador_full(parent: ET.Element, row: dict,
                          has_rnc: bool = True,
                          has_id_extranjero: bool = True,
                          has_pais: bool = False):
    comprador = ET.SubElement(parent, 'Comprador')
    if has_rnc:
        add(comprador, 'RNCComprador',          v(row, 'RNCComprador'))
    if has_id_extranjero:
        add(comprador, 'IdentificadorExtranjero', v(row, 'IdentificadorExtranjero'))
    add(comprador, 'RazonSocialComprador',  v(row, 'RazonSocialComprador'))
    add(comprador, 'ContactoComprador',     v(row, 'ContactoComprador'))
    add(comprador, 'CorreoComprador',       v(row, 'CorreoComprador'))
    add(comprador, 'DireccionComprador',    v(row, 'DireccionComprador'))
    add(comprador, 'MunicipioComprador',    v(row, 'MunicipioComprador'))
    add(comprador, 'ProvinciaComprador',    v(row, 'ProvinciaComprador'))
    if has_pais:
        add(comprador, 'PaisComprador',     v(row, 'PaisComprador'))
    add(comprador, 'FechaEntrega',          v(row, 'FechaEntrega'))
    add(comprador, 'ContactoEntrega',       v(row, 'ContactoEntrega'))
    add(comprador, 'DireccionEntrega',      v(row, 'DireccionEntrega'))
    add(comprador, 'TelefonoAdicional',     v(row, 'TelefonoAdicional'))
    add(comprador, 'FechaOrdenCompra',      v(row, 'FechaOrdenCompra'))
    add(comprador, 'NumeroOrdenCompra',     v(row, 'NumeroOrdenCompra'))
    add(comprador, 'CodigoInternoComprador',v(row, 'CodigoInternoComprador'))
    add(comprador, 'ResponsablePago',       v(row, 'ResponsablePago'))
    add(comprador, 'InformacionAdicionalComprador', v(row, 'InformacionAdicionalComprador'))
    return comprador


def _build_informaciones_adicionales_std(parent: ET.Element, row: dict):
    """InformacionesAdicionales estándar (tipos 31,32,33,34,44,45)."""
    cols = [
        ('IA_FechaEmbarque',    'FechaEmbarque'),
        ('IA_NumeroEmbarque',   'NumeroEmbarque'),
        ('IA_NumeroContenedor', 'NumeroContenedor'),
        ('IA_NumeroReferencia', 'NumeroReferencia'),
        ('IA_PesoBruto',        'PesoBruto'),
        ('IA_PesoNeto',         'PesoNeto'),
        ('IA_UnidadPesoBruto',  'UnidadPesoBruto'),
        ('IA_UnidadPesoNeto',   'UnidadPesoNeto'),
        ('IA_CantidadBulto',    'CantidadBulto'),
        ('IA_UnidadBulto',      'UnidadBulto'),
        ('IA_VolumenBulto',     'VolumenBulto'),
        ('IA_UnidadVolumen',    'UnidadVolumen'),
    ]
    present = any(v(row, col) is not None for col, _ in cols)
    if present:
        ia = ET.SubElement(parent, 'InformacionesAdicionales')
        for col, tag in cols:
            add(ia, tag, v(row, col))


def _build_informaciones_adicionales_46(parent: ET.Element, row: dict):
    """InformacionesAdicionales extendida para tipo 46 (Exportaciones)."""
    cols_46 = [
        ('IA_FechaEmbarque',           'FechaEmbarque'),
        ('IA_NumeroEmbarque',          'NumeroEmbarque'),
        ('IA_NumeroContenedor',        'NumeroContenedor'),
        ('IA_NumeroReferencia',        'NumeroReferencia'),
        ('IA_NombrePuertoEmbarque',    'NombrePuertoEmbarque'),
        ('IA_CondicionesEntrega',      'CondicionesEntrega'),
        ('IA_TotalFob',                'TotalFob'),
        ('IA_Seguro',                  'Seguro'),
        ('IA_Flete',                   'Flete'),
        ('IA_OtrosGastos',             'OtrosGastos'),
        ('IA_TotalCif',                'TotalCif'),
        ('IA_RegimenAduanero',         'RegimenAduanero'),
        ('IA_NombrePuertoSalida',      'NombrePuertoSalida'),
        ('IA_NombrePuertoDesembarque', 'NombrePuertoDesembarque'),
        ('IA_PesoBruto',               'PesoBruto'),
        ('IA_PesoNeto',                'PesoNeto'),
        ('IA_UnidadPesoBruto',         'UnidadPesoBruto'),
        ('IA_UnidadPesoNeto',          'UnidadPesoNeto'),
        ('IA_CantidadBulto',           'CantidadBulto'),
        ('IA_UnidadBulto',             'UnidadBulto'),
        ('IA_VolumenBulto',            'VolumenBulto'),
        ('IA_UnidadVolumen',           'UnidadVolumen'),
    ]
    present = any(v(row, col) is not None for col, _ in cols_46)
    if present:
        ia = ET.SubElement(parent, 'InformacionesAdicionales')
        for col, tag in cols_46:
            add(ia, tag, v(row, col))


def _build_transporte_std(parent: ET.Element, row: dict):
    """Transporte estándar (tipos 31,32,33,34,44,45)."""
    cols = [
        ('TR_Conductor',          'Conductor'),
        ('TR_DocumentoTransporte','DocumentoTransporte'),
        ('TR_Ficha',              'Ficha'),
        ('TR_Placa',              'Placa'),
        ('TR_RutaTransporte',     'RutaTransporte'),
        ('TR_ZonaTransporte',     'ZonaTransporte'),
        ('TR_NumeroAlbaran',      'NumeroAlbaran'),
    ]
    present = any(v(row, col) is not None for col, _ in cols)
    if present:
        tr = ET.SubElement(parent, 'Transporte')
        for col, tag in cols:
            add(tr, tag, v(row, col))


def _build_transporte_46(parent: ET.Element, row: dict):
    """Transporte extendido para tipo 46 (Exportaciones)."""
    cols = [
        ('TR_ViaTransporte',                         'ViaTransporte'),
        ('TR_PaisOrigen',                            'PaisOrigen'),
        ('TR_DireccionDestino',                      'DireccionDestino'),
        ('TR_PaisDestino',                           'PaisDestino'),
        ('TR_RNCIdentificacionCompaniaTransportista','RNCIdentificacionCompaniaTransportista'),
        ('TR_NombreCompaniaTransportista',           'NombreCompaniaTransportista'),
        ('TR_NumeroViaje',                           'NumeroViaje'),
        ('TR_Conductor',                             'Conductor'),
        ('TR_DocumentoTransporte',                   'DocumentoTransporte'),
        ('TR_Ficha',                                 'Ficha'),
        ('TR_Placa',                                 'Placa'),
        ('TR_RutaTransporte',                        'RutaTransporte'),
        ('TR_ZonaTransporte',                        'ZonaTransporte'),
        ('TR_NumeroAlbaran',                         'NumeroAlbaran'),
    ]
    present = any(v(row, col) is not None for col, _ in cols)
    if present:
        tr = ET.SubElement(parent, 'Transporte')
        for col, tag in cols:
            add(tr, tag, v(row, col))


def _build_impuestos_adicionales_totales(parent: ET.Element, row: dict,
                                          prefix: str = 'ImpAd',
                                          xml_wrap: str = 'ImpuestosAdicionales',
                                          xml_item: str = 'ImpuestoAdicional',
                                          has_especifico: bool = True,
                                          has_advalorem: bool = True,
                                          has_otros: bool = True,
                                          requires_otros: bool = False):
    """Agrega ImpuestosAdicionales al parent si hay datos."""
    items = []
    for i in range(1, MAX_IA_TOT + 1):
        ti  = v(row, f'{prefix}_{i}_TipoImpuesto')
        ta  = v(row, f'{prefix}_{i}_TasaImpuestoAdicional')
        esp = v(row, f'{prefix}_{i}_MontoImpuestoSelectivoConsumoEspecifico') if has_especifico else None
        adv = v(row, f'{prefix}_{i}_MontoImpuestoSelectivoConsumoAdvalorem')  if has_advalorem  else None
        otr = v(row, f'{prefix}_{i}_OtrosImpuestosAdicionales')               if has_otros      else None
        if ti:
            items.append((ti, ta, esp, adv, otr))
    if not items:
        return
    wrap = ET.SubElement(parent, xml_wrap)
    for ti, ta, esp, adv, otr in items:
        item = ET.SubElement(wrap, xml_item)
        add(item, 'TipoImpuesto',            ti)
        add(item, 'TasaImpuestoAdicional',   ta)
        if has_especifico:
            add(item, 'MontoImpuestoSelectivoConsumoEspecifico', esp)
        if has_advalorem:
            add(item, 'MontoImpuestoSelectivoConsumoAdvalorem', adv)
        if has_otros:
            if requires_otros:
                add(item, 'OtrosImpuestosAdicionales', otr if otr else '0')
            else:
                add(item, 'OtrosImpuestosAdicionales', otr)


def _build_otra_moneda_full(parent: ET.Element, row: dict):
    """OtraMoneda completo (tipos 31,32,33,34)."""
    cols = [
        'OM_TipoMoneda', 'OM_TipoCambio',
        'OM_MontoGravadoTotalOtraMoneda',
        'OM_MontoGravado1OtraMoneda', 'OM_MontoGravado2OtraMoneda',
        'OM_MontoGravado3OtraMoneda', 'OM_MontoExentoOtraMoneda',
        'OM_TotalITBISOtraMoneda', 'OM_TotalITBIS1OtraMoneda',
        'OM_TotalITBIS2OtraMoneda', 'OM_TotalITBIS3OtraMoneda',
        'OM_MontoImpuestoAdicionalOtraMoneda',
    ]
    has_base = any(v(row, c) is not None for c in cols)
    has_inadd = any(v(row, f'OM_ImpAd_{i}_TipoImpuesto') is not None for i in range(1, 4))
    has_total = v(row, 'OM_MontoTotalOtraMoneda') is not None
    if not (has_base or has_inadd or has_total):
        return
    om = ET.SubElement(parent, 'OtraMoneda')
    add(om, 'TipoMoneda',                        v(row, 'OM_TipoMoneda'))
    add(om, 'TipoCambio',                        v(row, 'OM_TipoCambio'))
    add(om, 'MontoGravadoTotalOtraMoneda',        v(row, 'OM_MontoGravadoTotalOtraMoneda'))
    add(om, 'MontoGravado1OtraMoneda',            v(row, 'OM_MontoGravado1OtraMoneda'))
    add(om, 'MontoGravado2OtraMoneda',            v(row, 'OM_MontoGravado2OtraMoneda'))
    add(om, 'MontoGravado3OtraMoneda',            v(row, 'OM_MontoGravado3OtraMoneda'))
    add(om, 'MontoExentoOtraMoneda',              v(row, 'OM_MontoExentoOtraMoneda'))
    add(om, 'TotalITBISOtraMoneda',               v(row, 'OM_TotalITBISOtraMoneda'))
    add(om, 'TotalITBIS1OtraMoneda',              v(row, 'OM_TotalITBIS1OtraMoneda'))
    add(om, 'TotalITBIS2OtraMoneda',              v(row, 'OM_TotalITBIS2OtraMoneda'))
    add(om, 'TotalITBIS3OtraMoneda',              v(row, 'OM_TotalITBIS3OtraMoneda'))
    add(om, 'MontoImpuestoAdicionalOtraMoneda',   v(row, 'OM_MontoImpuestoAdicionalOtraMoneda'))
    _build_impuestos_adicionales_om(om, row)
    add(om, 'MontoTotalOtraMoneda',               v(row, 'OM_MontoTotalOtraMoneda'))


def _build_impuestos_adicionales_om(parent: ET.Element, row: dict):
    items = []
    for i in range(1, MAX_IA_TOT_OM + 1):
        ti  = v(row, f'OM_ImpAd_{i}_TipoImpuesto')
        ta  = v(row, f'OM_ImpAd_{i}_TasaImpuestoAdicional')
        esp = v(row, f'OM_ImpAd_{i}_MontoEspecifico')
        adv = v(row, f'OM_ImpAd_{i}_MontoAdvalorem')
        otr = v(row, f'OM_ImpAd_{i}_OtrosMontos')
        if ti:
            items.append((ti, ta, esp, adv, otr))
    if not items:
        return
    wrap = ET.SubElement(parent, 'ImpuestosAdicionalesOtraMoneda')
    for ti, ta, esp, adv, otr in items:
        item = ET.SubElement(wrap, 'ImpuestoAdicionalOtraMoneda')
        add(item, 'TipoImpuestoOtraMoneda',               ti)
        add(item, 'TasaImpuestoAdicionalOtraMoneda',       ta)
        add(item, 'MontoImpuestoSelectivoConsumoEspecificoOtraMoneda', esp)
        add(item, 'MontoImpuestoSelectivoConsumoAdvaloremOtraMoneda',  adv)
        add(item, 'OtrosImpuestosAdicionalesOtraMoneda',               otr)


def _build_item_codigos(item_el: ET.Element, row: dict, n: int):
    codigos = []
    for c in range(1, MAX_COD_ITEM + 1):
        tc = v(row, f'Item_{n}_Cod_{c}_TipoCodigo')
        ci = v(row, f'Item_{n}_Cod_{c}_CodigoItem')
        if tc and ci:
            codigos.append((tc, ci))
    if codigos:
        tabla = ET.SubElement(item_el, 'TablaCodigosItem')
        for tc, ci in codigos:
            cod = ET.SubElement(tabla, 'CodigosItem')
            add(cod, 'TipoCodigo', tc)
            add(cod, 'CodigoItem', ci)


def _build_item_subcantidades(item_el: ET.Element, row: dict, n: int):
    subs = []
    for s in range(1, MAX_SUBCANT + 1):
        sc = v(row, f'Item_{n}_Sub_{s}_Subcantidad')
        cc = v(row, f'Item_{n}_Sub_{s}_CodigoSubcantidad')
        if sc or cc:
            subs.append((sc, cc))
    if subs:
        tabla = ET.SubElement(item_el, 'TablaSubcantidad')
        for sc, cc in subs:
            si = ET.SubElement(tabla, 'SubcantidadItem')
            add(si, 'Subcantidad', sc)
            add(si, 'CodigoSubcantidad', cc)


def _build_item_subdescuentos(item_el: ET.Element, row: dict, n: int):
    subs = []
    for s in range(1, MAX_SUBDESC + 1):
        ti  = v(row, f'Item_{n}_SubDesc_{s}_TipoSubDescuento')
        pct = v(row, f'Item_{n}_SubDesc_{s}_SubDescuentoPorcentaje')
        mo  = v(row, f'Item_{n}_SubDesc_{s}_MontoSubDescuento')
        if ti:
            subs.append((ti, pct, mo))
    if subs:
        tabla = ET.SubElement(item_el, 'TablaSubDescuento')
        for ti, pct, mo in subs:
            sd = ET.SubElement(tabla, 'SubDescuento')
            add(sd, 'TipoSubDescuento',       ti)
            add(sd, 'SubDescuentoPorcentaje', pct)
            add(sd, 'MontoSubDescuento',      mo)


def _build_item_subrecargas(item_el: ET.Element, row: dict, n: int):
    subs = []
    for s in range(1, MAX_SUBREC + 1):
        ti  = v(row, f'Item_{n}_SubRec_{s}_TipoSubRecargo')
        pct = v(row, f'Item_{n}_SubRec_{s}_SubRecargoPorcentaje')
        mo  = v(row, f'Item_{n}_SubRec_{s}_MontoSubRecargo')
        if ti:
            subs.append((ti, pct, mo))
    if subs:
        tabla = ET.SubElement(item_el, 'TablaSubRecargo')
        for ti, pct, mo in subs:
            sr = ET.SubElement(tabla, 'SubRecargo')
            add(sr, 'TipoSubRecargo',       ti)
            add(sr, 'SubRecargoPorcentaje', pct)
            add(sr, 'MontoSubRecargo',      mo)


def _build_item_imp_tabla(item_el: ET.Element, row: dict, n: int):
    tipos = []
    for t in range(1, MAX_IMPTABLA + 1):
        ti = v(row, f'Item_{n}_ImpTabla_{t}_TipoImpuesto')
        if ti:
            tipos.append(ti)
    if tipos:
        tabla = ET.SubElement(item_el, 'TablaImpuestoAdicional')
        for ti in tipos:
            ia = ET.SubElement(tabla, 'ImpuestoAdicional')
            add(ia, 'TipoImpuesto', ti)


def _build_item_otra_moneda(item_el: ET.Element, row: dict, n: int):
    p  = v(row, f'Item_{n}_OM_PrecioOtraMoneda')
    d  = v(row, f'Item_{n}_OM_DescuentoOtraMoneda')
    re = v(row, f'Item_{n}_OM_RecargoOtraMoneda')
    m  = v(row, f'Item_{n}_OM_MontoItemOtraMoneda')
    if any(x is not None for x in [p, d, re, m]):
        om = ET.SubElement(item_el, 'OtraMonedaDetalle')
        add(om, 'PrecioOtraMoneda',    p)
        add(om, 'DescuentoOtraMoneda', d)
        add(om, 'RecargoOtraMoneda',   re)
        add(om, 'MontoItemOtraMoneda', m)


def _build_item_mineria(item_el: ET.Element, row: dict, n: int):
    pnk = v(row, f'Item_{n}_Mineria_PesoNetoKilogramo')
    pnm = v(row, f'Item_{n}_Mineria_PesoNetoMineria')
    ta  = v(row, f'Item_{n}_Mineria_TipoAfiliacion')
    li  = v(row, f'Item_{n}_Mineria_Liquidacion')
    if any(x is not None for x in [pnk, pnm, ta, li]):
        mi = ET.SubElement(item_el, 'Mineria')
        add(mi, 'PesoNetoKilogramo', pnk)
        add(mi, 'PesoNetoMineria',   pnm)
        add(mi, 'TipoAfiliacion',    ta)
        add(mi, 'Liquidacion',       li)



def _build_subtotales(parent: ET.Element, row: dict,
                      has_imp_adicional: bool = True):
    """Subtotales opcionales (máx 20). Columnas: Sub_N_Campo."""
    items = []
    for i in range(1, MAX_SUBTOTALES + 1):
        num = v(row, f'Sub_{i}_NumeroSubTotal')
        desc = v(row, f'Sub_{i}_DescripcionSubtotal')
        if num is None and desc is None:
            break
        items.append(i)
    if not items:
        return
    subtotales = ET.SubElement(parent, 'Subtotales')
    for i in items:
        sub = ET.SubElement(subtotales, 'Subtotal')
        add(sub, 'NumeroSubTotal',              v(row, f'Sub_{i}_NumeroSubTotal'))
        add(sub, 'DescripcionSubtotal',         v(row, f'Sub_{i}_DescripcionSubtotal'))
        add(sub, 'Orden',                       v(row, f'Sub_{i}_Orden'))
        add(sub, 'SubTotalMontoGravadoTotal',   v(row, f'Sub_{i}_SubTotalMontoGravadoTotal'))
        add(sub, 'SubTotalMontoGravadoI1',      v(row, f'Sub_{i}_SubTotalMontoGravadoI1'))
        add(sub, 'SubTotalMontoGravadoI2',      v(row, f'Sub_{i}_SubTotalMontoGravadoI2'))
        add(sub, 'SubTotalMontoGravadoI3',      v(row, f'Sub_{i}_SubTotalMontoGravadoI3'))
        add(sub, 'SubTotaITBIS',                v(row, f'Sub_{i}_SubTotaITBIS'))
        add(sub, 'SubTotaITBIS1',               v(row, f'Sub_{i}_SubTotaITBIS1'))
        add(sub, 'SubTotaITBIS2',               v(row, f'Sub_{i}_SubTotaITBIS2'))
        add(sub, 'SubTotaITBIS3',               v(row, f'Sub_{i}_SubTotaITBIS3'))
        if has_imp_adicional:
            add(sub, 'SubTotalImpuestoAdicional', v(row, f'Sub_{i}_SubTotalImpuestoAdicional'))
        add(sub, 'SubTotalExento',              v(row, f'Sub_{i}_SubTotalExento'))
        add(sub, 'MontoSubTotal',               v(row, f'Sub_{i}_MontoSubTotal'))
        add(sub, 'Lineas',                      v(row, f'Sub_{i}_Lineas'))


def _build_descuentos_o_recargos(parent: ET.Element, row: dict,
                                  has_norma1007: bool = True):
    """DescuentosORecargos opcionales (máx 20). Columnas: DR_N_Campo."""
    items = []
    for i in range(1, MAX_DR + 1):
        nl = v(row, f'DR_{i}_NumeroLinea')
        if nl is None:
            break
        items.append(i)
    if not items:
        return
    dor = ET.SubElement(parent, 'DescuentosORecargos')
    for i in items:
        dr = ET.SubElement(dor, 'DescuentoORecargo')
        add(dr, 'NumeroLinea',                      v(row, f'DR_{i}_NumeroLinea'))
        add(dr, 'TipoAjuste',                      v(row, f'DR_{i}_TipoAjuste'))
        if has_norma1007:
            add(dr, 'IndicadorNorma1007',           v(row, f'DR_{i}_IndicadorNorma1007'))
        add(dr, 'DescripcionDescuentooRecargo',     v(row, f'DR_{i}_DescripcionDescuentooRecargo'))
        add(dr, 'TipoValor',                       v(row, f'DR_{i}_TipoValor'))
        add(dr, 'ValorDescuentooRecargo',           v(row, f'DR_{i}_ValorDescuentooRecargo'))
        add(dr, 'MontoDescuentooRecargo',           v(row, f'DR_{i}_MontoDescuentooRecargo'))
        add(dr, 'MontoDescuentooRecargoOtraMoneda', v(row, f'DR_{i}_MontoDescuentooRecargoOtraMoneda'))
        add(dr, 'IndicadorFacturacionDescuentooRecargo',
            v(row, f'DR_{i}_IndicadorFacturacionDescuentooRecargo'))


def _build_paginacion(parent: ET.Element, row: dict,
                      has_imp_adicional: bool = True,
                      has_no_facturable: bool = True):
    """Paginacion opcional (máx 100). Columnas: Pag_N_Campo."""
    items = []
    for i in range(1, MAX_PAGINAS + 1):
        pn = v(row, f'Pag_{i}_PaginaNo')
        if pn is None:
            break
        items.append(i)
    if not items:
        return
    pag = ET.SubElement(parent, 'Paginacion')
    for i in items:
        pg = ET.SubElement(pag, 'Pagina')
        add(pg, 'PaginaNo',                          v(row, f'Pag_{i}_PaginaNo'))
        add(pg, 'NoLineaDesde',                      v(row, f'Pag_{i}_NoLineaDesde'))
        add(pg, 'NoLineaHasta',                      v(row, f'Pag_{i}_NoLineaHasta'))
        add(pg, 'SubtotalMontoGravadoPagina',        v(row, f'Pag_{i}_SubtotalMontoGravadoPagina'))
        add(pg, 'SubtotalMontoGravado1Pagina',       v(row, f'Pag_{i}_SubtotalMontoGravado1Pagina'))
        add(pg, 'SubtotalMontoGravado2Pagina',       v(row, f'Pag_{i}_SubtotalMontoGravado2Pagina'))
        add(pg, 'SubtotalMontoGravado3Pagina',       v(row, f'Pag_{i}_SubtotalMontoGravado3Pagina'))
        add(pg, 'SubtotalExentoPagina',              v(row, f'Pag_{i}_SubtotalExentoPagina'))
        add(pg, 'SubtotalItbisPagina',               v(row, f'Pag_{i}_SubtotalItbisPagina'))
        add(pg, 'SubtotalItbis1Pagina',              v(row, f'Pag_{i}_SubtotalItbis1Pagina'))
        add(pg, 'SubtotalItbis2Pagina',              v(row, f'Pag_{i}_SubtotalItbis2Pagina'))
        add(pg, 'SubtotalItbis3Pagina',              v(row, f'Pag_{i}_SubtotalItbis3Pagina'))
        if has_imp_adicional:
            add(pg, 'SubtotalImpuestoAdicionalPagina',
                v(row, f'Pag_{i}_SubtotalImpuestoAdicionalPagina'))
            sia_esp = v(row, f'Pag_{i}_SubtotalImpuestoSelectivoConsumoEspecificoPagina')
            sia_otr = v(row, f'Pag_{i}_SubtotalOtrosImpuesto')
            if sia_esp is not None or sia_otr is not None:
                sia = ET.SubElement(pg, 'SubtotalImpuestoAdicional')
                add(sia, 'SubtotalImpuestoSelectivoConsumoEspecificoPagina', sia_esp)
                add(sia, 'SubtotalOtrosImpuesto', sia_otr)
        add(pg, 'MontoSubtotalPagina',               v(row, f'Pag_{i}_MontoSubtotalPagina'))
        if has_no_facturable:
            add(pg, 'SubtotalMontoNoFacturablePagina',
                v(row, f'Pag_{i}_SubtotalMontoNoFacturablePagina'))


def _build_informacion_referencia(parent: ET.Element, row: dict,
                                   required: bool = False,
                                   has_razon: bool = False):
    ncf  = v(row, 'IR_NCFModificado')
    rnc  = v(row, 'IR_RNCOtroContribuyente')
    fech = v(row, 'IR_FechaNCFModificado')
    cod  = v(row, 'IR_CodigoModificacion')
    razon= v(row, 'IR_RazonModificacion') if has_razon else None
    if required or any(x is not None for x in [ncf, rnc, fech, cod, razon]):
        ir = ET.SubElement(parent, 'InformacionReferencia')
        add(ir, 'NCFModificado',          ncf)
        add(ir, 'RNCOtroContribuyente',   rnc)
        add(ir, 'FechaNCFModificado',     fech)
        add(ir, 'CodigoModificacion',     cod)
        if has_razon:
            add(ir, 'RazonModificacion',  razon)


# ---------------------------------------------------------------------------
# Totales por variante
# ---------------------------------------------------------------------------

def _build_totales_full(parent: ET.Element, row: dict,
                         has_imp_adicional: bool = True,
                         has_retencion: bool = False):
    """Totales completos con ITBIS (tipos 31, 32)."""
    tot = ET.SubElement(parent, 'Totales')
    add(tot, 'MontoGravadoTotal',       v(row, 'MontoGravadoTotal'))
    add(tot, 'MontoGravadoI1',          v(row, 'MontoGravadoI1'))
    add(tot, 'MontoGravadoI2',          v(row, 'MontoGravadoI2'))
    add(tot, 'MontoGravadoI3',          v(row, 'MontoGravadoI3'))
    add(tot, 'MontoExento',             v(row, 'MontoExento'))
    add(tot, 'ITBIS1',                  v(row, 'ITBIS1'))
    add(tot, 'ITBIS2',                  v(row, 'ITBIS2'))
    add(tot, 'ITBIS3',                  v(row, 'ITBIS3'))
    add(tot, 'TotalITBIS',              v(row, 'TotalITBIS'))
    add(tot, 'TotalITBIS1',             v(row, 'TotalITBIS1'))
    add(tot, 'TotalITBIS2',             v(row, 'TotalITBIS2'))
    add(tot, 'TotalITBIS3',             v(row, 'TotalITBIS3'))
    if has_imp_adicional:
        add(tot, 'MontoImpuestoAdicional', v(row, 'MontoImpuestoAdicional'))
        _build_impuestos_adicionales_totales(tot, row)
    add(tot, 'MontoTotal',              v(row, 'MontoTotal'))
    add(tot, 'MontoNoFacturable',       v(row, 'MontoNoFacturable'))
    add(tot, 'MontoPeriodo',            v(row, 'MontoPeriodo'))
    add(tot, 'SaldoAnterior',           v(row, 'SaldoAnterior'))
    add(tot, 'MontoAvancePago',         v(row, 'MontoAvancePago'))
    add(tot, 'ValorPagar',              v(row, 'ValorPagar'))
    if has_retencion:
        add(tot, 'TotalITBISRetenido',      v(row, 'TotalITBISRetenido'))
        add(tot, 'TotalISRRetencion',       v(row, 'TotalISRRetencion'))
        add(tot, 'TotalITBISPercepcion',    v(row, 'TotalITBISPercepcion'))
        add(tot, 'TotalISRPercepcion',      v(row, 'TotalISRPercepcion'))
    return tot


def _build_totales_41(parent: ET.Element, row: dict):
    """Totales para tipo 41 (Compras) - con retencion, sin ImpuestosAdicionales."""
    tot = ET.SubElement(parent, 'Totales')
    add(tot, 'MontoGravadoTotal',    v(row, 'MontoGravadoTotal'))
    add(tot, 'MontoGravadoI1',       v(row, 'MontoGravadoI1'))
    add(tot, 'MontoGravadoI2',       v(row, 'MontoGravadoI2'))
    add(tot, 'MontoGravadoI3',       v(row, 'MontoGravadoI3'))
    add(tot, 'MontoExento',          v(row, 'MontoExento'))
    add(tot, 'ITBIS1',               v(row, 'ITBIS1'))
    add(tot, 'ITBIS2',               v(row, 'ITBIS2'))
    add(tot, 'ITBIS3',               v(row, 'ITBIS3'))
    add(tot, 'TotalITBIS',           v(row, 'TotalITBIS'))
    add(tot, 'TotalITBIS1',          v(row, 'TotalITBIS1'))
    add(tot, 'TotalITBIS2',          v(row, 'TotalITBIS2'))
    add(tot, 'TotalITBIS3',          v(row, 'TotalITBIS3'))
    add(tot, 'MontoTotal',           v(row, 'MontoTotal'))
    add(tot, 'MontoPeriodo',         v(row, 'MontoPeriodo'))
    add(tot, 'SaldoAnterior',        v(row, 'SaldoAnterior'))
    add(tot, 'MontoAvancePago',      v(row, 'MontoAvancePago'))
    add(tot, 'ValorPagar',           v(row, 'ValorPagar'))
    add(tot, 'TotalITBISRetenido',   v(row, 'TotalITBISRetenido'))
    add(tot, 'TotalISRRetencion',    v(row, 'TotalISRRetencion'))
    add(tot, 'TotalITBISPercepcion', v(row, 'TotalITBISPercepcion'))
    add(tot, 'TotalISRPercepcion',   v(row, 'TotalISRPercepcion'))


def _build_totales_43(parent: ET.Element, row: dict):
    """Totales para tipo 43 (Gastos Menores) - muy simple."""
    tot = ET.SubElement(parent, 'Totales')
    add(tot, 'MontoExento',      v(row, 'MontoExento'))
    add(tot, 'MontoTotal',       v(row, 'MontoTotal'))
    add(tot, 'MontoPeriodo',     v(row, 'MontoPeriodo'))
    add(tot, 'SaldoAnterior',    v(row, 'SaldoAnterior'))
    add(tot, 'MontoAvancePago',  v(row, 'MontoAvancePago'))
    add(tot, 'ValorPagar',       v(row, 'ValorPagar'))


def _build_totales_44(parent: ET.Element, row: dict):
    """Totales para tipo 44 (Regímenes Especiales) - sin ITBIS, con ImpAd con OtrosImpuestos."""
    tot = ET.SubElement(parent, 'Totales')
    add(tot, 'MontoExento',              v(row, 'MontoExento'))
    add(tot, 'MontoImpuestoAdicional',   v(row, 'MontoImpuestoAdicional'))
    # ImpuestosAdicionales del tipo 44: TipoImpuesto, TasaImpuestoAdicional, OtrosImpuestosAdicionales (req)
    _build_impuestos_adicionales_totales(
        tot, row,
        has_especifico=False, has_advalorem=False, has_otros=True, requires_otros=True
    )
    add(tot, 'MontoTotal',               v(row, 'MontoTotal'))
    add(tot, 'MontoNoFacturable',        v(row, 'MontoNoFacturable'))
    add(tot, 'MontoPeriodo',             v(row, 'MontoPeriodo'))
    add(tot, 'SaldoAnterior',            v(row, 'SaldoAnterior'))
    add(tot, 'MontoAvancePago',          v(row, 'MontoAvancePago'))
    add(tot, 'ValorPagar',               v(row, 'ValorPagar'))


def _build_totales_46(parent: ET.Element, row: dict):
    """Totales para tipo 46 (Exportaciones) - solo ITBIS3."""
    tot = ET.SubElement(parent, 'Totales')
    add(tot, 'MontoGravadoTotal',   v(row, 'MontoGravadoTotal'))
    add(tot, 'MontoGravadoI3',      v(row, 'MontoGravadoI3'))
    add(tot, 'ITBIS3',              v(row, 'ITBIS3'))
    add(tot, 'TotalITBIS',          v(row, 'TotalITBIS'))
    add(tot, 'TotalITBIS3',         v(row, 'TotalITBIS3'))
    add(tot, 'MontoTotal',          v(row, 'MontoTotal'))
    add(tot, 'MontoNoFacturable',   v(row, 'MontoNoFacturable'))
    add(tot, 'MontoPeriodo',        v(row, 'MontoPeriodo'))
    add(tot, 'SaldoAnterior',       v(row, 'SaldoAnterior'))
    add(tot, 'MontoAvancePago',     v(row, 'MontoAvancePago'))
    add(tot, 'ValorPagar',          v(row, 'ValorPagar'))


def _build_totales_47(parent: ET.Element, row: dict):
    """Totales para tipo 47 (Compras Exterior)."""
    tot = ET.SubElement(parent, 'Totales')
    add(tot, 'MontoExento',      v(row, 'MontoExento'))
    add(tot, 'MontoTotal',       v(row, 'MontoTotal'))
    add(tot, 'MontoPeriodo',     v(row, 'MontoPeriodo'))
    add(tot, 'SaldoAnterior',    v(row, 'SaldoAnterior'))
    add(tot, 'MontoAvancePago',  v(row, 'MontoAvancePago'))
    add(tot, 'ValorPagar',       v(row, 'ValorPagar'))
    add(tot, 'TotalISRRetencion',v(row, 'TotalISRRetencion'))


# ---------------------------------------------------------------------------
# Item builders  por variante
# ---------------------------------------------------------------------------

def _build_items_full(detalles: ET.Element, row: dict,
                       has_subcantidad: bool = True,
                       has_grados_alcohol: bool = True,
                       has_imp_tabla: bool = True,
                       has_mineria: bool = True,
                       has_cant_ref: bool = True,
                       has_retencion: bool = False,
                       retencion_required: bool = False):
    """Items completos (tipos 31, 32, 33, 34, 44, 45, 46)."""
    for n in range(1, MAX_ITEMS + 1):
        nl = v(row, f'Item_{n}_NumeroLinea')
        if nl is None:
            break
        item_el = ET.SubElement(detalles, 'Item')
        add(item_el, 'NumeroLinea', nl)
        _build_item_codigos(item_el, row, n)
        add(item_el, 'IndicadorFacturacion', v(row, f'Item_{n}_IndicadorFacturacion'))

        # Retencion
        if has_retencion or retencion_required:
            ret_ind  = v(row, f'Item_{n}_Ret_IndicadorAgenteRetencionoPercepcion')
            ret_itb  = v(row, f'Item_{n}_Ret_MontoITBISRetenido')
            ret_isr  = v(row, f'Item_{n}_Ret_MontoISRRetenido')
            if retencion_required or ret_ind is not None:
                ret = ET.SubElement(item_el, 'Retencion')
                add(ret, 'IndicadorAgenteRetencionoPercepcion', ret_ind)
                add(ret, 'MontoITBISRetenido', ret_itb)
                add(ret, 'MontoISRRetenido',   ret_isr)

        add(item_el, 'NombreItem',             v(row, f'Item_{n}_NombreItem'))
        add(item_el, 'IndicadorBienoServicio', v(row, f'Item_{n}_IndicadorBienoServicio'))
        add(item_el, 'DescripcionItem',        v(row, f'Item_{n}_DescripcionItem'))
        add(item_el, 'CantidadItem',           v(row, f'Item_{n}_CantidadItem'))
        add(item_el, 'UnidadMedida',           v(row, f'Item_{n}_UnidadMedida'))
        if has_cant_ref:
            add(item_el, 'CantidadReferencia', v(row, f'Item_{n}_CantidadReferencia'))
            add(item_el, 'UnidadReferencia',   v(row, f'Item_{n}_UnidadReferencia'))
        if has_subcantidad:
            _build_item_subcantidades(item_el, row, n)
        if has_grados_alcohol:
            add(item_el, 'GradosAlcohol',          v(row, f'Item_{n}_GradosAlcohol'))
            add(item_el, 'PrecioUnitarioReferencia',v(row, f'Item_{n}_PrecioUnitarioReferencia'))
        add(item_el, 'FechaElaboracion',       v(row, f'Item_{n}_FechaElaboracion'))
        add(item_el, 'FechaVencimientoItem',   v(row, f'Item_{n}_FechaVencimientoItem'))
        if has_mineria:
            _build_item_mineria(item_el, row, n)
        add(item_el, 'PrecioUnitarioItem',     v(row, f'Item_{n}_PrecioUnitarioItem'))
        add(item_el, 'DescuentoMonto',         v(row, f'Item_{n}_DescuentoMonto'))
        _build_item_subdescuentos(item_el, row, n)
        add(item_el, 'RecargoMonto',           v(row, f'Item_{n}_RecargoMonto'))
        _build_item_subrecargas(item_el, row, n)
        if has_imp_tabla:
            _build_item_imp_tabla(item_el, row, n)
        _build_item_otra_moneda(item_el, row, n)
        add(item_el, 'MontoItem',              v(row, f'Item_{n}_MontoItem'))


def _build_items_41(detalles: ET.Element, row: dict):
    """Items para tipo 41 (Compras) - Retencion REQUIRED, sin subtables especiales."""
    for n in range(1, MAX_ITEMS + 1):
        nl = v(row, f'Item_{n}_NumeroLinea')
        if nl is None:
            break
        item_el = ET.SubElement(detalles, 'Item')
        add(item_el, 'NumeroLinea',            nl)
        _build_item_codigos(item_el, row, n)
        add(item_el, 'IndicadorFacturacion',   v(row, f'Item_{n}_IndicadorFacturacion'))
        # Retencion REQUIRED
        ret = ET.SubElement(item_el, 'Retencion')
        add(ret, 'IndicadorAgenteRetencionoPercepcion',
            v(row, f'Item_{n}_Ret_IndicadorAgenteRetencionoPercepcion'))
        add(ret, 'MontoITBISRetenido', v(row, f'Item_{n}_Ret_MontoITBISRetenido'))
        add(ret, 'MontoISRRetenido',   v(row, f'Item_{n}_Ret_MontoISRRetenido'))
        add(item_el, 'NombreItem',             v(row, f'Item_{n}_NombreItem'))
        add(item_el, 'IndicadorBienoServicio', v(row, f'Item_{n}_IndicadorBienoServicio'))
        add(item_el, 'DescripcionItem',        v(row, f'Item_{n}_DescripcionItem'))
        add(item_el, 'CantidadItem',           v(row, f'Item_{n}_CantidadItem'))
        add(item_el, 'UnidadMedida',           v(row, f'Item_{n}_UnidadMedida'))
        add(item_el, 'FechaElaboracion',       v(row, f'Item_{n}_FechaElaboracion'))
        add(item_el, 'FechaVencimientoItem',   v(row, f'Item_{n}_FechaVencimientoItem'))
        add(item_el, 'PrecioUnitarioItem',     v(row, f'Item_{n}_PrecioUnitarioItem'))
        add(item_el, 'DescuentoMonto',         v(row, f'Item_{n}_DescuentoMonto'))
        _build_item_subdescuentos(item_el, row, n)
        add(item_el, 'RecargoMonto',           v(row, f'Item_{n}_RecargoMonto'))
        _build_item_subrecargas(item_el, row, n)
        _build_item_otra_moneda(item_el, row, n)
        add(item_el, 'MontoItem',              v(row, f'Item_{n}_MontoItem'))


def _build_items_43(detalles: ET.Element, row: dict):
    """Items para tipo 43 (Gastos Menores) - muy simple."""
    for n in range(1, MAX_ITEMS + 1):
        nl = v(row, f'Item_{n}_NumeroLinea')
        if nl is None:
            break
        item_el = ET.SubElement(detalles, 'Item')
        add(item_el, 'NumeroLinea',            nl)
        _build_item_codigos(item_el, row, n)
        add(item_el, 'IndicadorFacturacion',   v(row, f'Item_{n}_IndicadorFacturacion'))
        add(item_el, 'NombreItem',             v(row, f'Item_{n}_NombreItem'))
        add(item_el, 'IndicadorBienoServicio', v(row, f'Item_{n}_IndicadorBienoServicio'))
        add(item_el, 'DescripcionItem',        v(row, f'Item_{n}_DescripcionItem'))
        add(item_el, 'CantidadItem',           v(row, f'Item_{n}_CantidadItem'))
        add(item_el, 'UnidadMedida',           v(row, f'Item_{n}_UnidadMedida'))
        add(item_el, 'PrecioUnitarioItem',     v(row, f'Item_{n}_PrecioUnitarioItem'))
        _build_item_otra_moneda(item_el, row, n)
        add(item_el, 'MontoItem',              v(row, f'Item_{n}_MontoItem'))


def _build_items_47(detalles: ET.Element, row: dict):
    """Items para tipo 47 (Compras Exterior) - Retencion REQUIRED con MontoISRRetenido req."""
    for n in range(1, MAX_ITEMS + 1):
        nl = v(row, f'Item_{n}_NumeroLinea')
        if nl is None:
            break
        item_el = ET.SubElement(detalles, 'Item')
        add(item_el, 'NumeroLinea',            nl)
        _build_item_codigos(item_el, row, n)
        add(item_el, 'IndicadorFacturacion',   v(row, f'Item_{n}_IndicadorFacturacion'))
        # Retencion REQUIRED, MontoISRRetenido REQUIRED en tipo 47
        ret = ET.SubElement(item_el, 'Retencion')
        add(ret, 'IndicadorAgenteRetencionoPercepcion',
            v(row, f'Item_{n}_Ret_IndicadorAgenteRetencionoPercepcion'))
        add(ret, 'MontoISRRetenido', v(row, f'Item_{n}_Ret_MontoISRRetenido'))
        add(item_el, 'NombreItem',             v(row, f'Item_{n}_NombreItem'))
        add(item_el, 'IndicadorBienoServicio', v(row, f'Item_{n}_IndicadorBienoServicio'))
        add(item_el, 'DescripcionItem',        v(row, f'Item_{n}_DescripcionItem'))
        add(item_el, 'CantidadItem',           v(row, f'Item_{n}_CantidadItem'))
        add(item_el, 'UnidadMedida',           v(row, f'Item_{n}_UnidadMedida'))
        add(item_el, 'PrecioUnitarioItem',     v(row, f'Item_{n}_PrecioUnitarioItem'))
        _build_item_otra_moneda(item_el, row, n)
        add(item_el, 'MontoItem',              v(row, f'Item_{n}_MontoItem'))


# ---------------------------------------------------------------------------
# Builders principales por tipo
# ---------------------------------------------------------------------------

def build_ecf_31(row: dict) -> str:
    root = ET.Element('ECF')
    enc  = ET.SubElement(root, 'Encabezado')
    add(enc, 'Version', '1.0')

    id_doc = ET.SubElement(enc, 'IdDoc')
    add(id_doc, 'TipoeCF',                   v(row, 'TipoeCF'))
    add(id_doc, 'eNCF',                      v(row, 'eNCF'))
    add(id_doc, 'FechaVencimientoSecuencia', v(row, 'FechaVencimientoSecuencia'))
    add(id_doc, 'IndicadorEnvioDiferido',    v(row, 'IndicadorEnvioDiferido'))
    add(id_doc, 'IndicadorMontoGravado',     v(row, 'IndicadorMontoGravado'))
    add(id_doc, 'TipoIngresos',              v(row, 'TipoIngresos'))
    add(id_doc, 'TipoPago',                  v(row, 'TipoPago'))
    add(id_doc, 'FechaLimitePago',           v(row, 'FechaLimitePago'))
    add(id_doc, 'TerminoPago',               v(row, 'TerminoPago'))
    _build_tabla_formas_pago(id_doc, row)
    add(id_doc, 'TipoCuentaPago',            v(row, 'TipoCuentaPago'))
    add(id_doc, 'NumeroCuentaPago',          v(row, 'NumeroCuentaPago'))
    add(id_doc, 'BancoPago',                 v(row, 'BancoPago'))
    add(id_doc, 'FechaDesde',                v(row, 'FechaDesde'))
    add(id_doc, 'FechaHasta',                v(row, 'FechaHasta'))
    add(id_doc, 'TotalPaginas',              v(row, 'TotalPaginas'))

    _build_emisor(enc, row)
    _build_comprador_full(enc, row, has_id_extranjero=False)
    _build_informaciones_adicionales_std(enc, row)
    _build_transporte_std(enc, row)
    _build_totales_full(enc, row, has_imp_adicional=True, has_retencion=True)
    _build_otra_moneda_full(enc, row)

    detalles = ET.SubElement(root, 'DetallesItems')
    _build_items_full(detalles, row, has_retencion=True, has_mineria=False)
    _build_subtotales(root, row)
    _build_descuentos_o_recargos(root, row)
    _build_paginacion(root, row)
    _build_informacion_referencia(root, row)
    add(root, 'FechaHoraFirma', v(row, 'FechaHoraFirma'))

    return _pretty(root)


def build_ecf_32(row: dict) -> str:
    root = ET.Element('ECF')
    enc  = ET.SubElement(root, 'Encabezado')
    add(enc, 'Version', '1.0')

    # IdDoc - tipo 32 NO tiene FechaVencimientoSecuencia
    id_doc = ET.SubElement(enc, 'IdDoc')
    add(id_doc, 'TipoeCF',               v(row, 'TipoeCF'))
    add(id_doc, 'eNCF',                  v(row, 'eNCF'))
    add(id_doc, 'IndicadorEnvioDiferido',v(row, 'IndicadorEnvioDiferido'))
    add(id_doc, 'IndicadorMontoGravado', v(row, 'IndicadorMontoGravado'))
    add(id_doc, 'TipoIngresos',          v(row, 'TipoIngresos'))
    add(id_doc, 'TipoPago',              v(row, 'TipoPago'))
    add(id_doc, 'FechaLimitePago',       v(row, 'FechaLimitePago'))
    add(id_doc, 'TerminoPago',           v(row, 'TerminoPago'))
    _build_tabla_formas_pago(id_doc, row)
    add(id_doc, 'TipoCuentaPago',        v(row, 'TipoCuentaPago'))
    add(id_doc, 'NumeroCuentaPago',      v(row, 'NumeroCuentaPago'))
    add(id_doc, 'BancoPago',             v(row, 'BancoPago'))
    add(id_doc, 'FechaDesde',            v(row, 'FechaDesde'))
    add(id_doc, 'FechaHasta',            v(row, 'FechaHasta'))
    add(id_doc, 'TotalPaginas',          v(row, 'TotalPaginas'))

    _build_emisor(enc, row)
    _build_comprador_full(enc, row)
    _build_informaciones_adicionales_std(enc, row)
    _build_transporte_std(enc, row)
    _build_totales_full(enc, row, has_imp_adicional=True, has_retencion=False)
    _build_otra_moneda_full(enc, row)

    detalles = ET.SubElement(root, 'DetallesItems')
    _build_items_full(detalles, row)
    _build_subtotales(root, row)
    _build_descuentos_o_recargos(root, row)
    _build_paginacion(root, row)
    _build_informacion_referencia(root, row)
    add(root, 'FechaHoraFirma', v(row, 'FechaHoraFirma'))

    return _pretty(root)


def build_ecf_33(row: dict) -> str:
    root = ET.Element('ECF')
    enc  = ET.SubElement(root, 'Encabezado')
    add(enc, 'Version', '1.0')

    id_doc = ET.SubElement(enc, 'IdDoc')
    add(id_doc, 'TipoeCF',                   v(row, 'TipoeCF'))
    add(id_doc, 'eNCF',                      v(row, 'eNCF'))
    add(id_doc, 'FechaVencimientoSecuencia', v(row, 'FechaVencimientoSecuencia'))
    add(id_doc, 'IndicadorEnvioDiferido',    v(row, 'IndicadorEnvioDiferido'))
    add(id_doc, 'IndicadorMontoGravado',     v(row, 'IndicadorMontoGravado'))
    add(id_doc, 'TipoIngresos',              v(row, 'TipoIngresos'))
    add(id_doc, 'TipoPago',                  v(row, 'TipoPago'))
    add(id_doc, 'FechaLimitePago',           v(row, 'FechaLimitePago'))
    add(id_doc, 'TerminoPago',               v(row, 'TerminoPago'))
    _build_tabla_formas_pago(id_doc, row)
    add(id_doc, 'TipoCuentaPago',            v(row, 'TipoCuentaPago'))
    add(id_doc, 'NumeroCuentaPago',          v(row, 'NumeroCuentaPago'))
    add(id_doc, 'BancoPago',                 v(row, 'BancoPago'))
    add(id_doc, 'FechaDesde',                v(row, 'FechaDesde'))
    add(id_doc, 'FechaHasta',                v(row, 'FechaHasta'))
    add(id_doc, 'TotalPaginas',              v(row, 'TotalPaginas'))

    _build_emisor(enc, row)
    _build_comprador_full(enc, row)
    _build_informaciones_adicionales_std(enc, row)
    _build_transporte_std(enc, row)
    _build_totales_full(enc, row, has_imp_adicional=True, has_retencion=True)
    _build_otra_moneda_full(enc, row)

    detalles = ET.SubElement(root, 'DetallesItems')
    _build_items_full(detalles, row, has_retencion=True, retencion_required=False)
    _build_subtotales(root, row)
    _build_descuentos_o_recargos(root, row)
    _build_paginacion(root, row)
    _build_informacion_referencia(root, row, required=True, has_razon=True)
    add(root, 'FechaHoraFirma', v(row, 'FechaHoraFirma'))

    return _pretty(root)


def build_ecf_34(row: dict) -> str:
    root = ET.Element('ECF')
    enc  = ET.SubElement(root, 'Encabezado')
    add(enc, 'Version', '1.0')

    # IdDoc tipo 34 especial: IndicadorNotaCredito, sin TablaFormasPago ni TipoCuenta
    id_doc = ET.SubElement(enc, 'IdDoc')
    add(id_doc, 'TipoeCF',               v(row, 'TipoeCF'))
    add(id_doc, 'eNCF',                  v(row, 'eNCF'))
    add(id_doc, 'IndicadorNotaCredito',  v(row, 'IndicadorNotaCredito'))
    add(id_doc, 'IndicadorEnvioDiferido',v(row, 'IndicadorEnvioDiferido'))
    add(id_doc, 'IndicadorMontoGravado', v(row, 'IndicadorMontoGravado'))
    add(id_doc, 'TipoIngresos',          v(row, 'TipoIngresos'))
    add(id_doc, 'TipoPago',              v(row, 'TipoPago'))
    add(id_doc, 'FechaLimitePago',       v(row, 'FechaLimitePago'))
    add(id_doc, 'FechaDesde',            v(row, 'FechaDesde'))
    add(id_doc, 'FechaHasta',            v(row, 'FechaHasta'))
    add(id_doc, 'TotalPaginas',          v(row, 'TotalPaginas'))

    _build_emisor(enc, row)
    _build_comprador_full(enc, row)
    _build_informaciones_adicionales_std(enc, row)
    _build_transporte_std(enc, row)
    _build_totales_full(enc, row, has_imp_adicional=True, has_retencion=True)
    _build_otra_moneda_full(enc, row)

    detalles = ET.SubElement(root, 'DetallesItems')
    _build_items_full(detalles, row, has_retencion=True, retencion_required=False)
    _build_subtotales(root, row)
    _build_descuentos_o_recargos(root, row)
    _build_paginacion(root, row)

    # InformacionReferencia REQUERIDA en tipo 34
    _build_informacion_referencia(root, row, required=True, has_razon=True)
    add(root, 'FechaHoraFirma', v(row, 'FechaHoraFirma'))

    return _pretty(root)


def build_ecf_41(row: dict) -> str:
    root = ET.Element('ECF')
    enc  = ET.SubElement(root, 'Encabezado')
    add(enc, 'Version', '1.0')

    # IdDoc: sin IndicadorEnvioDiferido, sin TipoIngresos, TipoPago opcional
    id_doc = ET.SubElement(enc, 'IdDoc')
    add(id_doc, 'TipoeCF',                   v(row, 'TipoeCF'))
    add(id_doc, 'eNCF',                      v(row, 'eNCF'))
    add(id_doc, 'FechaVencimientoSecuencia', v(row, 'FechaVencimientoSecuencia'))
    add(id_doc, 'IndicadorMontoGravado',     v(row, 'IndicadorMontoGravado'))
    add(id_doc, 'TipoPago',                  v(row, 'TipoPago'))
    add(id_doc, 'FechaLimitePago',           v(row, 'FechaLimitePago'))
    add(id_doc, 'TerminoPago',               v(row, 'TerminoPago'))
    _build_tabla_formas_pago(id_doc, row)
    add(id_doc, 'TipoCuentaPago',            v(row, 'TipoCuentaPago'))
    add(id_doc, 'NumeroCuentaPago',          v(row, 'NumeroCuentaPago'))
    add(id_doc, 'BancoPago',                 v(row, 'BancoPago'))
    add(id_doc, 'TotalPaginas',              v(row, 'TotalPaginas'))

    _build_emisor(enc, row, has_codigo_vendedor=False, has_zona_ruta=False)

    # Comprador REQUIRED en tipo 41, solo RNCComprador y RazonSocial req
    comp = ET.SubElement(enc, 'Comprador')
    add(comp, 'RNCComprador',          v(row, 'RNCComprador'))
    add(comp, 'RazonSocialComprador',  v(row, 'RazonSocialComprador'))
    add(comp, 'ContactoComprador',     v(row, 'ContactoComprador'))
    add(comp, 'CorreoComprador',       v(row, 'CorreoComprador'))
    add(comp, 'DireccionComprador',    v(row, 'DireccionComprador'))
    add(comp, 'MunicipioComprador',    v(row, 'MunicipioComprador'))
    add(comp, 'ProvinciaComprador',    v(row, 'ProvinciaComprador'))
    add(comp, 'CodigoInternoComprador',v(row, 'CodigoInternoComprador'))
    add(comp, 'ResponsablePago',       v(row, 'ResponsablePago'))
    add(comp, 'InformacionAdicionalComprador', v(row, 'InformacionAdicionalComprador'))

    _build_totales_41(enc, row)

    # OtraMoneda simplificada para 41
    cols_om = ['OM_TipoMoneda', 'OM_TipoCambio', 'OM_MontoGravadoTotalOtraMoneda',
               'OM_MontoGravado1OtraMoneda', 'OM_MontoGravado2OtraMoneda',
               'OM_MontoGravado3OtraMoneda', 'OM_MontoExentoOtraMoneda',
               'OM_TotalITBISOtraMoneda', 'OM_TotalITBIS1OtraMoneda',
               'OM_TotalITBIS2OtraMoneda', 'OM_TotalITBIS3OtraMoneda',
               'OM_MontoTotalOtraMoneda']
    if any(v(row, c) is not None for c in cols_om):
        om = ET.SubElement(enc, 'OtraMoneda')
        add(om, 'TipoMoneda',                 v(row, 'OM_TipoMoneda'))
        add(om, 'TipoCambio',                 v(row, 'OM_TipoCambio'))
        add(om, 'MontoGravadoTotalOtraMoneda',v(row, 'OM_MontoGravadoTotalOtraMoneda'))
        add(om, 'MontoGravado1OtraMoneda',    v(row, 'OM_MontoGravado1OtraMoneda'))
        add(om, 'MontoGravado2OtraMoneda',    v(row, 'OM_MontoGravado2OtraMoneda'))
        add(om, 'MontoGravado3OtraMoneda',    v(row, 'OM_MontoGravado3OtraMoneda'))
        add(om, 'MontoExentoOtraMoneda',      v(row, 'OM_MontoExentoOtraMoneda'))
        add(om, 'TotalITBISOtraMoneda',       v(row, 'OM_TotalITBISOtraMoneda'))
        add(om, 'TotalITBIS1OtraMoneda',      v(row, 'OM_TotalITBIS1OtraMoneda'))
        add(om, 'TotalITBIS2OtraMoneda',      v(row, 'OM_TotalITBIS2OtraMoneda'))
        add(om, 'TotalITBIS3OtraMoneda',      v(row, 'OM_TotalITBIS3OtraMoneda'))
        add(om, 'MontoTotalOtraMoneda',       v(row, 'OM_MontoTotalOtraMoneda'))

    detalles = ET.SubElement(root, 'DetallesItems')
    _build_items_41(detalles, row)
    _build_subtotales(root, row)
    _build_descuentos_o_recargos(root, row)
    _build_paginacion(root, row, has_imp_adicional=False, has_no_facturable=False)
    _build_informacion_referencia(root, row)
    add(root, 'FechaHoraFirma', v(row, 'FechaHoraFirma'))

    return _pretty(root)


def build_ecf_43(row: dict) -> str:
    root = ET.Element('ECF')
    enc  = ET.SubElement(root, 'Encabezado')
    add(enc, 'Version', '1.0')

    id_doc = ET.SubElement(enc, 'IdDoc')
    add(id_doc, 'TipoeCF',                   v(row, 'TipoeCF'))
    add(id_doc, 'eNCF',                      v(row, 'eNCF'))
    add(id_doc, 'FechaVencimientoSecuencia', v(row, 'FechaVencimientoSecuencia'))
    add(id_doc, 'TipoPago',                  v(row, 'TipoPago'))
    add(id_doc, 'TotalPaginas',              v(row, 'TotalPaginas'))

    _build_emisor(enc, row, has_codigo_vendedor=False, has_zona_ruta=False)
    _build_totales_43(enc, row)

    # OtraMoneda mínima tipo 43
    cols_om = ['OM_TipoMoneda', 'OM_TipoCambio',
               'OM_MontoExentoOtraMoneda', 'OM_MontoTotalOtraMoneda']
    if any(v(row, c) is not None for c in cols_om):
        om = ET.SubElement(enc, 'OtraMoneda')
        add(om, 'TipoMoneda',              v(row, 'OM_TipoMoneda'))
        add(om, 'TipoCambio',              v(row, 'OM_TipoCambio'))
        add(om, 'MontoExentoOtraMoneda',   v(row, 'OM_MontoExentoOtraMoneda'))
        add(om, 'MontoTotalOtraMoneda',    v(row, 'OM_MontoTotalOtraMoneda'))

    detalles = ET.SubElement(root, 'DetallesItems')
    _build_items_43(detalles, row)
    _build_subtotales(root, row, has_imp_adicional=False)
    _build_paginacion(root, row, has_imp_adicional=False, has_no_facturable=False)
    _build_informacion_referencia(root, row)
    add(root, 'FechaHoraFirma', v(row, 'FechaHoraFirma'))

    return _pretty(root)


def build_ecf_44(row: dict) -> str:
    root = ET.Element('ECF')
    enc  = ET.SubElement(root, 'Encabezado')
    add(enc, 'Version', '1.0')

    id_doc = ET.SubElement(enc, 'IdDoc')
    add(id_doc, 'TipoeCF',                   v(row, 'TipoeCF'))
    add(id_doc, 'eNCF',                      v(row, 'eNCF'))
    add(id_doc, 'FechaVencimientoSecuencia', v(row, 'FechaVencimientoSecuencia'))
    add(id_doc, 'IndicadorEnvioDiferido',    v(row, 'IndicadorEnvioDiferido'))
    add(id_doc, 'TipoIngresos',              v(row, 'TipoIngresos'))
    add(id_doc, 'TipoPago',                  v(row, 'TipoPago'))
    add(id_doc, 'FechaLimitePago',           v(row, 'FechaLimitePago'))
    add(id_doc, 'TerminoPago',               v(row, 'TerminoPago'))
    _build_tabla_formas_pago(id_doc, row)
    add(id_doc, 'TipoCuentaPago',            v(row, 'TipoCuentaPago'))
    add(id_doc, 'NumeroCuentaPago',          v(row, 'NumeroCuentaPago'))
    add(id_doc, 'BancoPago',                 v(row, 'BancoPago'))
    add(id_doc, 'FechaDesde',                v(row, 'FechaDesde'))
    add(id_doc, 'FechaHasta',                v(row, 'FechaHasta'))
    add(id_doc, 'TotalPaginas',              v(row, 'TotalPaginas'))

    _build_emisor(enc, row)

    # Comprador tipo 44: RazonSocialComprador REQUIRED
    comp = ET.SubElement(enc, 'Comprador')
    add(comp, 'RNCComprador',          v(row, 'RNCComprador'))
    add(comp, 'IdentificadorExtranjero', v(row, 'IdentificadorExtranjero'))
    add(comp, 'RazonSocialComprador',  v(row, 'RazonSocialComprador'))
    add(comp, 'ContactoComprador',     v(row, 'ContactoComprador'))
    add(comp, 'CorreoComprador',       v(row, 'CorreoComprador'))
    add(comp, 'DireccionComprador',    v(row, 'DireccionComprador'))
    add(comp, 'MunicipioComprador',    v(row, 'MunicipioComprador'))
    add(comp, 'ProvinciaComprador',    v(row, 'ProvinciaComprador'))
    add(comp, 'FechaEntrega',          v(row, 'FechaEntrega'))
    add(comp, 'ContactoEntrega',       v(row, 'ContactoEntrega'))
    add(comp, 'DireccionEntrega',      v(row, 'DireccionEntrega'))
    add(comp, 'TelefonoAdicional',     v(row, 'TelefonoAdicional'))
    add(comp, 'FechaOrdenCompra',      v(row, 'FechaOrdenCompra'))
    add(comp, 'NumeroOrdenCompra',     v(row, 'NumeroOrdenCompra'))
    add(comp, 'CodigoInternoComprador',v(row, 'CodigoInternoComprador'))
    add(comp, 'ResponsablePago',       v(row, 'ResponsablePago'))
    add(comp, 'InformacionAdicionalComprador', v(row, 'InformacionAdicionalComprador'))

    _build_informaciones_adicionales_std(enc, row)
    _build_transporte_std(enc, row)
    _build_totales_44(enc, row)

    # OtraMoneda para tipo 44
    cols_om44 = ['OM_TipoMoneda', 'OM_TipoCambio', 'OM_MontoExentoOtraMoneda',
                 'OM_MontoImpuestoAdicionalOtraMoneda', 'OM_MontoTotalOtraMoneda']
    if any(v(row, c) is not None for c in cols_om44):
        om = ET.SubElement(enc, 'OtraMoneda')
        add(om, 'TipoMoneda',                       v(row, 'OM_TipoMoneda'))
        add(om, 'TipoCambio',                       v(row, 'OM_TipoCambio'))
        add(om, 'MontoExentoOtraMoneda',            v(row, 'OM_MontoExentoOtraMoneda'))
        add(om, 'MontoImpuestoAdicionalOtraMoneda', v(row, 'OM_MontoImpuestoAdicionalOtraMoneda'))
        # ImpuestosAdicionalesOtraMoneda tipo 44
        items_om = []
        for i in range(1, MAX_IA_TOT_OM + 1):
            ti  = v(row, f'OM_ImpAd_{i}_TipoImpuesto')
            ta  = v(row, f'OM_ImpAd_{i}_TasaImpuestoAdicional')
            otr = v(row, f'OM_ImpAd_{i}_OtrosMontos')
            if ti:
                items_om.append((ti, ta, otr))
        if items_om:
            wrap = ET.SubElement(om, 'ImpuestosAdicionalesOtraMoneda')
            for ti, ta, otr in items_om:
                ia = ET.SubElement(wrap, 'ImpuestoAdicionalOtraMoneda')
                add(ia, 'TipoImpuestoOtraMoneda', ti)
                add(ia, 'TasaImpuestoAdicionalOtraMoneda', ta)
                add(ia, 'OtrosImpuestosAdicionalesOtraMoneda', otr)
        add(om, 'MontoTotalOtraMoneda', v(row, 'OM_MontoTotalOtraMoneda'))

    detalles = ET.SubElement(root, 'DetallesItems')
    _build_items_full(detalles, row,
                      has_subcantidad=False, has_grados_alcohol=False,
                      has_mineria=False, has_cant_ref=False,
                      has_imp_tabla=True)
    _build_subtotales(root, row)
    _build_descuentos_o_recargos(root, row)
    _build_paginacion(root, row)
    _build_informacion_referencia(root, row)
    add(root, 'FechaHoraFirma', v(row, 'FechaHoraFirma'))

    return _pretty(root)


def build_ecf_45(row: dict) -> str:
    root = ET.Element('ECF')
    enc  = ET.SubElement(root, 'Encabezado')
    add(enc, 'Version', '1.0')

    id_doc = ET.SubElement(enc, 'IdDoc')
    add(id_doc, 'TipoeCF',                   v(row, 'TipoeCF'))
    add(id_doc, 'eNCF',                      v(row, 'eNCF'))
    add(id_doc, 'FechaVencimientoSecuencia', v(row, 'FechaVencimientoSecuencia'))
    add(id_doc, 'IndicadorEnvioDiferido',    v(row, 'IndicadorEnvioDiferido'))
    add(id_doc, 'IndicadorMontoGravado',     v(row, 'IndicadorMontoGravado'))
    add(id_doc, 'TipoIngresos',              v(row, 'TipoIngresos'))
    add(id_doc, 'TipoPago',                  v(row, 'TipoPago'))
    add(id_doc, 'FechaLimitePago',           v(row, 'FechaLimitePago'))
    add(id_doc, 'TerminoPago',               v(row, 'TerminoPago'))
    _build_tabla_formas_pago(id_doc, row)
    add(id_doc, 'TipoCuentaPago',            v(row, 'TipoCuentaPago'))
    add(id_doc, 'NumeroCuentaPago',          v(row, 'NumeroCuentaPago'))
    add(id_doc, 'BancoPago',                 v(row, 'BancoPago'))
    add(id_doc, 'FechaDesde',                v(row, 'FechaDesde'))
    add(id_doc, 'FechaHasta',                v(row, 'FechaHasta'))
    add(id_doc, 'TotalPaginas',              v(row, 'TotalPaginas'))

    _build_emisor(enc, row)

    # Comprador tipo 45: RNCComprador y RazonSocialComprador REQUIRED
    comp = ET.SubElement(enc, 'Comprador')
    add(comp, 'RNCComprador',          v(row, 'RNCComprador'))
    add(comp, 'RazonSocialComprador',  v(row, 'RazonSocialComprador'))
    add(comp, 'ContactoComprador',     v(row, 'ContactoComprador'))
    add(comp, 'CorreoComprador',       v(row, 'CorreoComprador'))
    add(comp, 'DireccionComprador',    v(row, 'DireccionComprador'))
    add(comp, 'MunicipioComprador',    v(row, 'MunicipioComprador'))
    add(comp, 'ProvinciaComprador',    v(row, 'ProvinciaComprador'))
    add(comp, 'FechaEntrega',          v(row, 'FechaEntrega'))
    add(comp, 'ContactoEntrega',       v(row, 'ContactoEntrega'))
    add(comp, 'DireccionEntrega',      v(row, 'DireccionEntrega'))
    add(comp, 'TelefonoAdicional',     v(row, 'TelefonoAdicional'))
    add(comp, 'FechaOrdenCompra',      v(row, 'FechaOrdenCompra'))
    add(comp, 'NumeroOrdenCompra',     v(row, 'NumeroOrdenCompra'))
    add(comp, 'CodigoInternoComprador',v(row, 'CodigoInternoComprador'))
    add(comp, 'ResponsablePago',       v(row, 'ResponsablePago'))
    add(comp, 'InformacionAdicionalComprador', v(row, 'InformacionAdicionalComprador'))

    _build_informaciones_adicionales_std(enc, row)
    _build_transporte_std(enc, row)
    _build_totales_full(enc, row, has_imp_adicional=True, has_retencion=False)
    _build_otra_moneda_full(enc, row)

    detalles = ET.SubElement(root, 'DetallesItems')
    _build_items_full(detalles, row, has_mineria=False)
    _build_subtotales(root, row)
    _build_descuentos_o_recargos(root, row)
    _build_paginacion(root, row)
    _build_informacion_referencia(root, row)
    add(root, 'FechaHoraFirma', v(row, 'FechaHoraFirma'))

    return _pretty(root)


def build_ecf_46(row: dict) -> str:
    root = ET.Element('ECF')
    enc  = ET.SubElement(root, 'Encabezado')
    add(enc, 'Version', '1.0')

    id_doc = ET.SubElement(enc, 'IdDoc')
    add(id_doc, 'TipoeCF',                   v(row, 'TipoeCF'))
    add(id_doc, 'eNCF',                      v(row, 'eNCF'))
    add(id_doc, 'FechaVencimientoSecuencia', v(row, 'FechaVencimientoSecuencia'))
    add(id_doc, 'IndicadorEnvioDiferido',    v(row, 'IndicadorEnvioDiferido'))
    add(id_doc, 'TipoIngresos',              v(row, 'TipoIngresos'))
    add(id_doc, 'TipoPago',                  v(row, 'TipoPago'))
    add(id_doc, 'FechaLimitePago',           v(row, 'FechaLimitePago'))
    add(id_doc, 'TerminoPago',               v(row, 'TerminoPago'))
    _build_tabla_formas_pago(id_doc, row)
    add(id_doc, 'TipoCuentaPago',            v(row, 'TipoCuentaPago'))
    add(id_doc, 'NumeroCuentaPago',          v(row, 'NumeroCuentaPago'))
    add(id_doc, 'BancoPago',                 v(row, 'BancoPago'))
    add(id_doc, 'FechaDesde',                v(row, 'FechaDesde'))
    add(id_doc, 'FechaHasta',                v(row, 'FechaHasta'))
    add(id_doc, 'TotalPaginas',              v(row, 'TotalPaginas'))

    _build_emisor(enc, row)

    # Comprador tipo 46: RazonSocialComprador REQUIRED + PaisComprador extra
    comp = ET.SubElement(enc, 'Comprador')
    add(comp, 'RNCComprador',          v(row, 'RNCComprador'))
    add(comp, 'IdentificadorExtranjero', v(row, 'IdentificadorExtranjero'))
    add(comp, 'RazonSocialComprador',  v(row, 'RazonSocialComprador'))
    add(comp, 'ContactoComprador',     v(row, 'ContactoComprador'))
    add(comp, 'CorreoComprador',       v(row, 'CorreoComprador'))
    add(comp, 'DireccionComprador',    v(row, 'DireccionComprador'))
    add(comp, 'MunicipioComprador',    v(row, 'MunicipioComprador'))
    add(comp, 'ProvinciaComprador',    v(row, 'ProvinciaComprador'))
    add(comp, 'PaisComprador',         v(row, 'PaisComprador'))
    add(comp, 'FechaEntrega',          v(row, 'FechaEntrega'))
    add(comp, 'ContactoEntrega',       v(row, 'ContactoEntrega'))
    add(comp, 'DireccionEntrega',      v(row, 'DireccionEntrega'))
    add(comp, 'TelefonoAdicional',     v(row, 'TelefonoAdicional'))
    add(comp, 'FechaOrdenCompra',      v(row, 'FechaOrdenCompra'))
    add(comp, 'NumeroOrdenCompra',     v(row, 'NumeroOrdenCompra'))
    add(comp, 'CodigoInternoComprador',v(row, 'CodigoInternoComprador'))
    add(comp, 'ResponsablePago',       v(row, 'ResponsablePago'))
    add(comp, 'InformacionAdicionalComprador', v(row, 'InformacionAdicionalComprador'))

    _build_informaciones_adicionales_46(enc, row)
    _build_transporte_46(enc, row)
    _build_totales_46(enc, row)

    # OtraMoneda tipo 46
    cols_om46 = ['OM_TipoMoneda', 'OM_TipoCambio',
                 'OM_MontoGravadoTotalOtraMoneda', 'OM_MontoGravado3OtraMoneda',
                 'OM_TotalITBISOtraMoneda', 'OM_TotalITBIS3OtraMoneda',
                 'OM_MontoTotalOtraMoneda']
    if any(v(row, c) is not None for c in cols_om46):
        om = ET.SubElement(enc, 'OtraMoneda')
        add(om, 'TipoMoneda',                 v(row, 'OM_TipoMoneda'))
        add(om, 'TipoCambio',                 v(row, 'OM_TipoCambio'))
        add(om, 'MontoGravadoTotalOtraMoneda',v(row, 'OM_MontoGravadoTotalOtraMoneda'))
        add(om, 'MontoGravado3OtraMoneda',    v(row, 'OM_MontoGravado3OtraMoneda'))
        add(om, 'TotalITBISOtraMoneda',       v(row, 'OM_TotalITBISOtraMoneda'))
        add(om, 'TotalITBIS3OtraMoneda',      v(row, 'OM_TotalITBIS3OtraMoneda'))
        add(om, 'MontoTotalOtraMoneda',       v(row, 'OM_MontoTotalOtraMoneda'))

    detalles = ET.SubElement(root, 'DetallesItems')
    _build_items_full(detalles, row,
                      has_subcantidad=True, has_grados_alcohol=True,
                      has_imp_tabla=False, has_mineria=True,
                      has_cant_ref=True)
    _build_subtotales(root, row, has_imp_adicional=False)
    _build_descuentos_o_recargos(root, row)
    _build_paginacion(root, row, has_imp_adicional=False)
    _build_informacion_referencia(root, row)
    add(root, 'FechaHoraFirma', v(row, 'FechaHoraFirma'))

    return _pretty(root)


def build_ecf_47(row: dict) -> str:
    root = ET.Element('ECF')
    enc  = ET.SubElement(root, 'Encabezado')
    add(enc, 'Version', '1.0')

    id_doc = ET.SubElement(enc, 'IdDoc')
    add(id_doc, 'TipoeCF',                   v(row, 'TipoeCF'))
    add(id_doc, 'eNCF',                      v(row, 'eNCF'))
    add(id_doc, 'FechaVencimientoSecuencia', v(row, 'FechaVencimientoSecuencia'))
    add(id_doc, 'TipoPago',                  v(row, 'TipoPago'))
    add(id_doc, 'FechaLimitePago',           v(row, 'FechaLimitePago'))
    add(id_doc, 'TerminoPago',               v(row, 'TerminoPago'))
    _build_tabla_formas_pago(id_doc, row)
    add(id_doc, 'TipoCuentaPago',            v(row, 'TipoCuentaPago'))
    add(id_doc, 'NumeroCuentaPago',          v(row, 'NumeroCuentaPago'))
    add(id_doc, 'BancoPago',                 v(row, 'BancoPago'))
    add(id_doc, 'FechaDesde',                v(row, 'FechaDesde'))
    add(id_doc, 'FechaHasta',                v(row, 'FechaHasta'))
    add(id_doc, 'TotalPaginas',              v(row, 'TotalPaginas'))

    # Emisor tipo 47: sin CodigoVendedor, ZonaVenta, RutaVenta
    _build_emisor(enc, row, has_codigo_vendedor=False, has_zona_ruta=False)

    # Comprador tipo 47: opcional, solo IdentificadorExtranjero y RazonSocial
    id_ext = v(row, 'IdentificadorExtranjero')
    raz_soc = v(row, 'RazonSocialComprador')
    if id_ext or raz_soc:
        comp = ET.SubElement(enc, 'Comprador')
        add(comp, 'IdentificadorExtranjero', id_ext)
        add(comp, 'RazonSocialComprador',    raz_soc)

    # Transporte tipo 47: solo PaisDestino
    pais_dest = v(row, 'TR_PaisDestino')
    if pais_dest:
        tr = ET.SubElement(enc, 'Transporte')
        add(tr, 'PaisDestino', pais_dest)

    _build_totales_47(enc, row)

    # OtraMoneda tipo 47
    cols_om47 = ['OM_TipoMoneda', 'OM_TipoCambio',
                 'OM_MontoExentoOtraMoneda', 'OM_MontoTotalOtraMoneda']
    if any(v(row, c) is not None for c in cols_om47):
        om = ET.SubElement(enc, 'OtraMoneda')
        add(om, 'TipoMoneda',            v(row, 'OM_TipoMoneda'))
        add(om, 'TipoCambio',            v(row, 'OM_TipoCambio'))
        add(om, 'MontoExentoOtraMoneda', v(row, 'OM_MontoExentoOtraMoneda'))
        add(om, 'MontoTotalOtraMoneda',  v(row, 'OM_MontoTotalOtraMoneda'))

    detalles = ET.SubElement(root, 'DetallesItems')
    _build_items_47(detalles, row)
    _build_subtotales(root, row, has_imp_adicional=False)
    _build_paginacion(root, row, has_imp_adicional=False, has_no_facturable=False)
    _build_informacion_referencia(root, row)
    add(root, 'FechaHoraFirma', v(row, 'FechaHoraFirma'))

    return _pretty(root)


# ---------------------------------------------------------------------------
# Dispatcher principal
# ---------------------------------------------------------------------------

BUILDERS = {
    31: build_ecf_31,
    32: build_ecf_32,
    33: build_ecf_33,
    34: build_ecf_34,
    41: build_ecf_41,
    43: build_ecf_43,
    44: build_ecf_44,
    45: build_ecf_45,
    46: build_ecf_46,
    47: build_ecf_47,
}


def build_ecf(row: dict) -> tuple[str, str]:
    """
    Construye el XML para la fila dada.
    Retorna (xml_string, nombre_archivo) o lanza ValueError.
    """
    tipo_raw = row.get('TipoeCF')
    if _is_empty(tipo_raw):
        encf_raw = str(row.get('eNCF') or '').strip()
        if len(encf_raw) >= 3 and encf_raw[0].upper() == 'E' and encf_raw[1:3].isdigit():
            tipo_raw = encf_raw[1:3]
    if _is_empty(tipo_raw):
        raise ValueError("La columna 'TipoeCF' está vacía y no se pudo derivar del eNCF.")

    try:
        tipo = int(float(str(tipo_raw).strip()))
    except (ValueError, TypeError):
        raise ValueError(f"TipoeCF inválido: {tipo_raw!r}")

    if tipo not in BUILDERS:
        raise ValueError(
            f"TipoeCF {tipo} no soportado. Tipos válidos: {sorted(BUILDERS)}"
        )

    encf = _clean(row.get('eNCF')) or f'ECF_{tipo}'
    filename = ''.join(c for c in encf if c.isalnum() or c in '-_') + '.xml'

    xml_str = BUILDERS[tipo](row)
    return xml_str, filename
