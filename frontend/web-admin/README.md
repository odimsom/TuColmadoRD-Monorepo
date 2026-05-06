# WebAdmin

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 21.2.6.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Selenium e2e for account creation

This project includes a Selenium test that validates the full account creation flow in `/auth/register`.

```bash
npm run test:e2e:account
```

What this test validates:

- Valid form submission in the register screen.
- Terms modal behavior (disabled button until terms are accepted).
- Successful account creation state.
- Session token persisted in `localStorage`.

Notes:

- The test starts `ng serve` automatically.
- A local mock API is started by the test on `http://127.0.0.1:8080` to make the run deterministic.
- Requires a local Chrome installation.

## Selenium suite for Tarea 4

This repository includes a complete Selenium suite aligned to the assignment requirements (happy path, negative, and limit scenarios for authentication flows).

```bash
npm run test:e2e:task4
```

Para ejecutar la misma suite con Chrome visible (no headless), usa:

```bash
npm run test:e2e:task4:headed
```

Para grabar video con una ejecucion mas lenta (pausas entre acciones):

```bash
npm run test:e2e:task4:headed:slow
```

Generated artifacts:

- HTML report: `tests/e2e/artifacts/report-task4.html`
- Automatic screenshots for each scenario: `tests/e2e/artifacts/screenshots/`

Included stories in the suite:

- US01: Login success.
- US02: Login invalid credentials.
- US03: Login field validation limits.
- US04: Account creation success.
- US05: Register validation limits and backend error handling.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
