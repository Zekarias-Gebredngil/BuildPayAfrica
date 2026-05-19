import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../core/auth/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="min-h-screen bg-slate-50 text-slate-900">
      <header class="sticky top-0 z-10 border-b bg-white/90 backdrop-blur">
        <div class="mx-auto flex max-w-6xl items-center justify-between px-4 py-3">
          <a routerLink="/" class="text-lg font-semibold tracking-tight">BuildPay Africa</a>

          <nav class="flex items-center gap-3 text-sm">
            <a routerLink="/marketplace" routerLinkActive="font-semibold" class="hover:underline">Marketplace</a>
            <a routerLink="/projects" routerLinkActive="font-semibold" class="hover:underline">Projects</a>
            <a routerLink="/orders" routerLinkActive="font-semibold" class="hover:underline">Orders</a>
            <a routerLink="/notifications" routerLinkActive="font-semibold" class="hover:underline">Notifications</a>
            <a *ngIf="isAdmin()" routerLink="/admin" routerLinkActive="font-semibold" class="hover:underline">Admin</a>
          </nav>

          <div class="flex items-center gap-2">
            <span *ngIf="isAuthed()" class="hidden text-xs text-slate-600 sm:inline">Roles: {{ roles().join(', ') }}</span>
            <a *ngIf="!isAuthed()" routerLink="/login" class="rounded-md bg-slate-900 px-3 py-1.5 text-sm text-white">Login</a>
            <button
              *ngIf="isAuthed()"
              (click)="logout()"
              class="rounded-md border px-3 py-1.5 text-sm hover:bg-slate-100"
            >
              Logout
            </button>
          </div>
        </div>
      </header>

      <main class="mx-auto max-w-6xl px-4 py-8">
        <router-outlet />
      </main>
    </div>
  `
})
export class ShellComponent {
  private readonly auth = inject(AuthService);

  readonly isAuthed = computed(() => this.auth.isAuthenticated);
  readonly roles = computed(() => this.auth.roles);
  readonly isAdmin = computed(() => this.auth.roles.includes('Admin'));

  logout() {
    this.auth.logout();
  }
}

