import { HttpErrorResponse } from '@angular/common/http';

/** Convierte un error HTTP en un mensaje legible para el usuario. */
export function messageFrom(err: HttpErrorResponse): string {
  if (err.status === 0) {
    return 'No se pudo conectar con el servidor. Verifica que la API esté en ejecución.';
  }
  return err.error?.error ?? `Error ${err.status}: ${err.statusText}`;
}
