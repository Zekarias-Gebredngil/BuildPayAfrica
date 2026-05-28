import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

interface Product { id: string; name: string; price: number; quantity: number; categoryId: string; imageUrl?: string; }
interface Category { id: string; name: string; }

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './inventory.component.html'
})
export class InventoryComponent {
  private readonly http = inject(HttpClient);
  private readonly fb   = inject(FormBuilder);

  products   = signal<Product[]>([]);
  categories = signal<Category[]>([]);
  loading    = signal(true);
  error      = signal<string | null>(null);
  categoriesLoading = signal(false);
  categoriesError = signal<string | null>(null);
  showForm   = signal(false);
  creating   = signal(false);
  createError = signal<string | null>(null);

  form = this.fb.group({
    name:       ['', [Validators.required, Validators.maxLength(200)]],
    price:      [0,  [Validators.required, Validators.min(0.01)]],
    quantity:   [0,  [Validators.required, Validators.min(0)]],
    categoryId: ['', [Validators.required]],
    imageUrl:   ['']
  });

  constructor() { this.load(); }

  load() {
    this.loading.set(true);
    this.http.get<Product[]>('/api/products').subscribe({
      next: (p) => { this.products.set(p); this.loading.set(false); },
      error: () => { this.loading.set(false); this.error.set('Failed to load products'); }
    });

    this.categoriesLoading.set(true);
    this.categoriesError.set(null);
    this.http.get<Category[]>('/api/categories').subscribe({
      next: (c) => { this.categories.set(c); this.categoriesLoading.set(false); },
      error: () => {
        this.categories.set([]);
        this.categoriesLoading.set(false);
        this.categoriesError.set('Failed to load categories. Please refresh or contact an administrator.');
      }
    });
  }

  create() {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();
    this.creating.set(true);
    this.createError.set(null);
    this.http.post('/api/products', v).subscribe({
      next: () => { this.creating.set(false); this.showForm.set(false); this.form.reset(); this.load(); },
      error: (e) => { this.creating.set(false); this.createError.set(e?.error?.message ?? 'Failed to create product'); }
    });
  }

  stockBadge(qty: number) {
    return qty === 0 ? 'bp-badge-red' : qty < 10 ? 'bp-badge-yellow' : 'bp-badge-green';
  }
  stockLabel(qty: number) {
    return qty === 0 ? 'Out of Stock' : qty < 10 ? 'Low Stock' : 'In Stock';
  }
}
