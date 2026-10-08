import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <header class="topbar">
      <h1>Gestión de comercios</h1>
      <nav>
        <a routerLink="/upload" routerLinkActive="active">1. Cargar archivo</a>
        <a routerLink="/process" routerLinkActive="active">2. Procesar</a>
        <a routerLink="/quarantine" routerLinkActive="active">3. Errores</a>
      </nav>
    </header>
    <main class="container"><router-outlet /></main>
  `,
})
export class App {}
