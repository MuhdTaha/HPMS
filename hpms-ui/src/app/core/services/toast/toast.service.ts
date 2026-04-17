import { Injectable, inject } from '@angular/core';
import { MessageService } from 'primeng/api';

type ToastSeverity = 'success' | 'info' | 'warn' | 'error';

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly messageService = inject(MessageService);

  show(severity: ToastSeverity, summary: string, detail: string) {
    this.messageService.add({
      severity,
      summary,
      detail,
      closable: true,
      life: 5000
    });
  }

  success(summary: string, detail: string) {
    this.show('success', summary, detail);
  }

  info(summary: string, detail: string) {
    this.show('info', summary, detail);
  }

  warn(summary: string, detail: string) {
    this.show('warn', summary, detail);
  }

  error(summary: string, detail: string) {
    this.show('error', summary, detail);
  }
}