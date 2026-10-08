export interface UploadResult {
  registrosInsertados: number;
}

export interface ProcessResult {
  registrosEnCuarentena: number;
}

export interface QuarantineRecord {
  id: number;
  pcProcessdate: string;
  pcTipodoc: string | null;
  pcNumdoc: string | null;
  pcNomcomred: string | null;
  pcRazonsocial: string | null;
  pcDireccion: string | null;
  pcTelefono: string | null;
  pcEmail: string | null;
  motivo: string;
}
