"""
xsd_validator.py
Validación de XML generados contra esquemas XSD de la DGII.
"""

import os
import logging
from pathlib import Path

from lxml import etree

logger = logging.getLogger(__name__)

XSD_DIR = os.environ.get("ECF_XSD_DIR", "")

_schema_cache: dict[int, etree.XMLSchema | None] = {}

XSD_FILES = {
    31: "e-CF 31 v.1.0.xsd",
    32: "e-CF 32 v.1.0.xsd",
    33: "e-CF 33 v.1.0.xsd",
    34: "e-CF 34 v.1.0.xsd",
    41: "e-CF 41 v.1.0.xsd",
    43: "e-CF 43 v.1.0.xsd",
    44: "e-CF 44 v.1.0.xsd",
    45: "e-CF 45 v.1.0.xsd",
    46: "e-CF 46 v.1.0.xsd",
    47: "e-CF 47 v.1.0.xsd",
}


def _load_schema(tipo: int) -> etree.XMLSchema | None:
    """Carga y cachea el schema XSD para el tipo dado."""
    if tipo in _schema_cache:
        return _schema_cache[tipo]

    if not XSD_DIR:
        _schema_cache[tipo] = None
        return None

    filename = XSD_FILES.get(tipo)
    if not filename:
        _schema_cache[tipo] = None
        return None

    xsd_path = Path(XSD_DIR) / filename
    if not xsd_path.is_file():
        logger.warning("XSD no encontrado: %s", xsd_path)
        _schema_cache[tipo] = None
        return None

    try:
        schema_doc = etree.parse(str(xsd_path))
        schema = etree.XMLSchema(schema_doc)
        _schema_cache[tipo] = schema
        return schema
    except etree.Error as exc:
        logger.error("Error cargando XSD tipo %d: %s", tipo, exc)
        _schema_cache[tipo] = None
        return None


def validate_xml(xml_str: str, tipo: int) -> list[str]:
    """
    Valida un XML contra el XSD del tipo dado.
    Retorna lista de errores (vacía si es válido).
    Si no hay XSD disponible, retorna lista vacía.
    """
    schema = _load_schema(tipo)
    if schema is None:
        return []

    try:
        doc = etree.fromstring(xml_str.encode("utf-8"))
        schema.validate(doc)
        return [str(e) for e in schema.error_log]
    except etree.XMLSyntaxError as exc:
        return [f"XML mal formado: {exc}"]
