import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-delivery-scheduling',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './delivery-scheduling.component.html'
})
export class DeliverySchedulingComponent {
  private readonly http = inject(HttpClient);
  private readonly fb   = inject(FormBuilder);

  assigning = signal(false);
  success   = signal(false);
  error     = signal<string | null>(null);

  form = this.fb.group({
    deliveryId:    ['', [Validators.required,
      Validators.pattern(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i)]],
    driverUserId:  ['', [Validators.required,
      Validators.pattern(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i)]],
    pickupAddress:  ['', Validators.required],
    dropoffAddress: ['', Validators.required],
    deliveryFee:    [null as number | null]
  });

  assign() {
    if (this.form.invalid) return;
    const { deliveryId, driverUserId, pickupAddress, dropoffAddress, deliveryFee } = this.form.getRawValue();
    this.assigning.set(true);
    this.error.set(null);
    this.http.post(`/api/deliveries/${deliveryId}/assign`, { driverUserId, pickupAddress, dropoffAddress, deliveryFee }).subscribe({
      next:  () => { this.assigning.set(false); this.success.set(true); this.form.reset(); },
      error: (e) => { this.assigning.set(false); this.error.set(e?.error?.message ?? 'Assignment failed'); }
    });
  }
}
