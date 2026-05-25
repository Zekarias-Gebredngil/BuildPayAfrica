import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';

interface PendingVerification { id: string; userId: string; licenceNumber: string; status: string; documentUrls: string[]; }

@Component({
  selector: 'app-admin-verifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './verifications.component.html'
})
export class AdminVerificationsComponent {
  private readonly http = inject(HttpClient);

  verifications = signal<PendingVerification[]>([]);
  loading       = signal(true);
  error         = signal<string | null>(null);
  processingId  = signal<string | null>(null);

  constructor() { this.load(); }

  load() {
    this.loading.set(true);
    this.error.set(null);
    this.http.get<PendingVerification[]>('/api/contractors/verifications/pending').pipe(catchError(() => of([])))
      .subscribe((verifications) => {
        this.verifications.set(verifications);
        this.loading.set(false);
      });
  }

  setVerificationStatus(id: string, status: 'Approved' | 'Rejected', reason?: string) {
    this.processingId.set(id);
    this.http.put(`/api/contractors/verifications/${id}/status`, { status, rejectionReason: reason ?? '' }).subscribe({
      next:  () => { this.processingId.set(null); this.load(); },
      error: (e) => { this.processingId.set(null); this.error.set(e?.error?.message ?? 'Update failed'); }
    });
  }
}
