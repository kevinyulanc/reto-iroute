import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'upload' },
  {
    path: 'upload',
    title: 'Cargar archivo',
    loadComponent: () => import('./pages/upload/upload-page').then((m) => m.UploadPage),
  },
  {
    path: 'process',
    title: 'Procesar',
    loadComponent: () => import('./pages/process/process-page').then((m) => m.ProcessPage),
  },
  {
    path: 'quarantine',
    title: 'Errores',
    loadComponent: () => import('./pages/quarantine/quarantine-page').then((m) => m.QuarantinePage),
  },
  { path: '**', redirectTo: 'upload' },
];
