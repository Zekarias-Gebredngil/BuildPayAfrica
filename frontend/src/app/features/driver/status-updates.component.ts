import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-status-updates',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './status-updates.component.html'
})
export class StatusUpdatesComponent {
  private readonly http = inject(HttpClient);
  private readonly fb   = inject(FormBuilder);

  sending  = signal(false);
  success  = signal(false);
  error    = signal<string | null>(null);

  form = this.fb.group({
    deliveryId: ['', [Validators.required,
      Validators.pattern(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i)]],
    latitude:   [0, [Validators.required, Validators.min(-90),  Validators.max(90)]],
    longitude:  [0, [Validators.required, Validators.min(-180), Validators.max(180)]]
  });

  send() {
    if (this.form.invalid) return;
    const { deliveryId, latitude, longitude } = this.form.getRawValue();
    this.sending.set(true); this.error.set(null); this.success.set(false);
    this.http.post(`/api/deliveries/${deliveryId}/location`, { latitude, longitude }).subscribe({
      next:  () => { this.sending.set(false); this.success.set(true); },
      error: (e) => { this.sending.set(false); this.error.set(e?.error?.message ?? 'Failed to send location'); }
    });
  }

  useCurrentLocation() {
    if (!navigator.geolocation) { this.error.set('Geolocation not supported'); return; }
    navigator.geolocation.getCurrentPosition(
      (pos) => this.form.patchValue({ latitude: pos.coords.latitude, longitude: pos.coords.longitude }),
      ()    => this.error.set('Could not get location')
    );
  }
}
