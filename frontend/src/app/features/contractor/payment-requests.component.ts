import { Component } from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-payment-requests',
  template: `
    <h2>Payment Requests</h2>
    <p class="bp-text-muted">Request escrow releases for completed milestones.</p>
  `
})
export class PaymentRequestsComponent {}

