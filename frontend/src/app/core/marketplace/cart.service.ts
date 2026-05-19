import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type CartItem = {
  productId: string;
  name: string;
  unitPrice: number;
  quantity: number;
};

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly storageKey = 'buildpay_cart';
  private readonly items$ = new BehaviorSubject<CartItem[]>(this.read());

  cart$ = this.items$.asObservable();

  get items(): CartItem[] {
    return this.items$.value;
  }

  add(item: Omit<CartItem, 'quantity'>, quantity = 1) {
    const next = [...this.items];
    const existing = next.find((x) => x.productId === item.productId);
    if (existing) existing.quantity += quantity;
    else next.push({ ...item, quantity });
    this.write(next);
  }

  remove(productId: string) {
    this.write(this.items.filter((x) => x.productId !== productId));
  }

  setQuantity(productId: string, quantity: number) {
    const next = [...this.items];
    const existing = next.find((x) => x.productId === productId);
    if (!existing) return;
    existing.quantity = Math.max(1, Math.floor(quantity));
    this.write(next);
  }

  clear() {
    this.write([]);
  }

  total(): number {
    return this.items.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0);
  }

  private write(items: CartItem[]) {
    localStorage.setItem(this.storageKey, JSON.stringify(items));
    this.items$.next(items);
  }

  private read(): CartItem[] {
    try {
      const raw = localStorage.getItem(this.storageKey);
      if (!raw) return [];
      return JSON.parse(raw) as CartItem[];
    } catch {
      return [];
    }
  }
}

