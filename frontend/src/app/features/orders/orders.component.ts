import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';

type OrderItem = { id: string; productName: string; quantity: number; unitPrice: number; lineTotal: number };

type OrderDto = {
  id: string;
  totalAmount: number;
  status: string;
  createdAt: string;
  items: OrderItem[];
};

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './orders.component.html'
})
export class OrdersComponent {
  private readonly http = inject(HttpClient);
  orders: OrderDto[] = [];
  loading = true;
  error: string | null = null;
  expandedId: string | null = null;

  constructor() {
    this.http.get<OrderDto[]>(`/api/orders/my`).subscribe({
      next:  (res) => { this.orders = res; this.loading = false; },
      error: ()    => { this.loading = false; this.error = 'Failed to load orders'; }
    });
  }

  toggleExpand(id: string) {
    this.expandedId = this.expandedId === id ? null : id;
  }

  statusClass(status: string): string {
    const map: Record<string, string> = {
      Pending:    'bp-badge-yellow',
      Processing: 'bp-badge-blue',
      Shipped:    'bp-badge-purple',
      Delivered:  'bp-badge-green',
      Cancelled:  'bp-badge-red'
    };
    return map[status] ?? 'bp-badge-gray';
  }

  statusIcon(status: string): string {
    const map: Record<string, string> = {
      Pending:    '⏳',
      Processing: '⚙️',
      Shipped:    '🚚',
      Delivered:  '✅',
      Cancelled:  '✕'
    };
    return map[status] ?? '📦';
  }
}
