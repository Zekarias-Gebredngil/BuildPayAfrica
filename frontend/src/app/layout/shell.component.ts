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

  readonly isAuthed  = computed(() => this.auth.isAuthenticated);
  readonly roles     = computed(() => this.auth.roles);
  readonly isAdmin   = computed(() => this.auth.roles.includes('Admin'));
  readonly userName  = computed(() => this.auth.displayName);

  mobileOpen = signal(false);

  toggleMobile() { this.mobileOpen.update(v => !v); }
  closeMobile()  { this.mobileOpen.set(false); }

  logout() {
    this.auth.logout();
    this.closeMobile();
  }

  get initials(): string {
    const n = this.userName();
    if (!n) return '?';
    return n.split(' ').map((w: string) => w[0]).join('').toUpperCase().slice(0, 2);
  }
}
