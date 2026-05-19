import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CartService } from '../../core/marketplace/cart.service';
import { AuthService } from '../../core/auth/auth.service';
import { RouterLink } from '@angular/router';

type ProductDto = {
  id: string;
  supplierId: string;
  name: string;
  price: number;
  quantity: number;
  categoryId: string;
  imageUrl: string;
};

@Component({
  selector: 'app-marketplace',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="grid gap-6">
      <div class="flex flex-col items-start justify-between gap-3 rounded-2xl border bg-white p-6 sm:flex-row sm:items-center">
        <div>
          <h2 class="text-xl font-semibold">Marketplace</h2>
          <p class="mt-1 text-sm text-slate-600">Browse products and place an order.</p>
        </div>
        <a routerLink="/cart" class="rounded-md border px-4 py-2 text-sm hover:bg-slate-100">
          Cart ({{ cart.items.length }}) — {{ cart.total() | number : '1.0-2' }}
        </a>
      </div>

      <div *ngIf="loading" class="text-sm text-slate-600">Loading products…</div>
      <div *ngIf="error" class="rounded-md border border-red-200 bg-red-50 p-2 text-sm text-red-700">{{ error }}</div>

      <div class="grid gap-4 md:grid-cols-3" *ngIf="!loading">
        <div *ngFor="let p of products" class="rounded-xl border bg-white p-4">
          <div class="text-sm font-semibold">{{ p.name }}</div>
          <div class="mt-1 text-sm text-slate-600">Price: {{ p.price | number : '1.0-2' }}</div>
          <div class="mt-1 text-xs text-slate-500">Stock: {{ p.quantity }}</div>
          <button
            class="mt-3 w-full rounded-md bg-slate-900 px-3 py-2 text-sm text-white disabled:opacity-60"
            [disabled]="p.quantity <= 0"
            (click)="addToCart(p)"
          >
            Add to cart
          </button>
        </div>
      </div>

      <div *ngIf="auth.isAuthenticated && auth.roles.includes('Supplier')" class="rounded-2xl border bg-white p-6">
        <h3 class="text-sm font-semibold">Supplier tools</h3>
        <p class="mt-1 text-sm text-slate-600">Use the API to add products. UI for supplier management is minimal in this MVP.</p>
      </div>
    </div>
  `
})
export class MarketplaceComponent {
  private readonly http = inject(HttpClient);
  readonly cart = inject(CartService);
  readonly auth = inject(AuthService);

  products: ProductDto[] = [];
  loading = true;
  error: string | null = null;

  constructor() {
    this.http.get<ProductDto[]>(`/api/products`).subscribe({
      next: (res) => {
        this.products = res;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'Failed to load products';
      }
    });
  }

  addToCart(p: ProductDto) {
    this.cart.add({ productId: p.id, name: p.name, unitPrice: p.price }, 1);
  }
}

