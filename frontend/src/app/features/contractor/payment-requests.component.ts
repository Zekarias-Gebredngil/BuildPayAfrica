import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-payment-requests',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './payment-requests.component.html'
})
export class PaymentRequestsComponent {
  private readonly http = inject(HttpClient);
  private readonly fb   = inject(FormBuilder);

  submitting = signal(false);
  success    = signal(false);
  error      = signal<string | null>(null);

  form = this.fb.group({
    milestoneId: ['', [Validators.required,
      Validators.pattern(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i)]],
    projectId:   ['', [Validators.required,
      Validators.pattern(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i)]],
    amount:      [0, [Validators.required, Validators.min(1)]]
  });

  submit() {
    if (this.form.invalid) return;
    const { milestoneId, projectId, amount } = this.form.getRawValue();
    this.submitting.set(true);
    this.error.set(null);
    this.http.post(`/api/payments/milestones/${milestoneId}/request-release`, { projectId, amount }).subscribe({
      next:  () => { this.submitting.set(false); this.success.set(true); this.form.reset(); },
      error: (e) => { this.submitting.set(false); this.error.set(e?.error?.message ?? 'Request failed'); }
    });
  }
}
