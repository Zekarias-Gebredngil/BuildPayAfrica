import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../core/marketplace/cart.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './cart.component.html'
})
export class CartComponent {
  private readonly http   = inject(HttpClient);
  private readonly router = inject(Router);
  readonly cart = inject(CartService);

  loading = false;
  error: string | null = null;

  remove(productId: string) { this.cart.remove(productId); }

  setQty(productId: string, value: string) {
    const qty = Number(value);
    if (!Number.isFinite(qty)) return;
    this.cart.setQuantity(productId, qty);
  }

  checkout() {
    this.loading = true;
    this.error = null;

    const payload = {
      items: this.cart.items.map(i => ({ productId: i.productId, quantity: i.quantity }))
    };

    this.http.post<{ id: string }>(`/api/orders`, payload).subscribe({
      next: () => {
        this.loading = false;
        this.cart.clear();
        this.router.navigateByUrl('/orders');
      },
      error: (e) => {
        this.loading = false;
        this.error = e?.error?.message ?? 'Checkout failed. Please try again.';
      }
    });
  }
}
