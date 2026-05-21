import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-active-delivery-map',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './active-delivery-map.component.html'
})
export class ActiveDeliveryMapComponent {
  private readonly http = inject(HttpClient);
  deliveryId = signal('');
  delivery   = signal<any>(null);
  loading    = signal(false);
  error      = signal<string | null>(null);

  load() {
    const id = this.deliveryId().trim();
    if (!id) return;
    this.loading.set(true); this.error.set(null);
    this.http.get<any>(`/api/deliveries/${id}`).subscribe({
      next:  (d) => { this.delivery.set(d); this.loading.set(false); },
      error: (e) => { this.loading.set(false); this.error.set(e?.status === 404 ? 'Not found.' : 'Failed to load'); }
    });
  }
}
