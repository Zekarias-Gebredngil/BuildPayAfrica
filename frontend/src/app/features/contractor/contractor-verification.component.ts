import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface VerificationStatus {
  id: string; status: string; licenceNumber: string;
  documentUrls: string[]; rejectionReason?: string;
}

@Component({
  selector: 'app-contractor-verification',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './contractor-verification.component.html'
})
export class ContractorVerificationComponent {
  private readonly http = inject(HttpClient);
  private readonly fb   = inject(FormBuilder);

  existing  = signal<VerificationStatus | null>(null);
  loading   = signal(true);
  submitting = signal(false);
  success   = signal(false);
  error     = signal<string | null>(null);

  form = this.fb.group({
    licenceNumber: ['', [Validators.required, Validators.maxLength(50)]],
    doc1: ['', [Validators.required]],
    doc2: ['']
  });

  constructor() { this.loadStatus(); }

  loadStatus() {
    this.loading.set(true);
    this.http.get<VerificationStatus>('/api/contractors/verifications/me').subscribe({
      next:  (v) => { this.existing.set(v); this.loading.set(false); },
      error: (e) => { this.loading.set(false); if (e.status !== 404) this.error.set('Failed to load status'); }
    });
  }

  submit() {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();
    const documentUrls = [v.doc1!, ...(v.doc2 ? [v.doc2] : [])];

    this.submitting.set(true);
    this.error.set(null);

    this.http.post('/api/contractors/verifications', {
      licenceNumber: v.licenceNumber,
      documentUrls
    }).subscribe({
      next: () => { this.submitting.set(false); this.success.set(true); this.loadStatus(); },
      error: (e) => { this.submitting.set(false); this.error.set(e?.error?.message ?? 'Submission failed'); }
    });
  }

  statusBadge(s: string) {
    return s === 'Approved' ? 'bp-badge-green' : s === 'Rejected' ? 'bp-badge-red' : 'bp-badge-yellow';
  }
}
