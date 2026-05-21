import { CommonModule } from '@angular/common';
import { Component, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-revenue-analytics',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './revenue-analytics.component.html'
})
export class RevenueAnalyticsComponent {
  private readonly http = inject(HttpClient);
  orders   = signal<any[]>([]);
  products = signal<any[]>([]);
  loading  = signal(true);

  readonly totalRevenue = computed(() =>
    this.orders().filter(o => o.status !== 'Cancelled')
      .reduce((s: number, o: any) => s + (o.totalAmount ?? 0), 0));

  readonly ordersByStatus = computed(() => {
    const counts: Record<string, number> = {};
    this.orders().forEach((o: any) => { counts[o.status] = (counts[o.status] ?? 0) + 1; });
    return Object.entries(counts).map(([status, count]) => ({ status, count }));
  });

  constructor() {
    this.http.get<any[]>('/api/orders/history').subscribe({
      next: (o) => { this.orders.set(o); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
    this.http.get<any[]>('/api/products').subscribe({
      next: (p) => this.products.set(p), error: () => {}
    });
  }
}
