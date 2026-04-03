# TuColmadoRD - Frontend Monorepo

Este repositorio contiene las aplicaciones frontend para la plataforma **TuColmadoRD**, un ecosistema SaaS B2B para la gestión, automatización y escalabilidad de colmados.

## 📦 Estructura del Proyecto

El monorepo está compuesto por las siguientes aplicaciones:

- **[landing-page](./landing-page/)**: Sitio web público promocional e interactivo para mostrar los planes.
  - **Tecnologías:** Vue 3, Vite, Tailwind CSS.
  - **Puerto de desarrollo:** `5173`

- **[web-admin](./web-admin/)**: Panel administrativo para los dueños de colmados, que incluye configuración, punto de venta (POS) e inventario.
  - **Tecnologías:** Angular 19, Tailwind CSS, DaisyUI, Iconify.
  - **Puerto de desarrollo:** `4200`

## 🚀 Requisitos Previos

- Node.js (v18 o superior)
- npm (v9 o superior)

## 🛠️ Instalación y Ejecución

Para ejecutar ambos proyectos de manera independiente:

### 1. Landing Page (Vue)
Navega a la carpeta de la landing page, instala las dependencias y arranca el servidor de desarrollo:
```bash
cd landing-page
npm install
npm run dev
```

### 2. Web Admin (Angular)
Navega a la carpeta del admin, instala las dependencias y arranca el servidor de Angular:
```bash
cd web-admin
npm install
npm run start # o ng serve
```

## 🎨 Diseño y Estilos
Ambos proyectos utilizan **Tailwind CSS**. Adicionalmente, el proyecto `web-admin` implementa la librería de componentes **DaisyUI** y cuenta con un tema global (`tucolmadomate`) configurado expresamente como un diseño oscuro (Dark Mate) unificado de la marca.