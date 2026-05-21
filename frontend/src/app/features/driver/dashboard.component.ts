import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-driver-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="bp-fade-in">
      <div style="margin-bottom:1.5rem;">
        <h2 style="font-size:1.5rem;font-weight:700;margin:0 0 0.25rem;">Welcome back{{ displayName() ? ', ' + displayName() : '' }} 🚚</h2>
        <p style="color:var(--bp-text-muted);margin:0;">Your driver workspace.</p>
      </div>
      <div class="bp-card" style="padding:1.5rem;">
        <h3 style="font-size:1rem;font-weight:600;margin:0 0 1rem;">Quick Actions</h3>
        <div style="display:flex;flex-wrap:wrap;gap:0.75rem;">
          <a routerLink="/features/driver/queue"  class="bp-btn bp-btn-primary bp-btn-sm">📋 Delivery Queue</a>
          <a routerLink="/features/driver/map"    class="bp-btn bp-btn-secondary bp-btn-sm">🗺️ Active Map</a>
          <a routerLink="/features/driver/status" class="bp-btn bp-btn-ghost bp-btn-sm">📡 Status Updates</a>
          <a routerLink="/features/driver/earnings" class="bp-btn bp-btn-ghost bp-btn-sm">💰 Earnings</a>
        </div>
      </div>
    </div>
  `
})
export class DriverDashboardComponent {
  readonly displayName = inject(AuthService).displayName$;
}
