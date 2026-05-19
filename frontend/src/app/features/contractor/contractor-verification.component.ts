import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

type UploadResponse = { url: string };

@Component({
  selector: 'app-contractor-verification',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="grid gap-6">
      <div class="rounded-2xl border bg-white p-6">
        <h2 class="text-xl font-semibold">Contractor verification</h2>
        <p class="mt-1 text-sm text-slate-600">Upload documents and submit for admin approval.</p>
      </div>

      <div class="rounded-2xl border bg-white p-6">
        <form class="grid gap-3" [formGroup]="form" (ngSubmit)="submit()">
          <label class="grid gap-1 text-sm">
            <span>License number</span>
            <input class="rounded-md border px-3 py-2" formControlName="licenseNumber" />
          </label>

          <div class="grid gap-2 sm:grid-cols-2">
            <div class="rounded-xl border p-4">
              <div class="text-sm font-semibold">License document</div>
              <input class="mt-2 text-sm" type="file" accept=".png,.jpg,.jpeg,.pdf" (change)="upload($event, 'licenseUrl')" />
              <div *ngIf="form.value.licenseUrl" class="mt-2 text-xs underline">
                <a [href]="form.value.licenseUrl!" target="_blank" rel="noopener">View uploaded</a>
              </div>
            </div>

            <div class="rounded-xl border p-4">
              <div class="text-sm font-semibold">ID document</div>
              <input class="mt-2 text-sm" type="file" accept=".png,.jpg,.jpeg,.pdf" (change)="upload($event, 'idUrl')" />
              <div *ngIf="form.value.idUrl" class="mt-2 text-xs underline">
                <a [href]="form.value.idUrl!" target="_blank" rel="noopener">View uploaded</a>
              </div>
            </div>
          </div>

          <button class="rounded-md bg-slate-900 px-4 py-2 text-sm text-white disabled:opacity-60" [disabled]="form.invalid || loading">
            {{ loading ? 'Submitting…' : 'Submit verification' }}
          </button>
        </form>

        <div *ngIf="success" class="mt-3 rounded-md border border-emerald-200 bg-emerald-50 p-2 text-sm text-emerald-800">
          Submitted. Wait for admin approval.
        </div>
        <div *ngIf="error" class="mt-3 rounded-md border border-red-200 bg-red-50 p-2 text-sm text-red-700">{{ error }}</div>
      </div>
    </div>
  `
})
export class ContractorVerificationComponent {
  private readonly http = inject(HttpClient);
  private readonly fb = inject(FormBuilder);

  loading = false;
  success = false;
  error: string | null = null;

  form = this.fb.group({
    licenseNumber: ['', [Validators.required]],
    licenseUrl: ['', [Validators.required]],
    idUrl: ['', [Validators.required]]
  });

  upload(event: Event, control: 'licenseUrl' | 'idUrl') {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);

    this.loading = true;
    this.error = null;

    this.http.post<UploadResponse>(`/api/files`, formData).subscribe({
      next: (res) => {
        this.loading = false;
        this.form.patchValue({ [control]: res.url });
      },
      error: () => {
        this.loading = false;
        this.error = 'Upload failed';
      }
    });
  }

  submit() {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();

    this.loading = true;
    this.success = false;
    this.error = null;

    this.http
      .post(`/api/contractors/verification`, {
        licenseNumber: v.licenseNumber,
        documents: [
          { documentType: 'License', fileUrl: v.licenseUrl },
          { documentType: 'Id', fileUrl: v.idUrl }
        ]
      })
      .subscribe({
        next: () => {
          this.loading = false;
          this.success = true;
        },
        error: (e) => {
          this.loading = false;
          this.error = e?.error?.message ?? 'Submit failed';
        }
      });
  }
}

