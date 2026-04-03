import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

declare global {
  interface Window {
    backToLauncher?: () => void;
    chrome?: {
      webview?: {
        postMessage: (message: string) => void;
      };
    };
  }
}

window.backToLauncher = () => {
  if (window.chrome?.webview) {
    window.chrome.webview.postMessage('back-to-launcher');
  }
};

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
