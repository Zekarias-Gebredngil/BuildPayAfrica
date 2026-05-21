import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../core/auth/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.component.html'
})
export class ShellComponent {
  private readonly auth = inject(AuthService);

  readonly isAuthed    = this.auth.isAuthenticated$;
  readonly roles       = this.auth.roles$;
  readonly userName    = this.auth.displayName$;
  readonly isAdmin     = computed(() => this.auth.roles$().includes('Admin'));
  readonly primaryRole = computed(() => {
    const roles = this.auth.roles$();
    if (roles.includes('Admin'))      return 'Admin';
    if (roles.includes('Customer'))   return 'Customer';
    if (roles.includes('Contractor')) return 'Contractor';
    if (roles.includes('Supplier'))   return 'Supplier';
    if (roles.includes('Driver'))     return 'Driver';
    return '';
  });

  readonly navItems = computed(() => {
    if (!this.isAuthed()) return [];

    const role = this.primaryRole();
    switch (role) {
      case 'Customer':
        return [
          { label: 'Dashboard',    path: '/features/customer/dashboard',      icon: '📊' },
          { label: 'Projects',     path: '/features/customer/projects',        icon: '🏗️' },
          { label: 'Milestones',   path: '/features/customer/milestones',      icon: '🎯' },
          { label: 'Escrow Wallet',path: '/features/customer/wallet',          icon: '🔒' },
          { label: 'Marketplace',  path: '/features/customer/marketplace',     icon: '🏪' },
          { label: 'Cart',         path: '/features/customer/cart',            icon: '🛒' },
          { label: 'Contractors',  path: '/features/customer/contractors',     icon: '👷' },
          { label: 'Orders',       path: '/features/customer/orders',          icon: '📦' },
          { label: 'Deliveries',   path: '/features/customer/deliveries',      icon: '🚚' },
        ];
      case 'Contractor':
        return [
          { label: 'Dashboard',        path: '/features/contractor/dashboard',   icon: '📊' },
          { label: 'Verification',     path: '/features/contractor/verification',icon: '✅' },
          { label: 'Assigned Projects',path: '/features/contractor/projects',    icon: '🏗️' },
          { label: 'Milestone Updates',path: '/features/contractor/milestones',  icon: '🎯' },
          { label: 'Payment Requests', path: '/features/contractor/payments',    icon: '💳' },
          { label: 'Earnings',         path: '/features/contractor/earnings',    icon: '💰' },
          { label: 'Profile & Ratings',path: '/features/contractor/profile',     icon: '⭐' },
        ];
      case 'Supplier':
        return [
          { label: 'Dashboard',         path: '/features/supplier/dashboard',   icon: '📊' },
          { label: 'Product Listings',  path: '/features/supplier/products',    icon: '📋' },
          { label: 'Inventory',         path: '/features/supplier/inventory',   icon: '📦' },
          { label: 'Orders Received',   path: '/features/supplier/orders',      icon: '🛒' },
          { label: 'Delivery Scheduling',path: '/features/supplier/deliveries', icon: '🚚' },
          { label: 'Revenue & Analytics',path: '/features/supplier/revenue',    icon: '📈' },
        ];
      case 'Driver':
        return [
          { label: 'Dashboard',     path: '/features/driver/dashboard', icon: '📊' },
          { label: 'Delivery Queue',path: '/features/driver/queue',     icon: '📋' },
          { label: 'Active Map',    path: '/features/driver/map',       icon: '🗺️' },
          { label: 'Status Updates',path: '/features/driver/status',    icon: '📡' },
          { label: 'Earnings',      path: '/features/driver/earnings',  icon: '💰' },
        ];
      case 'Admin':
        return [
          { label: 'Dashboard',         path: '/features/admin',                icon: '📊' },
          { label: 'User Management',   path: '/features/admin/users',          icon: '👥' },
          { label: 'Verifications',     path: '/features/admin/verifications',  icon: '✅' },
          { label: 'Product Moderation',path: '/features/admin/products',       icon: '🏪' },
          { label: 'Transactions',      path: '/features/admin/transactions',   icon: '💳' },
          { label: 'Escrow Monitoring', path: '/features/admin/escrow',         icon: '🔒' },
          { label: 'Delivery Oversight',path: '/features/admin/deliveries',     icon: '🚚' },
          { label: 'Platform Settings', path: '/features/admin/settings',       icon: '⚙️' },
        ];
      default:
        return [];
    }
  });

  mobileOpen = signal(false);

  toggleMobile() { this.mobileOpen.update(v => !v); }
  closeMobile()  { this.mobileOpen.set(false); }

  logout() {
    this.auth.logout();
    this.closeMobile();
  }

  get initials(): string {
    const n = this.auth.displayName$();
    if (!n) return '?';
    return n.split(' ').map((w: string) => w[0]).join('').toUpperCase().slice(0, 2);
  }
}
