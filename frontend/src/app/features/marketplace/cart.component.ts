import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { CartService } from '../../core/marketplace/cart.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid gap-4">
      <div class="rounded-2xl border bg-white p-6">
        <h2 class="text-xl font-semibold">Cart</h2>
        <p class="mt-1 text-sm text-slate-600">Review items and place your order.</p>
      </div>

      <div *ngIf="cart.items.length === 0" class="rounded-2xl border bg-white p-6 text-sm text-slate-600">
        Your cart is empty.
      </div>

      <div *ngIf="cart.items.length > 0" class="rounded-2xl border bg-white p-6">
        <div class="grid gap-3">
          <div *ngFor="let i of cart.items" class="flex items-center justify-between gap-3 border-b pb-3">
            <div>
              <div class="text-sm font-semibold">{{ i.name }}</div>
              <div class="text-xs text-slate-500">Unit: {{ i.unitPrice | number : '1.0-2' }}</div>
            </div>

            <div class="flex items-center gap-2">
              <input
                class="w-20 rounded-md border px-2 py-1 text-sm"
                type="number"
                min="1"
                [value]="i.quantity"
                (change)="setQty(i.productId, $any($event.target).value)"
              />
              <button class="rounded-md border px-2 py-1 text-sm hover:bg-slate-100" (click)="remove(i.productId)">Remove</button>
            </div>
          </div>
        </div>

        <div class="mt-4 flex items-center justify-between">
          <div class="text-sm text-slate-600">Total</div>
          <div class="text-lg font-semibold">{{ cart.total() | number : '1.0-2' }}</div>
        </div>

        <div class="mt-4 flex flex-wrap gap-2">
          <button class="rounded-md border px-4 py-2 text-sm hover:bg-slate-100" (click)="cart.clear()">Clear</button>
          <button
            class="rounded-md bg-slate-900 px-4 py-2 text-sm text-white disabled:opacity-60"
            [disabled]="loading"
            (click)="checkout()"
          >
            {{ loading ? 'Placing…' : 'Checkout' }}
          </button>
        </div>

        <div *ngIf="error" class="mt-3 rounded-md border border-red-200 bg-red-50 p-2 text-sm text-red-700">{{ error }}</div>
      </div>
    </div>
  `
})
export class CartComponent {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  readonly cart = inject(CartService);

  loading = false;
  error: string | null = null;

  remove(productId: string) {
    this.cart.remove(productId);
  }

  setQty(productId: string, value: string) {
    const qty = Number(value);
    if (!Number.isFinite(qty)) return;
    this.cart.setQuantity(productId, qty);
  }

  checkout() {
    this.loading = true;
    this.error = null;

    const payload = {
      items: this.cart.items.map((i) => ({ productId: i.productId, quantity: i.quantity }))
    };

    this.http.post<{ id: string }>(`/api/orders`, payload).subscribe({
      next: () => {
        this.loading = false;
        this.cart.clear();
        this.router.navigateByUrl('/orders');
      },
      error: (e) => {
        this.loading = false;
        this.error = e?.error?.message ?? 'Checkout failed';
      }
    });
  }
}

