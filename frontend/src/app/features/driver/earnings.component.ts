import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-driver-earnings',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="bp-page-header"><h2>💰 Earnings</h2><p>Your delivery earnings history.</p></div>
    <div class="bp-card">
      <div class="bp-empty-state">
        <div class="bp-empty-icon">💰</div>
        <h3>Earnings Dashboard</h3>
        <p>Your delivery fees will be tracked here once deliveries are completed and payments are processed.</p>
        <a routerLink="/features/driver/queue" class="bp-btn bp-btn-primary bp-btn-sm" style="margin-top:1rem;">View Delivery Queue →</a>
      </div>
    </div>
  `
})
export class EarningsComponent {}
