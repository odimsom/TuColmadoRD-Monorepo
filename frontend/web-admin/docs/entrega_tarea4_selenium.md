# Tarea 4 - Pruebas Automatizadas con Selenium

## Datos del estudiante
- Proyecto base: TuColmadoRD (individual)
- Actividad: Individual
- Herramienta de automatizacion: Selenium WebDriver con JavaScript (sin Selenium IDE)

## Entregables obligatorios

### 1) Repositorios de codigo (publicos)
- Repositorio principal: https://github.com/odimsom/TuColmadoRD
- Repositorio frontend: https://github.com/odimsom/TuColmadoRD.Frontend
- Repositorio auth: https://github.com/odimsom/tu_colmado_auth

### 2) Historias de usuario + criterios de aceptacion/rechazo
- Azure Dashboard: https://dev.azure.com/20231065/TuColmadoRD/_dashboards/dashboard/ad00a297-f8a4-4c4e-90ef-54ae5e597165
- Archivo de importacion Azure DevOps (historico): web-admin/docs/azure-devops/import_tarea4_completado.csv

### 3) Video demostrativo (publico)
- YouTube: https://youtu.be/6BL5Q5CluFA

## Cobertura de pruebas Selenium

Se implemento un minimo de 5 historias de usuario y al menos un caso por historia:
- US01: Login exitoso (camino feliz)
- US02: Login invalido (negativa)
- US03: Validaciones de login (limites)
- US04: Registro exitoso (camino feliz)
- US05: Registro con validaciones + error de backend (negativa y limites)

Flujos cubiertos:
- Inicio de sesion (login)
- Registro de cuenta (create del formulario CRUD de usuarios/tenant)

## Evidencias generadas automaticamente
- Reporte HTML: web-admin/tests/e2e/artifacts/report-task4.html
- Capturas por escenario: web-admin/tests/e2e/artifacts/screenshots/

## Ejecucion local de pruebas
- Ejecucion normal (headless): npm run test:e2e:task4
- Ejecucion visual para demo: npm run test:e2e:task4:headed
- Ejecucion visual lenta para grabacion: npm run test:e2e:task4:headed:slow

## Permisos de acceso requeridos (Azure DevOps)
Otorgar acceso en Azure DevOps a los siguientes correos:
- ktejada@itla.edu.do
- 20186927@itla.edu.do

Nota de acceso:
- Los repositorios y el video estan publicos.
- Azure DevOps es el unico recurso que requiere permisos explicitos para evaluacion.

## Cumplimiento de restricciones academicas
- No se uso Selenium IDE.
- Se utilizo Selenium WebDriver con JavaScript.
- Evidencias y criterios documentados en Azure DevOps y artefactos de prueba.

## Checklist final antes de entregar
- Repositorios publicos y accesibles
- Video publico en YouTube
- Dashboard/board de Azure accesible para docentes
- Importacion de historias/tareas en Azure completada
- Reporte HTML y capturas generadas
- Ultima ejecucion de pruebas en verde
