import { CommonModule } from '@angular/common';
import { Component, HostListener, computed, inject, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthService } from '../core/auth/auth.service';

type NavItem = {
  label: string;
  path: string;
  icon: string;
};

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, MatIconModule],
  templateUrl: './shell.component.html'
})
export class ShellComponent {
  private readonly auth = inject(AuthService);
  private readonly mobileBreakpoint = 768;

  readonly isAuthed = this.auth.isAuthenticated$;
  readonly roles = this.auth.roles$;
  readonly userName = this.auth.displayName$;
  readonly isAdmin = computed(() => this.auth.roles$().includes('Admin'));
  readonly mobileOpen = signal(false);
  readonly sidebarCollapsed = signal(false);
  readonly isMobile = signal(this.getViewportWidth() <= this.mobileBreakpoint);
  readonly currentYear = new Date().getFullYear();

  readonly primaryRole = computed(() => {
    const roles = this.auth.roles$();
    if (roles.includes('Admin')) return 'Admin';
    if (roles.includes('Customer')) return 'Customer';
    if (roles.includes('Contractor')) return 'Contractor';
    if (roles.includes('Supplier')) return 'Supplier';
    if (roles.includes('Driver')) return 'Driver';
    return '';
  });

  readonly navItems = computed<NavItem[]>(() => {
    if (!this.isAuthed()) return [];

    switch (this.primaryRole()) {
      case 'Customer':
        return [
          { label: 'Dashboard', path: '/features/customer/dashboard', icon: 'dashboard' },
          { label: 'Projects', path: '/features/customer/projects', icon: 'folder_open' },
          { label: 'Milestones', path: '/features/customer/milestones', icon: 'flag' },
          { label: 'Escrow Wallet', path: '/features/customer/wallet', icon: 'account_balance_wallet' },
          { label: 'Marketplace', path: '/features/customer/marketplace', icon: 'storefront' },
          { label: 'Cart', path: '/features/customer/cart', icon: 'shopping_cart' },
          { label: 'Contractors', path: '/features/customer/contractors', icon: 'engineering' },
          { label: 'Orders', path: '/features/customer/orders', icon: 'inventory_2' },
          { label: 'Deliveries', path: '/features/customer/deliveries', icon: 'local_shipping' }
        ];
      case 'Contractor':
        return [
          { label: 'Dashboard', path: '/features/contractor/dashboard', icon: 'dashboard' },
          { label: 'Verification', path: '/features/contractor/verification', icon: 'verified' },
          { label: 'Assigned Projects', path: '/features/contractor/projects', icon: 'construction' },
          { label: 'Milestone Updates', path: '/features/contractor/milestones', icon: 'task_alt' },
          { label: 'Payment Requests', path: '/features/contractor/payments', icon: 'payments' },
          { label: 'Earnings', path: '/features/contractor/earnings', icon: 'savings' },
          { label: 'Profile & Ratings', path: '/features/contractor/profile', icon: 'stars' }
        ];
      case 'Supplier':
        return [
          { label: 'Dashboard', path: '/features/supplier/dashboard', icon: 'dashboard' },
          { label: 'Product Listings', path: '/features/supplier/products', icon: 'category' },
          { label: 'Inventory', path: '/features/supplier/inventory', icon: 'inventory' },
          { label: 'Orders Received', path: '/features/supplier/orders', icon: 'receipt_long' },
          { label: 'Delivery Scheduling', path: '/features/supplier/deliveries', icon: 'event_available' },
          { label: 'Revenue & Analytics', path: '/features/supplier/revenue', icon: 'query_stats' }
        ];
      case 'Driver':
        return [
          { label: 'Dashboard', path: '/features/driver/dashboard', icon: 'dashboard' },
          { label: 'Delivery Queue', path: '/features/driver/queue', icon: 'format_list_bulleted' },
          { label: 'Active Map', path: '/features/driver/map', icon: 'map' },
          { label: 'Status Updates', path: '/features/driver/status', icon: 'gps_fixed' },
          { label: 'Earnings', path: '/features/driver/earnings', icon: 'paid' }
        ];
      case 'Admin':
        return [
          { label: 'Dashboard', path: '/features/admin', icon: 'dashboard' },
          { label: 'User Management', path: '/features/admin/users', icon: 'manage_accounts' },
          { label: 'Verifications', path: '/features/admin/verifications', icon: 'verified_user' },
          { label: 'Product Moderation', path: '/features/admin/products', icon: 'store' },
          { label: 'Transactions', path: '/features/admin/transactions', icon: 'credit_card' },
          { label: 'Escrow Monitoring', path: '/features/admin/escrow', icon: 'lock' },
          { label: 'Delivery Oversight', path: '/features/admin/deliveries', icon: 'local_shipping' },
          { label: 'Platform Settings', path: '/features/admin/settings', icon: 'settings' }
        ];
      default:
        return [];
    }
  });

  get initials(): string {
    const name = this.auth.displayName$();
    if (!name) return '?';

    return name
      .split(' ')
      .filter(Boolean)
      .map((word: string) => word[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  toggleMobile(): void {
    this.mobileOpen.update((open) => !open);
  }

  closeMobile(): void {
    this.mobileOpen.set(false);
  }

  toggleSidebar(): void {
    this.sidebarCollapsed.update((collapsed) => !collapsed);
  }

  @HostListener('window:resize')
  onResize(width = this.getViewportWidth()): void {
    const nextIsMobile = width <= this.mobileBreakpoint;
    this.isMobile.set(nextIsMobile);

    if (!nextIsMobile) {
      this.closeMobile();
    }
  }

  logout(): void {
    this.auth.logout();
    this.closeMobile();
  }

  private getViewportWidth(): number {
    return typeof window === 'undefined' ? 1024 : window.innerWidth;
  }
}
