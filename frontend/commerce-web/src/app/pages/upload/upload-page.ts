import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { CommerceService } from '../../core/commerce.service';
import { CsvPreview, parseCsv } from '../../core/csv';
import { messageFrom } from '../../core/http-error';

const FILE_NAME = /^commerce_\d{8}\.csv$/i;
const PREVIEW_LIMIT = 20;

@Component({
  selector: 'app-upload-page',
  template: `
    <section class="card">
      <h2>Cargar archivo de comercios</h2>
      <p class="muted">
        Selecciona el archivo <strong>commerce_DDMMYYYY.csv</strong>, revisa la vista previa y
        envíalo al servidor.
      </p>

      <input type="file" accept=".csv" (change)="onFileSelected($event)" />

      @if (error()) { <div class="msg error">{{ error() }}</div> }
      @if (success()) { <div class="msg ok">{{ success() }}</div> }

      @if (preview(); as p) {
        <p class="muted">Vista previa: {{ previewRows().length }} de {{ p.rows.length }} registros</p>
        <div class="table-wrap">
          <table>
            <thead>
              <tr>@for (h of p.headers; track $index) { <th>{{ h }}</th> }</tr>
            </thead>
            <tbody>
              @for (row of previewRows(); track $index) {
                <tr>@for (cell of row; track $index) { <td>{{ cell }}</td> }</tr>
              }
            </tbody>
          </table>
        </div>
        <button type="button" [disabled]="!canSend()" (click)="send()">
          {{ sending() ? 'Enviando...' : 'Enviar al servidor' }}
        </button>
      }
    </section>
  `,
})
export class UploadPage {
  private readonly api = inject(CommerceService);

  readonly file = signal<File | null>(null);
  readonly preview = signal<CsvPreview | null>(null);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);
  readonly sending = signal(false);

  readonly previewRows = computed(() => this.preview()?.rows.slice(0, PREVIEW_LIMIT) ?? []);
  readonly canSend = computed(() => this.file() !== null && !this.sending());

  async onFileSelected(event: Event): Promise<void> {
    const selected = (event.target as HTMLInputElement).files?.[0] ?? null;
    this.file.set(null);
    this.preview.set(null);
    this.error.set(null);
    this.success.set(null);
    if (!selected) return;

    if (!FILE_NAME.test(selected.name)) {
      this.error.set('El nombre del archivo debe tener el formato commerce_DDMMYYYY.csv.');
      return;
    }
    if (selected.size === 0) {
      this.error.set('El archivo está vacío.');
      return;
    }

    const parsed = parseCsv(await selected.text());
    if (parsed.rows.length === 0) {
      this.error.set('El archivo no contiene registros.');
      return;
    }
    this.file.set(selected);
    this.preview.set(parsed);
  }

  send(): void {
    const file = this.file();
    if (!file) return;

    this.sending.set(true);
    this.error.set(null);
    this.api.upload(file).subscribe({
      next: (res) => {
        this.success.set(`Archivo cargado: ${res.registrosInsertados} registros insertados.`);
        this.file.set(null); // evita enviar el mismo archivo dos veces
        this.sending.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(messageFrom(err));
        this.sending.set(false);
      },
    });
  }
}
