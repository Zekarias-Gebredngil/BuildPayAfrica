import { Component } from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-status-updates',
  template: `
    <h2>Status Updates</h2>
    <p class="bp-text-muted">Update delivery status (Assigned → In Transit → Delivered).</p>
  `
})
export class StatusUpdatesComponent {}

