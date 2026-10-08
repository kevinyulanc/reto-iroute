import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { CommerceService } from '../../core/commerce.service';
import { messageFrom } from '../../core/http-error';
import { QuarantineRecord } from '../../core/models';

@Component({
  selector: 'app-quarantine-page',
  imports: [DatePipe],
  template: `
    <section class="card">
      <h2>Registros con errores</h2>
      <button type="button" [disabled]="loading()" (click)="load()">
        {{ loading() ? 'Cargando...' : 'Actualizar' }}
      </button>

      @if (error()) { <div class="msg error">{{ error() }}</div> }

      @if (!loading() && !error() && records().length === 0) {
        <p class="muted">No hay registros en cuarentena.</p>
      }

      @if (records().length > 0) {
        <div class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Id</th><th>Fecha proceso</th><th>Tipo doc</th><th>Num doc</th>
                <th>Nombre comercial</th><th>Razón social</th><th>Motivo</th>
              </tr>
            </thead>
            <tbody>
              @for (r of records(); track r.id) {
                <tr>
                  <td>{{ r.id }}</td>
                  <td>{{ r.pcProcessdate | date: 'dd/MM/yyyy' }}</td>
                  <td>{{ r.pcTipodoc }}</td>
                  <td>{{ r.pcNumdoc }}</td>
                  <td>{{ r.pcNomcomred }}</td>
                  <td>{{ r.pcRazonsocial }}</td>
                  <td>{{ r.motivo }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </section>
  `,
  styles: [`
    td { white-space: nowrap; }
    td:last-child {
      white-space: normal;
      word-break: break-word;
    }
        table {
      min-width: 0;
      width: 100%;
    }
  `],
})
export class QuarantinePage implements OnInit {
  private readonly api = inject(CommerceService);

  readonly records = signal<QuarantineRecord[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.getQuarantine().subscribe({
      next: (data) => {
        this.records.set(data);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(messageFrom(err));
        this.loading.set(false);
      },

    });
  }
}

