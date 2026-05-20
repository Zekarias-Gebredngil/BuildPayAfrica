import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';

type UploadResponse = { url: string };

@Component({
  selector: 'app-contractor-verification',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './contractor-verification.component.html'
})
export class ContractorVerificationComponent {
  private readonly http = inject(HttpClient);
  private readonly fb   = inject(FormBuilder);

  loading = false;
  success = false;
  error: string | null = null;

  licenseFileName  = '';
  idFileName       = '';
  uploadingLicense = false;
  uploadingId      = false;

  form = this.fb.group({
    licenseNumber: ['', [Validators.required]],
    licenseUrl:    ['', [Validators.required]],
    idUrl:         ['', [Validators.required]]
  });

  upload(event: Event, control: 'licenseUrl' | 'idUrl') {
    const input = event.target as HTMLInputElement;
    const file  = input.files?.[0];
    if (!file) return;

    if (control === 'licenseUrl') { this.uploadingLicense = true; this.licenseFileName = file.name; }
    else                          { this.uploadingId      = true; this.idFileName      = file.name; }

    const formData = new FormData();
    formData.append('file', file);

    this.http.post<UploadResponse>(`/api/files`, formData).subscribe({
      next: (res) => {
        if (control === 'licenseUrl') this.uploadingLicense = false;
        else                          this.uploadingId      = false;
        this.form.patchValue({ [control]: res.url });
      },
      error: () => {
        if (control === 'licenseUrl') this.uploadingLicense = false;
        else                          this.uploadingId      = false;
        this.error = 'Upload failed. Please try again.';
      }
    });
  }

  submit() {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();

    this.loading = true;
    this.success = false;
    this.error   = null;

    this.http.post(`/api/contractors/verification`, {
      licenseNumber: v.licenseNumber,
      documents: [
        { documentType: 'License', fileUrl: v.licenseUrl },
        { documentType: 'Id',      fileUrl: v.idUrl }
      ]
    }).subscribe({
      next:  () => { this.loading = false; this.success = true; },
      error: (e) => { this.loading = false; this.error = e?.error?.message ?? 'Submission failed'; }
    });
  }
}
