import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

type OrderDto = {
  id: string;
  totalAmount: number;
  status: string;
  createdAt: string;
  items: { id: string; productName: string; quantity: number; unitPrice: number; lineTotal: number }[];
};

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid gap-6">
      <div class="rounded-2xl border bg-white p-6">
        <h2 class="text-xl font-semibold">Orders</h2>
        <p class="mt-1 text-sm text-slate-600">Your purchase history.</p>
      </div>

      <div *ngIf="loading" class="text-sm text-slate-600">Loading…</div>
      <div *ngIf="error" class="rounded-md border border-red-200 bg-red-50 p-2 text-sm text-red-700">{{ error }}</div>

      <div class="grid gap-3" *ngIf="!loading">
        <div *ngFor="let o of orders" class="rounded-2xl border bg-white p-5">
          <div class="flex items-center justify-between gap-3">
            <div>
              <div class="text-sm font-semibold">Order {{ o.id }}</div>
              <div class="mt-1 text-xs text-slate-500">{{ o.createdAt | date : 'medium' }} • Status: {{ o.status }}</div>
            </div>
            <div class="text-lg font-semibold">{{ o.totalAmount | number : '1.0-2' }}</div>
          </div>

          <div class="mt-4 grid gap-2">
            <div *ngFor="let i of o.items" class="flex items-center justify-between text-sm">
              <div class="text-slate-700">{{ i.productName }} × {{ i.quantity }}</div>
              <div class="text-slate-900">{{ i.lineTotal | number : '1.0-2' }}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class OrdersComponent {
  private readonly http = inject(HttpClient);
  orders: OrderDto[] = [];
  loading = true;
  error: string | null = null;

  constructor() {
    this.http.get<OrderDto[]>(`/api/orders/my`).subscribe({
      next: (res) => {
        this.orders = res;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'Failed to load orders';
      }
    });
  }
}

