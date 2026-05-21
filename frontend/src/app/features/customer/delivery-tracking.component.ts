import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

interface DeliveryDto {
  id: string; orderId: string; status: string;
  driverUserId?: string; pickupAddress?: string; dropoffAddress?: string;
  deliveryFee?: number; updatedAtUtc: string;
}

@Component({
  selector: 'app-delivery-tracking',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './delivery-tracking.component.html'
})
export class DeliveryTrackingComponent {
  private readonly http = inject(HttpClient);

  deliveryId = signal('');
  delivery   = signal<DeliveryDto | null>(null);
  loading    = signal(false);
  error      = signal<string | null>(null);

  track() {
    const id = this.deliveryId().trim();
    if (!id) return;
    this.loading.set(true);
    this.error.set(null);
    this.delivery.set(null);
    this.http.get<DeliveryDto>(`/api/deliveries/${id}`).subscribe({
      next:  (d) => { this.delivery.set(d); this.loading.set(false); },
      error: (e) => { this.loading.set(false); this.error.set(e?.status === 404 ? 'Delivery not found.' : 'Failed to load delivery'); }
    });
  }

  statusSteps = ['Pending_Assignment', 'Assigned', 'In_Transit', 'Delivered'];

  stepIndex(status: string) { return this.statusSteps.indexOf(status); }

  statusLabel(s: string) {
    return s.replace('_', ' ');
  }

  statusIcon(s: string) {
    const m: Record<string, string> = {
      Pending_Assignment: '⏳', Assigned: '👤', In_Transit: '🚚', Delivered: '✅'
    };
    return m[s] ?? '📦';
  }
}
