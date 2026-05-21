import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';

interface OrderItem { productId: string; productName: string; quantity: number; unitPrice: number; lineTotal: number; }
interface OrderDto  { id: string; status: string; totalAmount: number; createdAtUtc: string; items: OrderItem[]; }

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './orders.component.html'
})
export class OrdersComponent {
  private readonly http = inject(HttpClient);
  orders     = signal<OrderDto[]>([]);
  loading    = signal(true);
  error      = signal<string | null>(null);
  expandedId = signal<string | null>(null);

  constructor() {
    this.http.get<OrderDto[]>('/api/orders/history').subscribe({
      next:  (res) => { this.orders.set(res); this.loading.set(false); },
      error: ()    => { this.loading.set(false); this.error.set('Failed to load orders'); }
    });
  }

  toggle(id: string) { this.expandedId.set(this.expandedId() === id ? null : id); }

  statusBadge(s: string) {
    const m: Record<string, string> = { Pending:'bp-badge-yellow', Assigned:'bp-badge-blue', InTransit:'bp-badge-purple', Delivered:'bp-badge-green', Cancelled:'bp-badge-red' };
    return m[s] ?? 'bp-badge-gray';
  }

  statusIcon(s: string) {
    const m: Record<string, string> = { Pending:'⏳', Assigned:'👤', InTransit:'🚚', Delivered:'✅', Cancelled:'✕' };
    return m[s] ?? '📦';
  }

  steps = ['Pending', 'Assigned', 'InTransit', 'Delivered'];
  stepIndex(s: string) { return this.steps.indexOf(s); }
}
