export interface CsvPreview {
  headers: string[];
  rows: string[][];
}

/** Parser CSV liviano para la vista previa (detecta delimitador y soporta comillas). */
export function parseCsv(text: string): CsvPreview {
  const clean = text.replace(/^\uFEFF/, '');
  const firstLine = clean.split(/\r?\n/, 1)[0] ?? '';
  const delimiter = [',', ';', '\t', '|'].reduce(
    (best, d) => (count(firstLine, d) > count(firstLine, best) ? d : best),
    ',',
  );

  const rows: string[][] = [];
  let row: string[] = [];
  let field = '';
  let inQuotes = false;

  for (let i = 0; i < clean.length; i++) {
    const c = clean[i];
    if (inQuotes) {
      if (c === '"') {
        if (clean[i + 1] === '"') { field += '"'; i++; } else { inQuotes = false; }
      } else {
        field += c;
      }
    } else if (c === '"') {
      inQuotes = true;
    } else if (c === delimiter) {
      row.push(field);
      field = '';
    } else if (c === '\n' || c === '\r') {
      if (c === '\r' && clean[i + 1] === '\n') i++;
      row.push(field);
      field = '';
      rows.push(row);
      row = [];
    } else {
      field += c;
    }
  }
  if (field !== '' || row.length > 0) {
    row.push(field);
    rows.push(row);
  }

  const nonEmpty = rows.filter((r) => r.some((v) => v.trim() !== ''));
  const [headers = [], ...data] = nonEmpty;
  return { headers: headers.map((h) => h.trim()), rows: data };
}

function count(text: string, char: string): number {
  return text.split(char).length - 1;
}
