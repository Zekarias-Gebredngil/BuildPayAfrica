import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-delivery-queue',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './delivery-queue.component.html'
})
export class DeliveryQueueComponent {
  private readonly http = inject(HttpClient);
  deliveryId = signal('');
  delivery   = signal<any>(null);
  loading    = signal(false);
  error      = signal<string | null>(null);
  updating   = signal(false);

  lookup() {
    const id = this.deliveryId().trim();
    if (!id) return;
    this.loading.set(true); this.error.set(null); this.delivery.set(null);
    this.http.get<any>(`/api/deliveries/${id}`).subscribe({
      next:  (d) => { this.delivery.set(d); this.loading.set(false); },
      error: (e) => { this.loading.set(false); this.error.set(e?.status === 404 ? 'Delivery not found.' : 'Failed to load'); }
    });
  }

  updateStatus(status: string) {
    const id = this.delivery()?.id;
    if (!id) return;
    this.updating.set(true);
    this.http.put(`/api/deliveries/${id}/status`, { status }).subscribe({
      next:  () => { this.updating.set(false); this.lookup(); },
      error: (e) => { this.updating.set(false); this.error.set(e?.error?.message ?? 'Update failed'); }
    });
  }
}
