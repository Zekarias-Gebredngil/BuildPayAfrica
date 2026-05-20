import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CartService } from '../../core/marketplace/cart.service';
import { AuthService } from '../../core/auth/auth.service';
import { RouterLink } from '@angular/router';

export type ProductDto = {
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
  templateUrl: './marketplace.component.html'
})
export class MarketplaceComponent {
  private readonly http = inject(HttpClient);
  readonly cart = inject(CartService);
  readonly auth = inject(AuthService);

  products: ProductDto[] = [];
  loading = true;
  error: string | null = null;
  searchQuery = signal('');
  addedIds = signal<Set<string>>(new Set());

  constructor() {
    this.http.get<ProductDto[]>(`/api/products`).subscribe({
      next:  (res) => { this.products = res; this.loading = false; },
      error: ()    => { this.loading = false; this.error = 'Failed to load products'; }
    });
  }

  get filteredProducts(): ProductDto[] {
    const q = this.searchQuery().toLowerCase();
    if (!q) return this.products;
    return this.products.filter(p => p.name.toLowerCase().includes(q));
  }

  addToCart(p: ProductDto) {
    this.cart.add({ productId: p.id, name: p.name, unitPrice: p.price }, 1);
    const ids = new Set(this.addedIds());
    ids.add(p.id);
    this.addedIds.set(ids);
    setTimeout(() => {
      const next = new Set(this.addedIds());
      next.delete(p.id);
      this.addedIds.set(next);
    }, 1500);
  }

  isAdded(id: string): boolean {
    return this.addedIds().has(id);
  }

  onSearch(event: Event) {
    this.searchQuery.set((event.target as HTMLInputElement).value);
  }
}
