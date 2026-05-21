import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-supplier-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html'
})
export class SupplierDashboardComponent {
  private readonly http = inject(HttpClient);
  readonly auth = inject(AuthService);
  readonly displayName = this.auth.displayName$;

  loading  = signal(true);
  products = signal<any[]>([]);
  orders   = signal<any[]>([]);

  constructor() {
    forkJoin({
      products: this.http.get<any[]>('/api/products').pipe(catchError(() => of([]))),
      orders:   this.http.get<any[]>('/api/orders/history').pipe(catchError(() => of([])))
    }).subscribe(({ products, orders }) => {
      this.products.set(products);
      this.orders.set(orders);
      this.loading.set(false);
    });
  }

  get lowStock()    { return this.products().filter(p => p.quantity < 10).length; }
  get pendingOrders(){ return this.orders().filter(o => o.status === 'Pending').length; }
}
