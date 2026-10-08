import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommerceService } from '../../core/commerce.service';
import { messageFrom } from '../../core/http-error';

@Component({
  selector: 'app-process-page',
  imports: [RouterLink],
  template: `
    <section class="card">
      <h2>Procesar información por fecha</h2>
      <p class="muted">
        Elige la fecha de proceso (columna <strong>pc_processdate</strong>). Los registros inválidos
        se moverán a cuarentena.
      </p>

      <label>
        Fecha de proceso
        <input type="date" [value]="date()" (input)="onDateChange($event)" />
      </label>
      <button type="button" [disabled]="!date() || loading()" (click)="process()">
        {{ loading() ? 'Procesando...' : 'Procesar' }}
      </button>

      @if (error()) { <div class="msg error">{{ error() }}</div> }
      @if (result() !== null) {
        <div class="msg ok">
          Proceso terminado: {{ result() }} registros enviados a cuarentena.
          <a routerLink="/quarantine">Ver errores</a>
        </div>
      }
    </section>
  `,
  styles: `
    label { display: inline-flex; gap: 10px; align-items: center; margin-right: 12px; }
  `,
})
export class ProcessPage {
  private readonly api = inject(CommerceService);

  readonly date = signal('');
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly result = signal<number | null>(null);

  onDateChange(event: Event): void {
    this.date.set((event.target as HTMLInputElement).value);
    this.result.set(null);
    this.error.set(null);
  }

  process(): void {
    if (!this.date()) return;

    this.loading.set(true);
    this.error.set(null);
    this.result.set(null);
    this.api.process(this.date()).subscribe({
      next: (res) => {
        this.result.set(res.registrosEnCuarentena);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(messageFrom(err));
        this.loading.set(false);
      },
    });
  }
}
