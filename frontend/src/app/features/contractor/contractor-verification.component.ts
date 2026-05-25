import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { catchError, of, forkJoin, map, switchMap } from 'rxjs';

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

  selectedFiles = new Map<string, File>();

  form = this.fb.group({
    licenceNumber: ['', [Validators.required, Validators.maxLength(50)]]
  });

  constructor() { this.loadStatus(); }

  loadStatus() {
    this.loading.set(true);
    this.http.get<VerificationStatus>('/api/contractors/verifications/me').subscribe({
      next:  (v) => { this.existing.set(v); this.loading.set(false); },
      error: (e) => { this.loading.set(false); if (e.status !== 404) this.error.set('Failed to load status'); }
    });
  }

  onFileSelect(event: Event, key: string) {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.selectedFiles.set(key, input.files[0]);
    } else {
      this.selectedFiles.delete(key);
    }
  }

  private uploadFile(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ url: string }>('/api/files', formData).pipe(map(res => res.url));
  }

  submit() {
    if (this.form.invalid) return;
    const doc1 = this.selectedFiles.get('doc1');
    if (!doc1) {
      this.error.set('Licence document is required.');
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    const uploads = [this.uploadFile(doc1)];
    const doc2 = this.selectedFiles.get('doc2');
    if (doc2) uploads.push(this.uploadFile(doc2));

    forkJoin(uploads).pipe(
      switchMap(urls => {
        return this.http.post('/api/contractors/verifications', {
          licenceNumber: this.form.value.licenceNumber,
          documentUrls: urls
        });
      })
    ).subscribe({
      next: () => { this.submitting.set(false); this.success.set(true); this.loadStatus(); },
      error: (e) => { this.submitting.set(false); this.error.set(e?.error?.message ?? 'Submission failed'); }
    });
  }

  get mappedStatus() {
    const s = this.existing()?.status;
    const str = String(s);
    if (str === '0') return 'Pending';
    if (str === '1') return 'Approved';
    if (str === '2') return 'Rejected';
    return s ?? 'Not submitted';
  }

  statusBadge(s: string) {
    const str = String(s);
    if (str === '1' || str === 'Approved') return 'bp-badge-green';
    if (str === '2' || str === 'Rejected') return 'bp-badge-red';
    return 'bp-badge-yellow';
  }
}
